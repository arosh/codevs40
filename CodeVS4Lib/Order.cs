namespace CodeVS4
{
    public enum EOrderType
    {
        MoveU, MoveD, MoveL, MoveR, BuildWorker, BuildKnight, BuildFighter, BuildAssassin, BuildVillage, BuildBase
    }

    public interface IOrder
    {
        int UnitId { get; }
        EOrderType Type { get; }
    }

    public class Order : IOrder
    {
        public int UnitId { get; private set; }
        public EOrderType Type { get; private set; }
        public Order() { }
        public Order(int unitId, EOrderType type)
        {
            UnitId = unitId;
            Type = type;
        }
    }
}
