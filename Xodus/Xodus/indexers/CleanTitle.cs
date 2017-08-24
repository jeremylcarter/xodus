using System;
using System.Text.RegularExpressions;

namespace Xodus
{
    public class CleanTitle
    {
        public static string Get(string title)
        {
            var t = Regex.Replace(title, "&#(\\d+);", "");
            t = Regex.Replace(t, "(&#[0-9]+)([^;^0-9]+)", "\\\\1;\\\\2");
            t = t.Replace("&quot;", "\"").Replace("&amp;", "&");
            t = t.ToLower();
            return t;
        }

        public static string GetSearch(string title)
        {
            var movie = title.ToLower();
            movie = Regex.Replace(movie, @"(\d{4})", "");
            movie = Regex.Replace(movie, @"&#(\d+);", "");
            movie = Regex.Replace(movie, @"(&#[0-9]+)([^;^0-9]+)", @"\\1;\\2");
            return movie;
        }

        public static string GetUrl(string title)
        {
            if (null == title)
                return "";

            title = title.ToLower();
            title = title.Replace("/", "-");
            title = title.Replace(" ", "-");
            title = title.Replace("--", "-");
            return title;
        }

        public static string Query(string title)
        {
            return title.Replace("\\", "").Replace(":", "").Replace(" -", "-").Replace("-", " ");
        }

        public static string Query2(string title)
        {
            title = title.Replace("\'", "");
            var ar = title.Split(':');

            if (ar.Length > 0)
            {
                Array.Reverse(ar);
                title += ar[0];
                ar = title.Split(new[] {" -"}, StringSplitOptions.None);
                if (ar.Length > 0)
                {
                    Array.Reverse(ar);
                    title += ar[0];
                }
            }

            title = title.Replace('-', ' ');
            return title;
        }
    }
}