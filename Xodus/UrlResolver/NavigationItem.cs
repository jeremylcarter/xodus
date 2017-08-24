using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;

namespace UrlResolver
{
    public class NavigationItem
    {
        public int Episode;
        public string ImdbLink;
        public bool IsTv;
        public bool IsWatchlistItem;
        public int PageIndex;
        public List<IResolver> Resolvers;
        public int Season;
        public string SeriesIMDB;

        public string FormattedName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SeriesName) && Tag.Equals("EpisodeList"))
                    return Name;

                if (Year > 0 && !string.IsNullOrWhiteSpace(Name))
                    return $"{Name} ({Year})";
                return Name;
            }
        }

        public string FormattedEpisode
        {
            get
            {
                if (Episode > 0 && Season > 0)
                    return "S" + Season.ToString("00") + "x" + Episode.ToString("00");
                return "";
            }
        }

        public int Year { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public BitmapImage Image { get; set; }

        public string SeriesName { get; set; }

        public string ImdbId { get; set; }
    }
}