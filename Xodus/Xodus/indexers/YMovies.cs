using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using UrlResolver;

namespace Xodus
{
    public class YMovies : IIndexer
    {
        public async Task<List<IResolver>> GetMovieLink(string movie, int year)
        {
            var list = new List<IResolver>();

            try
            {
                var sources = new List<string>();
                var title = CleanTitle.Get(movie);
                var quotetitle = CleanTitle.GetSearch(title).Replace(' ', '+');
                var url = $"https://yesmovies.to/movie/search/{quotetitle}.html";
                var httpClient = Utilities.GetHttpClient();
                var result = await httpClient.GetStringAsync(url);
                var html = new HtmlDocument();
                html.LoadHtml(result);
                var divs = html.DocumentNode.Descendants("div").Where(
                    x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "ml-item");

                var dataUrl = "";

                foreach (var div in divs)
                {
                    var r = div.Descendants("a");

                    foreach (var link in r)
                        if (link.Attributes.Contains("title"))
                        {
                            var match = Regex.Match(link.Attributes["title"].Value, @"\((\d{4})");
                            if (match.Success)
                            {
                                var linkYear = 0;
                                if (int.TryParse(match.Groups[1].Value, out linkYear))
                                    if (linkYear == year)
                                    {
                                        dataUrl = link.Attributes["href"].Value;
                                        break;
                                    }
                            }
                            else
                            {
                                dataUrl = link.Attributes["href"].Value;
                            }
                        }

                    if (!string.IsNullOrEmpty(dataUrl))
                        break;
                }

                if (!string.IsNullOrEmpty(dataUrl))
                {
                    var servernum = "";
                    var code = Regex.Matches(dataUrl, @"-(\d+)");
                    foreach (Match c in code)
                        servernum = c.Groups[1].Value;

                    url = $"https://yesmovies.to/ajax/v4_movie_episodes/{servernum}";
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", dataUrl);
                    var data = await httpClient.GetStringAsync(url);
                    var serverresponse = JsonConvert.DeserializeObject<YMoviesServerResponse>(data);
                    var funk = new HtmlDocument();
                    funk.LoadHtml(serverresponse.html);
                    var paslist = funk.DocumentNode.Descendants("div")
                        .FirstOrDefault(
                            x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "pas-list");
                    var ids = paslist.Descendants("li").Where(x => x.Attributes.Contains("data-id"));
                    var servers = paslist.Descendants("li").Where(x => x.Attributes.Contains("data-server"));
                    var labels = paslist.Descendants("a").Where(x => x.Attributes.Contains("title"));

                    var idList = new List<string>();

                    foreach (var id in ids)
                        idList.Add(id.Attributes["data-id"].Value);

                    var serverList = new List<string>();
                    foreach (var server in servers)
                        serverList.Add(server.Attributes["data-server"].Value);

                    var labelList = new List<string>();

                    foreach (var label in labels)
                        labelList.Add(label.Attributes["title"].Value);

                    foreach (var id in idList)
                    {
                        var test = $"https://yesmovies.to/ajax/movie_token?eid={id}&mid={servernum}";
                        var script = await httpClient.GetStringAsync(test);

                        var param1 = "";
                        var param2 = "";

                        if (script.Contains("_x="))
                        {
                            var tokens = script.Split(',');
                            var xVal = tokens[0].Replace("_x='", "");
                            xVal = xVal.Replace("'", "");
                            xVal = xVal.Replace(";", "");
                            var yVal = tokens[1].Replace("_y='", "");
                            yVal = yVal.Replace("'", "");
                            yVal = yVal.Replace(";", "");
                            param1 = xVal.Trim();
                            param2 = yVal.Trim();
                        }

                        test = $"https://yesmovies.to/ajax/movie_sources/{id}?x={param1}&y={param2}";

                        var fuck = await httpClient.GetStringAsync(test);
                        var source = JsonConvert.DeserializeObject<YMoviesSource>(fuck);

                        if (null != source)
                            foreach (var i in source?.playlist)
                            foreach (var s in i?.sources)
                                sources.Add(s?.file);
                    }
                }

                foreach (var source in sources)
                {
                    var gv = new GoogleVideo(source);

                    if (string.IsNullOrEmpty(await gv.GetMediaUrl()))
                        continue;
                    gv.VideoSource = GetName();
                    list.Add(gv);
                }
            }
            catch (Exception)
            {
            }

            return list;
        }

        public string GetName()
        {
            return "ymovies";
        }

        public async Task<List<IResolver>> GetTvLink(string movie, int year, int season, int episode)
        {
            var list = new List<IResolver>();
            try
            {
                var sources = new List<string>();
                var title = CleanTitle.Get(movie);
                var search = title + " Season " + season;
                var quotetitle = CleanTitle.GetSearch(search).Replace(' ', '+');
                var url = $"https://yesmovies.to/movie/search/{quotetitle}.html";
                var httpClient = Utilities.GetHttpClient();
                var result = await httpClient.GetStringAsync(url);
                var html = new HtmlDocument();
                html.LoadHtml(result);
                var divs = html.DocumentNode.Descendants("div").Where(
                    x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "ml-item");

                var dataUrl = "";

                foreach (var div in divs)
                {
                    var r = div.Descendants("a");

                    foreach (var link in r)
                        if (link.Attributes.Contains("title"))
                        {
                            var match = Regex.Match(link.Attributes["title"].Value, @"(.*?)\s+-\s+Season\s+(\d)");
                            if (match.Success)
                            {
                                var linkYear = 0;
                                if (int.TryParse(match.Groups[2].Value, out linkYear))
                                    if (linkYear == season)
                                    {
                                        dataUrl = link.Attributes["href"].Value;
                                        break;
                                    }
                            }
                            else
                            {
                                dataUrl = link.Attributes["href"].Value;
                            }
                        }

                    if (!string.IsNullOrEmpty(dataUrl))
                        break;
                }

                if (!string.IsNullOrEmpty(dataUrl))
                {
                    var servernum = "";
                    var code = Regex.Matches(dataUrl, @"-(\d+)");
                    foreach (Match c in code)
                        servernum = c.Groups[1].Value;

                    url = $"https://yesmovies.to/ajax/v4_movie_episodes/{servernum}";
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", dataUrl);
                    var data = await httpClient.GetStringAsync(url);
                    var serverresponse = JsonConvert.DeserializeObject<YMoviesServerResponse>(data);
                    var funk = new HtmlDocument();
                    funk.LoadHtml(serverresponse.html);
                    var paslist = funk.DocumentNode.Descendants("div")
                        .FirstOrDefault(
                            x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "pas-list");
                    var ids = paslist.Descendants("li").Where(x => x.Attributes.Contains("data-id"));
                    var servers = paslist.Descendants("li").Where(x => x.Attributes.Contains("data-server"));
                    var labels = paslist.Descendants("a").Where(x => x.Attributes.Contains("title"));

                    var idList = new List<string>();

                    foreach (var id in ids)
                        idList.Add(id.Attributes["data-id"].Value);

                    var serverList = new List<string>();
                    foreach (var server in servers)
                        serverList.Add(server.Attributes["data-server"].Value);

                    var labelList = new List<string>();

                    foreach (var label in labels)
                        labelList.Add(label.Attributes["title"].Value);

                    var idt = 0;
                    foreach (var id in idList)
                    {
                        var eptest = Regex.Match(labelList[idt].ToLower(), @"episode.*?(\d+).*?").Groups[1].Value;
                        var ep = int.Parse(eptest);
                        if (ep != episode)
                        {
                            idt++;
                            continue;
                        }

                        var test = $"https://yesmovies.to/ajax/movie_token?eid={id}&mid={servernum}";
                        var script = await httpClient.GetStringAsync(test);

                        var param1 = "";
                        var param2 = "";

                        if (script.Contains("_x="))
                        {
                            var tokens = script.Split(',');
                            var xVal = tokens[0].Replace("_x='", "");
                            xVal = xVal.Replace("'", "");
                            xVal = xVal.Replace(";", "");
                            var yVal = tokens[1].Replace("_y='", "");
                            yVal = yVal.Replace("'", "");
                            yVal = yVal.Replace(";", "");
                            param1 = xVal.Trim();
                            param2 = yVal.Trim();
                        }

                        test = $"https://yesmovies.to/ajax/movie_sources/{id}?x={param1}&y={param2}";

                        var fuck = await httpClient.GetStringAsync(test);
                        var source = JsonConvert.DeserializeObject<YMoviesSource>(fuck);

                        if (null != source)
                            foreach (var i in source?.playlist)
                            foreach (var s in i?.sources)
                                sources.Add(s?.file);
                        idt++;
                    }
                }

                foreach (var source in sources)
                {
                    var gv = new GoogleVideo(source);

                    if (string.IsNullOrEmpty(await gv.GetMediaUrl()))
                        continue;

                    gv.VideoSource = GetName();
                    list.Add(gv);
                }
            }
            catch (Exception ex)
            {
            }

            return list;
        }

        public class YMoviesServerResponse
        {
            public bool status { get; set; }
            public string html { get; set; }
        }

        public class Source
        {
            public string file { get; set; }
            public string type { get; set; }
        }

        public class Track
        {
            public bool @default { get; set; }
            public string file { get; set; }
            public string label { get; set; }
            public string kind { get; set; }
        }

        public class Playlist
        {
            public List<Source> sources { get; set; }
            public List<Track> tracks { get; set; }
        }

        public class YMoviesSource
        {
            public List<Playlist> playlist { get; set; }
        }
    }
}