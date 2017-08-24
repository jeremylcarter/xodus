using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using HtmlAgilityPack;

namespace Xodus
{
    public class Subscene
    {
        private readonly string base_link = "https://www.subscene.com";


        private readonly string EPISODE_PATTERN =
                @"<a href=\""(?<link>/subtitles/[^\""]*)\"">(?<title>[^<]+)\((?<year>\d{4})\)</a>\s+</div>\s+<div class=\""subtle count\"">\s+(?<numsubtitles>\d+)"
            ;

        private readonly string[] seasons =
        {
            "Specials", "First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Ninth", "Tenth",
            "Eleventh", "Twelfth", "Thirteenth", "Fourteenth", "Fifteenth", "Sixteenth", "Seventeenth",
            "Eighteenth", "Nineteenth", "Twentieth", "Twenty-first", "Twenty-second", "Twenty-third", "Twenty-fourth",
            "Twenty-fifth", "Twenty-sixth",
            "Twenty-seventh", "Twenty-eighth", "Twenty-ninth"
        };


        private readonly string SUBTITLE_PATTERN =
                @"<td class=\""a1\"">\s+<a href=\""(?<link>/subtitles/[^\""]+)\"">\s+<span class=\""[^\""]+ (?<quality>\w+-icon)\"">\s+(?<language>[^\r\n\t]+)\s+</span>\s+<span>\s+(?<filename>[^\r\n\t]+)\s+</span>\s+</a>\s+</td>\s+<td class=\""[^\""]+\"">\s+(?<numfiles>[^\r\n\t]*)\s+</td>\s+<td class=\""(?<hiclass>[^\""]+)\"">(?:.*?)<td class=\""a6\"">\s+<div>\s+(?<comment>[^\""]+)&nbsp;\s*</div>"
            ;

        private string lang;

        public Dictionary<string, Dictionary<string, object>> SupportedLanguages =
            new Dictionary<string, Dictionary<string, object>>();

        public Subscene()
        {
            SupportedLanguages.Add("Albanian",
                new Dictionary<string, object>
                {
                    {"id", 1},
                    {"3let", "alb"},
                    {"2let", "sq"},
                    {"name", "Albanian"}
                });
            SupportedLanguages.Add("Arabic",
                new Dictionary<string, object>
                {
                    {"id", 2},
                    {"3let", "ara"},
                    {"2let", "ar"},
                    {"name", "Arabic"}
                });

            SupportedLanguages.Add("Big 5 code",
                new Dictionary<string, object> {{"id", 3}, {"3let", "chi"}, {"2let", "zh"}, {"name", "Chinese"}});
            SupportedLanguages.Add("Brazillian Portuguese",
                new Dictionary<string, object>
                {
                    {"id", 4},
                    {"3let", "por"},
                    {"2let", "pb"},
                    {"name", "Brazilian Portuguese"}
                });
            SupportedLanguages.Add("Bulgarian",
                new Dictionary<string, object> {{"id", 5}, {"3let", "bul"}, {"2let", "bg"}, {"name", "Bulgarian"}});
            SupportedLanguages.Add("Chinese BG code",
                new Dictionary<string, object> {{"id", 7}, {"3let", "chi"}, {"2let", "zh"}, {"name", "Chinese"}});
            SupportedLanguages.Add("Croatian",
                new Dictionary<string, object> {{"id", 8}, {"3let", "hrv"}, {"2let", "hr"}, {"name", "Croatian"}});
            SupportedLanguages.Add("Czech",
                new Dictionary<string, object> {{"id", 9}, {"3let", "cze"}, {"2let", "cs"}, {"name", "Czech"}});
            SupportedLanguages.Add("Danish",
                new Dictionary<string, object> {{"id", 10}, {"3let", "dan"}, {"2let", "da"}, {"name", "Danish"}});
            SupportedLanguages.Add("Dutch",
                new Dictionary<string, object> {{"id", 11}, {"3let", "dut"}, {"2let", "nl"}, {"name", "Dutch"}});
            SupportedLanguages.Add("English",
                new Dictionary<string, object> {{"id", 13}, {"3let", "eng"}, {"2let", "en"}, {"name", "English"}});
            SupportedLanguages.Add("Estonian",
                new Dictionary<string, object> {{"id", 16}, {"3let", "est"}, {"2let", "et"}, {"name", "Estonian"}});
            SupportedLanguages.Add("Farsi/Persian",
                new Dictionary<string, object> {{"id", 46}, {"3let", "per"}, {"2let", "fa"}, {"name", "Persian"}});
            SupportedLanguages.Add("Finnish",
                new Dictionary<string, object> {{"id", 17}, {"3let", "fin"}, {"2let", "fi"}, {"name", "Finnish"}});
            SupportedLanguages.Add("French",
                new Dictionary<string, object> {{"id", 18}, {"3let", "fre"}, {"2let", "fr"}, {"name", "French"}});
            SupportedLanguages.Add("German",
                new Dictionary<string, object> {{"id", 19}, {"3let", "ger"}, {"2let", "de"}, {"name", "German"}});
            SupportedLanguages.Add("Greek",
                new Dictionary<string, object> {{"id", 21}, {"3let", "gre"}, {"2let", "el"}, {"name", "Greek"}});
            SupportedLanguages.Add("Hebrew",
                new Dictionary<string, object> {{"id", 22}, {"3let", "heb"}, {"2let", "he"}, {"name", "Hebrew"}});
            SupportedLanguages.Add("Hungarian",
                new Dictionary<string, object> {{"id", 23}, {"3let", "hun"}, {"2let", "hu"}, {"name", "Hungarian"}});
            SupportedLanguages.Add("Icelandic",
                new Dictionary<string, object> {{"id", 25}, {"3let", "ice"}, {"2let", "is"}, {"name", "Icelandic"}});
            SupportedLanguages.Add("Indonesian",
                new Dictionary<string, object> {{"id", 44}, {"3let", "ind"}, {"2let", "id"}, {"name", "Indonesian"}});
            SupportedLanguages.Add("Italian",
                new Dictionary<string, object> {{"id", 26}, {"3let", "ita"}, {"2let", "it"}, {"name", "Italian"}});
            SupportedLanguages.Add("Japanese",
                new Dictionary<string, object> {{"id", 27}, {"3let", "jpn"}, {"2let", "ja"}, {"name", "Japanese"}});
            SupportedLanguages.Add("Korean",
                new Dictionary<string, object> {{"id", 28}, {"3let", "kor"}, {"2let", "ko"}, {"name", "Korean"}});
            SupportedLanguages.Add("Lithuanian",
                new Dictionary<string, object> {{"id", 43}, {"3let", "lit"}, {"2let", "lt"}, {"name", "Lithuanian"}});
            SupportedLanguages.Add("Malay",
                new Dictionary<string, object> {{"id", 50}, {"3let", "may"}, {"2let", "ms"}, {"name", "Malay"}});
            SupportedLanguages.Add("Norwegian",
                new Dictionary<string, object> {{"id", 30}, {"3let", "nor"}, {"2let", "no"}, {"name", "Norwegian"}});
            SupportedLanguages.Add("Polish",
                new Dictionary<string, object> {{"id", 31}, {"3let", "pol"}, {"2let", "pl"}, {"name", "Polish"}});
            SupportedLanguages.Add("Portuguese",
                new Dictionary<string, object> {{"id", 32}, {"3let", "por"}, {"2let", "pt"}, {"name", "Portuguese"}});
            SupportedLanguages.Add("Romanian",
                new Dictionary<string, object> {{"id", 33}, {"3let", "rum"}, {"2let", "ro"}, {"name", "Romanian"}});
            SupportedLanguages.Add("Russian",
                new Dictionary<string, object> {{"id", 34}, {"3let", "rus"}, {"2let", "ru"}, {"name", "Russian"}});
            SupportedLanguages.Add("Serbian",
                new Dictionary<string, object> {{"id", 35}, {"3let", "scc"}, {"2let", "sr"}, {"name", "Serbian"}});
            SupportedLanguages.Add("Slovak",
                new Dictionary<string, object> {{"id", 36}, {"3let", "slo"}, {"2let", "sk"}, {"name", "Slovak"}});
            SupportedLanguages.Add("Slovenian",
                new Dictionary<string, object> {{"id", 37}, {"3let", "slv"}, {"2let", "sl"}, {"name", "Slovenian"}});
            SupportedLanguages.Add("Spanish",
                new Dictionary<string, object> {{"id", 38}, {"3let", "spa"}, {"2let", "es"}, {"name", "Spanish"}});
            SupportedLanguages.Add("Swedish",
                new Dictionary<string, object> {{"id", 39}, {"3let", "swe"}, {"2let", "sv"}, {"name", "Swedish"}});
            SupportedLanguages.Add("Thai",
                new Dictionary<string, object> {{"id", 40}, {"3let", "tha"}, {"2let", "th"}, {"name", "Thai"}});
            SupportedLanguages.Add("Turkish",
                new Dictionary<string, object> {{"id", 41}, {"3let", "tur"}, {"2let", "tr"}, {"name", "Turkish"}});
            SupportedLanguages.Add("Vietnamese",
                new Dictionary<string, object> {{"id", 45}, {"3let", "vie"}, {"2let", "vi"}, {"name", "Vietnamese"}});
        }


        private async Task<(string, string)> GetUrlString(string url, string cookies = null)
        {
            Debug.WriteLine("Subscene: Getting URL: " + url);
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.11; rv:41.0) Gecko/20100101 Firefox/41.0");

            if (!string.IsNullOrWhiteSpace(cookies))
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", cookies);

            var result = await httpClient.GetAsync(url);

            if (!result.IsSuccessStatusCode)
                throw new Exception();

            var content = await result.Content.ReadAsStringAsync();
            var returnuri = result.RequestMessage.RequestUri.AbsoluteUri;

            return (content, returnuri);
        }


        public string FindMovie(string content, string title, int year)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            var secmatches = Regex.Matches(content,
                @"<h2 class=\""(?<section>\w+)\"">(?:[^<]+)</h2>\s+<ul>(?<content>.*?)</ul>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            var found_urls = new List<string>();
            var found_movies = new List<Dictionary<string, string>>();
            foreach (var match in secmatches)
            {
                var matchfixup = match as Match;
                if (matchfixup == null)
                    continue;

                var msp = Regex.Matches(matchfixup.Value,
                    @"<a href=\""(?<link>/subtitles/[^\""]*)\"">(?<title>[^<]+)\((?<year>\d{4})\)</a>\s+</div>\s+<div class=\""subtle count\"">\s+(?<numsubtitles>\d+)",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);

                foreach (var ms in msp)
                {
                    var msf = ms as Match;

                    if (msf == null)
                        continue;

                    found_urls.Add(msf.Groups[1].Value);
                    var dtitle = msf.Groups[2].Value.Trim();
                    var dyear = msf.Groups[3].Value;
                    var dict = new Dictionary<string, string>();
                    dict.Add("t", dtitle);
                    dict.Add("y", dyear);
                    dict.Add("is_exact", matchfixup.Groups[1].Value == "exact" ? "true" : "false");
                    dict.Add("is_close", matchfixup.Groups[1].Value == "exact" ? "false" : "true");
                    dict.Add("l", msf.Groups[1].Value);
                    dict.Add("c", msf.Groups[4].Value);
                    found_movies.Add(dict);
                }
            }

            foreach (var moviedict in found_movies)
                if (moviedict["t"].ToLower() == title.ToLower())
                    if (moviedict["y"] == year.ToString())
                        return moviedict["l"];

            foreach (var moviedict in found_movies)
                if (moviedict["t"].ToLower() == title.ToLower())
                {
                    var yearFixup = year - 1;
                    if (moviedict["y"] == yearFixup.ToString())
                        return moviedict["l"];
                }

            var close_movies = new List<Dictionary<string, string>>();
            foreach (var moviedict in found_movies)
                if (moviedict["is_exact"].ToLower() == "true")
                    return moviedict["l"];
                else
                    close_movies.Add(moviedict);

            if (close_movies.Count > 0)
            {
                var sort = close_movies.OrderByDescending(x => int.Parse(x["c"])).ToList();
                return sort[0]["l"];
            }

            return "";
        }

        private List<int> GetLanguageCodes(string language)
        {
            var codes = new List<int>();
            foreach (var lang in SupportedLanguages)
            {
                var l = lang.Value["2let"] as string;
                if (string.IsNullOrWhiteSpace(l))
                    continue;

                if (l.ToLower() == language.ToLower())
                    codes.Add((int) lang.Value["id"]);
            }

            return codes;
        }

        private async Task<List<Dictionary<string, object>>> GetAllSubs(string url, string language,
            string filename = "", string episode = "")
        {
            var code = GetLanguageCodes(language);
            var langFilter = $"LanguageFilter={code[0]}";
            var resp = await GetUrlString(url, langFilter);
            var content = resp.Item1;


            var matches = Regex.Matches(content, SUBTITLE_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var subtitles = new List<Dictionary<string, object>>();

            foreach (Match match in matches)
            {
                var numfiles = 0;

                if (match.Groups[5].Value != "")
                    int.TryParse(match.Groups[5].Value, out numfiles);

                var languagefound = match.Groups[3].Value;
                var language_info = SupportedLanguages[languagefound];
                if ((string) language_info["2let"] == language)
                    try
                    {
                        var link = $"{base_link}{match.Groups[1].Value}";
                        var subtitle_name = match.Groups[4].Value.Trim();
                        var hearing_imp = match.Groups[6].Value == "a41";
                        var rating = 0;
                        if (match.Groups[2].Value == "positive-icon")
                            rating = 5;

                        var sync = !string.IsNullOrWhiteSpace(filename) &&
                                   filename?.ToLower() == subtitle_name.ToLower();

                        subtitles.Add(new Dictionary<string, object>
                        {
                            {"rating", rating},
                            {"filename", subtitle_name},
                            {"sync", sync},
                            {"link", link},
                            {"lang", language_info},
                            {"hearing_imp", hearing_imp}
                        });
                    }
                    catch (Exception)
                    {
                    }
            }
            return subtitles;
        }

        public async Task<StorageFile> DownloadSubtitle(string link, string episode = "")
        {
            var extensions = new[] {".srt", ".sub", ".txt", ".smi", ".ssa", ".ass"};
            var downloadlink_pattern = "...<a href=\"(.+?)\" rel=\"nofollow\" onclick=\"DownloadSubtitle";
            StorageFile returnFile = null;

            var response = await GetUrlString(link);

            var match = Regex.Match(response.Item1, downloadlink_pattern);

            if (null != match)
            {
                var downloadlink = $"{base_link}{match.Groups[1].Value}";
                var viewstate = 0;
                var previouspage = 0;
                var subtitleid = 0;
                var typeid = "zip";
                var filmid = 0;
                var param = new Dictionary<string, string>();
                param.Add("__EVENTTARGET", "s$lc$bcr$downloadLink");
                param.Add("__EVENTARGUMENT", "");
                param.Add("__VIEWSTATE", viewstate.ToString());
                param.Add("__PREVIOUSPAGE", previouspage.ToString());
                param.Add("subtitleId", subtitleid.ToString());
                param.Add("typeId", typeid);
                param.Add("filmId", filmid.ToString());
                var httpClient = Utilities.GetHttpClient();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.2.3) Gecko/20100401 Firefox/3.6.3 ( .NET CLR 3.5.30729)");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", link);

                var postresp = await httpClient.PostAsync(downloadlink, new FormUrlEncodedContent(param));

                using (var memStream = await postresp.Content.ReadAsStreamAsync())
                {
                    var folder = ApplicationData.Current.LocalFolder;
                    StorageFolder subFolder = null;
                    try
                    {
                        subFolder = await folder.CreateFolderAsync("subs", CreationCollisionOption.ReplaceExisting);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    StorageFile zipFile = null;

                    try
                    {
                        zipFile = await subFolder.CreateFileAsync("dl.zip", CreationCollisionOption.ReplaceExisting);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    try
                    {
                        using (var fileStream = await zipFile.OpenStreamForWriteAsync())
                        {
                            const int BUFFER_SIZE = 1024;
                            var buf = new byte[BUFFER_SIZE];

                            var bytesread = 0;
                            while ((bytesread = await memStream.ReadAsync(buf, 0, BUFFER_SIZE)) > 0)
                                await fileStream.WriteAsync(buf, 0, bytesread);
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    ZipFile.ExtractToDirectory(zipFile.Path, subFolder.Path);

                    var test = await subFolder.GetFilesAsync();

                    foreach (var file in test)
                        if (extensions.Contains(file.FileType))
                        {
                            if (file.FileType.Contains("smi") || file.FileType.Contains("sami"))
                                returnFile = await SubtitleConverter.ConvertToSRT(file, lang);
                            else
                                returnFile = file;
                            break;
                        }
                }
            }
            return returnFile;
        }

        public async Task<List<Dictionary<string, object>>> SearchEpisode(string title, int season, int episode,
            string language,
            string filename)
        {
            var list = new List<Dictionary<string, object>>();

            var escaped_title = PrepareSearchTitle(title);

            var search_string = escaped_title + " - " + seasons[season] + " Season";

            var url = $"{base_link}/subtitles/title?q=" + Uri.EscapeDataString(search_string) + "&r=true";
            var response = await GetUrlString(url);

            var found_urls = new List<string>();

            var matches = Regex.Matches(response.Item1, EPISODE_PATTERN,
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                if (found_urls.Contains(match.Groups[1].Value))
                    continue;

                found_urls.Add(match.Groups[1].Value);
            }


            return list;
        }

        public async Task<List<Dictionary<string, object>>> SearchMovie(string title, int year, string language,
            string filename)
        {
            Debug.WriteLine("Searching for: " + title);
            lang = language;
            var escaped_title = Uri.EscapeDataString(PrepareSearchTitle(title));
            var url = $"{base_link}/subtitles/title?q={escaped_title}&r=true";

            var result = await GetUrlString(url);
            var mov = FindMovie(result.Item1, title, year);

            if (!string.IsNullOrWhiteSpace(mov))
            {
                Debug.WriteLine("Found subtitles...");
                url = $"{base_link}{mov}";
                return await GetAllSubs(url, language, filename);
            }

            return null;
        }

        private string PrepareSearchTitle(string title)
        {
            title = title.Trim();
            var re = new Regex("\\s+\\(\\d\\d\\d\\d\\)$");
            title = re.Replace(title, "");
            return title;
        }
    }
}