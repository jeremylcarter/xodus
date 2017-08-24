using System.Threading.Tasks;

namespace UrlResolver
{
    public interface IResolver
    {
        int VideoQuality { get; set; }
        bool isTV { get; set; }
        NavigationItem item { get; set; }
        string imdb { get; set; }
        string VideoSource { get; set; }
        string VideoSourceName { get; set; }
        string SourceName { get; set; }
        Task<string> GetMediaUrl();
    }
}