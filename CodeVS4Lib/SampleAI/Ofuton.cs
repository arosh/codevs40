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

        private IEnumerable<MyUnit> MyFreeWorkers { get { return myUnits.Values.Where(unit => unit.IsFree && unit.Type == UnitType.Worker); } }
        private IEnumerable<MyUnit> MyFreeWarriors { get { return myUnits.Values.Where(unit => unit.IsFree && GameConstant.IsWarrior(unit.Type)); } }

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

        private void ThinkRole()
        {
        }

        private int ThinkSearcherNum()
        {
            // @magicnumber
            return 10;
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

            if (NeedToSearch())
            {
                var searchersList = new[]{
                    myUnits.Values.Where(unit => GameConstant.IsWarrior(unit.Type)),
                    myUnits.Values.Where(unit => unit.Type == UnitType.Worker)
                };

                int searcherNum = ThinkSearcherNum();
                int k = 0;

                foreach (var searchers in searchersList)
                {
                    while (k < searcherNum)
                    {
                        if (NeedToSearch() == false)
                            break;

                        MyUnit u = null;
                        Point p = null;
                        SearchBestFreeSearcher(seeArea2, searchers, out u, out p);

                        if (u != null)
                        {
                            u.MoveTo = p;
                            // ここでdを+1とか+2とかしても良いかもしれない
                            // @magicnumber
                            UpdateSeeArea(seeArea2, p, GameConstant.GetViewRange(u.Type) * 2);
                            ++k;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void SearchBestFreeSearcher(bool[,] seeArea, IEnumerable<MyUnit> searchers, out MyUnit u, out Point p)
        {
            int d = int.MaxValue;
            u = null;
            p = null;

            Point priorityPoint;

            if (GetResourcePoints(resource).Count(resourcePoint => Game.Manhattan(resourcePoint, GameConstant.BasePoint[isTopLeft ? 0 : 1]) <= 99) < 10)
            {
                priorityPoint = GameConstant.BasePoint[isTopLeft ? 0 : 1];
            }
            else
            {
                priorityPoint = GameConstant.BasePoint[isTopLeft ? 1 : 0];
            }

            var freeSearchers = searchers.Where(unit => unit.IsFree);

            foreach (var unit in freeSearchers)
            {
                Point q = SearchNearestShadowArea(unit.Point, seeArea, priorityPoint);
                if (q != null && d > Game.Manhattan(unit.Point, q))
                {
                    d = Game.Manhattan(unit.Point, q);
                    u = unit;
                    p = q;
                }
            }
        }

        private Point SearchNearestShadowArea(Point point, bool[,] see, Point priorityPoint)
        {
            Point p = null;

            for (int d = 0; d <= 99 + 99 && p == null; d++)
            {
                for (int y = Math.Max(0, point.Y - d); y <= Math.Min(GameConstant.FieldSize - 1, point.Y + d); y++)
                {
                    int dx = (d - Math.Abs(y - point.Y));
                    int[] xs = { point.X - dx, point.X + dx };
                    foreach (int x in xs)
                    {
                        if (0 <= x && x < GameConstant.FieldSize && see[x, y] == false)
                        {
                            var q = new Point(x, y);
                            if (p == null || Game.Manhattan(p, priorityPoint) > Game.Manhattan(q, priorityPoint))
                            {
                                p = q;
                            }
                        }
                    }
                }
            }
            return p;
        }

        private void ThinkWorker()
        {
            #region 村の建設
            //if (NeedToBuildVillage() && currentResource >= GameConstant.GetCost(UnitType.Village))
            //{
            //    var resourcePoints = GetResourcePoints(resource);

            //    // 現在判明している資源の位置への距離の和が最小となる点
            //    // 重み付けしてもよいかも
            //    Point p = FieldIter
            //        .MinBy(point =>
            //            resourcePoints
            //            .OrderBy(r => Game.Manhattan(r, myCastle.Point))
            //            .Take(3)
            //            .Sum(resourcePoint =>
            //                Game.Manhattan(point, resourcePoint)));

            //    var freeWorker = myUnits.Values
            //        .Where(unit => unit.IsFree && unit.Type == UnitType.Worker);

            //    if (freeWorker.Count() > 0)
            //    {
            //        MyUnit u = freeWorker.MinBy(unit => Game.Manhattan(unit.Point, p));

            //        // @magicnumber
            //        if (Game.Manhattan(u.Point, p) <= 10)
            //        {
            //            // @magicnumber
            //            u.Produce = UnitType.Village;
            //        }
            //        else
            //        {
            //            u.MoveTo = p;
            //        }
            //    }

            //}
            #endregion

            #region 拠点の建設
            if (NeedToBuildFactory() && currentResource >= GameConstant.GetCost(UnitType.Factory))
            {
                var resourcePoints = GetResourcePoints(resource);
                // 現在判明している資源の位置への距離の和が最小となる点
                // 重み付けしてもよいかも
                Point p = FieldIter
                    .MinBy(point =>
                        resourcePoints
                        .OrderBy(r => Game.Manhattan(r, myCastle.Point))
                        .Take(3)
                        .Sum(resourcePoint =>
                            Game.Manhattan(point, resourcePoint)));

                if (MyFreeWorkers.Count() > 0)
                {
                    MyUnit u = MyFreeWorkers.MinBy(unit => Game.Manhattan(unit.Point, p));

                    if (Game.Manhattan(u.Point, p) == 0)
                    {
                        u.Produce = UnitType.Factory;
                    }
                    else
                    {
                        u.MoveTo = p;
                    }
                }
            }
            #endregion

            var candidates = SortResourcePriority().Take(ThinkMaxControlledResourceNum());
            foreach (var point in candidates)
            {
                var move = MyFreeWorkers.OrderBy(unit => Game.Manhattan(unit.Point, point)).Take(5);
                foreach (var unit in move)
                {
                    unit.MoveTo = point;
                }
            }

            foreach (var worker in MyFreeWorkers)
            {
                worker.MoveTo = myCastle.Point;
            }
        }

        private int ThinkLeastWorkerNum()
        {
            // @magicnumber
            return 35;
        }

        private int ThinkMaxControlledResourceNum()
        {
            // @magicnumber
            return 14;
        }

        private bool NeedToSearch()
        {
            if (GetResourcePoints(resource).Count() == 20 && enCastle.IsDiscovered)
            {
                return false;
            }

            return true;
        }


        private IEnumerable<Point> SortResourcePriority()
        {
            Point r = enCastle.IsDiscovered ? enCastle.Point : GameConstant.BasePoint[isTopLeft ? 1 : 0];
            return GetResourcePoints(resource)
                .OrderBy(point => Game.Manhattan(point, myCastle.Point) - Game.Manhattan(point, r));
        }

        private int[,] CountNumWorkers()
        {
            int[,] numWorkers = new int[GameConstant.FieldSize, GameConstant.FieldSize];
            var myWorkers = myUnits.Values.Where(unit => unit.Type == UnitType.Worker);

            foreach (var unit in myWorkers)
            {
                if (unit.IsMoving && resource[unit.MoveTo.X, unit.MoveTo.Y])
                {
                    numWorkers[unit.MoveTo.X, unit.MoveTo.Y]++;
                }
            }

            return numWorkers;
        }

        private int[,] CountNumWarriors()
        {
            int[,] numWarriors = new int[100, 100];
            var myWarriors = myUnits.Values.Where(unit => GameConstant.IsWarrior(unit.Type));

            foreach (var unit in myWarriors)
            {
                if (unit.IsMoving && resource[unit.MoveTo.X, unit.MoveTo.Y])
                {
                    numWarriors[unit.MoveTo.X, unit.MoveTo.Y]++;
                }
            }

            return numWarriors;
        }

        private void ThinkWarrior()
        {
            {
                var freeWarrior = myUnits.Values.Where(unit => unit.IsFree && GameConstant.IsWarrior(unit.Type));
                var move = freeWarrior.OrderBy(unit => Game.Manhattan(unit.Point, myCastle.Point)).Take(9);
                foreach (var unit in move)
                {
                    unit.MoveTo = myCastle.Point;
                }
            }

            var candidates = SortResourcePriority().Take(ThinkMaxControlledResourceNum());
            foreach (var point in candidates)
            {
                var freeWarrior = myUnits.Values.Where(unit => unit.IsFree && GameConstant.IsWarrior(unit.Type));
                var move = freeWarrior.OrderBy(unit => Game.Manhattan(unit.Point, point)).Take(5);
                foreach (var unit in move)
                {
                    unit.MoveTo = point;
                }
            }

            Point p = enCastle.IsDiscovered ? enCastle.Point : SearchEnemyCastle();
            foreach (var unit in myUnits.Values)
            {
                if (unit.IsFree && unit.IsWarrior)
                {
                    unit.MoveTo = p;
                }
            }
        }

        private Point SearchEnemyCastle()
        {
            // 相手側
            Point q = GameConstant.BasePoint[isTopLeft ? 1 : 0];
            bool[,] b = new bool[GameConstant.FieldSize, GameConstant.FieldSize];
            var resourcePoints = GetResourcePoints(resource);

            for (int x = 0; x < GameConstant.FieldSize; x++)
            {
                for (int y = 0; y < GameConstant.FieldSize; y++)
                {
                    var r = new Point(x, y);
                    b[x, y] = !seeArea[x, y];
                    if (Game.Manhattan(q, r) > 40)
                    {
                        b[x, y] = false;
                    }

                    foreach (var point in resourcePoints)
                    {
                        if (Game.Manhattan(point, r) <= 10)
                        {
                            b[x, y] = false;
                        }
                    }
                }
            }

            Point p = null;

            for (int x = 0; x < GameConstant.FieldSize; x++)
            {
                for (int y = 0; y < GameConstant.FieldSize; y++)
                {
                    if (b[x, y])
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

        private bool NeedToBuildVillage()
        {
            int workerCount = myUnits.Values.Count(unit => unit.Type == UnitType.Worker);
            int villageCount = myUnits.Values.Count(unit => unit.Type == UnitType.Village);
            // @magicnumber
            if (villageCount < 1 && GetResourcePoints(resource).Count() >= 3)
                return true;
            return false;
        }


        private bool NeedToBuildFactory()
        {
            int workerCount = myUnits.Values.Count(unit => unit.Type == UnitType.Worker);
            int villageCount = myUnits.Values.Count(unit => unit.Type == UnitType.Village);
            int factoryCount = myUnits.Values.Count(unit => unit.Type == UnitType.Factory);
            // @magicnumber
            //if (villageCount >= 1 && workerCount >= 30 && factoryCount < 1 && GetResourcePoints(resource).Count() >= 3)
            if (workerCount >= ThinkLeastWorkerNum() && factoryCount < 1 && GetResourcePoints(resource).Count() >= 3)
                return true;
            return false;
        }

        // @magicnumber
        private bool NeedToBuildWorker()
        {
            var workers = myUnits.Values.Where(unit => unit.Type == UnitType.Worker);
            int workersCount = workers.Count();
            if (workersCount < ThinkLeastWorkerNum())
            {
                return true;
            }

            if (workers.Where(unit => unit.IsFree).Count() > 0)
            {
                return false;
            }

            var warriors = myUnits.Values.Where(unit => GameConstant.IsWarrior(unit.Type));
            int warriorsCount = warriors.Count();

            // @magicnumber
            if (warriorsCount < workersCount * 2)
            {
                return false;
            }

            if (workers.Count() < ThinkMaxControlledResourceNum() * 5)
            {
                return true;
            }

            return false;
        }

        private void ThinkBuild()
        {
            int canUse = currentResource;

            if (NeedToBuildVillage())
            {
                canUse -= GameConstant.GetCost(UnitType.Village);
            }

            if (NeedToBuildFactory())
            {
                canUse -= GameConstant.GetCost(UnitType.Factory);
            }

            foreach (var unit in myUnits.Values)
            {
                if (unit.Type == UnitType.Castle || unit.Type == UnitType.Village)
                {
                    if (canUse >= GameConstant.GetCost(UnitType.Worker))
                    {
                        if (NeedToBuildWorker())
                        {
                            canUse -= GameConstant.GetCost(UnitType.Worker);
                            unit.Produce = UnitType.Worker;
                        }
                    }
                }
            }

            foreach (var unit in myUnits.Values)
            {
                if (unit.Type == UnitType.Factory)
                {
                    UnitType unitType;
                    int rnd = GameConstant.Random.Next(100);
                    if (rnd < 33)
                    {
                        unitType = UnitType.Knight;
                    }
                    else if (rnd < 66)
                    {
                        unitType = UnitType.Fighter;
                    }
                    else
                    {
                        unitType = UnitType.Assassin;
                    }

                    if (canUse >= GameConstant.GetCost(unitType))
                    {
                        canUse -= GameConstant.GetCost(unitType);
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

        public static Point MoveToNextPoint(Point from, Point to)
        {
            int dx = Math.Abs(to.X - from.X);
            int dy = Math.Abs(to.Y - from.Y);
            if (dy <= dx)
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
