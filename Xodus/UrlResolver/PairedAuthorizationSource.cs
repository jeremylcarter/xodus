using System.Threading.Tasks;

namespace UrlResolver
{
    public interface IPairedAuthorizationSource
    {
        string GetUrl();
        string GetPairUri();
        Task<bool> CheckAuthorization();
    }
}