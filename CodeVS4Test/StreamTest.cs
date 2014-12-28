using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeVS4;

namespace CodeVS4Test
{
    [TestClass]
    public class StreamTest
    {
        [TestMethod]
        public void UnitTypeIntToEnumTest()
        {
            Assert.AreEqual(0, Stream.UnitTypeEnumToInt(UnitType.Worker));
            Assert.AreEqual(1, Stream.UnitTypeEnumToInt(UnitType.Knight));
            Assert.AreEqual(2, Stream.UnitTypeEnumToInt(UnitType.Fighter));
            Assert.AreEqual(3, Stream.UnitTypeEnumToInt(UnitType.Assassin));
            Assert.AreEqual(4, Stream.UnitTypeEnumToInt(UnitType.Castle));
            Assert.AreEqual(5, Stream.UnitTypeEnumToInt(UnitType.Village));
            Assert.AreEqual(6, Stream.UnitTypeEnumToInt(UnitType.Factory));
        }

        [TestMethod]
        public void UnitTypeEnumToIntTest()
        {
            Assert.AreEqual(UnitType.Worker, Stream.UnitTypeIntToEnum(0));
            Assert.AreEqual(UnitType.Knight, Stream.UnitTypeIntToEnum(1));
            Assert.AreEqual(UnitType.Fighter, Stream.UnitTypeIntToEnum(2));
            Assert.AreEqual(UnitType.Assassin, Stream.UnitTypeIntToEnum(3));
            Assert.AreEqual(UnitType.Castle, Stream.UnitTypeIntToEnum(4));
            Assert.AreEqual(UnitType.Village, Stream.UnitTypeIntToEnum(5));
            Assert.AreEqual(UnitType.Factory, Stream.UnitTypeIntToEnum(6));
        }

        [TestMethod]
        public void StringToPointTest()
        {
            Assert.IsTrue((new Point(0, 0).Equals(Stream.StringToPoint("0 0"))));
            Assert.IsTrue((new Point(0, 1).Equals(Stream.StringToPoint("1 0"))));
        }

        [TestMethod]
        public void StringToIntArrayTest()
        {
            var arr = Stream.StringToIntArray("0 1 2");
            Assert.AreEqual(3, arr.Length);
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, arr);
        }

        [TestMethod]
        public void StringToIUnitTest()
        {
            var unit = Stream.StringToIUnit("0 7 16 50000 4");
            Assert.IsNotNull(unit);
            Assert.AreEqual(0, unit.Id);
            Assert.IsNotNull(unit.Point);
            Assert.AreEqual(7, unit.Point.Y);
            Assert.AreEqual(16, unit.Point.X);
            Assert.AreEqual(50000, unit.Hp);
            Assert.AreEqual(UnitType.Castle, unit.Type);
        }

        [TestMethod]
        public void OrderToStringTest()
        {
            Assert.AreEqual("12 R", Stream.OrderToString(new Order(12, OrderType.MoveR)));
            Assert.AreEqual("22 D", Stream.OrderToString(new Order(22, OrderType.MoveD)));
        }
    }
}
