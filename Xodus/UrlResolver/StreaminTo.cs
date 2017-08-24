using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UrlResolver
{
    public class StreaminTo : IResolver, IPairedAuthorizationSource
    {
        private readonly string url;

        public StreaminTo(string uri)
        {
            url = uri;
            SourceName = "STREAMINTO";
        }

        public string GetUrl()
        {
            return url;
        }

        public string GetPairUri()
        {
            return "http://api.streamin.to/pair";
        }

        public async Task<bool> CheckAuthorization()
        {
            var ret = false;

            try
            {
                var httpClient = Utilities.GetHttpClient();
                var result = await httpClient.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                        "Mozilla / 5.0(Windows NT 6.1; WOW64; rv: 39.0) Gecko / 20100101 Firefox / 39.0");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", url);
                    var content = await httpClient.GetStringAsync("http://api.streamin.to/pair/check.php");
                    var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

                    if (!string.Equals(json["status"], "200"))
                        return false;
                }

                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }

            return ret;
        }

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

        public string SourceName { get; set; }

        public int VideoQuality { get; set; }
        public bool isTV { get; set; }

        public string imdb { get; set; }

        public async Task<string> GetMediaUrl()
        {
            Debug.WriteLine("Streamin");
            try
            {
                var media_id = url.Substring(url.LastIndexOf('/') + 1);
                var embedurl = $"http://streamin.to/embed-{media_id}.html";
                var client = Utilities.GetHttpClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "Mozilla / 5.0(Windows NT 6.1; WOW64; rv: 39.0) Gecko / 20100101 Firefox / 39.0");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", url);
                var html = await client.GetStringAsync(embedurl);
                if (html.ToLower().Equals("file was deleted"))
                    return "";

                var re = new Regex("(eval\\(function.*?)\n", RegexOptions.Singleline);
                var packed = re.Match(html);
                var p = new Unpacker();
                var s = p.Unpack(packed.Groups[1].Value);
                var re2 = new Regex("file\\s*:\\s*[\'|\"](http.+?)[\'|\"]", RegexOptions.Compiled);
                var s2 = re2.Matches(s)[0].Value;
                s2 = s2.Replace("file:", "");
                s2 = s2.Replace("\\", "");
                s2 = s2.Replace("\"", "");
                return s2;
            }
            catch (Exception)
            {
            }

            return "";
        }

        public NavigationItem item { get; set; }
    }
}