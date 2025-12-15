namespace Arcade.Games.TowerDef.Towers;

public static class TowerUpgrades
{
    public static int GetUpgradeCost(Tower tower)
    {
        return (int)(GetBaseCost(tower.Type) * 0.7f * tower.Level);
    }

    public static int GetSellValue(Tower tower)
    {
        var total = GetBaseCost(tower.Type);

        for (int i = 1; i < tower.Level; i++)
        {
            total += (int)(GetBaseCost(tower.Type) * 0.7f * i);
        }

        return (int)(total * 0.6f);
    }

    public static int GetBaseCost(TowerType type)
    {
        return type switch
        {
            TowerType.Basic => 50,
            TowerType.Sniper => 100,
            TowerType.Cannon => 150,
            TowerType.Freeze => 120,
            TowerType.Poison => 110,
            TowerType.Lightning => 180,
            _ => 50
        };
    }
}