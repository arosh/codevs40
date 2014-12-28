using System;
using System.Diagnostics;

namespace CodeVS4
{
    public enum UnitType
    {
        Worker, Knight, Fighter, Assassin, Castle, Village, Factory
    }

    public enum OrderType
    {
        MoveU, MoveD, MoveL, MoveR, BuildWorker, BuildKnight, BuildFighter, BuildAssassin, BuildVillage, BuildFactory
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

        public static int GetDefaultHp(UnitType unitType)
        {
            return HpTbl[(int)unitType];
        }

        public static int GetAttackRange(UnitType unitType)
        {
            return AttackRangeTbl[(int)unitType];
        }

        public static int GetViewRange(UnitType unitType)
        {
            return ViewRangeTbl[(int)unitType];
        }

        public static int GetCost(UnitType unitType)
        {
            Debug.Assert(unitType != UnitType.Castle);
            return CostTbl[(int)unitType];
        }

        public static int GetAttack(UnitType atk, UnitType def)
        {
            return AttackTbl[(int)atk, (int)def];
        }

        public static OrderType Build(UnitType unitType)
        {
            switch (unitType)
            {
                case UnitType.Worker:
                    return OrderType.BuildWorker;

                case UnitType.Knight:
                    return OrderType.BuildKnight;

                case UnitType.Fighter:
                    return OrderType.BuildFighter;

                case UnitType.Assassin:
                    return OrderType.BuildAssassin;

                case UnitType.Village:
                    return OrderType.BuildVillage;

                case UnitType.Factory:
                    return OrderType.BuildFactory;
            }

            throw new ArgumentException("生産できないものを生産しようとしました。", "unitType");
        }
    }
}
