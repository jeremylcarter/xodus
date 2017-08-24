using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CloudFlareUtilities;
using HtmlAgilityPack;
using UrlResolver;

namespace Xodus
{
    public class Dayt : IIndexer
    {
        public async Task<List<IResolver>> GetMovieLink(string movie, int year)
        {
            var list = new List<IResolver>();

            try
            {
                var title = Regex.Replace(movie, "\\/:*?\"'<>|!,", "");
                title = title.Replace(" ", "-");
                title = title.Replace("--", "-");
                title = title.ToLower();

                var url = $"http://xpau.se/watch/{title}";
                var handler = new ClearanceHandler {MaxRetries = 2};
                var httpClient = new HttpClient(handler);
                var result = await httpClient.GetAsync(url);
                var newt = result.Content.Headers.ContentLocation.OriginalString;
                url = $"http://xpau.se/watch/{newt}";
                result = await httpClient.GetAsync(url);
                var wtf = Encoding.UTF8.GetString(await result.Content.ReadAsByteArrayAsync());
                var matches = Regex.Matches(wtf, @"Date\s*:\s*.+?>.+?(\d{4})");
                foreach (Match c in matches)
                {
                    var x = 0;
                }
                var html = new HtmlDocument();
                html.LoadHtml(wtf);

                var tit = html.DocumentNode.Descendants("title").FirstOrDefault();

                var docTit = "";
                if (null != tit)
                    docTit = tit.InnerText;

                var quality = 1;

                if (docTit.ToLower().Contains("1080p"))
                    quality = 3;
                else
                    quality = 2;

                var divs = html.DocumentNode.Descendants("div").FirstOrDefault(x => x.Attributes.Contains("id") &&
                                                                                    x.Attributes["id"].Value ==
                                                                                    "5throw");

                var links = divs.Descendants("a").Where(x => x.Attributes.Contains("rel") &&
                                                             x.Attributes["rel"].Value == "nofollow");

                foreach (var link in links)
                {
                    var href = link.Attributes["href"].Value;

                    if (href.Contains("mail.ru"))
                    {
                        var mov = await CldMailRu(href);

                        if (mov.Length > 0)
                        {
                            var gr = new GenericResolver(mov);
                            gr.VideoQuality = quality;
                            gr.VideoSource = GetName();
                            list.Add(gr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var y = 0;
            }

            return list;
        }

        public string GetName()
        {
            return "dayt";
        }

        public async Task<List<IResolver>> GetTvLink(string movie, int year, int season, int episode)
        {
            var list = new List<IResolver>();

            try
            {
                var title = Regex.Replace(movie, "\\/:*?\"'<>|!,", "");
                title = title.Replace(" ", "-");
                title = title.Replace("--", "-");
                title = title.ToLower();

                var url = $"http://xpau.se/watch/{title}/s{season}/e{episode}";
                var handler = new ClearanceHandler {MaxRetries = 2};
                var httpClient = new HttpClient(handler);
                var result = await httpClient.GetAsync(url);
                var newt = result.Content.Headers.ContentLocation.OriginalString;

                url = $"http://xpau.se/watch/{title}/s{season}/{newt}";
                result = await httpClient.GetAsync(url);
                var wtf = Encoding.UTF8.GetString(await result.Content.ReadAsByteArrayAsync());
                var matches = Regex.Matches(wtf, @"Date\s*:\s*.+?>.+?(\d{4})");
                foreach (Match c in matches)
                {
                    var x = 0;
                }
                var html = new HtmlDocument();
                html.LoadHtml(wtf);

                var tit = html.DocumentNode.Descendants("title").FirstOrDefault();

                var docTit = "";
                if (null != tit)
                    docTit = tit.InnerText;

                var quality = 1;

                if (docTit.ToLower().Contains("1080p"))
                    quality = 3;
                else
                    quality = 2;

                var divs = html.DocumentNode.Descendants("div").FirstOrDefault(x => x.Attributes.Contains("id") &&
                                                                                    x.Attributes["id"].Value ==
                                                                                    "5throw");

                var links = divs.Descendants("a").Where(x => x.Attributes.Contains("rel") &&
                                                             x.Attributes["rel"].Value == "nofollow");

                foreach (var link in links)
                {
                    var href = link.Attributes["href"].Value;

                    if (href.Contains("mail.ru"))
                    {
                        var mov = await CldMailRu(href);

                        var gr = new GenericResolver(mov);
                        gr.VideoQuality = quality;
                        gr.VideoSource = GetName();
                        list.Add(gr);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return list;
        }

        private async Task<string> CldMailRu(string link)
        {
            var v = link.Substring(link.LastIndexOf("public") + 6);
            //var v = tokens[tokens.Length - 1];

            var handler = new ClearanceHandler {MaxRetries = 2};
            var httpClient = new HttpClient(handler);
            var result = await httpClient.GetAsync(link);
            var wtf = Encoding.UTF8.GetString(await result.Content.ReadAsByteArrayAsync());

            var tok = Regex.Match(wtf, "\"tokens\"\\s*:\\s*{\\s*\"download\"\\s*:\\s*\"([^\"]+)").Groups[1].Value;
            var url = Regex.Match(wtf, "\"weblink_get\"\\s*:\\s*\\[.+?\"url\"\\s*:\\s*\"([^\"]+)").Groups[1].Value;
            var theThing = $"{url}{v}?key={tok}";
            return theThing;
        }
    }
}