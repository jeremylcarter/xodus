using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UrlResolver
{
    public class Vidzi : IResolver
    {
        public string uri;

        public Vidzi(string url)
        {
            uri = url;
            SourceName = "VIDZI";
        }

        public string VideoSource { get; set; }


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

        public string SourceName { get; set; }

        public int VideoQuality { get; set; }
        public bool isTV { get; set; }

        public string imdb { get; set; }
        public NavigationItem item { get; set; }

        public async Task<string> GetMediaUrl()
        {
            Debug.WriteLine("VIDZI");
            try
            {
                var mediaid = uri.Substring(uri.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
                var url = $"http://vidzi.tv/embed-{mediaid}";
                var httpClient = Utilities.GetHttpClient();
                var content = await httpClient.GetStringAsync(new Uri(url));
                var sr = new StringReader(content);

                var re = new Regex("(eval\\(function.*?)\n", RegexOptions.Singleline);
                var packed = re.Match(content);
                var p = new Unpacker();

                var s = "";
                try
                {
                    s = p.Unpack(packed.Groups[1].Value);
                }
                catch (Exception)
                {
                    s = content;
                }

                /*
                while (sr.Peek() != -1)
                {
                    var line = sr.ReadLine();
                    if (line.Contains("file:"))
                    {
                        if (line.Contains("m3u8"))
                            continue;
                        var fileUri = line.Substring(line.IndexOf("\"", StringComparison.OrdinalIgnoreCase));
                        fileUri = fileUri.Trim();
                        fileUri = fileUri.Replace("\"", "");
                        return fileUri;
                    }
                }
                */


                var re2 = new Regex("file\\s*:\\s*[\'|\"](http.+?)[\'|\"]", RegexOptions.Compiled);
                var s2 = re2.Matches(s)[0].Value;
                s2 = s2.Replace("file:", "");
                s2 = s2.Replace("\\", "");
                s2 = s2.Replace("\"", "");
                return s2;
            }
            catch (Exception)
            {
            }

            return "";
        }
    }
}