namespace CodeVS4
{
    public interface IUnit
    {
        UnitType Type { get; }
        int Id { get; }
        Point Point { get; }
        int Hp { get; }
    }

    public class Unit : IUnit
    {
        public UnitType Type { get; private set; }
        public int Id { get; private set; }
        public Point Point { get; protected set; }
        public int Hp { get; private set; }

        public Unit(UnitType type)
            : this(type, -1) { }

        public Unit(UnitType type, int id)
            : this(type, id, null) { }

        public Unit(UnitType type, int id, Point point)
            : this(type, id, point, GameConstant.GetDefaultHp(type)) { }

        public Unit(UnitType type, int id, Point point, int hp)
        {
            this.Type = type;
            this.Id = id;
            this.Point = point;
            this.Hp = hp;
        }

        public Unit(IUnit unit) : this(unit.Type, unit.Id, unit.Point, unit.Hp) { }

        public override string ToString()
        {
            return string.Format("Unit {{ Type = {0}, Id = {1}, Point = {2}, Hp = {3} }}", Type, Id, Point, Hp);
        }
    }
}
