using System.Collections.Generic;
using System.Linq;
using BilliardGame.Physics;

namespace BilliardGame.Rules;

public enum PlayerGroup
{
    Unknown,
    Solids,
    Stripes
}

public sealed class GameRules
{
    private readonly TurnContext _turn = new();

    public bool PlayerOneTurn { get; private set; } = true;
    public PlayerGroup PlayerOneGroup { get; private set; } = PlayerGroup.Unknown;
    public bool GameOver { get; private set; }
    public bool WinnerIsPlayerOne { get; private set; }
    public string Status { get; private set; } = "Break!";
    public bool TableOpen => PlayerOneGroup == PlayerGroup.Unknown;

    public void Reset()
    {
        PlayerOneTurn = true;
        PlayerOneGroup = PlayerGroup.Unknown;
        GameOver = false;
        WinnerIsPlayerOne = false;
        Status = "Break!";
        _turn.Reset();
    }

    public void BeginTurn()
    {
        _turn.Reset();
    }

    public void RegisterCollision(Ball a, Ball b)
    {
        if (_turn.FirstContact is not null)
        {
            return;
        }

        if (a.IsCueBall && !b.InPocket)
        {
            _turn.FirstContact = b.Category;
        }
        else if (b.IsCueBall && !a.InPocket)
        {
            _turn.FirstContact = a.Category;
        }
    }

    public void RegisterPocket(Ball ball)
    {
        _turn.Pocketed.Add(ball);
        if (ball.IsCueBall)
        {
            _turn.CueBallPocketed = true;
        }
    }

    public TurnResolution CompleteTurn(IReadOnlyList<Ball> balls)
    {
        if (GameOver)
        {
            return new TurnResolution(false, false, true, WinnerIsPlayerOne, Status);
        }

        var currentGroup = PlayerOneTurn ? PlayerOneGroup : OpponentGroup(PlayerOneGroup);
        var evaluation = Evaluate(balls, currentGroup);

        if (evaluation.GameWon)
        {
            GameOver = true;
            WinnerIsPlayerOne = evaluation.WinnerIsPlayerOne;
        }
        else if (evaluation.SwitchPlayer)
        {
            PlayerOneTurn = !PlayerOneTurn;
        }

        Status = evaluation.Message;

        return evaluation;
    }

    private TurnResolution Evaluate(IReadOnlyList<Ball> balls, PlayerGroup currentGroup)
    {
        bool foul = false;
        bool madeLegalBall = false;
        bool eightPocketed = false;
        bool playerWins = false;

        foreach (var ball in _turn.Pocketed.Where(b => !b.IsCueBall))
        {
            if (ball.Category == BallCategory.Eight)
            {
                eightPocketed = true;
                var hasOwn = HasRemainingBalls(currentGroup, balls);
                if (currentGroup == PlayerGroup.Unknown || hasOwn)
                {
                    foul = true;
                    playerWins = false;
                }
                else
                {
                    playerWins = true;
                }
            }
            else
            {
                var pocketedGroup = GroupFromCategory(ball.Category);
                if (TableOpen)
                {
                    AssignGroups(pocketedGroup);
                    currentGroup = PlayerOneTurn ? PlayerOneGroup : OpponentGroup(PlayerOneGroup);
                }

                if (currentGroup == PlayerGroup.Unknown || currentGroup == pocketedGroup)
                {
                    madeLegalBall = true;
                }
                else
                {
                    foul = true;
                }
            }
        }

        if (!TableOpen && currentGroup != PlayerGroup.Unknown)
        {
            if (_turn.FirstContact is null)
            {
                foul = true;
            }
            else
            {
                var required = CategoryFromGroup(currentGroup);
                if (_turn.FirstContact != required)
                {
                    var shootingEight = !HasRemainingBalls(currentGroup, balls);
                    if (!shootingEight || _turn.FirstContact != BallCategory.Eight)
                    {
                        foul = true;
                    }
                }
            }
        }

        if (_turn.CueBallPocketed)
        {
            foul = true;
        }

        if (eightPocketed)
        {
            if (playerWins && !foul)
            {
                var message = $"Spieler {(PlayerOneTurn ? 1 : 2)} gewinnt!";
                return new TurnResolution(false, false, true, PlayerOneTurn, message);
            }

            var lossMessage = $"Spieler {(PlayerOneTurn ? 2 : 1)} gewinnt (8 falsch versenkt).";
            return new TurnResolution(true, true, true, !PlayerOneTurn, lossMessage);
        }

        if (foul)
        {
            return new TurnResolution(true, true, false, !PlayerOneTurn, "Foul: Ball in Hand.");
        }

        if (madeLegalBall)
        {
            return new TurnResolution(false, false, false, PlayerOneTurn, "Weiter am Tisch.");
        }

        return new TurnResolution(true, false, false, !PlayerOneTurn, "Kein Ball versenkt.");
    }

    private void AssignGroups(PlayerGroup group)
    {
        if (PlayerOneGroup != PlayerGroup.Unknown)
        {
            return;
        }

        if (PlayerOneTurn)
        {
            PlayerOneGroup = group;
        }
        else
        {
            PlayerOneGroup = OpponentGroup(group);
        }
    }

    private static bool HasRemainingBalls(PlayerGroup group, IReadOnlyList<Ball> balls)
    {
        if (group == PlayerGroup.Unknown)
        {
            return true;
        }

        var category = CategoryFromGroup(group);
        return balls.Any(b => !b.InPocket && !b.IsCueBall && b.Category == category);
    }

    private static PlayerGroup GroupFromCategory(BallCategory category) => category switch
    {
        BallCategory.Solid => PlayerGroup.Solids,
        BallCategory.Stripe => PlayerGroup.Stripes,
        _ => PlayerGroup.Unknown
    };

    private static BallCategory CategoryFromGroup(PlayerGroup group) => group switch
    {
        PlayerGroup.Solids => BallCategory.Solid,
        PlayerGroup.Stripes => BallCategory.Stripe,
        _ => BallCategory.Solid
    };

    private static PlayerGroup OpponentGroup(PlayerGroup group) => group switch
    {
        PlayerGroup.Solids => PlayerGroup.Stripes,
        PlayerGroup.Stripes => PlayerGroup.Solids,
        _ => PlayerGroup.Unknown
    };

    private sealed class TurnContext
    {
        public BallCategory? FirstContact { get; set; }
        public bool CueBallPocketed { get; set; }
        public List<Ball> Pocketed { get; } = new();

        public void Reset()
        {
            FirstContact = null;
            CueBallPocketed = false;
            Pocketed.Clear();
        }
    }
}
