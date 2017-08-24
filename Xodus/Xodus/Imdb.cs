using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using HtmlAgilityPack;

namespace Xodus
{
    public class Imdb
    {
        private readonly string IMDB_BASE = "http://www.imdb.com";
        // http://www.imdb.com/search/title?title_type=feature,tv_movie&production_status=released&groups=oscar_best_picture_winners&sort=year,desc&count=40&start=1


        public async Task<List<(string, string)>> GetGenre(string genre, bool useKeyword, int page = 1)
        {
            var titles = new List<(string, string)>();
            try
            {
                var web = Utilities.GetHttpClient();
                var content = "";

                if (useKeyword)
                    content = await web.GetStringAsync(
                        $"http://www.imdb.com/search/title?title_type=feature,tv_movie,documentary&num_votes=100,&release_date=,date[0]&keywords={genre}&sort=moviemeter,asc&count=40&start=1&page={page}");
                else
                    content = await web.GetStringAsync(
                        $"http://www.imdb.com/search/title?title_type=feature,tv_movie,documentary&num_votes=100,&release_date=,date[0]&genres={genre}&sort=moviemeter,asc&count=40&start=1&page={page}");
                var html = new HtmlDocument();
                html.LoadHtml(content);
                var links = html.DocumentNode.Descendants("h3");


                foreach (var link in links)
                    try
                    {
                        var title = link.Descendants("a")?.FirstOrDefault()?.InnerText;
                        var yearList = link.Descendants("span")
                            .FirstOrDefault(y => y.Attributes.Contains("class") &&
                                                 y.Attributes["class"].Value == "lister-item-year text-muted unbold")
                            ?.InnerText;
                        var href = link.Descendants("a").FirstOrDefault(x => x.Attributes.Contains("href"))
                            .Attributes["href"].Value;
                        var parts = href?.Split('/');
                        var imdb = parts?[2];
                        var item = title + " " + yearList;

                        if (!string.IsNullOrWhiteSpace(item) && !item.Equals("\n"))
                            titles.Add((title + " " + yearList, imdb));
                    }
                    catch (Exception)
                    {
                    }
            }
            catch (Exception)
            {
            }

            return titles;
        }

        public async Task<List<(string, string)>> GetBestPicture(int page = 1)
        {
            var titles = new List<(string, string)>();
            try
            {
                var web = Utilities.GetHttpClient();
                var content = await web.GetStringAsync(
                    $"http://www.imdb.com/search/title?title_type=feature,tv_movie&production_status=released&groups=oscar_best_picture_winners&sort=year,desc&count=40&start=1&page={page}");
                var html = new HtmlDocument();
                html.LoadHtml(content);
                var links = html.DocumentNode.Descendants("h3");


                foreach (var link in links)
                    try
                    {
                        var title = link.Descendants("a")?.FirstOrDefault()?.InnerText;
                        var yearList = link.Descendants("span")
                            .FirstOrDefault(y => y.Attributes.Contains("class") &&
                                                 y.Attributes["class"].Value == "lister-item-year text-muted unbold")
                            ?.InnerText;
                        var href = link.Descendants("a").FirstOrDefault(x => x.Attributes.Contains("href"))
                            .Attributes["href"].Value;
                        var parts = href?.Split('/');
                        var imdb = parts?[2];
                        var item = title + " " + yearList;

                        if (!string.IsNullOrWhiteSpace(item) && !item.Equals("\n"))
                            titles.Add((title + " " + yearList, imdb));
                    }
                    catch (Exception)
                    {
                    }
            }
            catch (Exception)
            {
            }

            return titles;
        }


        public async Task<List<(string, string)>> GetInTheaters(int page = 1)
        {
            var titles = new List<(string, string)>();
            try
            {
                var web = Utilities.GetHttpClient();
                var content = await web.GetStringAsync(
                    $"http://www.imdb.com/search/title?title_type=feature&num_votes=1000,&release_date=date[365],date[0]&sort=release_date_us,desc&count=40&start=1&page={page}");
                var html = new HtmlDocument();
                html.LoadHtml(content);
                var links = html.DocumentNode.Descendants("h3");


                foreach (var link in links)
                    try
                    {
                        var title = link.Descendants("a")?.FirstOrDefault()?.InnerText;
                        var yearList = link.Descendants("span")
                            .FirstOrDefault(y => y.Attributes.Contains("class") &&
                                                 y.Attributes["class"].Value == "lister-item-year text-muted unbold")
                            ?.InnerText;
                        var href = link.Descendants("a").FirstOrDefault(x => x.Attributes.Contains("href"))
                            .Attributes["href"].Value;
                        var parts = href?.Split('/');
                        var imdb = parts?[2];
                        var item = title + " " + yearList;

                        if (!string.IsNullOrWhiteSpace(item) && !item.Equals("\n"))
                            titles.Add((title + " " + yearList, imdb));
                    }
                    catch (Exception)
                    {
                    }
            }
            catch (Exception)
            {
            }

            return titles;
        }

        public async Task<List<(string, string)>> GetMostVoted(int page = 1)
        {
            var titles = new List<(string, string)>();
            try
            {
                var web = Utilities.GetHttpClient();
                var content = await web.GetStringAsync(
                    $"http://www.imdb.com/search/title?title_type=feature,tv_movie&languages=en&num_votes=1000,&production_status=released&sort=num_votes,desc&count=40&start=1&page={page}");
                var html = new HtmlDocument();
                html.LoadHtml(content);
                var links = html.DocumentNode.Descendants("h3");


                foreach (var link in links)
                    try
                    {
                        var title = link.Descendants("a")?.FirstOrDefault()?.InnerText;
                        var yearList = link.Descendants("span")
                            .FirstOrDefault(y => y.Attributes.Contains("class") &&
                                                 y.Attributes["class"].Value == "lister-item-year text-muted unbold")
                            ?.InnerText;
                        var href = link.Descendants("a").FirstOrDefault(x => x.Attributes.Contains("href"))
                            .Attributes["href"].Value;
                        var parts = href?.Split('/');
                        var imdb = parts?[2];
                        var item = title + " " + yearList;

                        if (!string.IsNullOrWhiteSpace(item) && !item.Equals("\n"))
                            titles.Add((title + " " + yearList, imdb));
                    }
                    catch (Exception)
                    {
                    }
            }
            catch (Exception)
            {
            }

            return titles;
        }

        public async Task<List<(string, string, string)>> SearchMovies(string query)
        {
            var list = new List<(string, string, string)>();

            try
            {
                var encoded = Uri.EscapeDataString(query);
                var url = $"http://www.imdb.com/find?q={encoded}&s=tt&ttype=ft&ref_=fn_ft";
                var web = Utilities.GetHttpClient();
                var content2 = await web.GetStringAsync(url);
                var content = new HtmlDocument();
                content.LoadHtml(content2);
                var results = content.DocumentNode.Descendants("td").Where(x => x.Attributes.Contains("class"))
                    .Where(x => x.Attributes["class"].Value == "result_text");


                foreach (var result in results)
                {
                    var link = result.Descendants("a").FirstOrDefault();
                    var href = link.Attributes["href"].Value;
                    var parts = href?.Split('/');
                    var imdb = parts?[2];
                    var title = result.InnerText;
                    title = title.Replace("(TV Series)", "");
                    title = title.Trim();
                    list.Add((title, href, imdb));
                }
            }
            catch (Exception)
            {
            }

            return list;
        }

        public async Task<List<(string, string, string)>> SearchTv(string query)
        {
            var list = new List<(string, string, string)>();

            try
            {
                var encoded = Uri.EscapeDataString(query);
                var url = $"http://www.imdb.com/find?q={encoded}&s=tt&ttype=tv&ref_=fn_tv";
                var web = Utilities.GetHttpClient();
                var content2 = await web.GetStringAsync(url);
                var content = new HtmlDocument();
                content.LoadHtml(content2);
                var results = content.DocumentNode.Descendants("td").Where(x => x.Attributes.Contains("class"))
                    .Where(x => x.Attributes["class"].Value == "result_text");

                foreach (var result in results)
                {
                    var link = result.Descendants("a").FirstOrDefault();
                    var href = link.Attributes["href"].Value;
                    var title = result.InnerText;
                    var parts = href?.Split('/');
                    var imdb = parts?[2];
                    title = title.Replace("(TV Series)", "");
                    title = title.Trim();
                    list.Add((title, href, imdb));
                }
            }
            catch (Exception)
            {
            }
            return list;
        }

        public async Task<List<(string, string)>> GetTvSeasonEpisodeList(string url)
        {
            var list = new List<(string, string)>();

            try
            {
                var web = Utilities.GetHttpClient();
                var content = await web.GetStringAsync(IMDB_BASE + url);
                var result = new HtmlDocument();
                result.LoadHtml(content);
                var divs = result.DocumentNode.Descendants("div").FirstOrDefault(x => x.Attributes.Contains("class") &&
                                                                                      x.Attributes["class"].Value ==
                                                                                      "list detail eplist");


                var twat = divs.Descendants("div").Where(x => x.Attributes.Contains("class"))
                    .Where(x => x.Attributes["class"].Value == "image");
                foreach (var episodeDiv in twat)
                {
                    var link = episodeDiv.Descendants("a").FirstOrDefault();
                    var imdbLink = link.Attributes["href"].Value;
                    var title = link.Attributes["title"].Value;


                    var re1 = "((?:[a-z][a-z]+))"; // Word 1
                    var re2 = ".*?"; // Non-greedy match on filler
                    var re3 = "."; // Uninteresting: c
                    var re4 = ".*?"; // Non-greedy match on filler
                    var re5 = "(.)"; // Any Single Character 1
                    var re6 = "(\\d+)"; // Integer Number 1
                    var re7 = "(.)"; // Any Single Character 2
                    var re8 = "(\\d+)"; // Integer Number 2

                    var r = new Regex(re1 + re2 + re3 + re4 + re5 + re6 + re7 + re8,
                        RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    var m = r.Match(title);
                    if (m.Success)
                        continue;


                    list.Add((title, imdbLink));
                }
            }
            catch (Exception)
            {
            }

            return list;
        }

        public async Task<List<(string, string)>> GetTvSeasons(string uri)
        {
            try
            {
                var id = uri;
                id = uri.Replace("/title/", "");
                id = id.Substring(0, id.IndexOf("/", StringComparison.Ordinal));
                var web = Utilities.GetHttpClient();
                var content = await web.GetStringAsync(IMDB_BASE + uri);
                var result = new HtmlDocument();
                result.LoadHtml(content);
                var divs = result.DocumentNode.Descendants("div")
                    .FirstOrDefault(x => x.Attributes.Contains("class") &&
                                         x.Attributes["class"].Value == "seasons-and-year-nav");
                var innerDivs = divs.Descendants("div");
                var seasonDiv = innerDivs.ElementAtOrDefault(2);
                var seasons = seasonDiv.Descendants("a").FirstOrDefault();
                var totalSeasons = 0;

                int.TryParse(seasons.InnerText, out totalSeasons);

                var list = new List<(string, string)>();
                for (var i = 1; i <= totalSeasons; i++)
                {
                    var rl = new ResourceLoader();
                    var name = rl.GetString("Season");
                    name += " ";
                    name += i.ToString();
                    var href = $"/title/{id}/episodes?season={i}&ref_=tt_eps_sn_{i}";
                    list.Add((name, href));
                }

                return list;
            }
            catch (Exception)
            {
            }
            return new List<(string, string)>();
        }

        public async Task<List<(string, string, string)>> GetMostPopularTv(int page = 1)
        {
            var titles = new List<(string, string, string)>();

            try
            {
                var web = Utilities.GetHttpClient();
                var content = await web.GetStringAsync(
                    $"http://www.imdb.com/search/title?title_type=tv_series,mini_series&languages=en&num_votes=100,&release_date=,date[0]&sort=moviemeter,asc&count=40&start=1&page={page}");
                var html = new HtmlDocument();
                html.LoadHtml(content);
                var links = html.DocumentNode.Descendants("h3");


                foreach (var link in links)
                    try
                    {
                        var title = link.Descendants("a")?.FirstOrDefault()?.InnerText;
                        var href = link.Descendants("a")?.FirstOrDefault()?.Attributes["href"]?.Value;
                        var yearList = link.Descendants("span")
                            .FirstOrDefault(y => y.Attributes.Contains("class") &&
                                                 y.Attributes["class"].Value == "lister-item-year text-muted unbold")
                            ?.InnerText;
                        var parts = href?.Split('/');
                        var imdb = parts[2];
                        var item = title + " " + yearList;

                        if (!string.IsNullOrWhiteSpace(item) && !item.Equals("\n"))
                            titles.Add((title + " " + yearList, href, imdb));
                    }
                    catch (Exception)
                    {
                    }
            }
            catch (Exception)
            {
            }

            return titles;
        }

        public async Task<List<(string, string)>> GetMostPopular(int page = 1)
        {
            var titles = new List<(string, string)>();

            try
            {
                var web = Utilities.GetHttpClient();
                var content = await web.GetStringAsync(
                    $"http://www.imdb.com/search/title?title_type=feature,tv_movie&languages=en&num_votes=1000,&production_status=released&groups=top_1000&sort=moviemeter,asc&count=40&start=1&page={page}&ref_=adv_nxt");

                var html = new HtmlDocument();
                html.LoadHtml(content);
                var links = html.DocumentNode.Descendants("h3");


                foreach (var link in links)
                    try
                    {
                        var title = link.Descendants("a")?.FirstOrDefault()?.InnerText;
                        var yearList = link.Descendants("span")
                            .FirstOrDefault(y => y.Attributes.Contains("class") &&
                                                 y.Attributes["class"].Value == "lister-item-year text-muted unbold")
                            ?.InnerText;

                        var href = link.Descendants("a").FirstOrDefault(x => x.Attributes.Contains("href"))
                            .Attributes["href"].Value;
                        var parts = href?.Split('/');
                        var imdb = parts?[2];
                        var item = title + " " + yearList;

                        if (!string.IsNullOrWhiteSpace(item) && !item.Equals("\n"))
                            titles.Add((title + " " + yearList, imdb));
                    }
                    catch (Exception)
                    {
                    }
            }
            catch (Exception)
            {
            }

            return titles;
        }
    }
}