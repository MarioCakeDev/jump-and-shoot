using Godot;

namespace JumpAndShoot.scripts
{
    public class Stopwatch
    {
        private readonly int _cooldownMilliseconds;
        private uint _currentTime;

        public Stopwatch(int cooldownMilliseconds)
        {
            this._cooldownMilliseconds = cooldownMilliseconds;
            this.Reset();
        }

        public void Reset()
        {
            this._currentTime = OS.GetTicksMsec();
        }

        public bool IsRunCooldown()
        {
            return this._currentTime + this._cooldownMilliseconds <= OS.GetTicksMsec();
        }

        public void Stop()
        {
            this.Reset();
            this._currentTime += (uint) this._cooldownMilliseconds + 1;
        }
    }
}