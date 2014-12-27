namespace CodeVS4
{
    public interface IUnit
    {
        int Id { get; }
        EUnitType Type { get; }
        Point Point { get; }
        int Hp { get; }
    }

    public class Unit : IUnit
    {
        public int Id { get; private set; }
        public EUnitType Type { get; private set; }
        public Point Point { get; private set; }
        public int Hp { get; private set; }

        public Unit() { }

        public Unit(int id, EUnitType type, Point point, int hp) {
            this.Id = id;
            this.Type = type;
            this.Point = point;
            this.Hp = hp;
        }

        public Unit(int id, EUnitType type, Point point) : this(id, type, point, GameConstant.GetDefaultHp(type)) { }
    }
}
