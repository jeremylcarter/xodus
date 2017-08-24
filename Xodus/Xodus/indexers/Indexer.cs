using System.Collections.Generic;
using System.Threading.Tasks;
using UrlResolver;

namespace Xodus
{
    public interface IIndexer
    {
        Task<List<IResolver>> GetMovieLink(string movie, int year);
        Task<List<IResolver>> GetTvLink(string movie, int year, int season, int episode);

        string GetName();
    }
}