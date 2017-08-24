using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xodus;

namespace XodusTest
{
    [TestClass]
    public class DaytTest
    {
        [TestMethod]
        public async Task TestGetMovieLink()
        {
            var pt = new Dayt();
            var result = await pt.GetMovieLink("Logan", 2017);
            Assert.AreNotEqual(0, result.Count);
        }
    }
}