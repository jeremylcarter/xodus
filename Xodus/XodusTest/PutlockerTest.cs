using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xodus;

namespace XodusTest
{
    [TestClass]
    public class PutlockerTest
    {
        [TestMethod]
        public async Task TestGetMovieLink()
        {
            var pt = new Putlocker();
            var result = await pt.GetMovieLink("Inception", 2010);
            Assert.AreNotEqual(0, result.Count);
        }
    }
}