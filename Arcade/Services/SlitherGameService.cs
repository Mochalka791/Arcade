using System.Threading.Tasks;
using Arcade.Games.Slither.Models;
using Arcade.Games.Slither.Engine;
using System.Diagnostics;

namespace Arcade.Services
{

    public class SlitherGameService
    {
        public SlitherWorld World { get; } = new();

        private readonly Stopwatch _sw = new();
        private long _lastTicks;
        private bool _running;

        public void Start()
        {
            World.Initialize(botCount: 12, foodCount: 500);
            _running = true;
            _sw.Restart();
            _lastTicks = _sw.ElapsedTicks;
            _ = Loop();
        }

        private async Task Loop()
        {
            double target = 1.0 / 60.0;
            double freq = Stopwatch.Frequency;
            double ticksFrame = freq * target;

            while (_running)
            {
                long now = _sw.ElapsedTicks;
                long diff = now - _lastTicks;

                if (diff >= ticksFrame)
                {
                    float dt = (float)(diff / freq);
                    _lastTicks = now;

                    World.Update(dt, _input);
                }

                await Task.Delay(1);
            }
        }

        private Vec2 _input = Vec2.Zero;

        public void SetInput(Vec2 v)
        {
            _input = v;
        }

        public void Stop() => _running = false;
    }
}
