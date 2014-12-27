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
            Assert.AreEqual(0, Stream.UnitTypeEnumToInt(EUnitType.Worker));
            Assert.AreEqual(1, Stream.UnitTypeEnumToInt(EUnitType.Knight));
            Assert.AreEqual(2, Stream.UnitTypeEnumToInt(EUnitType.Fighter));
            Assert.AreEqual(3, Stream.UnitTypeEnumToInt(EUnitType.Assassin));
            Assert.AreEqual(4, Stream.UnitTypeEnumToInt(EUnitType.Castle));
            Assert.AreEqual(5, Stream.UnitTypeEnumToInt(EUnitType.Village));
            Assert.AreEqual(6, Stream.UnitTypeEnumToInt(EUnitType.Base));
        }

        [TestMethod]
        public void UnitTypeEnumToIntTest()
        {
            Assert.AreEqual(EUnitType.Worker, Stream.UnitTypeIntToEnum(0));
            Assert.AreEqual(EUnitType.Knight, Stream.UnitTypeIntToEnum(1));
            Assert.AreEqual(EUnitType.Fighter, Stream.UnitTypeIntToEnum(2));
            Assert.AreEqual(EUnitType.Assassin, Stream.UnitTypeIntToEnum(3));
            Assert.AreEqual(EUnitType.Castle, Stream.UnitTypeIntToEnum(4));
            Assert.AreEqual(EUnitType.Village, Stream.UnitTypeIntToEnum(5));
            Assert.AreEqual(EUnitType.Base, Stream.UnitTypeIntToEnum(6));
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
            Assert.AreEqual(EUnitType.Castle, unit.Type);
        }

        [TestMethod]
        public void OrderToStringTest()
        {
            Assert.AreEqual("12 R", Stream.OrderToString(new Order(12, EOrderType.MoveR)));
            Assert.AreEqual("22 D", Stream.OrderToString(new Order(22, EOrderType.MoveD)));
        }
    }
}
