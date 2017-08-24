using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UrlResolver
{
    public class Speedvid : IResolver
    {
        private readonly string url;

        public Speedvid(string uri)
        {
            url = uri;
            SourceName = "SPEEDVID";
        }

        public NavigationItem item { get; set; }
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
            var returnUrl = "";
            try
            {
                var media = url.Substring(url.LastIndexOf("/") + 1);
                var target = $"http://speedvid.net/embed-{media}.html";
                var httpClient = Utilities.GetHttpClient();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Common.IE_USERAGENT);

                var result = await httpClient.GetAsync(target);
                var content = result.Content.Headers;
                var html = await result.Content.ReadAsStringAsync();
                var q = GetPackedData(html);
                var cookie = "";
                if (content.Contains("Set-Cookie"))
                {
                    IEnumerable<string> values = new List<string>();
                    content.TryGetValues("Set-Cookies", out values);

                    foreach (var s in values)
                        cookie = s;
                }

                /*
                var sources = Regex.Matches(result, "sources\\s*:\\s*\\[(.+?)\\]");

                foreach (Match match in sources)
                {
                    var source = Regex.Matches(match.Groups[1].Value, "(?:\\\"|\\')(http.+?)(?:\\\"|\\')");

                    foreach (Match s in source)
                    {
                        string item = s.Groups[1].Value;

                        if (item.ToLower().Contains(".mp4"))
                        {
                            returnUrl = item;
                            break;
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(returnUrl))
                        break;
                }
                */
            }
            catch (Exception)
            {
            }
            return returnUrl;
        }


        private List<(string, string)> ParseToList(string html, string regex)
        {
            var results = new List<(string, string)>();

            return results;
        }

        private List<string> ScrapeSources(string html)
        {
            var list = new List<string>();

            var pattern1 =
                @"''[\""']?label\s*[\""']?\s*[:=]\s*[\""']?(?P<label>[^\""',]+)[\""']?(?:[^}\]]+)[\""']?\s*file\s*[\""']?\s*[:=,]?\s*[\""'](?P<url>[^\""']+)''";


            return list;
        }

        private string GetPackedData(string html)
        {
            var matches = Regex.Matches(html, "(eval\\(function.*?)\n", RegexOptions.Singleline);

            foreach (Match c in matches)
                try
                {
                    var u = new Unpacker();
                    var n = u.Unpack(c.Groups[1].Value);
                    var x = 0;
                }
                catch (Exception)
                {
                }

            return "";
        }
    }
}