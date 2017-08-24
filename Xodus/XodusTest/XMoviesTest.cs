using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xodus;

namespace XodusTest
{
    [TestClass]
    public class XMoviesTest
    {
        [TestMethod]
        public async Task TestGetMovieLink()
        {
            var pt = new XMovies();
            var result = await pt.GetMovieLink("Inception", 2010);
            Assert.AreNotEqual(0, result.Count);
        }
    }
}