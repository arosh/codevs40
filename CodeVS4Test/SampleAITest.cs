using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeVS4;
using System.Collections.Generic;

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
        [TestMethod]
        public void UpdateSeeAreaTest()
        {
            {
                bool[,] see = new bool[GameConstant.FieldSize, GameConstant.FieldSize];

                for (int x = 0; x < 100; x++)
                {
                    for (int y = 0; y < 100; y++)
                    {
                        see[x, y] = false;
                    }
                }

                var myUnits = new List<IUnit>();
                myUnits.Add(new Unit(UnitType.Worker, -1, new Point(0, 0)));
                SampleAI.UpdateSeeArea(see, myUnits);
                for (int y = 0; y < 100; y++)
                {
                    for (int x = 0; x < 100; x++)
                    {
                        if (x + y <= 4)
                        {
                            Assert.IsTrue(see[x, y]);
                        }
                        else
                        {
                            Assert.IsFalse(see[x, y]);
                        }
                    }
                }
            }

            {
                bool[,] see = new bool[GameConstant.FieldSize, GameConstant.FieldSize];
                var myUnits = new List<IUnit>();
                myUnits.Add(new Unit(UnitType.Worker, -1, new Point(50, 50)));
                SampleAI.UpdateSeeArea(see, myUnits);
                for (int y = 0; y < 100; y++)
                {
                    for (int x = 0; x < 100; x++)
                    {
                        if (Math.Abs(x - 50) + Math.Abs(y - 50) <= 4)
                        {
                            Assert.IsTrue(see[x, y]);
                        }
                        else
                        {
                            Assert.IsFalse(see[x, y]);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void MoveToNextPointTest()
        {
            var p = new Point(10, 10);
            Point ret;

            ret = SampleAI.MoveToNextPoint(p, new Point(0, 10));
            Assert.AreEqual(9, ret.X);
            Assert.AreEqual(10, ret.Y);

            ret = SampleAI.MoveToNextPoint(p, new Point(20, 10));
            Assert.AreEqual(11, ret.X);
            Assert.AreEqual(10, ret.Y);

            ret = SampleAI.MoveToNextPoint(p, new Point(10, 0));
            Assert.AreEqual(10, ret.X);
            Assert.AreEqual(9, ret.Y);

            ret = SampleAI.MoveToNextPoint(p, new Point(10, 20));
            Assert.AreEqual(10, ret.X);
            Assert.AreEqual(11, ret.Y);
        }
    }
}
