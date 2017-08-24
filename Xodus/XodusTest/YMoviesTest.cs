using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xodus;

namespace XodusTest
{
    [TestClass]
    public class YmoviesTest
    {
        [TestMethod]
        public async Task TestGetMovieLink()
        {
            var pt = new YMovies();
            var result = await pt.GetMovieLink("Inception", 2010);
            Assert.AreNotEqual(0, result.Count);
        }
    }
}