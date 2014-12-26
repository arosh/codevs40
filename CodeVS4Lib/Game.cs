﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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
        private const int FieldSize = 100;
        private static readonly Point[] basePoint = new[] { new Point(0, 0), new Point(99, 99) };
        private Player[] Players;
        private IList<IUnit>[] Units;
        private IEnumerable<IPoint> Resources;
        public int Turn { get; private set; }
        public int Id { get; private set; }
        public static readonly Random Random = new Random(114514);

        public Game()
        {
            Players = new[] { new Player(), new Player() };
            Units = new[] { new List<IUnit>(), new List<IUnit>() };
            Turn = 1;
            Id = 0;

            var castles = LocateCastles();
            for (int i = 0; i < 2; i++)
            {
                Units[i].Add(new Unit(Id++, EUnitType.Castle, castles[i]));
                
            }

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Units[i].Add(new Unit(Id++, EUnitType.Worker, castles[i]));
                }
            }

            Resources = LocateResources(castles);
        }

        public static int Manhattan(IPoint a, IPoint b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        public static IPoint RandomPoint()
        {
            int X = Random.Next(FieldSize);
            int Y = Random.Next(FieldSize);
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

        public static IEnumerable<IPoint> LocateResources(IPoint[] castles)
        {
            var a = LocateResource(basePoint[0], castles[0]);
            var b = LocateResource(basePoint[1], castles[1]);
            var ret = new List<IPoint>();
            ret.AddRange(a);
            ret.AddRange(b);
            return ret;
        }

        public static IPoint[] LocateCastles()
        {
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

        // 城もぶっ壊す
        private void RemoveUnitPhase()
        {

        }

        private void ObtainResourcePhase()
        {

        }

        private EGameResult EndPhase()
        {
            bool a = Units[0].Count(unit => unit.Type == EUnitType.Castle) > 0;
            bool b = Units[1].Count(unit => unit.Type == EUnitType.Castle) > 0;
            
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