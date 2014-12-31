using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using MoreLinq;

namespace CodeVS4.Ofuton
{
    public class Ofuton : IPlayer
    {
        public string Name { get { return "Ofuton"; } }
        private int remainingTime;
        private int currentResource;
        MyUnit myCastle;
        EnUnit enCastle;
        bool isTopLeft;
        private IDictionary<int, MyUnit> myUnits;
        private IDictionary<int, EnUnit> enUnits;
        bool[,] resource;
        bool[,] seeArea;

        private ISet<int> searcherIds;
        public static IEnumerable<Point> FieldIter = Enumerable.Range(0, 100).SelectMany(x => Enumerable.Range(0, 100).Select(y => new Point(x, y)));

        public Ofuton() { }

        public IEnumerable<IOrder> Think(Input input)
        {
            Input(input);

            ThinkRole();
            ThinkSearcher();
            ThinkWorker();
            ThinkWarrior();
            ThinkBuild();

            return Output();
        }

        /// <summary>
        /// 探索を行うべきユニットの数を考える
        /// </summary>
        /// <returns>探索を行うべきユニットの数</returns>
        private int ThinkSearcherNum()
        {
            // @magicnumber
            return Math.Min(10, myUnits.Values.Count(unit => GameConstant.IsMovable(unit.Type)));
        }

        private void ThinkRole()
        {
            // 自明に考えて、未開の地に近いやつを派遣したほうが良い気がする
            var searchers = myUnits
                .Values
                .Where(unit =>
                    GameConstant.IsMovable(unit.Type))
                .OrderByDescending(unit =>
                    Game.Manhattan(unit.Point, myCastle.Point))
                .Take(ThinkSearcherNum());

            searcherIds = new HashSet<int>(searchers.Select(unit => unit.Id));
        }

        private void ThinkSearcher()
        {
            bool[,] seeArea2 = new bool[GameConstant.FieldSize, GameConstant.FieldSize];
            for (int x = 0; x < GameConstant.FieldSize; x++)
            {
                for (int y = 0; y < GameConstant.FieldSize; y++)
                {
                    seeArea2[x, y] = seeArea[x, y];
                }
            }

            for (int i = 0; i < searcherIds.Count; i++)
            {
                MyUnit u = null;
                int d = int.MaxValue;

                var freeSearcher = myUnits.Values.Where(unit => unit.IsFree && searcherIds.Contains(unit.Id));

                foreach (var unit in freeSearcher)
                {
                    Point p = SearchNearestShadowArea(unit.Point, seeArea2);
                    if (d > Game.Manhattan(unit.Point, p))
                    {
                        d = Game.Manhattan(unit.Point, p);
                        u = unit;
                    }
                }

                if (u != null)
                {
                    Point p = SearchNearestShadowArea(u.Point, seeArea2);
                    u.MoveTo = p;
                    // @magicnumber
                    // ここでdを+1とか+2とかしても良いかもしれない
                    UpdateSeeArea(seeArea2, p, u.Type);
                }
            }
        }

        private void ThinkWorker()
        {
            int villageNum = myUnits.Values.Count(unit => unit.Type == UnitType.Village);

            // @magicnumber
            if (villageNum < 1)
            {
                var resourcePoints = GetResourcePoints(resource);
                // @magicnumber
                if (resourcePoints.Count() >= 3)
                {
                    Point p = FieldIter
                        .MinBy(point =>
                            resourcePoints
                            .Sum(resourcePoint =>
                                Game.Manhattan(point, resourcePoint)));

                    var freeWorker = myUnits.Values
                        .Where(unit => unit.IsFree && unit.Type == UnitType.Worker);
                    if (freeWorker.Count() > 0)
                    {
                        MyUnit u = freeWorker
                        .MinBy(unit => Game.Manhattan(unit.Point, p));

                        if (Game.Manhattan(u.Point, p) == 0)
                        {
                            u.Produce = UnitType.Village;
                        }
                        else
                        {
                            u.MoveTo = p;
                        }
                    }
                }
            }


        }


        private Point SearchNearestShadowArea(Point point, bool[,] see)
        {
            Point p = null;
            // 相手側
            Point r = GameConstant.BasePoint[isTopLeft ? 1 : 0];
            for (int x = 0; x < GameConstant.FieldSize; x++)
            {
                for (int y = 0; y < GameConstant.FieldSize; y++)
                {
                    if (see[x, y] == false)
                    {
                        var q = new Point(x, y);
                        if (p == null || Tie(Game.Manhattan(point, p), Game.Manhattan(r, p), Game.Manhattan(point, q), Game.Manhattan(r, q)) > 0)
                        {
                            p = q;
                        }
                    }
                }
            }
            return p;
        }

        private Point SearchNearestResource(int[,] numWorker, MyUnit unit)
        {
            Point p = null;
            // 自分側
            Point r = GameConstant.BasePoint[isTopLeft ? 0 : 1];
            for (int x = 0; x < GameConstant.FieldSize; x++)
            {
                for (int y = 0; y < GameConstant.FieldSize; y++)
                {
                    if (resource[x, y] == false)
                    {
                        continue;
                    }

                    int near = myUnits.Values.Count(u =>
                        u.Type == UnitType.Worker &&
                        Game.Manhattan(u.Point, new Point(x, y)) < Game.Manhattan(unit.Point, new Point(x, y)));
                    if (near < 5)
                    {
                        var q = new Point(x, y);
                        if (p == null || Tie(Game.Manhattan(unit.Point, p), Game.Manhattan(r, p), Game.Manhattan(unit.Point, q), Game.Manhattan(r, q)) > 0)
                        {
                            p = q;
                        }
                    }
                }
            }
            return p;
        }

        private int[,] CountNumWorker()
        {
            int[,] numWorker = new int[100, 100];

            foreach (var unit in myUnits.Values)
            {
                if (unit.Type == UnitType.Worker)
                {
                    if (resource[unit.Point.X, unit.Point.Y])
                    {
                        numWorker[unit.Point.X, unit.Point.Y]++;
                    }
                    else if (unit.IsMoving)
                    {
                        numWorker[unit.MoveTo.X, unit.MoveTo.Y]++;
                    }
                }
            }

            return numWorker;
        }

        private void ThinkWarrior()
        {
            foreach (var unit in myUnits.Values)
            {
                if (unit.IsWarrior)
                {
                    Point p = null;
                    if (enCastle.IsDiscovered)
                    {
                        p = enCastle.Point;
                    }
                    else
                    {
                        p = SearchEnemyCastle();
                    }

                    unit.MoveTo = p;
                }
            }
        }

        private Point SearchEnemyCastle()
        {
            Point p = null;
            Point q = GameConstant.BasePoint[isTopLeft ? 1 : 0];
            for (int x = 0; x < GameConstant.FieldSize; x++)
            {
                for (int y = 0; y < GameConstant.FieldSize; y++)
                {
                    if (seeArea[x, y] == false)
                    {
                        var r = new Point(x, y);
                        if (p == null || Game.Manhattan(q, p) > Game.Manhattan(q, r))
                        {
                            p = r;
                        }
                    }
                }
            }
            return p;
        }

        private void ThinkBuild()
        {
            int workerCount = myUnits.Values.Count(unit => unit.Type == UnitType.Worker);
            int factoryCount = myUnits.Values.Count(unit => unit.Type == UnitType.Factory);

            foreach (var unit in myUnits.Values)
            {
                if (unit.Type == UnitType.Castle && workerCount < 65 && currentResource >= GameConstant.GetCost(UnitType.Worker))
                {
                    workerCount++;
                    currentResource -= GameConstant.GetCost(UnitType.Worker);
                    unit.Produce = UnitType.Worker;
                }

                if (unit.Type == UnitType.Worker && factoryCount == 0 && unit.IsFree && currentResource >= GameConstant.GetCost(UnitType.Factory))
                {
                    factoryCount++;
                    currentResource -= GameConstant.GetCost(UnitType.Factory);
                    unit.Produce = UnitType.Factory;
                }

                if (unit.Type == UnitType.Factory)
                {
                    UnitType unitType = UnitType.Assassin;
                    if (currentResource >= GameConstant.GetCost(unitType))
                    {
                        currentResource -= GameConstant.GetCost(unitType);
                        unit.Produce = unitType;
                    }
                }
            }
        }

        #region Tools
        private static int Tie(int a0, int a1, int b0, int b1)
        {
            return (a0 != b0 ? a0 - b0 :
                   (a1 - b1));
        }

        // Random.Nextを使っていることに注意
        public static Point MoveToNextPoint(Point from, Point to)
        {
            int dx = Math.Abs(to.X - from.X);
            int dy = Math.Abs(to.Y - from.Y);
            if (GameConstant.Random.Next(dx + dy) < dx)
            {
                if (to.X < from.X)
                {
                    return from.MoveL();
                }
                else
                {
                    return from.MoveR();
                }
            }
            else
            {
                if (to.Y < from.Y)
                {
                    return from.MoveU();
                }
                else
                {
                    return from.MoveD();
                }
            }
        }

        public static IEnumerable<Point> GetResourcePoints(bool[,] resource)
        {
            var ret = new List<Point>();
            for (int x = 0; x < GameConstant.FieldSize; x++)
            {
                for (int y = 0; y < GameConstant.FieldSize; y++)
                {
                    if (resource[x, y])
                    {
                        ret.Add(new Point(x, y));
                    }
                }
            }
            return ret;
        }
        #endregion

        #region UpdateSeeArea
        public static void UpdateSeeArea(bool[,] seeArea, Point p, int d)
        {
            Debug.Assert(p != null);

            for (int y = Math.Max(0, p.Y - d); y <= Math.Min(GameConstant.FieldSize - 1, p.Y + d); ++y)
            {
                int yy = Math.Abs(y - p.Y);
                for (int x = Math.Max(0, p.X - d + yy); x <= Math.Min(GameConstant.FieldSize - 1, p.X + d - yy); ++x)
                {
                    seeArea[x, y] = true;
                }
            }
        }

        public static void UpdateSeeArea(bool[,] seeArea, Point p, UnitType unitType)
        {
            UpdateSeeArea(seeArea, p, GameConstant.GetViewRange(unitType));
        }

        public static void UpdateSeeArea(bool[,] seeArea, IEnumerable<IUnit> myUnits)
        {
            foreach (var unit in myUnits)
            {
                UpdateSeeArea(seeArea, unit.Point, GameConstant.GetViewRange(unit.Type));
            }
        }
        #endregion

        #region IO
        private void Input(Input input)
        {
            remainingTime = input.RemainingTimeMs;

            if (input.CurrentTurn == 0)
            {
                StageStart();
                Console.Error.WriteLine("stage:{0}", input.CurrentStage);
            }

            currentResource = input.CurrentResource;

            UpdateMyUnits(input);

            UpdateEnUnits(input);

            foreach (var p in input.ResourcePoints)
            {
                resource[p.X, p.Y] = true;
            }
        }

        private void UpdateMyUnits(Input input)
        {
            myUnits.Clear();

            foreach (var unit in input.MyUnits)
            {
                MyUnit u = new MyUnit(unit);

                myUnits[u.Id] = u;

                if (u.Type == UnitType.Castle)
                {
                    myCastle = u;
                    isTopLeft = Game.Manhattan(myCastle.Point, GameConstant.BasePoint[0]) < Game.Manhattan(myCastle.Point, GameConstant.BasePoint[1]);
                }

            }

            UpdateSeeArea(seeArea, myUnits.Values);
        }

        /// <summary>
        /// 新しく視認された情報で敵AIの情報を更新する
        /// </summary>
        /// <param name="input"></param>
        private void UpdateEnUnits(Input input)
        {
            foreach (var unit in input.EnUnits)
            {
                EnUnit u = new EnUnit(unit);

                enUnits[u.Id] = u;

                if (u.Type == UnitType.Castle)
                {
                    enCastle = u;
                }
            }
        }

        /// <summary>
        /// 戦闘によって与えたダメージの予測や見えない敵の移動方向の予測を行う(つもり)
        /// </summary>
        private void UpdateEnUnits2()
        {

        }

        private void StageStart()
        {
            myUnits = new Dictionary<int, MyUnit>();
            enUnits = new Dictionary<int, EnUnit>();
            enCastle = new EnUnit(UnitType.Castle);
            enCastle.NotDiscover();

            resource = new bool[GameConstant.FieldSize, GameConstant.FieldSize];
            seeArea = new bool[GameConstant.FieldSize, GameConstant.FieldSize];
            for (int x = 0; x < GameConstant.FieldSize; x++)
            {
                for (int y = 0; y < GameConstant.FieldSize; y++)
                {
                    resource[x, y] = false;
                    seeArea[x, y] = false;
                }
            }
        }

        private IEnumerable<IOrder> Output()
        {
            var output = new List<IOrder>();
            foreach (var unit in myUnits.Values)
            {
                var order = unit.CreateOrder();
                if (order != null)
                {
                    output.Add(order);
                    if (unit.IsProducing)
                    {
                        unit.Free();
                    }
                }
            }
            return output;
        }
        #endregion
    }
}
