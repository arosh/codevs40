using CodeVS4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;

namespace CodeVS4Test
{
    [TestClass]
    public class GameTest
    {
        private readonly Point[] basePoint = new[] { new Point(0, 0), new Point(99, 99) };

        [TestMethod]
        public void ManhattanTest()
        {
            Assert.AreEqual(Game.Manhattan(new Point(0, 0), new Point(0, 0)), 0);
            Assert.AreEqual(Game.Manhattan(new Point(0, 0), new Point(0, 1)), 1);
            Assert.AreEqual(Game.Manhattan(new Point(0, 0), new Point(1, 1)), 2);
            Assert.AreEqual(Game.Manhattan(new Point(0, 0), new Point(99, 99)), 198);
            Assert.AreEqual(Game.Manhattan(new Point(0, 0), new Point(99, 23)), 99 + 23);
        }

        [TestMethod]
        public void LocateCastleTest()
        {
            for (int i = 0; i < 1024; i++)
            {
                var pp = Game.LocateCastles();
                Assert.IsTrue(Game.Manhattan(pp[0], basePoint[0]) <= 40);
                Assert.IsTrue(Game.Manhattan(pp[1], basePoint[1]) <= 40);
            }
        }

        [TestMethod]
        public void LocateResourceTest()
        {
            for (int i = 0; i < 1024; i++)
            {
                var pp = Game.LocateCastles();

                {
                    var rr = Game.LocateResource(basePoint[0], pp[0]);
                    foreach (var r in rr)
                    {
                        Assert.IsTrue(Game.Manhattan(r, basePoint[0]) <= 99);
                        Assert.IsTrue(Game.Manhattan(r, pp[0]) > 10);
                    }
                    Assert.AreEqual(10, rr.Distinct().Count());
                }

                {
                    var rr = Game.LocateResource(basePoint[1], pp[1]);
                    foreach (var r in rr)
                    {
                        Assert.IsTrue(Game.Manhattan(r, basePoint[1]) <= 99);
                        Assert.IsTrue(Game.Manhattan(r, pp[1]) > 10);
                    }
                    Assert.AreEqual(10, rr.Distinct().Count());
                }
            }
        }

        [TestMethod]
        public void LocateResourcesTest()
        {
            for (int i = 0; i < 1024; i++)
            {
                var castles = Game.LocateCastles();
                var rr = Game.LocateResources(castles);
                Assert.AreEqual(20, rr.Distinct().Count());
                Assert.IsTrue(rr.Count(p => Game.Manhattan(p, basePoint[0]) <= 99) >= 10);
                Assert.IsTrue(rr.Count(p => Game.Manhattan(p, basePoint[1]) <= 99) >= 10);
                Assert.AreEqual(0, rr.Count(p => Game.Manhattan(p, castles[0]) <= 10));
                Assert.AreEqual(0, rr.Count(p => Game.Manhattan(p, castles[1]) <= 10));
            }
        }
    }
}
