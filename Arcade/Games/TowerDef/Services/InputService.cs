namespace Arcade.Games.TowerDef.Services;
using Arcade.Games.TowerDefense.Core;
using Arcade.Games.TowerDef.Towers;
using Arcade.Games.TowerDef.Pathing;

public class InputService
{
    private readonly GameState _state;
    private readonly List<Tower> _towers;
    private readonly List<PathPoint> _path;

    public TowerType? SelectedTower { get; private set; }

    public InputService(
        GameState state,
        List<Tower> towers,
        List<PathPoint> path)
    {
        _state = state;
        _towers = towers;
        _path = path;
    }

    public void SelectTower(TowerType type)
    {
        SelectedTower = type;
    }

    public bool TryPlaceTower(float x, float y)
    {
        if (SelectedTower == null || _state.GameOver)
            return false;

        if (IsOnPath(x, y))
            return false;

        var cost = TowerUpgrades.GetBaseCost(SelectedTower.Value);
        if (_state.Gold < cost)
            return false;

        var tower = TowerFactory.Create(SelectedTower.Value, x, y);

        _state.Gold -= cost;
        _towers.Add(tower);
        SelectedTower = null;

        return true;
    }

    private bool IsOnPath(float x, float y)
    {
        const float pathWidth = 50f;

        for (int i = 0; i < _path.Count - 1; i++)
        {
            var p1 = _path[i];
            var p2 = _path[i + 1];

            if (DistanceToSegment(x, y, p1, p2) < pathWidth)
                return true;
        }

        return false;
    }

    private static float DistanceToSegment(
        float px, float py,
        PathPoint a, PathPoint b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var lenSq = dx * dx + dy * dy;

        if (lenSq == 0)
            return Distance(px, py, a.X, a.Y);

        var t = ((px - a.X) * dx + (py - a.Y) * dy) / lenSq;
        t = Math.Clamp(t, 0, 1);

        var projX = a.X + t * dx;
        var projY = a.Y + t * dy;

        return Distance(px, py, projX, projY);
    }

    private static float Distance(
        float x1, float y1,
        float x2, float y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}