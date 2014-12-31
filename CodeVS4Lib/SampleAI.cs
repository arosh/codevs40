using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace CodeVS4
{
    public class UnitEx : Unit
    {
        public Point MoveTo { get; set; }
        public UnitType? Produce { get; set; }

        private void Init()
        {
            MoveTo = null;
            Produce = null;
        }

        public UnitEx(UnitType type)
            : base(type)
        {
            Init();
        }

        public UnitEx(UnitType type, int id)
            : base(type, id)
        {
            Init();
        }

        public UnitEx(UnitType type, int id, Point point)
            : base(type, id, point)
        {
            Init();
        }

        public UnitEx(IUnit unit)
            : base(unit)
        {
            Init();
        }

        public bool IsMoving { get { return MoveTo != null; } }

        public bool IsProducing { get { return Produce.HasValue; } }
        public bool IsFree { get { return IsMoving == false && IsProducing == false; } }
        public void Free()
        {
            MoveTo = null;
            Produce = null;
        }

        public void Discover(Point p)
        {
            Point = p;
        }

        public void NotDiscover()
        {
            Point = null;
        }

        public bool IsDiscovered
        {
            get
            {
                return Point != null;
            }
        }

        public bool IsWarrior
        {
            get
            {
                return Type == UnitType.Knight || Type == UnitType.Fighter || Type == UnitType.Assassin;
            }
        }

        // 副作用(Random.Next)がある処理をgetterにするのは罪
        public IOrder CreateOrder()
        {
            Debug.Assert(this.Id != -1);

            if (IsProducing)
            {
                Debug.Assert(
                    this.Type == UnitType.Worker ||
                    this.Type == UnitType.Castle ||
                    this.Type == UnitType.Village ||
                    this.Type == UnitType.Factory
                    );
#if DEBUG
                switch (this.Type)
                {
                    case UnitType.Worker:
                        Debug.Assert(Produce == UnitType.Village || Produce == UnitType.Factory);
                        break;
                    case UnitType.Castle:
                        Debug.Assert(Produce == UnitType.Worker);
                        break;
                    case UnitType.Village:
                        Debug.Assert(Produce == UnitType.Worker);
                        break;
                    case UnitType.Factory:
                        Debug.Assert(Produce == UnitType.Knight || Produce == UnitType.Fighter || Produce == UnitType.Assassin);
                        break;
                }
#endif
                return new Order(this.Id, GameConstant.Build(Produce.Value));
            }

            if (IsMoving)
            {
                Debug.Assert(this.Point != null);
                Debug.Assert(this.Type == UnitType.Worker || this.Type == UnitType.Knight || this.Type == UnitType.Fighter || this.Type == UnitType.Assassin);

                int dx = Math.Abs(Point.X - MoveTo.X);
                int dy = Math.Abs(Point.Y - MoveTo.Y);
                if (dx + dy > 0)
                {
                    if (GameConstant.Random.Next(dx + dy) < dx)
                    {
                        if (Point.X < MoveTo.X)
                        {
                            return new Order(this.Id, OrderType.MoveR);
                        }

                        if (Point.X > MoveTo.X)
                        {
                            return new Order(this.Id, OrderType.MoveL);
                        }
                    }
                    else
                    {
                        if (Point.Y < MoveTo.Y)
                        {
                            return new Order(this.Id, OrderType.MoveD);
                        }

                        if (Point.Y > MoveTo.Y)
                        {
                            return new Order(this.Id, OrderType.MoveU);
                        }
                    }
                }
            }

            return null;
        }
    }

    public class SampleAI : IPlayer
    {
        public string Name { get { return "NiCoPa"; } }
        private int remainingTime;
        private int currentResource;
        UnitEx myCastle;
        UnitEx enCastle;
        bool isTopLeft;
        private IDictionary<int, UnitEx> myUnits;
        private IDictionary<int, UnitEx> enUnits;
        bool[,] resource;
        bool[,] seeArea;

        public SampleAI() { }

        public IEnumerable<IOrder> Think(Input input)
        {
            Input(input);

            thinkWorker();
            thinkWarrior();
            thinkBuild();

            return Output();
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

        private void thinkWorker()
        {
            foreach (var unit in myUnits.Values)
            {
                if (unit.Type == UnitType.Worker)
                {
                    if (unit.IsMoving && unit.Point.Equals(unit.MoveTo))
                    {
                        unit.Free();
                    }
                }
            }

            int[,] numWorker = CountNumWorker();

            bool[,] seeArea2 = new bool[GameConstant.FieldSize, GameConstant.FieldSize];
            for (int x = 0; x < GameConstant.FieldSize; x++)
            {
                for (int y = 0; y < GameConstant.FieldSize; y++)
                {
                    seeArea2[x, y] = seeArea[x, y];
                }
            }


            // 割り振りはpriority_queueを使って距離が近いやつから貪欲にやったほうが良い
            foreach (var unit in myUnits.Values)
            {
                if (unit.Type == UnitType.Worker)
                {
                    if (unit.IsFree)
                    {
                        if (resource[unit.Point.X, unit.Point.Y])
                        {
                            // do nothing
                        }
                        else
                        {
                            // Point p = SearchNearestResource(numWorker, unit);
                            Point p = null;

                            if (p == null)
                            {
                                p = SearchNearestShadowArea(unit, seeArea2);
                                if (p != null)
                                {
                                    UpdateSeeArea(seeArea2, p, GameConstant.GetViewRange(unit.Type));
                                }
                            }

                            if (p != null)
                            {
                                unit.MoveTo = p;
                                numWorker[p.X, p.Y]++;
                            }
                        }
                    }
                }
            }
        }

        private static int Tie(int a0, int a1, int b0, int b1)
        {
            return (a0 != b0 ? a0 - b0 :
                   (a1 - b1));
        }

        public static Point MoveToNextPoint(Point from, Point to)
        {
            int dx = Math.Abs(to.X - from.X);
            int dy = Math.Abs(to.Y - from.Y);
            if (dx >= dy)
            {
                if (to.X < from.X)
                {
                    return new Point(from.X - 1, from.Y);
                }
                else
                {
                    return new Point(from.X + 1, from.Y);
                }
            }
            else
            {
                if (to.Y < from.Y)
                {
                    return new Point(from.X, from.Y - 1);
                }
                else
                {
                    return new Point(from.X, from.Y + 1);
                }
            }
        }

        private Point SearchNearestShadowArea(UnitEx unit, bool[,] see)
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
                        if (p == null || Tie(Game.Manhattan(unit.Point, p), Game.Manhattan(r, p), Game.Manhattan(unit.Point, q), Game.Manhattan(r, q)) > 0)
                        {
                            p = q;
                        }
                    }
                }
            }
            return p;
        }

        private Point SearchNearestResource(int[,] numWorker, UnitEx unit)
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

        private void thinkWarrior()
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

        private void thinkBuild()
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

        public static void UpdateSeeArea(bool[,] seeArea, Point p, int d)
        {
            for (int y = Math.Max(0, p.Y - d); y <= Math.Min(GameConstant.FieldSize - 1, p.Y + d); ++y)
            {
                int yy = Math.Abs(y - p.Y);
                for (int x = Math.Max(0, p.X - d + yy); x <= Math.Min(GameConstant.FieldSize - 1, p.X + d - yy); ++x)
                {
                    seeArea[x, y] = true;
                }
            }
        }

        public static void UpdateSeeArea(bool[,] seeArea, IEnumerable<IUnit> myUnits)
        {
            foreach (var unit in myUnits)
            {
                Debug.Assert(unit.Point != null);
                UpdateSeeArea(seeArea, unit.Point, GameConstant.GetViewRange(unit.Type));
            }
        }

        private void Input(Input input)
        {
            remainingTime = input.RemainingTimeMs;

            if (input.CurrentTurn == 0)
            {
                StageStart();
                Console.Error.WriteLine("stage:{0}", input.CurrentStage);
            }

            currentResource = input.CurrentResource;

            {
                var mp = new Dictionary<int, UnitEx>();
                foreach (var unit in input.MyUnits)
                {
                    UnitEx u = new UnitEx(unit);

                    mp[u.Id] = u;

                    if (u.Type == UnitType.Castle)
                    {
                        myCastle = u;
                        isTopLeft = Game.Manhattan(myCastle.Point, GameConstant.BasePoint[0]) < Game.Manhattan(myCastle.Point, GameConstant.BasePoint[1]);
                    }

                }

                myUnits = mp;
                UpdateSeeArea(seeArea, myUnits.Values);
            }

            {
                var mp = new Dictionary<int, UnitEx>();
                foreach (var unit in input.EnUnits)
                {
                    UnitEx u;
                    if (enUnits.ContainsKey(unit.Id))
                    {
                        u = enUnits[unit.Id];
                    }
                    else
                    {
                        u = new UnitEx(unit);
                    }

                    mp[u.Id] = u;

                    if (u.Type == UnitType.Castle)
                    {
                        enCastle = u;
                    }
                }
                enUnits = mp;
            }

            foreach (var p in input.ResourcePoints)
            {
                resource[p.X, p.Y] = true;
            }
        }

        private void StageStart()
        {
            myUnits = new Dictionary<int, UnitEx>();
            enUnits = new Dictionary<int, UnitEx>();
            enCastle = new UnitEx(UnitType.Castle);
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
    }
}
