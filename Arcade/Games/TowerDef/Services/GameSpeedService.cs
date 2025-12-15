using Arcade.Games.TowerDefense.Core;

namespace Arcade.Games.TowerDef.Services;

public class GameSpeedService
{
    private readonly GameState _state;

    public GameSpeedService(GameState state)
    {
        _state = state;
    }

    public void Toggle()
    {
        _state.GameSpeed = _state.GameSpeed switch
        {
            1f => 2f,
            2f => 4f,
            _ => 1f
        };
    }

    public void Set(float speed)
    {
        _state.GameSpeed = speed;
    }
}