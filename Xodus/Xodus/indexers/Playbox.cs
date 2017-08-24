using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UrlResolver;

namespace Xodus
{
    public class Playbox : IIndexer
    {
        private readonly string base_link = "http://playboxhd.com";
        private readonly string search_link = "/api/box?type=search&os=Android&v=291.0&k=0&keyword=";
        private readonly string sources_link = "/api/box?type=detail&id=";
        private readonly string stream_link = "/api/box?type=stream&id=";

        public async Task<List<IResolver>> GetMovieLink(string movie, int year)
        {
            var list = new List<IResolver>();

            try
            {
                ;
                var title = Uri.EscapeDataString(movie);

                var url = $"{base_link}{search_link}{movie}";

                var httpClient = Utilities.GetHttpClient();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Apple-iPhone/701.341");
                var result = await httpClient.GetStringAsync(url);
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var json = JsonConvert.DeserializeObject<PlayboxResponse>(result, settings);

                PlayboxFilm pf = null;

                foreach (var data in json?.data?.films)
                {
                    var publishDate = int.Parse(data.publishDate);
                    if (publishDate == year)
                    {
                        pf = data;
                        break;
                    }
                }

                if (null == pf)
                    return list;

                url = $"{base_link}{sources_link}{pf.id}";
                url += "&os=Android&v=291.0&k=0&al=key";

                result = await httpClient.GetStringAsync(url);

                var json2 = JsonConvert.DeserializeObject<SourceResponse>(result, settings);

                var id = json2?.data?.chapters[0]?.id;

                url = $"{base_link}{stream_link}{id}";
                url += "&os=Android&v=291.0";

                result = await httpClient.GetStringAsync(url);


                var json3 = JsonConvert.DeserializeObject<StreamResponse>(result, settings);

                foreach (var datum in json3.data)
                {
                    var stream = datum.stream;
                    var enc = Convert.FromBase64String(stream);
                    var a = new AesEnDecryption();
                    var n = a.Decrypt(enc);
                    var z = Encoding.UTF8.GetString(n);
                    var gv = await Utilities.GetResolver(GetName(), z);
                    Debug.WriteLine(datum.quality);

                    gv.VideoQuality = 1;

                    if (datum.quality.Contains("1080"))
                        gv.VideoQuality = 3;

                    if (datum.quality.Contains("720"))
                        gv.VideoQuality = 2;


                    if (null != gv)
                    {
                        if (gv is GoogleVideo)
                            if (string.IsNullOrEmpty(await gv.GetMediaUrl()))
                                continue;

                        list.Add(gv);
                    }
                }


                var q = 0;
            }
            catch (Exception ex)
            {
                var x = 0;
            }

            return list;
        }

        public string GetName()
        {
            return "playbox";
        }

        public async Task<List<IResolver>> GetTvLink(string movie, int year, int season, int episode)
        {
            var list = new List<IResolver>();
            return list;
        }


        public class PlayboxFilm
        {
            public int id { get; set; }
            public string title { get; set; }
            public string poster { get; set; }
            public double score { get; set; }
            public string publishDate { get; set; }
            public string pg { get; set; }
            public string imdb { get; set; }
            public string description { get; set; }
        }

        public class Data
        {
            public List<PlayboxFilm> films { get; set; }
            public string more { get; set; }
        }

        public class PlayboxResponse
        {
            public int result { get; set; }
            public string msg { get; set; }
            public Data data { get; set; }
            public string cf { get; set; }
        }

        public class Chapter
        {
            public int id { get; set; }
            public string title { get; set; }
            public List<object> streams { get; set; }
            public string subscene { get; set; }
            public int order { get; set; }
        }

        public class ChapData
        {
            public int id { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string poster { get; set; }
            public string genre { get; set; }
            public string state { get; set; }
            public bool kidmode { get; set; }
            public List<Chapter> chapters { get; set; }
            public bool isMusic { get; set; }
            public List<string> images { get; set; }
        }

        public class SourceResponse
        {
            public int result { get; set; }
            public string msg { get; set; }
            public ChapData data { get; set; }
            public string cf { get; set; }
        }

        public class Datum
        {
            public string server { get; set; }
            public string stream { get; set; }
            public string quality { get; set; }
            public int parseType { get; set; }
            public object e { get; set; }
        }

        public class StreamResponse
        {
            public int result { get; set; }
            public string msg { get; set; }
            public List<Datum> data { get; set; }
            public string cf { get; set; }
        }
    }
}