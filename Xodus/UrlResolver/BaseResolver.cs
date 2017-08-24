using System;
using System.Threading.Tasks;

namespace UrlResolver
{
    public class BaseResolver : IResolver


    {
        private string url = "";

        public BaseResolver(string uri)
        {
            url = uri;
            SourceName = "BASE";
        }

        public bool isTV { get; set; }

        public string imdb { get; set; }
        public NavigationItem item { get; set; }

        public string VideoSourceName
        {
            get
            {
                switch (VideoQuality)
                {
                    case 3:
                        return "1080P";
                    case 2:
                        return "HD";
                    case 1:
                        return "";
                    default:
                        return "";
                }
            }
            set { }
        }

        public string VideoSource { get; set; }
        public string SourceName { get; set; }
        public int VideoQuality { get; set; } = 1;

        public Task<string> GetMediaUrl()
        {
            throw new NotImplementedException();
        }
    }
}