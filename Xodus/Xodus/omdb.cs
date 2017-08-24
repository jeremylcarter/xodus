using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Xodus
{
    public class OmdbApi
    {
        private readonly string OMDB_BASE = "http://www.omdbapi.com/?";

        public async Task<OmdbResponse> SearchMovieTitle(string title, int year = 0)
        {
            var query = "&type=movie&t=";
            query += Uri.EscapeDataString(title);

            if (year != 0)
                query += "&y=" + year;

            return await invokeApi(query);
        }

        public async Task<OmdbResponse> SearchTvTitle(string title, int year = 0)
        {
            var query = "&type=series&t=";
            query += Uri.EscapeDataString(title);

            if (year != 0)
                query += "&y=" + year;

            return await invokeApi(query);
        }

        public async Task<OmdbResponse> SearchMovieIMDB(string imdb, int year = 0)
        {
            var query = "&type=movie&i=";
            query += Uri.EscapeDataString(imdb);

            if (year != 0)
                query += "&y=" + year;

            return await invokeApi(query);
        }

        private async Task<OmdbResponse> invokeApi(string query)
        {
            var httpClient = Utilities.GetHttpClient();
            var encodedQuery = query;
            var uri = $"{OMDB_BASE}{encodedQuery}";
            var result = await httpClient.GetStringAsync(new Uri(uri));

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            return JsonConvert.DeserializeObject<OmdbResponse>(result, settings);
        }
    }
}