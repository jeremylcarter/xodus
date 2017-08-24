using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace Xodus
{
    public class Tmdb
    {
        private static readonly string API_KEY = "78d8b4fbaea42faf2820200c9198807e";
        private static readonly string IMAGE_LINK = "https://image.tmdb.org/t/p/";
        private static readonly string BASE_LINK = "https://api.themoviedb.org/3/movie/{0}/images?api_key=" + API_KEY;
        private static readonly string MOVIE_LINK = "https://api.themoviedb.org/3/movie/{0}?api_key=" + API_KEY;
        private static readonly string TVDB_LINK = "http://thetvdb.com/api/GetSeriesByRemoteID.php?imdbid=";
        private static readonly string FANART_KEY = "api_key=41eff6a348318bbc3f754054e0ff37c5";
        private static readonly string FANART_URL = "http://webservice.fanart.tv/v3/tv/";

        public async Task<(string, string)> GetPoster(string imdb, CancellationTokenSource cts, bool isTV = false,
            int season = 0)
        {
            var url = "";
            var url2 = "";
            var description = "";
            try
            {
                if (!isTV)
                {
                    url2 = string.Format(MOVIE_LINK, imdb);
                    var httpClient = Utilities.GetHttpClient();

                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    var response2 = await httpClient.GetAsync(url2, cts.Token);

                    var content = await response2.Content.ReadAsStringAsync();

                    var data = JsonConvert.DeserializeObject<TmdbMovieResponse>(content, settings);

                    if (null != data)
                    {
                        var path = data.poster_path;
                        var width = "w" + "500"; // response.backdrops[0].height.ToString();
                        var image = $"{IMAGE_LINK}{width}{path}";
                        url = image;
                    }

                    var fixup = data.overview;

                    if (fixup.Length > 400)
                    {
                        fixup = data.overview.Substring(0, 400);
                        fixup += "...";
                    }
                    return (fixup, url);
                }

                else
                {
                    var testurl = $"{TVDB_LINK}{imdb}";
                    var httpClient = Utilities.GetHttpClient();
                    var response2 = await httpClient.GetAsync(testurl, cts.Token);

                    var content = await response2.Content.ReadAsStringAsync();
                    var xml = new XmlDocument();
                    xml.LoadXml(content);
                    var shit = xml.GetElementsByTagName("seriesid");
                    var desc = xml.GetElementsByTagName("Overview");
                    description = desc[0].InnerText;

                    var fixup = description;

                    if (fixup.Length > 400)
                    {
                        fixup = description.Substring(0, 400);
                        fixup += "...";
                    }

                    description = fixup;

                    var id = shit[0].InnerText;
                    testurl = $"{FANART_URL}{id}?{FANART_KEY}";
                    content = await httpClient.GetStringAsync(testurl);
                    var resp = JsonConvert.DeserializeObject<FanartResponse>(content);

                    if (season > 0)
                        if (resp?.seasonposter.Count >= season)
                        {
                            var seasonList = resp?.seasonposter?.Where(x => int.Parse(x.season) > 0)
                                .OrderBy(x => int.Parse(x.season)).ToList();
                            url = seasonList.FirstOrDefault(x => int.Parse(x.season) == season && x.lang == "en")?.url;
                        }

                    if (string.IsNullOrWhiteSpace(url))
                        if (resp?.tvposter?.Count > 0)
                            url = resp.tvposter[0].url;
                }
            }
            catch (Exception)
            {
                url = "";
            }
            return (description, url);
        }

        public async Task<string> GetImdbBackdrop(string imdb, CancellationTokenSource cts, bool isTV = false)
        {
            var url = "";
            try
            {
                if (!isTV)
                {
                    url = string.Format(BASE_LINK, imdb);
                    var httpClient = Utilities.GetHttpClient();
                    var response2 = await httpClient.GetAsync(url, cts.Token);

                    var content = await response2.Content.ReadAsStringAsync();

                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    var response = JsonConvert.DeserializeObject<TmdbResponse>(content, settings);

                    if (null != response)
                    {
                        var path = response.backdrops[0].file_path;
                        var width = "w" + "1920"; // response.backdrops[0].height.ToString();
                        var image = $"{IMAGE_LINK}{width}{path}";
                        url = image;
                    }
                }
                else
                {
                    var testurl = $"{TVDB_LINK}{imdb}";
                    var httpClient = Utilities.GetHttpClient();
                    var response2 = await httpClient.GetAsync(testurl, cts.Token);

                    var content = await response2.Content.ReadAsStringAsync();

                    var xml = new XmlDocument();
                    xml.LoadXml(content);
                    var shit = xml.GetElementsByTagName("seriesid");
                    var id = shit[0].InnerText;
                    testurl = $"{FANART_URL}{id}?{FANART_KEY}";
                    content = await httpClient.GetStringAsync(testurl);
                    var resp = JsonConvert.DeserializeObject<FanartResponse>(content);


                    if (resp.showbackground?.Count > 0)
                    {
                        url = resp.showbackground[0].url;
                    }
                    else
                    {
                        if (resp.tvthumb?.Count > 0)
                            url = resp.tvthumb[0].url;
                    }
                }
            }
            catch (Exception)
            {
                url = "";
            }
            return url;
        }

        public class Backdrop
        {
            public double aspect_ratio { get; set; }
            public string file_path { get; set; }
            public int height { get; set; }
            public string iso_639_1 { get; set; }
            public double vote_average { get; set; }
            public int vote_count { get; set; }
            public int width { get; set; }
        }

        public class Poster
        {
            public double aspect_ratio { get; set; }
            public string file_path { get; set; }
            public int height { get; set; }
            public string iso_639_1 { get; set; }
            public double vote_average { get; set; }
            public int vote_count { get; set; }
            public int width { get; set; }
        }

        public class TmdbResponse
        {
            public int id { get; set; }
            public List<Backdrop> backdrops { get; set; }
            public List<Poster> posters { get; set; }
        }
    }
}