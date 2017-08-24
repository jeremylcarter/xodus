using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace Xodus
{
    public sealed class SubtitleConverter
    {
        private static readonly Dictionary<string, int> languages = new Dictionary<string, int>
        {
            {"ko", 51949},
            {"jp", 51932}
        };

        public static string ExtractDialog(string line)
        {
            line = Regex.Replace(line, "<[^>]*>", "");
            return Regex.Replace(line, "&nbsp;", "").Trim();
        }

        public static string ExtractTimestamp(string line)
        {
            var ts = "";

            if (line.ToLower().Contains("start="))
            {
                var startIndex = line.IndexOf("start=") + 6;

                var startParse = line.Substring(startIndex);

                var i = 0;
                while (true)
                    if (!char.IsDigit(startParse[i]))
                    {
                        if (startParse[i] == '>')
                            break;
                        i++;
                    }
                    else
                    {
                        ts += startParse[i];
                        i++;
                    }
                var q = 0;
            }

            return ts;
        }

        public static async Task<StorageFile> ConvertToSRT(StorageFile file, string lang)
        {
            StorageFile newFile = null;

            try
            {
                var sami = new List<string>();
                var shit = new List<(TimeSpan, string)>();
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                var codepage = 0;
                if (languages.Keys.Contains(lang.ToLower()))
                    codepage = languages[lang.ToLower()];

                Encoding encoding = null;

                if (codepage != 0)
                    encoding = Encoding.GetEncoding(codepage);

                using (var sr = new StreamReader(await file.OpenStreamForReadAsync(), encoding))
                {
                    var test = new List<(int, string)>();

                    var start = false;
                    var line = "";

                    var ts = "";
                    var dialog = "";


                    while ((line = sr.ReadLine()) != null)
                    {
                        if (start)
                        {
                            if (string.IsNullOrWhiteSpace(ts))
                                ts = ExtractTimestamp(line);

                            dialog = ExtractDialog(line);


                            if (!string.IsNullOrWhiteSpace(ts) && !string.IsNullOrWhiteSpace(dialog))
                            {
                                var n = int.Parse(ts);
                                var t = TimeSpan.FromMilliseconds(n);
                                shit.Add((t, dialog));
                                ts = "";
                                dialog = "";
                            }
                        }

                        if (line.ToLower() == "<body>")
                            start = true;
                        if (line.ToLower() == "</body>")
                            break;
                    }
                }

                newFile = await
                    ApplicationData.Current.TemporaryFolder.CreateFileAsync("subtemp.srt",
                        CreationCollisionOption.ReplaceExisting);


                using (var s = new StreamWriter(await newFile.OpenStreamForWriteAsync()))
                {
                    var index = 1;


                    foreach (var item in shit)
                    {
                        var endTime = item.Item1.Add(TimeSpan.FromMilliseconds(2000));
                        s.WriteLine(index.ToString());
                        s.WriteLine(item.Item1.ToString(@"hh\:mm\:ss") + ",000 --> " + endTime.ToString(@"hh\:mm\:ss") +
                                    ",000");
                        s.WriteLine(item.Item2);
                        s.WriteLine();
                        index++;
                    }
                }
            }
            catch (Exception)
            {
            }

            return newFile;
        }
    }
}