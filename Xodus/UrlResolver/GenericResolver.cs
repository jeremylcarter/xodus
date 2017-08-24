using System.Threading.Tasks;

namespace UrlResolver
{
    public class GenericResolver : IResolver
    {
        private readonly string url;

        public GenericResolver(string uri)
        {
            url = uri;
            if (url.Contains("720p"))
                VideoQuality = 2;

            if (url.Contains("1080p"))
                VideoQuality = 3;

            SourceName = "GENERIC";
        }

        public bool isTV { get; set; }

        public string imdb { get; set; }
        public string VideoSource { get; set; }


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

        public int VideoQuality { get; set; } = 1;

        public string SourceName { get; set; }


        public async Task<string> GetMediaUrl()
        {
            if (url.Contains("speedy.to"))
                return "";

            return url;
        }

        public NavigationItem item { get; set; }
    }
}