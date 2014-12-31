using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeVS4;

namespace CodeVS4Test
{
    [TestClass]
    public class GameConstantTest
    {
        [TestMethod]
        public void GetViewRangeTest()
        {
            Assert.AreEqual(10, GameConstant.GetViewRange(UnitType.Castle));
            Assert.AreEqual(4, GameConstant.GetViewRange(UnitType.Worker));
        }
    }
}
