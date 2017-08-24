using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using UrlResolver;

namespace Xodus
{
    public class Plocker : IIndexer
    {
        public static readonly string base_link = "https://putlocker.rs";

        public static readonly string movie_search_path =
            "/filter?keyword=%s&sort=post_date:Adesc&type[]=movie&release[]=%s";

        public async Task<List<IResolver>> GetMovieLink(string movie, int year)
        {
            Debug.WriteLine("Plocker - Movies");
            var sources = new List<IResolver>();

            try
            {
                var clean_title = CleanTitle.GetUrl(movie);
                var query =
                    $"{base_link}/filter?keyword={clean_title}&sort=post_date:Adesc&type[]=movie&release[]={year}";

                var httpWeb = Utilities.GetHttpClient();
                httpWeb.Timeout = TimeSpan.FromSeconds(30);
                httpWeb.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Common.FF_USERAGENT);
                var response2 = await httpWeb.GetAsync(query).Result.Content.ReadAsStringAsync();
                var response = new HtmlDocument();
                response.LoadHtml(response2);
                var results_list = response.DocumentNode.Descendants("div").FirstOrDefault(
                    x => x.Attributes.Contains("class") &&
                         x.Attributes["class"].Value == "item");

                var re = new Regex("(\\/watch\\/)([^\\\"]*)");
                var match = re.Matches(results_list.InnerHtml);

                var film_id = match[0].Groups[2].Value;

                query = $"{base_link}/watch/{film_id}";

                var film_response2 = await httpWeb.GetAsync(query).Result.Content.ReadAsStringAsync();
                var film_response = new HtmlDocument();
                film_response.LoadHtml(film_response2);
                re = new Regex("(data-ts=\\\")(.*?)(\\\">)");
                var ts = re.Matches(film_response.DocumentNode.InnerHtml)[0].Groups[2].Value;

                var sources_dom_list = film_response.DocumentNode.Descendants("ul")
                    .Where(x => x.Attributes.Contains("class"))
                    .Where(x => x.Attributes["class"].Value == "episodes range active");

                var sources_list = new List<string>();

                re = new Regex("([\\/])(.{0,6})(\\\">)");

                foreach (var sources_dom in sources_dom_list)
                {
                    var source_id = re.Matches(sources_dom.InnerHtml)[0].Groups[2].Value;
                    sources_list.Add(source_id);
                }

                foreach (var source in sources_list)
                {
                    re = new Regex("[^', u\\]\\[]+");
                    var dictionary = new Dictionary<string, string>();
                    dictionary.Add("id", source);
                    dictionary.Add("update", "0");
                    dictionary.Add("ts", ts);
                    var token2 = GetToken(dictionary);

                    query = $"{base_link}/ajax/episode/info?ts={ts}&_={token2}&id={source}&update=0";

                    var httpClient = Utilities.GetHttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(30);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    var info_response = await httpClient.GetStringAsync(query);

                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    var grabber_dict = JsonConvert.DeserializeObject<GrabberDict>(info_response, settings);

                    if (grabber_dict.type == "direct")
                    {
                        var token64 = grabber_dict.@params.token;
                        var newQuery = $"/grabber-api/?ts={ts}&id={source}&token={token64}&mobile=0";
                        var super = base_link + newQuery;
                        var response3 = await httpClient.GetStringAsync(super);
                        var grabbersource = JsonConvert.DeserializeObject<GrabberSource>(response3);

                        foreach (var link in grabbersource.data)
                        {
                            var qual = Putlocker.getQuality(link.file);

                            var gv = new GoogleVideo(link.file);

                            if (qual == Putlocker.VideoQuality.UHD)
                                gv.VideoQuality = 3;
                            else
                                gv.VideoQuality = qual == Putlocker.VideoQuality.SD ? 2 : 2;

                            gv.VideoSource = GetName();
                            sources.Add(gv);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return sources;
        }

        public string GetName()
        {
            return "plocker";
        }

        public async Task<List<IResolver>> GetTvLink(string movie, int year, int season, int episode)
        {
            var resolvers = new List<IResolver>();
            try
            {
                var clean_title = CleanTitle.GetUrl(movie);
                var query = $"{base_link}/filter?keyword={clean_title}&sort=post_date:Adesc&type[]=series";

                var httpWeb = Utilities.GetHttpClient();
                httpWeb.Timeout = TimeSpan.FromSeconds(30);
                var search_response2 = await httpWeb.GetAsync(query).Result.Content.ReadAsStringAsync();
                var search_response = new HtmlDocument();
                search_response.LoadHtml(search_response2);
                var results_list = search_response.DocumentNode.Descendants("div")
                    .FirstOrDefault(x => x.Attributes.Contains("class") &&
                                         x.Attributes["class"].Value == "items");

                var film_tries = new List<string>();

                film_tries.Add(@"\/" + clean_title + "-0" + season + "[^-0-9](.+?)\\\"");
                film_tries.Add(@"\/" + clean_title + "-" + season + "[^-0-9](.+?)\\\"");
                film_tries.Add(@"\/" + clean_title + "[^-0-9](.+?)\\\"");

                Match film_id = null;

                Regex re = null;

                foreach (var exp in film_tries)
                {
                    re = new Regex(exp);
                    film_id = re.Match(results_list.InnerHtml);

                    if (film_id.Success)
                        break;
                }

                var filmid = film_id?.Value;

                filmid = filmid?.Substring(0, filmid.Length - 1);

                query = $"https://putlocker.rs/watch{filmid}";

                var film_response2 = await httpWeb.GetAsync(query).Result.Content.ReadAsStringAsync();
                var film_response = new HtmlDocument();
                film_response.LoadHtml(film_response2);

                re = new Regex("(data-ts=\\\")(.*?)(\\\">)");
                var ts = re.Matches(film_response.DocumentNode.InnerHtml)[0].Groups[2].Value;

                var sources_dom_list = film_response.DocumentNode.Descendants("ul")
                    .Where(x => x.Attributes.Contains("class"))
                    .Where(x => x.Attributes["class"].Value == "episodes range active");

                var sources_list = new List<string>();

                re = new Regex("([^\\/]*)\\\">" + episode + "[^0-9]");

                var stringEpisode = episode.ToString();
                foreach (var x in sources_dom_list)
                {
                    if (!re.IsMatch(x.InnerHtml))
                        stringEpisode = episode.ToString("00");
                    break;
                }


                re = new Regex("([^\\/]*)\\\">" + stringEpisode + "[^0-9]");

                foreach (var sources_dom in sources_dom_list)
                {
                    var source_id = re.Matches(sources_dom.InnerHtml)[0].Groups[1].Value;
                    sources_list.Add(source_id);
                }

                foreach (var source in sources_list)
                {
                    re = new Regex("[^', u\\]\\[]+");
                    var dictionary = new Dictionary<string, string>();
                    dictionary.Add("id", source);
                    dictionary.Add("update", "0");
                    dictionary.Add("ts", ts);
                    var token2 = GetToken(dictionary);
                    query = $"{base_link}/ajax/episode/info?ts={ts}&_={token2}&id={source}&update=0";

                    var httpClient = Utilities.GetHttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(30);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    var info_response = await httpClient.GetStringAsync(query);

                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    var grabber_dict = JsonConvert.DeserializeObject<GrabberDict>(info_response, settings);

                    if (grabber_dict.type == "direct")
                    {
                        var token64 = grabber_dict.@params.token;
                        var newQuery = $"/grabber-api/?ts={ts}&id={source}&token={token64}&mobile=0";
                        var super = base_link + newQuery;
                        var response2 = await httpClient.GetStringAsync(super);
                        var grabbersource = JsonConvert.DeserializeObject<GrabberSource>(response2);

                        foreach (var link in grabbersource.data)
                        {
                            var qual = Putlocker.getQuality(link.file);

                            var gv = new GoogleVideo(link.file);

                            if (qual == Putlocker.VideoQuality.UHD)
                                gv.VideoQuality = 3;
                            else
                                gv.VideoQuality = qual == Putlocker.VideoQuality.SD ? 2 : 2;

                            resolvers.Add(gv);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return resolvers;
        }

        private int GetToken(Dictionary<string, string> sources)
        {
            var token = 0;

            //int[] d = new int[] { id, update, ts };
            var u = 0;
            while (u < 3)
            {
                var s = 0;
                var o = 0;
                var r = 0;

                var list = new List<int>();
                for (var z = 0; z < 256; z++)
                    list.Add(z);

                var i = list.ToArray();
                var n = 0;
                var a = 0;
                var j = "";

                if (u == 0)
                    j = "id";

                if (u == 1)
                    j = "update";

                if (u == 2)
                    j = "ts";

                var e = sources[j];

                for (var t = 0; t < 256; t++)
                {
                    n = (n + i[t] + j[t % j.Length]) % 256;

                    r = i[t];
                    i[t] = i[n];
                    i[n] = r;
                }

                s = 0;
                n = 0;

                for (o = 0; o < e.Length; o++)
                {
                    s = (s + 1) % 256;
                    n = (n + i[s]) % 256;

                    r = i[s];
                    i[s] = i[n];
                    i[n] = r;

                    a += e[o] ^ (i[(i[s] + i[n]) % 256] * o + o);
                }

                u++;
                token += a;
            }

            return token;
        }
    }
}