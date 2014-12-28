using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeVS4;

namespace CodeVS4Test
{
    [TestClass]
    public class UnitExTest
    {
        [TestMethod]
        public void IsMovingTest()
        {
            var u = new UnitEx(UnitType.Knight);
            Assert.IsFalse(u.IsMoving);
            Assert.IsTrue(u.IsFree);

            u.MoveTo = new Point(2, 3);
            Assert.IsTrue(u.IsMoving);
            Assert.IsFalse(u.IsFree);

            u.Free();
            Assert.IsFalse(u.IsMoving);
            Assert.IsTrue(u.IsFree);
        }

        [TestMethod]
        public void IsProducingTest()
        {
            var u = new UnitEx(UnitType.Factory);
            Assert.IsFalse(u.IsProducing);
            Assert.IsTrue(u.IsFree);

            u.Produce = UnitType.Assassin;
            Assert.IsTrue(u.IsProducing);
            Assert.IsFalse(u.IsFree);

            u.Free();
            Assert.IsFalse(u.IsMoving);
            Assert.IsTrue(u.IsFree);
        }

        [TestMethod]
        public void IsDiscoverdTest()
        {
            var u = new UnitEx(UnitType.Knight);
            Assert.IsFalse(u.IsDiscovered);

            u.Discover(new Point(2, 3));
            Assert.IsTrue(u.Point.Equals(new Point(2, 3)));
            Assert.IsTrue(u.IsDiscovered);

            u.NotDiscover();
            Assert.IsNull(u.Point);
            Assert.IsFalse(u.IsDiscovered);
        }

        [TestMethod]
        public void CreateOrderTest()
        {
            {
                var u = new UnitEx(UnitType.Worker, 0);
                Assert.IsNull(u.CreateOrder());

                u.Produce = UnitType.Village;
                var order = u.CreateOrder();
                Assert.AreEqual(0, order.UnitId);
                Assert.AreEqual(OrderType.BuildVillage, order.Type);
            }

            {
                var u = new UnitEx(UnitType.Worker, 0, new Point(1, 1));
                Assert.IsNull(u.CreateOrder());

                {
                    u.MoveTo = new Point(1, 2);
                    var order = u.CreateOrder();
                    Assert.AreEqual(0, order.UnitId);
                    Assert.AreEqual(OrderType.MoveD, order.Type);
                }

                {
                    u.MoveTo = new Point(2, 1);
                    var order = u.CreateOrder();
                    Assert.AreEqual(0, order.UnitId);
                    Assert.AreEqual(OrderType.MoveR, order.Type);
                }

                {
                    u.MoveTo = new Point(1, 0);
                    var order = u.CreateOrder();
                    Assert.AreEqual(0, order.UnitId);
                    Assert.AreEqual(OrderType.MoveU, order.Type);
                }

                {
                    u.MoveTo = new Point(0, 1);
                    var order = u.CreateOrder();
                    Assert.AreEqual(0, order.UnitId);
                    Assert.AreEqual(OrderType.MoveL, order.Type);
                }
            }

        }
    }

    [TestClass]
    public class SampleAITest
    {
    }
}
