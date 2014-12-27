namespace CodeVS4
{
    public interface IOrder
    {
        int UnitId { get; }
        OrderType Type { get; }
    }

    public class Order : IOrder
    {
        public int UnitId { get; private set; }
        public OrderType Type { get; private set; }
        public Order() { }
        public Order(int unitId, OrderType type)
        {
            UnitId = unitId;
            Type = type;
        }
    }
}
