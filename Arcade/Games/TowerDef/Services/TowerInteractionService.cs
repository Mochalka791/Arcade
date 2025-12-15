namespace Arcade.Games.TowerDef.Services;
using Arcade.Games.TowerDefense.Core;
using Arcade.Games.TowerDef.Towers;



public class TowerInteractionService
{
    private readonly GameState _state;
    private readonly List<Tower> _towers;

    public Tower? SelectedTower { get; private set; }

    public TowerInteractionService(GameState state, List<Tower> towers)
    {
        _state = state;
        _towers = towers;
    }

    public void SelectTower(Tower? tower)
    {
        SelectedTower = tower;
    }

    public bool CanUpgrade(Tower tower)
    {
        return _state.Gold >= TowerUpgrades.GetUpgradeCost(tower);
    }

    public int GetUpgradeCost(Tower tower)
    {
        return TowerUpgrades.GetUpgradeCost(tower);
    }

    public int GetSellValue(Tower tower)
    {
        return TowerUpgrades.GetSellValue(tower);
    }

    public void Upgrade()
    {
        if (SelectedTower == null)
            return;

        var cost = TowerUpgrades.GetUpgradeCost(SelectedTower);
        if (_state.Gold < cost)
            return;

        _state.Gold -= cost;
        SelectedTower.Upgrade();
    }

    public void Sell()
    {
        if (SelectedTower == null)
            return;

        _state.Gold += TowerUpgrades.GetSellValue(SelectedTower);
        _towers.Remove(SelectedTower);
        SelectedTower = null;
    }
}