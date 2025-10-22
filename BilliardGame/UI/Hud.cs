using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BilliardGame.UI;

public readonly record struct HudState(bool IsPlayer, bool PlayerOneTurn, bool GameOver, float Charge, bool IsCharging, string StatusText);

public sealed class Hud
{
    private SpriteFont _font = default!;

    public void Load(SpriteFont font)
    {
        _font = font;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, HudState state)
    {
        var roleText = $"Rolle: {(state.IsPlayer ? "Spieler" : "Gast")}";
        spriteBatch.DrawString(_font, roleText, new Vector2(20f, 20f), Color.White);

        var turnText = $"Am Zug: Spieler {(state.PlayerOneTurn ? 1 : 2)}";
        spriteBatch.DrawString(_font, turnText, new Vector2(20f, 50f), Color.White);

        spriteBatch.DrawString(_font, state.StatusText, new Vector2(20f, 80f), Color.Gold);

        if (state.IsCharging)
        {
            DrawPowerBar(spriteBatch, pixel, state.Charge);
        }

        if (state.GameOver)
        {
            var text = "Spiel beendet! [Leertaste] für neues Spiel.";
            spriteBatch.DrawString(_font, text, new Vector2(20f, 140f), Color.Yellow);
        }
    }

    private static void DrawPowerBar(SpriteBatch spriteBatch, Texture2D pixel, float charge)
    {
        var outer = new Rectangle(20, 110, 220, 14);
        spriteBatch.Draw(pixel, outer, Color.White * 0.2f);

        var inner = new Rectangle(outer.X + 2, outer.Y + 2, (int)((outer.Width - 4) * MathHelper.Clamp(charge, 0f, 1f)), outer.Height - 4);
        spriteBatch.Draw(pixel, inner, Color.OrangeRed);
    }
}
