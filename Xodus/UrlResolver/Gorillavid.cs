using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace UrlResolver
{
    public class Gorillavid : IResolver
    {
        private readonly string url;

        public Gorillavid(string uri)
        {
            url = uri;
            SourceName = "GORILLAVID";
        }

        public int VideoQuality { get; set; }

        public bool isTV { get; set; }
        public NavigationItem item { get; set; }
        public string imdb { get; set; }
        public string VideoSource { get; set; }
        public string VideoSourceName { get; set; }
        public string SourceName { get; set; }

        public async Task<string> GetMediaUrl()
        {
            Debug.WriteLine("GorillaVid");
            var file = "";

            try
            {
                var newUrl = url;
                var httpClient = Utilities.GetHttpClient();
                var url2 = await httpClient.GetAsync(new Uri(newUrl));
                var response = await url2.Content.ReadAsStringAsync();

                if (response.Contains("404 - File Not Found"))
                    return "";

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(response);

                var op = htmlDocument.DocumentNode.Descendants("input").Where(x => x.Attributes.Contains("name"))
                    .Where(x => x.Attributes["name"].Value == "op").FirstOrDefault().Attributes["value"].Value;
                var usr_login = htmlDocument.DocumentNode.Descendants("input").Where(x => x.Attributes.Contains("name"))
                    .Where(x => x.Attributes["name"].Value == "usr_login").FirstOrDefault().Attributes["value"].Value;
                var id = htmlDocument.DocumentNode.Descendants("input").Where(x => x.Attributes.Contains("name"))
                    .Where(x => x.Attributes["name"].Value == "id").FirstOrDefault().Attributes["value"].Value;
                var fname = htmlDocument.DocumentNode.Descendants("input").Where(x => x.Attributes.Contains("name"))
                    .Where(x => x.Attributes["name"].Value == "fname").FirstOrDefault().Attributes["value"].Value;
                var referer = htmlDocument.DocumentNode.Descendants("input").Where(x => x.Attributes.Contains("name"))
                    .Where(x => x.Attributes["name"].Value == "referer").FirstOrDefault().Attributes["value"].Value;
                var method_free = htmlDocument.DocumentNode.Descendants("input")
                    .Where(x => x.Attributes.Contains("name"))
                    .Where(x => x.Attributes["name"].Value == "method_free").FirstOrDefault().Attributes["value"].Value;

                var keys = new Dictionary<string, string>();
                keys.Add("op", op);
                keys.Add("usr_login", usr_login);
                keys.Add("id", id);
                keys.Add("fname", fname);
                keys.Add("referer", referer);
                keys.Add("method_free", method_free);
                var result = await httpClient.PostAsync(new Uri(newUrl), new FormUrlEncodedContent(keys));

                var data = await result.Content.ReadAsStringAsync();


                if (data.Contains("file://"))
                {
                    file = data.Substring(data.IndexOf("file:"));
                    file = file.Substring(file.IndexOf("\"") + 1);
                    file = file.Substring(0, file.IndexOf("\""));
                    file = file.Trim();
                }
                else if (data.Contains("src: 'http"))
                {
                    file = data.Substring(data.IndexOf("src: 'http") + 5);
                    file = file.Substring(file.IndexOf("\"") + 1);
                    file = file.Substring(0, file.IndexOf("\""));
                    file = file.Trim();
                }
            }
            catch (Exception)
            {
            }

            return file;
        }
    }
}