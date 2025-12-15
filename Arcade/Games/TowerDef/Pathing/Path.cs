namespace Arcade.Games.TowerDef.Pathing;

public static class Path
{
    /// <summary>
    /// Standard‑Pfad (aktueller Spielpfad)
    /// </summary>
    public static List<PathPoint> CreateDefault()
    {
        return new List<PathPoint>
        {
            new(0, 300),
            new(200, 300),
            new(200, 150),
            new(400, 150),
            new(400, 450),
            new(600, 450),
            new(600, 200),
            new(800, 200)
        };
    }

    /// <summary>
    /// Alternativer Pfad (für später)
    /// </summary>
    public static List<PathPoint> CreateAlternative()
    {
        return new List<PathPoint>
        {
            new(0, 200),
            new(250, 200),
            new(250, 400),
            new(500, 400),
            new(500, 150),
            new(800, 150)
        };
    }
}