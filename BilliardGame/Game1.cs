using System.Collections.Generic;
using BilliardGame.Physics;
using BilliardGame.Rules;
using BilliardGame.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BilliardGame;

public enum GamePhase
{
    Aim,
    Charging,
    Simulating,
    Resolving
}

public sealed class Game1 : Game
{
    private const float FixedDelta = 1f / 120f;
    private const float ShotMaxSpeed = 1700f;
    private const float MinShotPower = 0.05f;

    private static readonly Dictionary<int, Color> BallPalette = new()
    {
        [1] = new Color(255, 204, 0),
        [2] = new Color(0, 102, 255),
        [3] = new Color(204, 0, 0),
        [4] = new Color(153, 0, 204),
        [5] = new Color(255, 102, 0),
        [6] = new Color(0, 153, 102),
        [7] = new Color(128, 0, 64),
        [8] = Color.Black,
        [9] = new Color(255, 204, 0),
        [10] = new Color(0, 102, 255),
        [11] = new Color(204, 0, 0),
        [12] = new Color(153, 0, 204),
        [13] = new Color(255, 102, 0),
        [14] = new Color(0, 153, 102),
        [15] = new Color(128, 0, 64)
    };

    private readonly GraphicsDeviceManager _graphics;
    private readonly Table _table;
    private readonly List<Ball> _balls = new();
    private readonly SpatialHash _spatialHash;
    private readonly CollisionResolver _collisionResolver;
    private readonly GameRules _rules = new();
    private readonly Hud _hud = new();

    private SpriteBatch _spriteBatch = default!;
    private Texture2D _whitePixel = default!;
    private Texture2D _ballMask = default!;
    private SpriteFont _font = default!;

    private Vector2 _tableOffset;
    private MouseState _previousMouse;
    private KeyboardState _previousKeyboard;
    private Vector2 _aimDirection = Vector2.UnitX;
    private float _accumulator;
    private float _charge;
    private GamePhase _phase = GamePhase.Aim;
    private bool _isPlayerRole;
    private bool _awaitingRack;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            SynchronizeWithVerticalRetrace = true
        };

        IsMouseVisible = true;
        Content.RootDirectory = "Content";

        _table = Table.CreateStandard();
        _spatialHash = new SpatialHash(_table.BallRadius * 4f);
        _collisionResolver = new CollisionResolver(_table);
    }

    protected override void Initialize()
    {
        base.Initialize();
        ResetGame();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });

        _ballMask = CreateCircleTexture(96);

        _font = Content.Load<SpriteFont>("DefaultFont");
        _hud.Load(_font);

        UpdateLayout();
    }

    protected override void Update(GameTime gameTime)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        UpdateLayout();
        HandleInput(delta);

        if (_phase == GamePhase.Simulating)
        {
            _accumulator += delta;
            _accumulator = MathF.Min(_accumulator, FixedDelta * 8f);

            while (_accumulator >= FixedDelta && _phase == GamePhase.Simulating)
            {
                FixedUpdate(FixedDelta);
                _accumulator -= FixedDelta;
            }

            if (_phase == GamePhase.Resolving)
            {
                ResolveAfterMotion();
            }
        }
        else
        {
            _accumulator = 0f;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(6, 60, 40));

        _spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);

        DrawTable();
        DrawPockets();
        DrawAimGuide();
        DrawBalls();
        DrawHud();

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void FixedUpdate(float dt)
    {
        var anyMoving = false;

        foreach (var ball in _balls)
        {
            if (ball.InPocket)
            {
                continue;
            }

            ball.Position += ball.Velocity * dt;

            if (_table.TryPocket(ball))
            {
                _rules.RegisterPocket(ball);
                continue;
            }

            ball.ApplyDamping(dt, _table.RollFriction);

            if (!ball.Sleeping)
            {
                anyMoving = true;
            }
        }

        _collisionResolver.Step(_balls, _spatialHash, _rules);

        foreach (var ball in _balls)
        {
            if (!ball.InPocket && !ball.Sleeping)
            {
                anyMoving = true;
                break;
            }
        }

        if (!anyMoving)
        {
            _phase = GamePhase.Resolving;
        }
    }

    private void ResolveAfterMotion()
    {
        foreach (var ball in _balls)
        {
            if (!ball.InPocket)
            {
                ball.Stop();
            }
        }

        var resolution = _rules.CompleteTurn(_balls);

        if (resolution.CueBallInHand)
        {
            PlaceCueBallInHand();
        }

        if (resolution.GameWon)
        {
            _awaitingRack = true;
            _phase = GamePhase.Aim;
            _charge = 0f;
        }
        else
        {
            PrepareForAim();
        }
    }

    private void PrepareForAim()
    {
        _phase = GamePhase.Aim;
        _charge = 0f;
        _rules.BeginTurn();
    }

    private void HandleInput(float delta)
    {
        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (keyboard.IsKeyDown(Keys.F1) && !_previousKeyboard.IsKeyDown(Keys.F1))
        {
            _isPlayerRole = !_isPlayerRole;
        }

        if (_awaitingRack)
        {
            if (keyboard.IsKeyDown(Keys.Space) && !_previousKeyboard.IsKeyDown(Keys.Space))
            {
                ResetGame();
            }

            _previousMouse = mouse;
            _previousKeyboard = keyboard;
            return;
        }

        if (_phase is GamePhase.Aim or GamePhase.Charging)
        {
            UpdateAimDirection(mouse);
        }

        if (_phase == GamePhase.Aim && mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
        {
            _phase = GamePhase.Charging;
            _charge = 0f;
        }
        else if (_phase == GamePhase.Charging)
        {
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                _charge = MathF.Min(1f, _charge + delta * 0.7f);
            }
            else if (_previousMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released)
            {
                FireShot();
            }
        }

        _previousMouse = mouse;
        _previousKeyboard = keyboard;
    }

    private void FireShot()
    {
        var cueBall = CueBall;

        if (cueBall.InPocket)
        {
            PlaceCueBallInHand();
        }

        var normalizedPower = MathF.Clamp(_charge, MinShotPower, 1f);
        var speed = normalizedPower * ShotMaxSpeed;

        cueBall.InPocket = false;
        cueBall.Stop();
        cueBall.Velocity = _aimDirection * speed;

        _phase = GamePhase.Simulating;
        _charge = 0f;
        _accumulator = 0f;
    }

    private void UpdateAimDirection(MouseState mouse)
    {
        var cueBall = CueBall;
        if (cueBall.InPocket)
        {
            return;
        }

        var mouseWorld = Vector2.Clamp(ScreenToTable(new Vector2(mouse.X, mouse.Y)), Vector2.Zero, new Vector2(_table.Width, _table.Height));
        var direction = mouseWorld - cueBall.Position;

        if (direction.LengthSquared() > 0.001f)
        {
            _aimDirection = Vector2.Normalize(direction);
        }
    }

    private void DrawTable()
    {
        var surface = new Rectangle((int)_tableOffset.X, (int)_tableOffset.Y, (int)_table.Width, (int)_table.Height);
        _spriteBatch.Draw(_whitePixel, surface, new Color(8, 92, 57));
    }

    private void DrawPockets()
    {
        foreach (var pocket in _table.Pockets)
        {
            var dest = new Rectangle(
                (int)(_tableOffset.X + pocket.X - _table.PocketRadius),
                (int)(_tableOffset.Y + pocket.Y - _table.PocketRadius),
                (int)(_table.PocketRadius * 2f),
                (int)(_table.PocketRadius * 2f));

            _spriteBatch.Draw(_ballMask, dest, Color.Black * 0.85f);
        }
    }

    private void DrawBalls()
    {
        foreach (var ball in _balls)
        {
            if (ball.InPocket)
            {
                continue;
            }

            DrawBall(ball);
        }
    }

    private void DrawBall(Ball ball)
    {
        var screen = _tableOffset + ball.Position;
        var diameter = (int)MathF.Round(ball.Diameter);
        var dest = new Rectangle((int)(screen.X - ball.Radius), (int)(screen.Y - ball.Radius), diameter, diameter);

        switch (ball.Category)
        {
            case BallCategory.Cue:
                _spriteBatch.Draw(_ballMask, dest, Color.White);
                break;
            case BallCategory.Stripe:
                _spriteBatch.Draw(_ballMask, dest, Color.White);
                var stripeRect = new Rectangle(dest.X, dest.Y + dest.Height / 3, dest.Width, dest.Height / 3);
                _spriteBatch.Draw(_whitePixel, stripeRect, ball.Color);
                _spriteBatch.Draw(_ballMask, dest, Color.Black * 0.12f);
                break;
            default:
                _spriteBatch.Draw(_ballMask, dest, ball.Color);
                break;
        }

        _spriteBatch.Draw(_ballMask, dest, Color.Black * 0.2f);

        if (!ball.IsCueBall)
        {
            var label = ball.Number.ToString();
            var size = _font.MeasureString(label);
            var textPos = new Vector2(screen.X - size.X / 2f, screen.Y - size.Y / 2f);
            var textColor = ball.Category == BallCategory.Stripe ? Color.Black : Color.White;
            _spriteBatch.DrawString(_font, label, textPos, textColor);
        }
    }

    private void DrawAimGuide()
    {
        if (_phase is not (GamePhase.Aim or GamePhase.Charging) || _rules.GameOver || _awaitingRack)
        {
            return;
        }

        var cueBall = CueBall;
        if (cueBall.InPocket)
        {
            return;
        }

        var cueScreen = _tableOffset + cueBall.Position;
        var length = 200f + _charge * 320f;
        var angle = MathF.Atan2(_aimDirection.Y, _aimDirection.X);
        var rect = new Rectangle((int)cueScreen.X, (int)cueScreen.Y, (int)length, 2);
        _spriteBatch.Draw(_whitePixel, rect, null, Color.Yellow, angle, new Vector2(0f, 0.5f), SpriteEffects.None, 0f);
    }

    private void DrawHud()
    {
        var state = new HudState(_isPlayerRole, _rules.PlayerOneTurn, _rules.GameOver, _charge, _phase == GamePhase.Charging, _rules.Status);
        _hud.Draw(_spriteBatch, _whitePixel, state);
    }

    private Ball CueBall => _balls[0];

    private void ResetGame()
    {
        RackBalls();
        _rules.Reset();
        _awaitingRack = false;
        _accumulator = 0f;
        _charge = 0f;
        _aimDirection = Vector2.UnitX;
        PrepareForAim();
        _previousMouse = Mouse.GetState();
        _previousKeyboard = Keyboard.GetState();
    }

    private void RackBalls()
    {
        _balls.Clear();

        var cueBall = new Ball(0, BallCategory.Cue, _table.BallRadius, 0.17f, Color.White);
        cueBall.Reset(new Vector2(_table.Width * 0.25f, _table.Height / 2f));
        _balls.Add(cueBall);

        int[] order = { 1, 10, 3, 12, 5, 8, 2, 11, 4, 14, 6, 13, 7, 15, 9 };
        var origin = new Vector2(_table.Width * 0.7f, _table.Height / 2f);
        var rowSpacing = cueBall.Diameter * 0.8660254f;
        var columnSpacing = cueBall.Diameter;
        var index = 0;

        for (var row = 0; row < 5; row++)
        {
            for (var col = 0; col <= row; col++)
            {
                var number = order[index++];
                var category = CategoryForNumber(number);
                var color = ColorForNumber(number);
                var position = new Vector2(
                    origin.X + rowSpacing * row,
                    origin.Y - columnSpacing * row * 0.5f + columnSpacing * col);

                var ball = new Ball(number, category, _table.BallRadius, 0.17f, color);
                ball.Reset(position);
                _balls.Add(ball);
            }
        }
    }

    private void PlaceCueBallInHand()
    {
        var cueBall = CueBall;
        var position = new Vector2(_table.Width * 0.25f, _table.Height / 2f);
        var attempts = 0;
        var maxAttempts = 50;

        while (attempts < maxAttempts)
        {
            var overlap = false;
            foreach (var ball in _balls)
            {
                if (ball == cueBall || ball.InPocket)
                {
                    continue;
                }

                if (Vector2.Distance(position, ball.Position) < cueBall.Radius + ball.Radius + 1f)
                {
                    position.Y += cueBall.Diameter * 1.1f;
                    if (position.Y > _table.Height - cueBall.Radius)
                    {
                        position.Y = cueBall.Radius;
                        position.X += cueBall.Diameter * 1.2f;
                    }

                    overlap = true;
                    break;
                }
            }

            if (!overlap)
            {
                break;
            }

            attempts++;
        }

        var min = new Vector2(_table.BallRadius, _table.BallRadius);
        var max = new Vector2(_table.Width - _table.BallRadius, _table.Height - _table.BallRadius);
        position = Vector2.Clamp(position, min, max);

        cueBall.Reset(position);
    }

    private Color ColorForNumber(int number) => BallPalette.TryGetValue(number, out var color) ? color : Color.White;

    private static BallCategory CategoryForNumber(int number) => number switch
    {
        8 => BallCategory.Eight,
        >= 1 and <= 7 => BallCategory.Solid,
        _ => BallCategory.Stripe
    };

    private Vector2 ScreenToTable(Vector2 screen) => screen - _tableOffset;

    private void UpdateLayout()
    {
        var viewport = GraphicsDevice.Viewport;
        _tableOffset = new Vector2(
            (viewport.Width - _table.Width) / 2f,
            (viewport.Height - _table.Height) / 2f);
    }

    private Texture2D CreateCircleTexture(int diameter)
    {
        var texture = new Texture2D(GraphicsDevice, diameter, diameter);
        var data = new Color[diameter * diameter];
        var radius = diameter / 2f;
        var center = new Vector2(radius, radius);
        var radiusSquared = radius * radius;

        for (var y = 0; y < diameter; y++)
        {
            for (var x = 0; x < diameter; x++)
            {
                var index = y * diameter + x;
                var position = new Vector2(x + 0.5f, y + 0.5f);
                var distance = Vector2.DistanceSquared(position, center);

                if (distance <= radiusSquared)
                {
                    var edge = radiusSquared - distance;
                    var alpha = edge < 1.5f ? MathHelper.Clamp(edge, 0f, 1f) : 1f;
                    data[index] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    data[index] = Color.Transparent;
                }
            }
        }

        texture.SetData(data);
        return texture;
    }
}
