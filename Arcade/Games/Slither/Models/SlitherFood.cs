namespace Arcade.Games.Slither.Models
{
    // Food-Objekt im Spielfeld
    public class SlitherFood
    {
        public int Id { get; set; }

        // Position des Foods
        public Vec2 Pos { get; set; }

        // Wert des Foods (Wachstum)
        public float Value { get; set; } = 1f;

        // Farbe des Foods
        public string Color { get; set; } = "#ffd166";
    }
}
