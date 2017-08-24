using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using HtmlAgilityPack;
using UrlResolver;

namespace Xodus
{
    public class WatchFree : IIndexer
    {
        private readonly string base_link = "http://watchfree.to";
        private readonly string moviessearch_link = "/?keyword={0}&search_section=1";
        private readonly string tvsearch_link = "/?keyword={0}&search_section=2";

        public string GetName()
        {
            return "watchfree";
        }

        public async Task<List<IResolver>> GetTvLink(string movie, int year, int season, int episode)
        {
            var url = "";
            var resolvers = new List<IResolver>();
            try
            {
                var httpClient = Utilities.GetHttpClient();
                url = base_link + string.Format(tvsearch_link, CleanTitle.GetUrl(Uri.EscapeDataString(movie)));
                var uri = new Uri(url);
                var result = await httpClient.GetStringAsync(uri);
                var document = new HtmlDocument();
                document.LoadHtml(result);
                var results = document.DocumentNode.Descendants("div").Where(x => x.Attributes.Contains("class"));
                var items = results.Where(x => x.Attributes["class"].Value == "item");

                var searchMovie = movie;
                if (movie.Length > 13)
                    searchMovie = movie.Substring(0, 13);

                items = items.Where(x => x.InnerText.ToLower().Contains(searchMovie.ToLower()));
                var dev = items.FirstOrDefault(x => x.InnerText.Contains(string.Format("({0})", year)));
                var anchor = dev.FirstChild.Attributes["href"].Value;
                anchor = anchor.Replace("watch-", "tv-");
                url = base_link + anchor + $"/season-{season}-episode-{episode}";

                var sources = await httpClient.GetStringAsync(new Uri(url));
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(sources);

                var links = htmlDoc.DocumentNode.Descendants("td").Where(x => x.Attributes.Contains("class"));
                links = links.Where(x => x.Attributes["class"].Value == "link_middle");

                url = "";
                foreach (var link in links)
                {
                    var strongs = link.Descendants("strong").FirstOrDefault();
                    var anc = strongs.Descendants("a").FirstOrDefault().Attributes["href"].Value;
                    var t = new Uri(base_link + anc);
                    var ew = new WwwFormUrlDecoder(t.Query);
                    var gtfo = ew.GetFirstValueByName("gtfo");
                    var shit = Convert.FromBase64String(gtfo);
                    var shit2 = Encoding.UTF8.GetString(shit);

                    var resolver = await Utilities.GetResolver(GetName(), shit2);
                    if (null != resolver)
                        resolvers.Add(resolver);
                }
            }
            catch (Exception ex)
            {
                url = "";
                Debug.WriteLine(ex.ToString());
            }

            return resolvers;
        }

        public async Task<List<IResolver>> GetMovieLink(string movie, int year)
        {
            Debug.WriteLine("WatchFree - Movies");
            var resolvers = new List<IResolver>();
            var url = "";
            try
            {
                var httpClient = Utilities.GetHttpClient();
                url = base_link + string.Format(moviessearch_link, Uri.EscapeDataString(movie));
                var uri = new Uri(url);
                var result = await httpClient.GetStringAsync(uri);
                var document = new HtmlDocument();
                document.LoadHtml(result);
                var results = document.DocumentNode.Descendants("div").Where(x => x.Attributes.Contains("class"));
                var items = results.Where(x => x.Attributes["class"].Value == "item");

                var searchMovie = movie;
                if (movie.Length > 13)
                    searchMovie = movie.Substring(0, 13);

                items = items.Where(x => x.InnerText.ToLower().Contains(searchMovie.ToLower()));
                var dev = items.FirstOrDefault(x => x.InnerText.Contains(string.Format("({0})", year)));
                var anchor = dev.FirstChild.Attributes["href"].Value;
                url = base_link + anchor;

                var sources = await httpClient.GetStringAsync(new Uri(url));
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(sources);

                var links = htmlDoc.DocumentNode.Descendants("td").Where(x => x.Attributes.Contains("class"));
                links = links.Where(x => x.Attributes["class"].Value == "link_middle");

                url = "";

                foreach (var link in links)
                {
                    var strongs = link.Descendants("strong").FirstOrDefault();
                    var anc = strongs.Descendants("a").FirstOrDefault().Attributes["href"].Value;
                    var t = new Uri(base_link + anc);
                    var ew = new WwwFormUrlDecoder(t.Query);
                    var gtfo = ew.GetFirstValueByName("gtfo");
                    var shit = Convert.FromBase64String(gtfo);
                    var shit2 = Encoding.UTF8.GetString(shit);

                    Debug.WriteLine("WATCHFREE: " + shit2);

                    var resolver = await Utilities.GetResolver(GetName(), shit2);
                    if (null != resolver)
                        resolvers.Add(resolver);
                }
            }
            catch (Exception ex)
            {
                url = "";
                Debug.WriteLine(ex.ToString());
            }

            return resolvers;
        }
    }
}