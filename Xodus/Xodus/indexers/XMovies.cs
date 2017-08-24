using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CloudFlareUtilities;
using HtmlAgilityPack;
using Newtonsoft.Json;
using UrlResolver;

namespace Xodus
{
    public class Source
    {
        public string file { get; set; }
        public string type { get; set; }
        public string label { get; set; }
        public bool? @default { get; set; }
    }

    public class Track
    {
        public string file { get; set; }
        public string label { get; set; }
        public string kind { get; set; }
        public bool @default { get; set; }
    }

    public class Playlist
    {
        public List<Source> sources { get; set; }
        public List<Track> tracks { get; set; }
        public string image { get; set; }
    }

    public class XMoviesResponse
    {
        public List<Playlist> playlist { get; set; }
    }

    public class XMovies : IIndexer
    {
        private static readonly string base_link = "https://xmovies8.ru";

        public async Task<List<IResolver>> GetMovieLink(string movie, int year)
        {
            var list = new List<IResolver>();

            try
            {
                var cleantitle = movie.Replace("'", "-");
                var url = $"{base_link}/movies/search?s=" + Uri.EscapeDataString(cleantitle);
                var handler = new ClearanceHandler {MaxRetries = 2};
                var httpClient = new HttpClient(handler);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", url);
                var result = await httpClient.GetStringAsync(url);
                var html = new HtmlDocument();
                html.LoadHtml(result);
                var h = html.DocumentNode.Descendants("h2").Where(x => x.Attributes.Contains("class") &&
                                                                       x.Attributes["class"].Value == "tit");


                foreach (var res in h)
                {
                    var movieurl = "";

                    var a = res.Descendants("a")
                        .Where(x => x.Attributes.Contains("href") && x.Attributes.Contains("title"));

                    foreach (var link in a)
                    {
                        var href = link.Attributes["href"].Value;
                        var title = link.Attributes["title"].Value;

                        if (title.ToLower().Contains(movie.ToLower()) && title.ToLower().Contains(year.ToString()))
                        {
                            movieurl = href;
                            break;
                        }
                    }


                    if (!string.IsNullOrWhiteSpace(movieurl))
                    {
                        movieurl = movieurl.Replace("/watching.html", "");
                        movieurl = movieurl.TrimEnd('/');
                        movieurl = movieurl + "/watching.html";
                        var referer = movieurl;

                        var fuck = await httpClient.GetStringAsync(movieurl);
                        var shit = Regex.Matches(fuck, "load_player\\(.+?(\\d+)");
                        var id = shit[0].Groups[1].Value;
                        var dict = new Dictionary<string, string>();
                        dict.Add("id", id);

                        var posturl = $"https://xmovies8.ru/ajax/movie/load_player_v3?id={id}";

                        var client = Utilities.GetHttpClient();
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", referer);
                        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                        var postresult = await client.PostAsync(posturl, new FormUrlEncodedContent(dict));
                        var post = await postresult.Content.ReadAsStringAsync();
                        var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(post);
                        var value = json["value"] as string;

                        if (value == null)
                            throw new NullReferenceException();

                        var result2 = "";
                        if (!value.StartsWith("https://"))
                            result2 = "https:" + value;
                        else
                            result2 = value;

                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", referer);
                        var request = await httpClient.GetAsync(result2);
                        var test1 = request.RequestMessage.RequestUri.AbsoluteUri;

                        //   var res3 = await httpClient.GetAsync(test1);


                        if (!test1.Contains("xmovies8"))
                        {
                            var resolver = await Utilities.GetResolver(GetName(), test1);
                            if (null != resolver)
                            {
                                list.Add(resolver);
                                continue;
                            }
                        }

                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", referer);
                        var fuck2 = await httpClient.GetStringAsync(test1);

                        var fuck3 = Regex.Unescape(fuck2).Replace("\\", " ");

                        var settings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        };

                        var fuckoff = JsonConvert.DeserializeObject<XFuck>(fuck3, settings);
                        var cock = 0;


                        foreach (var x in fuckoff.playlist)
                        {
                            var gr = new GenericResolver(x.file);
                            gr.VideoSource = GetName();
                            list.Add(gr);
                        }
                    }
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
            return "xmovies";
        }

        public async Task<List<IResolver>> GetTvLink(string movie, int year, int season, int episode)
        {
            var list = new List<IResolver>();

            try
            {
                var cleantitle = movie.Replace("'", "-");
                var t = CleanTitle.Get(cleantitle);
                t = t.Replace("\'", "-");
                var themovie = $"{t} Season " + season;
                var url = $"{base_link}/movies/search?s=" + Uri.EscapeDataString(themovie);
                var handler = new ClearanceHandler {MaxRetries = 2};
                var httpClient = new HttpClient(handler);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", url);
                var result = await httpClient.GetStringAsync(url);
                var html = new HtmlDocument();
                html.LoadHtml(result);
                var h = html.DocumentNode.Descendants("h2").Where(x => x.Attributes.Contains("class") &&
                                                                       x.Attributes["class"].Value == "tit");


                foreach (var res in h)
                {
                    var movieurl = "";

                    var a = res.Descendants("a")
                        .Where(x => x.Attributes.Contains("href") && x.Attributes.Contains("title"));

                    foreach (var link in a)
                    {
                        var href = link.Attributes["href"].Value;
                        var title = link.Attributes["title"].Value;

                        if (title.ToLower().Contains(movie.ToLower()) &&
                            title.ToLower().Contains("season " + season))
                        {
                            movieurl = href;
                            break;
                        }
                    }


                    if (!string.IsNullOrWhiteSpace(movieurl))
                    {
                        movieurl = movieurl.Replace("/watching.html", "");
                        movieurl = movieurl.TrimEnd('/');
                        movieurl = movieurl + "/watching.html";
                        var referer = movieurl;

                        var fuck = await httpClient.GetStringAsync(movieurl);

                        var episodeDoc = new HtmlDocument();
                        episodeDoc.LoadHtml(fuck);
                        var res2 = episodeDoc.DocumentNode.Descendants("div").Where(
                            x => x.Attributes.Contains("class") &&
                                 x.Attributes["class"].Value.Contains("ep_link"));

                        foreach (var n in res2)
                        {
                            var anc = n.Descendants("a").FirstOrDefault(x => x.Attributes.Contains("href") &&
                                                                             x.InnerText.ToLower()
                                                                                 .Contains("episode " +
                                                                                           episode.ToString()));
                            movieurl = anc.Attributes["href"].Value;
                            fuck = await httpClient.GetStringAsync(movieurl);
                        }

                        var shit = Regex.Matches(fuck, "load_player\\(.+?(\\d+)");
                        var id = shit[0].Groups[1].Value;
                        var dict = new Dictionary<string, string>();
                        dict.Add("id", id);

                        var posturl = $"https://xmovies8.ru/ajax/movie/load_player_v3";
                        var client = Utilities.GetHttpClient();
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", referer);
                        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                        var postresult = await client.PostAsync(posturl, new FormUrlEncodedContent(dict));
                        var post = await postresult.Content.ReadAsStringAsync();
                        var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(post);
                        var value = json["value"] as string;

                        if (value == null)
                            throw new NullReferenceException();

                        var result2 = "";
                        if (!value.StartsWith("https://"))
                            result2 = "https:" + value;
                        else
                            result2 = value;

                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", referer);
                        var request = await httpClient.GetAsync(result2);
                        var test1 = request.RequestMessage.RequestUri.AbsoluteUri;

                        var resolver = await Utilities.GetResolver(GetName(), test1);
                        if (null != resolver)
                        {
                            list.Add(resolver);
                            continue;
                        }

                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", referer);
                        var fuck2 = await httpClient.GetStringAsync(test1);

                        var settings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        };

                        var fuckoff = JsonConvert.DeserializeObject<XFuck>(fuck2, settings);
                        foreach (var playlist in fuckoff.playlist)
                        {
                            var gv = new GenericResolver(playlist.file);
                            gv.VideoSource = GetName();
                            list.Add(gv);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return list;
        }
    }


    public class XPlaylist
    {
        public string file { get; set; }
    }

    public class Fucked
    {
        public List<XPlaylist> playlist { get; set; }
        public List<List<object>> tracks { get; set; }
    }

    internal class XMoviesFile
    {
        public string file { get; set; }
    }

    internal class XMoviesList
    {
        public List<XMoviesFile> playlist { get; set; }
    }

    public class XFuckPlaylist
    {
        public string file { get; set; }
    }

    public class XFuckTrack
    {
        public string file { get; set; }
        public string label { get; set; }
        public string kind { get; set; }
        public bool @default { get; set; }
    }

    public class XFuck
    {
        public List<XFuckPlaylist> playlist { get; set; }
        public List<object> tracks { get; set; }
    }
}