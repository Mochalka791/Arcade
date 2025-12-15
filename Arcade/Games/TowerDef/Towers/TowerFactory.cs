namespace Arcade.Games.TowerDef.Towers;

public static class TowerFactory
{
    public static Tower Create(
        TowerType type,
        float x,
        float y)
    {
        return type switch
        {
            TowerType.Basic =>
                new Tower(type, x, y, 100, 1.0f, 10),

            TowerType.Sniper =>
                new Tower(type, x, y, 200, 2.0f, 50),

            TowerType.Cannon =>
                new Tower(type, x, y, 80, 0.5f, 100),

            TowerType.Freeze =>
                new Tower(type, x, y, 90, 1.5f, 5),

            TowerType.Poison =>
                new Tower(type, x, y, 100, 1.0f, 3),

            TowerType.Lightning =>
                new Tower(type, x, y, 120, 1.2f, 40),

            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}