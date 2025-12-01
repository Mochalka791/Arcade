using Arcade.Games.Slither.Models;

namespace Arcade.Games.Slither.Engine
{

    public interface ISnakeBrain
    {
        Vec2 DecideDirection(SlitherSnake snake, SlitherWorld world);
    }
}
