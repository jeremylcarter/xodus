using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UrlResolver
{
    public class TheVideo : IResolver, IPairedAuthorizationSource
    {
        private string url;

        public TheVideo(string uri)
        {
            url = uri;
            SourceName = "THEVIDEO";
        }

        public async Task<bool> CheckAuthorization()
        {
            try
            {
                var mediaId = url.Substring(url.LastIndexOf('/') + 1);
                var check = $"https://thevideo.me/pair?file_code={mediaId}&check";
                var httpClient = Utilities.GetHttpClient();
                var UA = "URLResolver for Kodi/3.0.30";
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UA);
                var result = await httpClient.GetAsync(check);

                if (result.IsSuccessStatusCode)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    var jsonObject2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    var response = jsonObject2["response"] as JObject;
                    var jsonObject = response.ToObject<Dictionary<string, string>>();
                    //var jsonObject = JsonConvert.DeserializeObject<Dictionary < string, object >>(response as string);

                    if (jsonObject.Keys.Contains("1080p"))
                    {
                        url = jsonObject["1080p"];
                        VideoQuality = 3;
                        return true;
                    }

                    if (jsonObject.Keys.Contains("720p"))
                    {
                        url = jsonObject["720p"];
                        VideoQuality = 2;
                        return true;
                    }

                    if (jsonObject.Keys.Contains("480p"))
                    {
                        url = jsonObject["480p"];
                        VideoQuality = 1;
                        return true;
                    }

                    if (jsonObject.Keys.Contains("360p"))
                    {
                        url = jsonObject["360p"];
                        VideoQuality = 1;
                        return true;
                    }

                    if (jsonObject.Keys.Contains("240p"))
                    {
                        url = jsonObject["240p"];
                        VideoQuality = 1;
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public string GetPairUri()
        {
            return "https://thevideo.me/pair";
        }

        public string GetUrl()
        {
            return url;
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

        public int VideoQuality { get; set; } = 1;
        public bool isTV { get; set; }

        public string imdb { get; set; }

        public async Task<string> GetMediaUrl()
        {
            var retUrl = "";

            try
            {
                var mediaId = url.Substring(url.LastIndexOf('/') + 1);
                var embed = $"https://thevideo.me/embed-{mediaId}.html";
                var UA = "URLResolver for Kodi/3.0.30";
                var httpClient = Utilities.GetHttpClient();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UA);
            }
            catch (Exception)
            {
            }

            return retUrl;
        }

        public NavigationItem item { get; set; }
    }
}