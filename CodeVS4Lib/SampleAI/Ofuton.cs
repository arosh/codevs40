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

            int searcherNum = ThinkSearcherNum();

            for (int i = 0; i < searcherNum; i++)
            {
                
                int d = int.MaxValue;

                var freeSearcher = myUnits.Values.Where(unit => unit.IsFree && GameConstant.IsMovable(unit.Type));

                if (NeedToSearch())
                {
                    MyUnit u = null;
                    foreach (var unit in freeSearcher)
                    {
                        Point p = SearchNearestShadowArea(unit.Point, seeArea2);
                        if (p != null && d > Game.Manhattan(unit.Point, p))
                        {
                            d = Game.Manhattan(unit.Point, p);
                            u = unit;
                        }
                    }

                    if (u != null)
                    {
                        Point p = SearchNearestShadowArea(u.Point, seeArea2);
                        u.MoveTo = p;
                        // ここでdを+1とか+2とかしても良いかもしれない
                        // @magicnumber
                        UpdateSeeArea(seeArea2, p, u.Type);
                    }
                }
            }
        }

        private void ThinkWorker()
        {
            #region 村の建設
            int villageNum = myUnits.Values.Count(unit => unit.Type == UnitType.Village);

            // @magicnumber
            if (villageNum < 1 && currentResource >= GameConstant.GetCost(UnitType.Village))
            {
                var resourcePoints = GetResourcePoints(resource);
                // @magicnumber
                if (resourcePoints.Count() >= 3)
                {
                    // 現在判明している資源の位置への距離の和が最小となる点
                    // 重み付けしてもよいかも
                    Point p = FieldIter
                        .MinBy(point =>
                            resourcePoints
                            .Sum(resourcePoint =>
                                Game.Manhattan(point, resourcePoint)));

                    var freeWorker = myUnits.Values
                        .Where(unit => unit.IsFree && unit.Type == UnitType.Worker);

                    if (freeWorker.Count() > 0)
                    {
                        MyUnit u = freeWorker.MinBy(unit => Game.Manhattan(unit.Point, p));

                        // @magicnumber
                        if (Game.Manhattan(u.Point, p) <= 10)
                        {
                            // @magicnumber
                            u.Produce = UnitType.Village;
                        }
                        else
                        {
                            u.MoveTo = p;
                        }
                    }
                }
            }
            #endregion

            #region 拠点の建設
            int factoryNum = myUnits.Values.Count(unit => unit.Type == UnitType.Factory);
            if (factoryNum < 1 && currentResource >= GameConstant.GetCost(UnitType.Factory))
            {
                var resourcePoints = GetResourcePoints(resource);
                // @magicnumber
                if (resourcePoints.Count() >= 3)
                {
                    // 現在判明している資源の位置への距離の和が最小となる点
                    // 重み付けしてもよいかも
                    Point p = FieldIter
                        .MinBy(point =>
                            resourcePoints
                            .Sum(resourcePoint =>
                                Game.Manhattan(point, resourcePoint)));

                    var freeWorker = myUnits.Values
                        .Where(unit => unit.IsFree && unit.Type == UnitType.Worker);

                    if (freeWorker.Count() > 0)
                    {
                        MyUnit u = freeWorker.MinBy(unit => Game.Manhattan(unit.Point, p));

                        // @magicnumber
                        if (Game.Manhattan(u.Point, p) <= 10)
                        {
                            u.Produce = UnitType.Factory;
                        }
                        else
                        {
                            u.MoveTo = p;
                        }
                    }
                }
            }
            #endregion

            {
                var freeWorkers = myUnits.Values.Where(unit => unit.Type == UnitType.Worker && unit.IsFree);
                foreach (var unit in freeWorkers)
                {
                    if (resource[unit.Point.X, unit.Point.Y])
                    {
                        unit.MoveTo = unit.Point;
                    }
                }
            }

            while (true)
            {
                int[,] numWorkers = CountNumWorker();
                var freeWorkers = myUnits.Values.Where(unit => unit.Type == UnitType.Worker && unit.IsFree);
                if (freeWorkers.Count() == 0)
                {
                    break;
                }

                if (NeedToGetResource(numWorkers))
                {
                    MyUnit u = freeWorkers.MinBy(unit => Game.Manhattan(unit.Point, SearchNearestResource(numWorkers, unit)));
                    u.MoveTo = SearchNearestResource(numWorkers, u);
                }
                else
                {
                    break;
                }
            }

            // 残った村人は無理やりSearcherにしても良いかもしれない
        }

        private bool NeedToSearch()
        {
            if (GetResourcePoints(resource).Count() == 20 && enCastle.IsDiscovered)
            {
                return false;
            }

            return true;
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

        private bool NeedToGetResource(int[,] numWorkers)
        {
            foreach (var point in GetResourcePoints(resource))
            {
                if (numWorkers[point.X, point.Y] < 5)
                {
                    return true;
                }
            }

            return false;
        }

        private Point SearchNearestResource(int[,] numWorker, MyUnit unit)
        {
            // 取りに行くべきResourceに制約をつけるならココ
            var freeResouce = GetResourcePoints(resource).Where(point => numWorker[point.X, point.Y] < 5);

            Point p = null;
            // 自分側
            Point r = GameConstant.BasePoint[isTopLeft ? 0 : 1];

            foreach (var q in freeResouce)
            {
                if (p == null || Tie(Game.Manhattan(unit.Point, p), Game.Manhattan(r, p), Game.Manhattan(unit.Point, q), Game.Manhattan(r, q)) > 0)
                {
                    p = q;
                }
            }

            return p;
        }

        private int[,] CountNumWorker()
        {
            int[,] numWorker = new int[100, 100];
            var myWorkers = myUnits.Values.Where(unit => unit.Type == UnitType.Worker);

            foreach (var unit in myWorkers)
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
            int villageCount = myUnits.Values.Count(unit => unit.Type == UnitType.Village);
            int factoryCount = myUnits.Values.Count(unit => unit.Type == UnitType.Factory);

            int canUse = currentResource;

            // @magicnumber
            if (villageCount < 1 && workerCount >= 15)
            {
                canUse -= GameConstant.GetCost(UnitType.Village);
            }

            if (factoryCount < 1 && FieldIter.Count(point => seeArea[point.X, point.Y]) >= 3000)
            {
                canUse -= GameConstant.GetCost(UnitType.Factory);
            }

            foreach (var unit in myUnits.Values)
            {
                if (canUse >= GameConstant.GetCost(UnitType.Worker))
                {
                    if (unit.Type == UnitType.Castle || unit.Type == UnitType.Village)
                    {
                        int[,] numWorkers = CountNumWorker();
                        // @magicnumber
                        if (workerCount < 20 || (NeedToGetResource(numWorkers) && workerCount <= 60))
                        {
                            workerCount++;
                            canUse -= GameConstant.GetCost(UnitType.Worker);
                            unit.Produce = UnitType.Worker;
                        }
                    }
                }

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
