using System.Linq;
using Arcade.Games.Slither.Models;

namespace Arcade.Games.Slither.Engine
{

    public class SimpleBotBrain : ISnakeBrain
    {
        public Vec2 DecideDirection(SlitherSnake snake, SlitherWorld world)
        {
            // Nächstes Food bestimmen
            var food = world.Food
                .OrderBy(f => (f.Pos - snake.HeadPos).Length)
                .FirstOrDefault();

            // Kein Food vorhanden
            if (food == null)
                return snake.Direction;

            // Richtung zum Food zurückgeben
            return (food.Pos - snake.HeadPos).Normalized();
        }
    }
}
