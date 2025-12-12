using System.Linq;
using Arcade.Games.Slither.Models;

namespace Arcade.Games.Slither.Engine
{

    public class SimpleBotBrain : ISnakeBrain
    {
        public Vec2 DecideDirection(SlitherSnake snake, SlitherWorld world)
        {
            var food = world.Food
                .OrderBy(f => (f.Pos - snake.HeadPos).Length)
                .FirstOrDefault();

            if (food == null)
                return snake.Direction;
            return (food.Pos - snake.HeadPos).Normalized();
        }
    }
}
