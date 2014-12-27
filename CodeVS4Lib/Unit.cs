namespace CodeVS4
{
    public interface IUnit
    {
        int Id { get; }
        UnitType Type { get; }
        Point Point { get; }
        int Hp { get; }
    }

    public class Unit : IUnit
    {
        public int Id { get; private set; }
        public UnitType Type { get; private set; }
        public Point Point { get; protected set; }
        public int Hp { get; private set; }

        public Unit() { }

        public Unit(int id, UnitType type, Point point, int hp)
        {
            this.Id = id;
            this.Type = type;
            this.Point = point;
            this.Hp = hp;
        }

        public Unit(int id, UnitType type, Point point) : this(id, type, point, GameConstant.GetDefaultHp(type)) { }

        public Unit(IUnit unit) : this(unit.Id, unit.Type, unit.Point, unit.Hp) { }
    }
}
