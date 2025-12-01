using System;
using System.Collections.Generic;

namespace Arcade.Games.Slither.Models
{
    // Datentyp der Schlange: Spieler oder Bot
    public enum SnakeType { Player, Bot }

    // Hauptobjekt der Schlange
    public class SlitherSnake
    {
        // Eindeutige ID
        public Guid Id { get; set; } = Guid.NewGuid();

        // Spieler oder KI
        public SnakeType Type { get; set; }

        // Name der Schlange
        public string Name { get; set; } = "Bot";

        // Farbe der Schlange
        public string Color { get; set; } = "#5cf0c8";

        // Kopfposition
        public Vec2 HeadPos { get; set; }

        // Bewegungsrichtung (normalisiert)
        public Vec2 Direction { get; set; } = new(1, 0);

        // Geschwindigkeit in Welt-Einheiten
        public float Speed { get; set; } = 160f;

        // Radius der Segmente
        public float Radius { get; set; } = 12f;

        // Liste der Körpersegmente
        public List<Vec2> Segments { get; set; } = new();

        // Schlange ist tot
        public bool IsDead { get; set; }

        // Soll-Länge (für Wachstum)
        public float TargetLength { get; set; } = 40f;
    }
}
