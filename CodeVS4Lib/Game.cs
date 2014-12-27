using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeVS4
{
    interface IGame
    {
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
        private IList<IUnit>[] Units;
        private IEnumerable<Point> Resources;
        public int Turn { get; private set; }
        public int Id { get; private set; }

        public Game()
        {
            Players = new[] { new Player(), new Player() };
            Units = new[] { new List<IUnit>(), new List<IUnit>() };
            Turn = 1;
            Id = 0;

            var castles = LocateCastles();
            for (int i = 0; i < 2; i++)
            {
                Units[i].Add(new Unit(Id++, UnitType.Castle, castles[i]));
                
            }

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Units[i].Add(new Unit(Id++, UnitType.Worker, castles[i]));
                }
            }

            Resources = LocateResources(castles);
        }

        public static int Manhattan(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        public static Point RandomPoint()
        {
            int X = GameConstant.Random.Next(GameConstant.FieldSize);
            int Y = GameConstant.Random.Next(GameConstant.FieldSize);
            return new Point(X, Y);
        }

        public static IEnumerable<Point> LocateResource(Point basePoint, Point castle)
        {
            var ret = new List<Point>();
            for (int i = 0; i < 10; i++)
            {
                Point p;
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
                } while (f == false || Manhattan(p, basePoint) > 99 || Manhattan(p, castle) <= GameConstant.GetViewRange(UnitType.Castle));
                ret.Add(p);
            }
            return ret;
        }

        public static IEnumerable<Point> LocateResources(Point[] castles)
        {
            var a = LocateResource(GameConstant.BasePoint[0], castles[0]);
            var b = LocateResource(GameConstant.BasePoint[1], castles[1]);
            var ret = new List<Point>();
            ret.AddRange(a);
            ret.AddRange(b);
            return ret;
        }

        public static Point[] LocateCastles()
        {
            var ret = new Point[2];
            do
            {
                ret[0] = RandomPoint();
            } while (Manhattan(ret[0], GameConstant.BasePoint[0]) > 40);

            do
            {
                ret[1] = RandomPoint();
            } while (Manhattan(ret[1], GameConstant.BasePoint[1]) > 40);

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

        // 城もぶっ壊す
        private void RemoveUnitPhase()
        {

        }

        private void ObtainResourcePhase()
        {

        }

        private EGameResult EndPhase()
        {
            bool a = Units[0].Count(unit => unit.Type == UnitType.Castle) > 0;
            bool b = Units[1].Count(unit => unit.Type == UnitType.Castle) > 0;
            
            if (a == false && b == false)
            {
                return EGameResult.Draw;
            }

            if (b == false)
            {
                return EGameResult.Win;
            }

            if (a == false)
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
