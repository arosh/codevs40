namespace CodeVS4.SampleAI
{
    public class EnUnit : Unit
    {
        public void Init()
        {

        }

        public EnUnit(UnitType type)
            : base(type)
        {
            Init();
        }

        public EnUnit(UnitType type, int id)
            : base(type, id)
        {
            Init();
        }

        public EnUnit(UnitType type, int id, Point point)
            : base(type, id, point)
        {
            Init();
        }

        public EnUnit(IUnit unit)
            : base(unit)
        {
            Init();
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
    }
}
