using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using UrlResolver;

namespace Xodus
{
    public class Onseries : IIndexer
    {
        public async Task<List<IResolver>> GetMovieLink(string movie, int year)
        {
            var list = new List<IResolver>();
            return list;
        }

        public string GetName()
        {
            return "onseries";
        }

        public async Task<List<IResolver>> GetTvLink(string movie, int year, int season, int episode)
        {
            var list = new List<IResolver>();

            try
            {
                var httpClient = Utilities.GetHttpClient();
                var title = CleanTitle.Get(movie);
                title = Uri.EscapeDataString(title);

                var result = await httpClient.GetStringAsync($"http://mywatchseries.to/search/{title}");
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(result);

                var divs = htmlDocument.DocumentNode.Descendants("div").Where(x => x.Attributes.Contains("valign"));

                var getLink = "";
                foreach (var div in divs)
                {
                    var link = "";
                    var tit = "";
                    var inner = "";

                    link = div.Descendants("a").FirstOrDefault().Attributes["href"].Value;
                    tit = div.Descendants("a").FirstOrDefault().Attributes["title"].Value;
                    inner = div.Descendants("a").FirstOrDefault().InnerText;

                    var matches = Regex.Match(inner, "(\\d{4})");
                    var y = 0;

                    if (int.TryParse(matches.Value, out y))
                        if (y == year)
                        {
                            getLink = link;
                            break;
                        }
                }

                if (getLink.Length > 0)
                {
                    var test = await httpClient.GetStringAsync(getLink);
                    htmlDocument.LoadHtml(test);

                    var n = htmlDocument.DocumentNode.Descendants("a");

                    var text = "s" + season + "_e" + episode;

                    var r = htmlDocument.DocumentNode.Descendants("a").FirstOrDefault(
                        x => x.Attributes.Contains("href") &&
                             x.Attributes["href"].Value.Contains(text));

                    if (r != null)
                    {
                        test = await httpClient.GetStringAsync(r.Attributes["href"].Value);
                        htmlDocument.LoadHtml(test);

                        var links = htmlDocument.DocumentNode.Descendants("a")
                            .Where(x => x.Attributes.Contains("target"));

                        foreach (var link in links)
                            try
                            {
                                var uri = new Uri(link.Attributes["href"].Value);
                                var query = uri.Query;

                                if (!query.ToLower().Contains("r="))
                                    continue;

                                query = query.TrimStart('?');
                                var dicQueryString =
                                    query.Split('&')
                                        .ToDictionary(c => c.Split('=')[0],
                                            c => Uri.UnescapeDataString(c.Split('=')[1]));

                                var userId = dicQueryString["r"];
                                var something = Encoding.UTF8.GetString(Convert.FromBase64String(userId));

                                var resolver = await Utilities.GetResolver(GetName(), something);

                                if (resolver != null)
                                    list.Add(resolver);
                            }
                            catch (Exception)
                            {
                            }
                    }
                }
            }
            catch (Exception)
            {
            }

            return list;
        }
    }
}