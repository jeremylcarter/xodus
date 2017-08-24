using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UrlResolver
{
    public class OpenLoad : IResolver, IPairedAuthorizationSource
    {
        private readonly string base_url = "https://api.openload.co/1";
        private readonly string get_url = "/streaming/get?file=";
        private readonly string url;
        private string realUrl;

        public OpenLoad(string uri)
        {
            url = uri;
            SourceName = "OPENLOAD";
        }

        public string GetUrl()
        {
            return url;
        }

        public string GetPairUri()
        {
            return "http://olpair.com/pair";
        }

        public async Task<bool> CheckAuthorization()
        {
            try
            {
                var mediaId = url.Substring(0, url.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));
                mediaId = mediaId.Substring(mediaId.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);

                var check_url = $"{base_url}{get_url}{mediaId}";
                var httpClient = Utilities.GetHttpClient();
                var response = await httpClient.GetStringAsync(check_url);
                var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

                var status = json["status"];

                var x = status as long?;

                if (x == 403)
                    return false;
                if (x == 404)
                {
                    realUrl = "";
                    return true;
                }

                var success = JsonConvert.DeserializeObject<SuccessResult>(response);

                realUrl = success.result.url;
                return true;
            }
            catch (Exception)
            {
            }

            return false;
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

        public string SourceName { get; set; }

        public int VideoQuality { get; set; }


        public async Task<string> GetMediaUrl()
        {
            if (null != realUrl)
                return realUrl;

            return "";
        }

        public NavigationItem item { get; set; }

        public class Result
        {
            public string name { get; set; }
            public string size { get; set; }
            public string sha1 { get; set; }
            public string content_type { get; set; }
            public string upload_at { get; set; }
            public string url { get; set; }
            public string token { get; set; }
        }

        public class SuccessResult
        {
            public int status { get; set; }
            public string msg { get; set; }
            public Result result { get; set; }
        }
    }
}