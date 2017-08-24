using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xodus;

namespace XodusTest
{
    [TestClass]
    public class UtilitiesTest
    {
        [TestMethod]
        public void TestGetTitle()
        {
            var x = Utilities.GetTitle("Rogue One");
            Assert.IsNotNull(x.Item1);
        }
    }
}