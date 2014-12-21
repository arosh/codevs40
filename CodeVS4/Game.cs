using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CodeVS4
{
    interface IGame
    {
        IField Field { get; }
        int Turn { get; }
        EGameResult Next();
    }

    public enum EGameResult
    {
        Win, Lose, Draw, Continue
    }

    public class Game : IGame
    {
        private Player[] Players;
        private IEnumerable<IUnit>[] Units;
        private IEnumerable<IUnit> Resources;
        public IField Field { get; private set; }
        public int Turn { get; private set; }
        private static Random Random_ = new Random(114514);
        public static Random Random { get { return Random_; } }

        public Game()
        {
            Field = new Field();
            Players = new Player[2];
            Turn = 1;
            Resources = new List<IUnit>();

        }

        public static int Manhattan(IPoint a, IPoint b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        public static IPoint RandomPoint()
        {
            int X = Random.Next(100);
            int Y = Random.Next(100);
            return new Point(X, Y);
        }

        public static IEnumerable<IPoint> LocateResource(IPoint basePoint, IPoint castle)
        {
            var ret = new List<IPoint>();
            for (int i = 0; i < 10; i++)
            {
                IPoint p;
                bool f;
                do
                {
                    p = RandomPoint();
                    f = true;
                    foreach (var q in ret)
                    {
                        if (p.Equals(q))
                        {
                            f = false;
                            break;
                        }
                    }
                } while (f == false || Manhattan(p, basePoint) > 99 || Manhattan(p, castle) <= Unit.CastleView);
                ret.Add(p);
            }
            return ret;
        }

        public static IEnumerable<IPoint>[] LocateResources(IPoint[] castles)
        {
            return new[] { LocateResource(new Point(0, 0), castles[0]), LocateResource(new Point(99, 99), castles[1]) };
        }

        public static IPoint[] LocateCastle()
        {
            var basePoint = new[] { new Point(0, 0), new Point(99, 99) };

            var ret = new IPoint[2];
            do
            {
                ret[0] = RandomPoint();
            } while (Manhattan(ret[0], basePoint[0]) > 40);

            do
            {
                ret[1] = RandomPoint();
            } while (Manhattan(ret[1], basePoint[1]) > 40);

            return ret;
        }

        public EGameResult Next()
        {
            OrderPhase();
            MoveAndBuildPhase();
            BattlePhase();
            RemoveUnitPhase();
            ObtainResourcePhase();
            var ret = EndPhase();
            Turn++;
            return ret;
        }

        private void OrderPhase()
        {

        }

        private void MoveAndBuildPhase()
        {

        }

        private void BattlePhase()
        {

        }

        private void RemoveUnitPhase()
        {

        }

        private void ObtainResourcePhase()
        {

        }

        private EGameResult EndPhase()
        {
            int a = Players[0].Castle.Hp;
            int b = Players[1].Castle.Hp;

            if (a == 0 && b == 0)
            {
                return EGameResult.Draw;
            }

            if (b == 0)
            {
                return EGameResult.Win;
            }

            if (a == 0)
            {
                return EGameResult.Lose;
            }

            if (Turn >= 1000)
            {
                return EGameResult.Draw;
            }

            return EGameResult.Continue;
        }
    }
}
