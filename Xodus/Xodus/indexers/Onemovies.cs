using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using UrlResolver;

namespace Xodus
{
    public class Onemovies : IIndexer
    {
        public async Task<List<IResolver>> GetMovieLink(string movie, int year)
        {
            var list = new List<IResolver>();

            try
            {
                var sources = new List<string>();
                var title = CleanTitle.Get(movie);
                var quotetitle = CleanTitle.GetSearch(title).Replace(' ', '+');
                var url = $"https://gostream.is/ajax/suggest_search";
                var httpClient = Utilities.GetHttpClient();
                var dict = new Dictionary<string, string>();
                dict.Add("keyword", title);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                var post = await httpClient.PostAsync(url, new FormUrlEncodedContent(dict));
                var result = await post.Content.ReadAsStringAsync();
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var stuff = JsonConvert.DeserializeObject<OneMoviesFirstResult>(result);
                var html = new HtmlDocument();
                html.LoadHtml(stuff.content);

                var divs = html.DocumentNode.Descendants("a").Where(
                    x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "ss-title");

                var dataUrl = "";

                var exactMatch = false;
                var exactUrl = "";
                foreach (var div in divs)
                {
                    if (div.InnerText.ToLower() == title.ToLower())
                    {
                        exactUrl = div.Attributes["href"].Value;
                        exactMatch = true;
                        break;
                    }

                    dataUrl = div.Attributes["href"].Value;
                }

                if (exactMatch)
                    dataUrl = exactUrl;

                if (!string.IsNullOrEmpty(dataUrl))
                {
                    var servernum = "";
                    var code = Regex.Matches(dataUrl, @"-(\d+)");
                    foreach (Match c in code)
                        servernum = c.Groups[1].Value;

                    url = $"https://gostream.is/ajax/movie_episodes/{servernum}";
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", dataUrl);
                    var data = await httpClient.GetStringAsync(url);
                    var serverresponse = JsonConvert.DeserializeObject<YMoviesServerResponse>(data);
                    var funk = new HtmlDocument();
                    funk.LoadHtml(serverresponse.html);
                    var paslist = funk.DocumentNode.Descendants("div")
                        .FirstOrDefault(x => x.Attributes.Contains("class") &&
                                             x.Attributes["class"].Value == "les-content");
                    var ids = paslist.Descendants("a").Where(x => x.Attributes.Contains("data-id"));
                    var servers = paslist.Descendants("a").Where(x => x.Attributes.Contains("data-server"));
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
                        var test = $"https://gostream.is/ajax/movie_token?eid={id}&mid={servernum}";
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

                        test = $"https://gostream.is/ajax/movie_sources/{id}?x={param1}&y={param2}";

                        var fuck = await httpClient.GetStringAsync(test);
                        var source = JsonConvert.DeserializeObject<YMoviesSource>(fuck);

                        if (null != source)
                            foreach (var i in source?.playlist)
                            foreach (var s in i?.sources)
                                sources.Add(s?.file);
                    }
                }

                foreach (var source in sources)
                    if (source.Contains("google") || source.Contains("blogspot"))
                    {
                        var gv = new GoogleVideo(source);
                        gv.VideoSource = GetName();
                        list.Add(gv);
                    }
                    else
                    {
                        if (source.Length > 0 && !source.Contains("speedy.to"))
                        {
                            /*
                            GenericResolver gr = new GenericResolver(source);
                            gr.VideoSource = GetName();
                            list.Add(gr);
                            */
                        }
                    }
            }
            catch (Exception)
            {
            }

            return list;
        }

        public string GetName()
        {
            return "onemovies";
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
                var url = $"https://gostream.is/ajax/suggest_search";
                var httpClient = Utilities.GetHttpClient();
                var dict = new Dictionary<string, string>();
                dict.Add("keyword", search);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                var post = await httpClient.PostAsync(url, new FormUrlEncodedContent(dict));
                var result = await post.Content.ReadAsStringAsync();
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var stuff = JsonConvert.DeserializeObject<OneMoviesFirstResult>(result);
                var html = new HtmlDocument();
                html.LoadHtml(stuff.content);
                var divs = html.DocumentNode.Descendants("a").Where(
                    x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "ss-title");

                var dataUrl = "";

                foreach (var link in divs)
                {
                    {
                        {
                            var match = Regex.Match(link.InnerText, @"(.*?)\s+-\s+Season\s+(\d)");
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

                    url = $"https://gostream.is/ajax/movie_episodes/{servernum}";
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", dataUrl);
                    var data = await httpClient.GetStringAsync(url);
                    var serverresponse = JsonConvert.DeserializeObject<YMoviesServerResponse>(data);
                    var funk = new HtmlDocument();
                    funk.LoadHtml(serverresponse.html);
                    var paslist = funk.DocumentNode.Descendants("div")
                        .FirstOrDefault(x => x.Attributes.Contains("class") &&
                                             x.Attributes["class"].Value == "les-content");
                    var ids = paslist.Descendants("a").Where(x => x.Attributes.Contains("data-id"));
                    var servers = paslist.Descendants("a").Where(x => x.Attributes.Contains("data-server"));
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

                        var test = $"https://gostream.is/ajax/movie_token?eid={id}&mid={servernum}";
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

                        test = $"https://gostream.is/ajax/movie_sources/{id}?x={param1}&y={param2}";

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
                    gv.VideoSource = GetName();
                    list.Add(gv);
                }
            }
            catch (Exception)
            {
            }

            return list;
        }

        private class OneMoviesFirstResult
        {
            public int status { get; set; }
            public string message { get; set; }
            public string content { get; set; }
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