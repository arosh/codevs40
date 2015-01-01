using System;
using System.Diagnostics;

namespace CodeVS4.Ofuton
{
    public class MyUnit : Unit
    {
        public Point MoveTo { get; set; }
        public UnitType? Produce { get; set; }

        private void Init()
        {
            MoveTo = null;
            Produce = null;
        }

        public MyUnit(UnitType type)
            : base(type)
        {
            Init();
        }

        public MyUnit(UnitType type, int id)
            : base(type, id)
        {
            Init();
        }

        public MyUnit(UnitType type, int id, Point point)
            : base(type, id, point)
        {
            Init();
        }

        public MyUnit(IUnit unit)
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

        public bool IsWarrior
        {
            get
            {
                return Type == UnitType.Knight || Type == UnitType.Fighter || Type == UnitType.Assassin;
            }
        }

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
                    if (dy <= dx)
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
}
