using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UrlResolver;

namespace Xodus
{
    public class Utilities
    {
        public enum SortDirection
        {
            Ascending,
            Descending
        }

        public static HttpClient GetHttpClient(DelegatingHandler handler = null)
        {
            HttpClient httpClient;

            if (handler != null)
                httpClient = new HttpClient(handler);
            else
                httpClient = new HttpClient();


            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-us,en;q=0.8");
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            return httpClient;
        }

        public static async Task<IResolver> GetResolver(string source, string shit2)
        {
            if (shit2.ToLower().Contains("google") || shit2.ToLower().Contains("blogspot"))
            {
                var gv = new GoogleVideo(shit2);
                gv.VideoSource = source;
                gv.VideoQuality = 2;

                if (string.IsNullOrEmpty(await gv.GetMediaUrl()))
                    return null;

                return gv;
            }


            if (shit2.ToLower().Contains("gorillavid.in") || shit2.Contains("gorillavid.com"))
            {
                var gv = new Gorillavid(shit2);
                gv.VideoQuality = 1;
                gv.VideoSource = source;
                return gv;
            }

            if (shit2.ToLower().Contains("daclips.in") || shit2.Contains("daclips.com"))
            {
                var da = new DaClips(shit2);
                da.VideoQuality = 1;
                da.VideoSource = source;
                return da;
            }

            if (shit2.ToLower().Contains("vidzi.tv"))
            {
                var vz = new Vidzi(shit2);
                vz.VideoSource = source;
                vz.VideoQuality = 1;
                return vz;
            }

            if (shit2.ToLower().Contains("streamin.to"))
            {
                var st = new StreaminTo(shit2);
                st.VideoSource = source;
                st.VideoQuality = 1;
                return st;
            }

            if (shit2.ToLower().Contains("thevideo.me"))
            {
                var tv = new TheVideo(shit2);
                tv.VideoSource = source;
                return tv;
            }

            if (shit2.ToLower().Contains("openload"))
            {
                var ol = new OpenLoad(shit2);
                ol.VideoSource = source;
                ol.VideoQuality = 2;
                return ol;
            }

            return null;
        }

        public static (string, int) GetTitle(string title)
        {
            var year = 0;
            if (title.LastIndexOf("(", StringComparison.Ordinal) != 0)
            {
                var yearString = title.Substring(title.LastIndexOf("(", StringComparison.Ordinal) + 1);
                var chars = yearString.ToCharArray();

                yearString = "";

                foreach (var ch in chars)
                    if (char.IsDigit(ch))
                        yearString += ch;

                if (yearString.Length >= 4)
                {
                    yearString = yearString.Substring(0, 4);
                    int.TryParse(yearString, out year);
                }
            }

            var re = new Regex("&#(\\d+);");
            title = re.Replace(title, "");
            re = new Regex("(&#[0-9]+)([^;^0-9]+)");
            title = re.Replace(title, "\\\\1;\\\\2");
            title = title.Replace("&quot;", "\"").Replace("&amp;", "&");
            re = new Regex(@"\n|([[].+?[]])|([(].+?[)])|\s(vs|v[.])\s|(:|;|-|,|\'|\.|\?)");
            title = re.Replace(title, "");
            return (title.Trim(), year);
        }
    }
}