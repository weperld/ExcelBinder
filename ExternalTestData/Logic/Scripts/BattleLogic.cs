using System;

namespace MyGame.Logic
{
    public interface IBattleLogic
    {
        int CalculateDamage(int atk, int def);
        long GetExpNeeded(int level);
    }

    public class BattleLogic : IBattleLogic
    {
        public int CalculateDamage(int atk, int def)
        {
            return atk - def > 0 ? atk - def : 1;
        }
        public long GetExpNeeded(int level)
        {
            return (long)Math.Pow(level, 3) * 100;
        }
    }
}
