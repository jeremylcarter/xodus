using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Web.Http.Filters;
using UrlResolver;

namespace Xodus
{
    public class Putlocker : IIndexer
    {
        public enum VideoQuality
        {
            SD,
            HD,
            HDP,
            UHD
        }

        private readonly string base_link = "https://cartoonhd.global";

        public async Task<List<IResolver>> GetMovieLink(string movie, int year)
        {
            var returnValue = new List<IResolver>();
            try
            {
                var httpBaseFilter = new HttpBaseProtocolFilter
                {
                    AllowAutoRedirect = true
                };

                var cleanMovie = CleanTitle.GetUrl(movie);
                var url = $"{base_link}/movie/{cleanMovie}";
                var cc = new CookieContainer();
                var hch = new HttpClientHandler();
                hch.CookieContainer = cc;
                Debug.WriteLine(hch.AllowAutoRedirect);
                var httpClient = new HttpClient(hch);

                var testurl = await httpClient.GetAsync(base_link);
                var realurl = "https://" + testurl.RequestMessage.RequestUri.Host;
                url = $"{realurl}/movie/{cleanMovie}";

                var response = await httpClient.GetAsync(url);
                var responseCookies = cc.GetCookies(new Uri(url)).Cast<Cookie>();

                var result = await response.Content.ReadAsStringAsync();

                var cookiename = "";
                var cookievalue = "";
                foreach (var c in responseCookies)
                {
                    cookiename = c.Name;
                    cookievalue = c.Value;
                }


                var action = "getMovieEmb";
                var t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                var secondsSinceEpoch = (int) t.TotalSeconds;
                var elid = Convert.ToBase64String(Encoding.UTF8.GetBytes(secondsSinceEpoch.ToString()));
                elid = Uri.EscapeDataString(elid);
                var er = new Regex(@"var\s+tok\s*=\s*'([^']+)");
                var tok = er.Match(result).Groups[1].Value;
                er = new Regex("elid\\s*=\\s*\"([^\"]+)");
                var idE1 = er.Match(result).Groups[1].Value;

                var post = new Dictionary<string, string>();
                post.Add("action", action);
                post.Add("idEl", idE1);
                post.Add("token", tok);
                post.Add("elid", elid);

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; IA64; Trident/7.0; rv:11.0) like Gecko");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", realurl);
                var shit = await httpClient.PostAsync($"{realurl}/ajax/tnembeds.php",
                    new FormUrlEncodedContent(post));
                var result2 = await shit.Content.ReadAsStringAsync();
                var links = new Regex("\'(http.+?)\'");
                var links1 = links.Matches(result2);
                links = new Regex("\"(http.+?)\"");
                var links2 = links.Matches(result2);

                var sources = new List<string>();

                foreach (Match link in links1)
                {
                    Debug.WriteLine(getQuality(link.Value));
                    sources.Add(link.Value.Replace("\\", "").Replace("\"", ""));
                }

                foreach (Match link in links2)
                {
                    Debug.WriteLine(getQuality(link.Value));
                    sources.Add(link.Value.Replace("\\", "").Replace("\"", ""));
                }


                foreach (var source in sources)
                {
                    Debug.WriteLine("PUTLOCKER: " + source);
                    var resolver = await Utilities.GetResolver(GetName(), source);
                    if (null != resolver)
                        returnValue.Add(resolver);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TEST");
            }

            return returnValue;
        }

        public string GetName()
        {
            return "putlocker";
        }

        public async Task<List<IResolver>> GetTvLink(string movie, int year, int season, int episode)
        {
            var returnValue = new List<IResolver>();

            try
            {
                var cleanmovie = CleanTitle.GetUrl(movie);
                var url = $"{base_link}/tv-show/{cleanmovie}/season/{season}/episode/{episode}";
                var httpClient = Utilities.GetHttpClient();

                var testurl = await httpClient.GetAsync(base_link);
                var realurl = "https://" + testurl.RequestMessage.RequestUri.Host;
                url = $"{realurl}/tv-show/{cleanmovie}/season/{season}/episode/{episode}";
                var response = await httpClient.GetAsync(url);
                var result = await response.Content.ReadAsStringAsync();


                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    var usShow = cleanmovie + "-us";
                    url = $"{realurl}/tv-show/{usShow}/season/{season}/episode/{episode}";
                    response = await httpClient.GetAsync(url);
                    result = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                        throw new InvalidOperationException("Show Not Found");
                }

                var newUri = response.RequestMessage.RequestUri + "/ajax/tnembeds.php";
                var action = "getMovieEmb";
                var t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                var secondsSinceEpoch = (int) t.TotalSeconds;
                var elid = Convert.ToBase64String(Encoding.UTF8.GetBytes(secondsSinceEpoch.ToString()));
                elid = Uri.EscapeDataString(elid);
                var er = new Regex(@"var\s+tok\s*=\s*'([^']+)");
                var tok = er.Match(result).Groups[1].Value;
                er = new Regex("elid\\s*=\\s*\"([^\"]+)");
                var idE1 = er.Match(result).Groups[1].Value;

                action = newUri.Contains("/episode/") ? "getEpisodeEmb" : "getMovieEmb";

                var post = new Dictionary<string, string>();
                post.Add("action", action);
                post.Add("idEl", idE1);
                post.Add("token", tok);
                post.Add("elid", elid);

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; IA64; Trident/7.0; rv:11.0) like Gecko");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", realurl);
                var shit = await httpClient.PostAsync($"{realurl}/ajax/tnembeds.php",
                    new FormUrlEncodedContent(post));
                var result2 = await shit.Content.ReadAsStringAsync();
                var links = new Regex("\'(http.+?)\'");
                var links1 = links.Matches(result2);
                links = new Regex("\"(http.+?)\"");
                var links2 = links.Matches(result2);

                var sources = new List<string>();

                foreach (Match link in links1)
                    sources.Add(link.Value.Replace("\\", "").Replace("\"", ""));

                foreach (Match link in links2)
                    sources.Add(link.Value.Replace("\\", "").Replace("\"", ""));


                foreach (var source in sources)
                {
                    Debug.WriteLine("PUTLOCKER: " + source);
                    var resolver = await Utilities.GetResolver(GetName(), source);
                    if (null != resolver)
                        returnValue.Add(resolver);
                }
            }
            catch (Exception)
            {
            }

            return returnValue;
        }

        public static VideoQuality getQuality(string url)
        {
            try
            {
                var re = new Regex("itag=(\\d*)");
                var quality = re.Matches(url);

                if (quality.Count == 0)
                {
                    re = new Regex("=m(\\d*)");
                    quality = re.Matches(url);
                }

                if (quality.Count > 0)
                {
                    var q = quality[0].Groups[1].Value;

                    string[] UHD = {"266", "272", "313"};
                    string[] HDP = {"264", "271", "37", "137", "299", "96", "248", "303", "46"};
                    string[] HD = {"15", "22", "84", "136", "298", "120", "95", "247", "302", "45", "102"};

                    if (UHD.ToList().Contains(q))
                        return VideoQuality.UHD;

                    if (HDP.ToList().Contains(q))
                        return VideoQuality.HDP;

                    if (HD.ToList().Contains(q))
                        return VideoQuality.HD;
                }
            }
            catch (Exception)
            {
            }

            return VideoQuality.SD;
        }
    }
}