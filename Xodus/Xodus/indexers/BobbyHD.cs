using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CloudFlareUtilities;
using UrlResolver;

namespace Xodus
{
    public class BobbyHD : IIndexer
    {
        public async Task<List<IResolver>> GetMovieLink(string movie, int year)
        {
            var list = new List<IResolver>();

            var handler = new ClearanceHandler {MaxRetries = 2};
            var httpClient = Utilities.GetHttpClient(handler);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (iPhone; CPU iPhone OS 9_3_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13F69");

            try
            {
                var uri = "http://webapp.bobbyhd.com/search.php?keyword=" +
                          Uri.EscapeDataString(CleanTitle.Get(movie)) + "+" + year;

                var content = await httpClient.GetStringAsync(uri);
                var match = Regex.Match(content, @"alias=(.+?)\'\"">(.+?)</a>");
                var data = match.Groups[1].Value;
                Debug.WriteLine("alias: " + data);
                var data2 = await httpClient.GetStringAsync("http://webapp.bobbyhd.com/player.php?alias=" + data);
                match = Regex.Match(data2, @"changevideo\(\'(.+?)\'\)\"".+?data-toggle=\""tab\"">(.+?)</a>");

                var url = match.Groups[1].Value;

                if (url.ToLower().Contains("google"))
                {
                    var gv = new GoogleVideo(url);
                    gv.VideoSource = GetName();
                    list.Add(gv);
                }
            }
            catch (Exception ex)
            {
                var q = 0;
            }
            return list;
        }

        public string GetName()
        {
            return "bobby";
        }

        public async Task<List<IResolver>> GetTvLink(string movie, int year, int season, int episode)
        {
            var list = new List<IResolver>();
            return list;
        }
    }
}