using System;

namespace UrlResolver
{
    public interface IStreamingSource
    {
        Uri GetMediaUri(string url);
    }
}