namespace Arcade.Games.TowerDef.Pathing;

public class PathSelector
{
    private readonly List<List<PathPoint>> _paths = new();

    public PathSelector()
    {
        // Standardpfade registrieren
        _paths.Add(Path.CreateDefault());
        _paths.Add(Path.CreateAlternative());
    }

    /// <summary>
    /// Aktuell: zufälliger Pfad
    /// Später: intelligent (weniger Tower, kürzer, etc.)
    /// </summary>
    public List<PathPoint> SelectPath()
    {
        return _paths[Random.Shared.Next(_paths.Count)];
    }

    /// <summary>
    /// Für Boss‑Enemies:
    /// immer Hauptpfad
    /// </summary>
    public List<PathPoint> SelectBossPath()
    {
        return _paths[0];
    }

    /// <summary>
    /// Erweiterungspunkt:
    /// Pfadgewichtung (Danger / Length)
    /// </summary>
    public List<PathPoint> SelectWeightedPath(Func<List<PathPoint>, float> weightFunc)
    {
        return _paths
            .OrderBy(weightFunc)
            .First();
    }
}