using System;

namespace CodeVS4
{
    public enum EUnitType
    {
        Worker, Knight, Fighter, Assassin, Castle, Village, Base
    }

    static class GameConstant
    {
        public static readonly int FieldSize = 100;
        public static readonly Point[] BasePoint = { new Point(0, 0), new Point(99, 99) };

        public static readonly Random Random = new Random(810);

        private static readonly int[] HpTbl = { 2000, 5000, 5000, 5000, 50000, 20000, 20000 };
        private static readonly int[] AttackRangeTbl = { 2, 2, 2, 2, 10, 2, 2 };
        private static readonly int[] ViewRangeTbl = { 4, 4, 4, 4, 10, 10, 4 };
        private static readonly int[] CostTbl = { 40, 20, 40, 60, 0, 100, 500 };
        private static readonly int[,] AttackTbl =
        {
            { 100, 100, 100, 100, 100, 100, 100 },
            { 100, 500, 200, 200, 200, 200, 200 },
            { 500, 1600, 500, 200, 200, 200, 200 },
            { 1000, 500, 1000, 500, 200, 200, 200 },
            { 100, 100, 100, 100, 100, 100, 100},
            { 100, 100, 100, 100, 100, 100, 100},
            { 100, 100, 100, 100, 100, 100, 100}
        };

        public static int GetDefaultHp(EUnitType unitType)
        {
            return HpTbl[(int)unitType];
        }

        public static int GetAttackRange(EUnitType unitType)
        {
            return AttackRangeTbl[(int)unitType];
        }

        public static int GetViewRange(EUnitType unitType)
        {
            return ViewRangeTbl[(int)unitType];
        }

        public static int GetCost(EUnitType unitType)
        {
            return CostTbl[(int)unitType];
        }

        public static int GetAttack(EUnitType atk, EUnitType def)
        {
            return AttackTbl[(int)atk, (int)def];
        }
    }
}
