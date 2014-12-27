using System;
using System.Collections.Generic;

namespace CodeVS4
{
    class UnitEx : Unit
    {
        public Point MoveTo { get; private set; }
        public UnitType? Produce { get; private set; }

        public UnitEx() : base()
        {
            MoveTo = null;
            Produce = null;
        }

        public UnitEx(IUnit unit) : base(unit)
        {
            MoveTo = null;
            Produce = null;
        }


        public bool IsMoving { get { return MoveTo != null; } }
        public bool IsProducing { get { return Produce.HasValue; } }
        public bool IsFree { get { return IsMoving == false && IsProducing == false; } }
        public void Free()
        {
            MoveTo = null;
            Produce = null;
        }

        public void Discovered(Point p)
        {
            Point = p;
        }

        public void UnDiscovered()
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

        public IOrder Order
        {
            get
            {
                if (IsProducing)
                {
                    return new Order(this.Id, GameConstant.Build(Produce.Value));
                }

                if (IsMoving)
                {
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

                // throw new InvalidOperationException("命令がありません");
                return null;
            }
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

            {
                foreach (var p in input.ResourcePoints)
                {
                    resource[p.Y, p.X] = true;
                }
            }
        }

        private void StageStart()
        {
            myUnits = new Dictionary<int, UnitEx>();
            enUnits = new Dictionary<int, UnitEx>();
            enCastle = new UnitEx();
            enCastle.UnDiscovered();
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
