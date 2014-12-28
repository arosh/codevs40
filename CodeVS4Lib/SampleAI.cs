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
            :base(type, id, point)
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
                Debug.Assert(Type != null);
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
        public string Name { get { return "sampleAI"; } }
        private int remainingTime;
        private int currentResource;
        UnitEx myCastle;
        UnitEx enCastle;
        bool isTopLeft;
        private IDictionary<int, UnitEx> myUnits;
        private IDictionary<int, UnitEx> enUnits;
        bool[,] resource;
        bool[,] see;

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
                            Point p = SearchNearestResource(numWorker, unit);
                            
                            if(p == null)
                            {
                                p = SearchNearestShadowArea(unit);
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

        private Point SearchNearestShadowArea(UnitEx unit)
        {
            Point p = null;
            for (int x = 0; x < GameConstant.FieldSize; x++)
            {
                for (int y = 0; y < GameConstant.FieldSize; y++)
                {
                    if (see[x, y] == false)
                    {
                        var q = new Point(x, y);
                        if (p == null || Game.Manhattan(unit.Point, p) > Game.Manhattan(unit.Point, q))
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
            for (int x = 0; x < GameConstant.FieldSize; x++)
            {
                for (int y = 0; y < GameConstant.FieldSize; y++)
                {
                    if (resource[x, y] && numWorker[x, y] < 5)
                    {
                        var q = new Point(x, y);
                        if (p == null || Game.Manhattan(unit.Point, p) > Game.Manhattan(unit.Point, q))
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
                Point p = null;
                if (enCastle.IsDiscovered == false)
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

        private Point SearchEnemyCastle()
        {
            Point p = null;
            Point q = GameConstant.BasePoint[isTopLeft ? 1 : 0];
            for (int x = 0; x < GameConstant.FieldSize; x++)
            {
                for (int y = 0; y < GameConstant.FieldSize; y++)
                {
                    if (see[x, y] == false)
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
                if (unit.Type == UnitType.Castle && workerCount < 100 && currentResource >= GameConstant.GetCost(UnitType.Worker))
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
                    UnitEx u;
                    if (myUnits.ContainsKey(unit.Id))
                    {
                        u = myUnits[unit.Id];
                    }
                    else
                    {
                        u = new UnitEx(unit);
                    }

                    mp[u.Id] = u;

                    if (u.Type == UnitType.Castle)
                    {
                        myCastle = u;
                        isTopLeft = Game.Manhattan(myCastle.Point, GameConstant.BasePoint[0]) < Game.Manhattan(myCastle.Point, GameConstant.BasePoint[1]);
                    }

                    for (int x = 0; x < GameConstant.FieldSize; x++)
                    {
                        for (int y = 0; y < GameConstant.FieldSize; y++)
                        {
                            if (Game.Manhattan(new Point(x, y), u.Point) <= GameConstant.GetViewRange(u.Type))
                            {
                                see[x, y] = true;
                            }
                        }
                    }
                }
                myUnits = mp;
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
            see = new bool[GameConstant.FieldSize, GameConstant.FieldSize];
            for (int x = 0; x < GameConstant.FieldSize; x++)
            {
                for (int y = 0; y < GameConstant.FieldSize; y++)
                {
                    resource[x, y] = false;
                    see[x, y] = false;
                }
            }
        }
    }
}
