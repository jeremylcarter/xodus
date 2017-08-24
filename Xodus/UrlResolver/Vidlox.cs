using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UrlResolver
{
    public class Vidlox : IResolver
    {
        private readonly string url;

        public Vidlox(string uri)
        {
            url = uri;
            SourceName = "VIDLOX";
        }

        public string VideoSource { get; set; }
        public int VideoQuality { get; set; } = 1;


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

        public string SourceName { get; set; }
        public bool isTV { get; set; }

        public string imdb { get; set; }

        public async Task<string> GetMediaUrl()
        {
            var returnUrl = "";
            try
            {
                var media = url.Substring(url.LastIndexOf("/") + 1);
                var target = $"http://vidlox.tv/embed-{media}.html";
                var httpClient = Utilities.GetHttpClient();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Common.IE_USERAGENT);

                var result = await httpClient.GetStringAsync(target);
                var sources = Regex.Matches(result, "sources\\s*:\\s*\\[(.+?)\\]");

                foreach (Match match in sources)
                {
                    var source = Regex.Matches(match.Groups[1].Value, "(?:\\\"|\\')(http.+?)(?:\\\"|\\')");

                    foreach (Match s in source)
                    {
                        var item = s.Groups[1].Value;

                        if (item.ToLower().Contains(".mp4"))
                        {
                            returnUrl = item;
                            break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(returnUrl))
                        break;
                }
            }
            catch (Exception)
            {
            }
            return returnUrl;
        }

        public NavigationItem item { get; set; }
    }
}