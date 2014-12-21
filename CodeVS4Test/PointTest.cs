using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeVS4;

namespace CodeVS4Test
{
    [TestClass]
    public class PointTest
    {
        [TestMethod]
        public void EqualsTest()
        {
            IPoint p = new Point(0, 0);
            Assert.IsTrue(p.Equals(new Point(0, 0)));
            Assert.IsFalse(p.Equals(new Point(0, 1)));
            Assert.IsFalse(p.Equals(new Point(1, 0)));
        }
    }
}
