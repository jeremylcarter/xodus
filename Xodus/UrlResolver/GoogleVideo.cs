using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace UrlResolver
{
    public class GoogleVideo : IResolver
    {
        private readonly Dictionary<string, string> itag_map = new Dictionary<string, string>();

        private readonly string itag_map_const =
            "'5': '240', '6': '270', '17': '144', '18': '360', '22': '720', '34': '360', '35': '480'," +
            "'36': '240', '37': '1080', '38': '3072', '43': '360', '44': '480', '45': '720', '46': '1080'," +
            "'82': '360 [3D]', '83': '480 [3D]', '84': '720 [3D]', '85': '1080p [3D]', '100': '360 [3D]'," +
            "'101': '480 [3D]', '102': '720 [3D]', '92': '240', '93': '360', '94': '480', '95': '720'," +
            "'96': '1080', '132': '240', '151': '72', '133': '240', '134': '360', '135': '480'," +
            "'136': '720', '137': '1080', '138': '2160', '160': '144', '264': '1440'," +
            "'298': '720', '299': '1080', '266': '2160', '167': '360', '168': '480', '169': '720'," +
            "'170': '1080', '218': '480', '219': '480', '242': '240', '243': '360', '244': '480'," +
            "'245': '480', '246': '480', '247': '720', '248': '1080', '271': '1440', '272': '2160'," +
            "'302': '2160', '303': '1080', '308': '1440', '313': '2160', '315': '2160', '59': '480'";

        private readonly string url;

        public GoogleVideo(string uri)
        {
            url = uri;
            SourceName = "GVIDEO";

            var itags = itag_map_const.Split(',');

            foreach (var itag in itags)
            {
                var result = itag.Replace("'", "");
                var values = result.Split(':');
                itag_map.Add(values[0].Trim(), values[1].Trim());
            }

            VideoQuality = 2;

            if (uri.Contains("itag="))
            {
                var itag = uri.Substring(uri.IndexOf("itag=") + 5);
                itag = itag.Substring(0, itag.IndexOf("&"));
                var val = 0;

                if (int.TryParse(itag, out val))
                    switch (val)
                    {
                        case 22:
                        case 45:
                        case 84:
                        case 102:
                        case 95:
                        case 136:
                        case 298:
                        case 169:
                        case 247:
                            VideoQuality = 2;
                            break;

                        case 37:
                        case 38:
                        case 46:
                        case 85:
                        case 96:
                        case 137:
                        case 138:
                        case 264:
                        case 299:
                        case 266:
                        case 170:
                        case 248:
                        case 271:
                        case 272:
                        case 302:
                        case 303:
                        case 308:
                        case 313:
                        case 315:
                            VideoQuality = 3;
                            break;
                        default:
                            VideoQuality = 2;
                            break;
                    }
                else
                    VideoQuality = 2;
            }
        }

        /*
        22
        45
        84
        102
        95
        136
        298
        169
        247

        37
        38
        46
        85
        96
        137
        138
        264
        299
        266
        170
        248
        271
        272
        302
        303
        308
        313
        315
        */
        public bool isTV { get; set; }

        public string imdb { get; set; }
        public string VideoSource { get; set; }
        public string SourceName { get; set; }


        public string VideoSourceName
        {
            get
            {
                switch (VideoQuality)
                {
                    case 3:
                        return "1080P";
                    case 2:
                        return "HD";
                    case 1:
                        return "";
                    default:
                        return "";
                }
            }
            set { }
        }

        public int VideoQuality { get; set; }

        public async Task<string> GetMediaUrl()
        {
            Debug.WriteLine("GoogleVideo");
            try
            {
                var httpClient = Utilities.GetHttpClient();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Common.FF_USERAGENT);
                var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                    return response.RequestMessage.RequestUri.AbsoluteUri;
            }
            catch (Exception)
            {
            }

            return "";
        }

        public NavigationItem item { get; set; }
    }
}