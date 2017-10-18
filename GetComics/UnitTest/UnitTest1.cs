using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Framework.Common.Extension;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            new Exception("asdasd").WriteToFile("asdasd");
        }
    }
}
