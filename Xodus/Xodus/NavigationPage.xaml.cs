using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using UrlResolver;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Xodus
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigationPage : Page
    {
        private readonly List<(string, string, bool)> Genres = new List<(string, string, bool)>
        {
            ("Action", "action", false),
            ("Adventure", "adventure", false),
            ("Animation", "animation", false),
            ("Anime", "anime", true),
            ("Biography", "biography", false),
            ("Comedy", "comedy", false),
            ("Crime", "crime", false),
            ("Documentary", "documentary", false),
            ("Drama", "drama", false),
            ("Family", "family", false),
            ("Fantasy", "fantasy", false),
            ("History", "history", false),
            ("Horror", "horror", false),
            ("Music", "music", false),
            ("Musical", "musical", false),
            ("Mystery", "mystery", false),
            ("Romance", "romance", false),
            ("Science Fiction", "sci_fi", false),
            ("Sport", "sport", false),
            ("Thriller", "thriller", false),
            ("War", "war", false),
            ("Western", "western", false)
        };

        public NavigationPage()
        {
            InitializeComponent();
            NavList.Loaded += NavList_Loaded;
        }

        private void NavList_Loaded(object sender, RoutedEventArgs e)
        {
            NavList.Focus(FocusState.Programmatic);
        }

        private void LoadFirstItems()
        {
            var list = new List<NavigationItem>();
            var rl = new ResourceLoader();
            list.Add(new NavigationItem {Name = rl.GetString("MoviesPage"), Tag = "MoviesPage", IsTv = false});
            list.Add(new NavigationItem {Name = rl.GetString("TelevisionPage"), Tag = "TelevisionPage", IsTv = false});
            list.Add(new NavigationItem
            {
                Name = rl.GetString("Favorites"),
                Tag = "Watchlist",
                IsTv = true
            });
            list.Add(new NavigationItem {Name = rl.GetString("SearchAllMovies"), Tag = "SearchMovies", IsTv = false});
            list.Add(new NavigationItem {Name = rl.GetString("SearchAllTelevision"), Tag = "SearchTv", IsTv = true});
            NavList.ItemsSource = list;
        }

        private void LoadTelevisionPage(bool isTV = false)
        {
            var list = new List<NavigationItem>();
            var rl = new ResourceLoader();

            list.Add(new NavigationItem
            {
                Name = rl.GetString("MostPopularTelevision"),
                Tag = "MostPopular",
                IsTv = true
            });
            list.Add(new NavigationItem
            {
                Name = rl.GetString("TelevisionNetworks"),
                Tag = "TelevisionNetworks",
                IsTv = true
            });

            list.Add(new NavigationItem {Name = rl.GetString("SearchAllTelevision"), Tag = "SearchTv", IsTv = true});
            NavList.ItemsSource = list;
        }

        private void LoadMoviesPage(bool isTV = false)
        {
            var list = new List<NavigationItem>();
            var rl = new ResourceLoader();

            list.Add(new NavigationItem {Name = rl.GetString("MostPopularMovies"), Tag = "MostPopular", IsTv = false});
            list.Add(new NavigationItem {Name = rl.GetString("MostVotedMovies"), Tag = "MostVoted", IsTv = false});
            list.Add(new NavigationItem {Name = rl.GetString("NewMovies"), Tag = "NewMovies", IsTv = false});
            list.Add(new NavigationItem {Name = rl.GetString("BestPicture"), Tag = "BestPicture", IsTv = false});
            list.Add(new NavigationItem {Name = rl.GetString("MovieGenres"), Tag = "MovieGenres", IsTv = false});
            list.Add(new NavigationItem {Name = rl.GetString("SearchAllMovies"), Tag = "SearchMovies", IsTv = false});
            NavList.ItemsSource = list;
        }

        private async void LoadGenre(string genre, bool useKeyword, int page)
        {
            var imdb = new Imdb();
            var list = new List<NavigationItem>();

            if (page == 0)
                page = 1;

            var moviesResults = await imdb.GetGenre(genre, useKeyword, page);

            foreach (var item in moviesResults)
            {
                var ni = new NavigationItem();
                var info = Utilities.GetTitle(item.Item1);
                ni.Name = info.Item1;
                ni.ImdbId = item.Item2;
                ni.Year = info.Item2;
                ni.Tag = "MoviesSearch";
                ni.IsTv = false;
                list.Add(ni);
            }

            var rl = new ResourceLoader();
            var nextItem = new NavigationItem();
            nextItem.PageIndex = page + 1;
            nextItem.Name = rl.GetString("Next");
            nextItem.Name += "...";
            nextItem.Tag = "GenreList:" + genre;
            nextItem.IsTv = useKeyword;
            list.Add(nextItem);
            NavList.ItemsSource = list;
            ItemSourceChanged();
        }

        private async void LoadMostVoted(int page, bool isTV = false)
        {
            var imdb = new Imdb();
            var list = new List<NavigationItem>();
            if (!isTV)
            {
                var moviesResults = await imdb.GetMostVoted(page);
                foreach (var item in moviesResults)
                {
                    var ni = new NavigationItem();
                    var info = Utilities.GetTitle(item.Item1);
                    ni.Name = info.Item1;
                    ni.ImdbId = item.Item2;
                    ni.Year = info.Item2;
                    ni.Tag = "MoviesSearch";
                    ni.IsTv = isTV;
                    list.Add(ni);
                }

                var rl = new ResourceLoader();
                var nextItem = new NavigationItem();
                nextItem.PageIndex = page + 1;
                nextItem.Name = rl.GetString("Next");
                nextItem.Name += "...";
                nextItem.Tag = "MostVoted";
                list.Add(nextItem);
            }
            else
            {
                var results = await imdb.GetMostPopularTv(page);

                foreach (var item in results)
                {
                    var ni = new NavigationItem();
                    ni.Name = item.Item1;
                    ni.SeriesName = item.Item1;
                    ni.ImdbLink = item.Item2;
                    ni.SeriesIMDB = ni.ImdbLink;
                    ni.ImdbId = item.Item3;
                    ni.Tag = "TVSeasonList";
                    ni.IsTv = isTV;
                    list.Add(ni);
                }

                var rl = new ResourceLoader();
                var nextItem = new NavigationItem();
                nextItem.PageIndex = page + 1;
                nextItem.Name = rl.GetString("Next");
                nextItem.Name += "...";
                nextItem.Tag = "MostPopular";
                nextItem.IsTv = true;
                list.Add(nextItem);
            }


            NavList.ItemsSource = list;
            ItemSourceChanged();
        }

        private async void LoadGenres()
        {
            var list = new List<NavigationItem>();
            foreach (var item in Genres)
            {
                var ni = new NavigationItem();
                var info = item;
                ni.Name = info.Item1;
                ni.Year = 0;
                ni.ImdbId = "";
                ni.Tag = "GenreList:" + item.Item2;
                ni.IsTv = item.Item3;
                list.Add(ni);
            }

            NavList.ItemsSource = list;
            ItemSourceChanged();
        }

        private async void LoadBestPicture(int page, bool isTV = false)
        {
            var imdb = new Imdb();

            var list = new List<NavigationItem>();
            if (!isTV)
            {
                var moviesResults = await imdb.GetBestPicture(page);
                foreach (var item in moviesResults)
                {
                    var ni = new NavigationItem();
                    var info = Utilities.GetTitle(item.Item1);
                    ni.Name = info.Item1;
                    ni.Year = info.Item2;
                    ni.ImdbId = item.Item2;
                    ni.Tag = "MoviesSearch";
                    ni.IsTv = isTV;
                    list.Add(ni);
                }

                var rl = new ResourceLoader();
                var nextItem = new NavigationItem();
                nextItem.PageIndex = page + 1;
                nextItem.Name = rl.GetString("Next");
                nextItem.Name += "...";
                nextItem.Tag = "BestPicture";
                list.Add(nextItem);
            }


            NavList.ItemsSource = list;
            ItemSourceChanged();
        }

        private async void LoadNewMovies(int page, bool isTV = false)
        {
            var imdb = new Imdb();

            var list = new List<NavigationItem>();
            if (!isTV)
            {
                var moviesResults = await imdb.GetInTheaters(page);
                foreach (var item in moviesResults)
                {
                    var ni = new NavigationItem();
                    var info = Utilities.GetTitle(item.Item1);
                    ni.Name = info.Item1;
                    ni.Year = info.Item2;
                    ni.ImdbId = item.Item2;
                    ni.Tag = "MoviesSearch";
                    ni.IsTv = isTV;
                    list.Add(ni);
                }

                var rl = new ResourceLoader();
                var nextItem = new NavigationItem();
                nextItem.PageIndex = page + 1;
                nextItem.Name = rl.GetString("Next");
                nextItem.Name += "...";
                nextItem.Tag = "NewMovies";
                list.Add(nextItem);
            }


            NavList.ItemsSource = list;
            ItemSourceChanged();
        }

        private async void LoadMostPopularMovies(int page, bool isTV = false)
        {
            var imdb = new Imdb();

            var list = new List<NavigationItem>();
            if (!isTV)
            {
                var moviesResults = await imdb.GetMostPopular(page);
                foreach (var item in moviesResults)
                {
                    var ni = new NavigationItem();
                    var info = Utilities.GetTitle(item.Item1);
                    ni.Name = info.Item1;
                    ni.Year = info.Item2;
                    ni.ImdbId = item.Item2;
                    ni.Tag = "MoviesSearch";
                    ni.IsTv = isTV;
                    list.Add(ni);
                }

                var rl = new ResourceLoader();
                var nextItem = new NavigationItem();
                nextItem.PageIndex = page + 1;
                nextItem.Name = rl.GetString("Next");
                nextItem.Name += "...";
                nextItem.Tag = "MostPopular";
                list.Add(nextItem);
            }
            else
            {
                var results = await imdb.GetMostPopularTv(page);

                foreach (var item in results)
                {
                    var ni = new NavigationItem();
                    var info = Utilities.GetTitle(item.Item1);
                    ni.Name = info.Item1;
                    ni.Year = info.Item2;
                    ni.SeriesName = item.Item1;
                    ni.ImdbLink = item.Item2;
                    ni.SeriesIMDB = ni.ImdbLink;
                    ni.ImdbId = item.Item3;
                    ni.Tag = "TVSeasonList";
                    ni.IsTv = isTV;
                    list.Add(ni);
                }

                var rl = new ResourceLoader();
                var nextItem = new NavigationItem();
                nextItem.PageIndex = page + 1;
                nextItem.Name = rl.GetString("Next");
                nextItem.Name += "...";
                nextItem.Tag = "MostPopular";
                nextItem.IsTv = true;
                list.Add(nextItem);
            }


            NavList.ItemsSource = list;
            ItemSourceChanged();
        }

        private void ItemSourceChanged()
        {
            if (NavList?.Items?.Count > 0)
            {
                NavList.SelectedIndex = 0;
                NavList_SelectionChanged(null, null);
            }
        }

        private void ToggleLoadingControl()
        {
            MainPage.Instance.ToggleLoadingControl();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            MainPage.Instance.LoadingChanged -= Instance_LoadingChanged;
            MainPage.Instance.XPressed -= Instance_XPressed;
            MainPage.Instance.ToggleWatchStack(false);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            NavList.Focus(FocusState.Programmatic);
            MainPage.Instance.LoadingChanged += Instance_LoadingChanged;
            MainPage.Instance.XPressed += Instance_XPressed;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    MainPage.Instance.SetSidebarInformation(null);
                }
                catch (Exception)
                {
                }
            });

            try
            {
                ToggleLoadingControl();

                if (e.Parameter == null)
                {
                    MainPage.Instance.ChangeBackgroundImage("", false);
                    MainPage.Instance.SetNavText("");
                    LoadFirstItems();
                }
                else
                {
                    var item = e.Parameter as NavigationItem;

                    if (item != null)
                    {
                        if (item.Tag == "ShowResolvers")
                        {
                            foreach (var resolver in item.Resolvers)
                            {
                                resolver.isTV = item.IsTv;
                                resolver.imdb = item.ImdbId;
                                resolver.item = item;
                            }

                            NavList.ItemsSource = item.Resolvers;
                            NavList.ItemTemplate = Resources["SourceList"] as DataTemplate;


                            return;
                        }
                        if (item.Tag == "MoviesPage")
                        {
                            LoadMoviesPage(false);
                            return;
                        }

                        if (item.Tag == "TelevisionPage")
                        {
                            LoadTelevisionPage(true);
                            return;
                        }

                        if (item.Tag == "Watchlist")
                        {
                            var r = ResourceLoader.GetForCurrentView();
                            var list = new ObservableCollection<NavigationItem>();
                            var ni = new NavigationItem();
                            ni.Tag = "MoviesWatchlist";
                            ni.Name = r.GetString("Movies");
                            var ni2 = new NavigationItem();
                            ni2.Tag = "TVWatchlist";
                            ni2.Name = r.GetString("Television");
                            list.Add(ni);
                            list.Add(ni2);
                            NavList.ItemsSource = list;
                            MainPage.Instance.SetNavText(r.GetString("Favorites"));
                            return;
                        }

                        if (item.Tag == "MoviesWatchlist")
                        {
                            var list = App.Current.GetMoviesWatchList();
                            NavList.ItemsSource = list;
                            return;
                        }

                        if (item.Tag == "TVWatchlist")
                        {
                            var list = App.Current.GetTVWatchList();
                            NavList.ItemsSource = list;
                            return;
                        }

                        if (item.Tag == "TelevisionNetworks")
                        {
                            var tvMaze = new TVMaze();
                            var list = new List<NavigationItem>();
                            try
                            {
                                var results = tvMaze.GetChannels();
                                foreach (var result in results)
                                {
                                    var ni = new NavigationItem();
                                    ni.Name = result.Key;
                                    ni.ImdbLink = result.Value;
                                    ni.Tag = "TelevisionNetwork";
                                    ni.IsTv = true;
                                    list.Add(ni);
                                }
                            }
                            catch (Exception)
                            {
                            }

                            NavList.ItemsSource = list;
                            MainPage.Instance.SetNavText(new ResourceLoader().GetString("TelevisionNetworks"));
                            ItemSourceChanged();
                            return;
                        }

                        if (item.Tag == "TelevisionNetwork")
                        {
                            var tvMaze = new TVMaze();
                            var list = new List<NavigationItem>();
                            try
                            {
                                var results = await tvMaze.List(item.ImdbLink);

                                foreach (var result in results)
                                {
                                    var ni = new NavigationItem();
                                    ni.Name = result;
                                    ni.Tag = "PerformSearchTV";
                                    ni.IsTv = true;
                                    list.Add(ni);
                                }
                            }
                            catch (Exception)
                            {
                            }

                            NavList.ItemsSource = list;
                            MainPage.Instance.SetNavText(item?.Name);
                            ItemSourceChanged();
                            return;
                        }
                        if (item.Tag == "PerformSearchTV")
                        {
                            var imdb = new Imdb();
                            var results = await imdb.SearchTv(item.Name);

                            var list = new List<NavigationItem>();
                            foreach (var item2 in results)
                            {
                                var ni = new NavigationItem();
                                var info = Utilities.GetTitle(item2.Item1);
                                ni.Name = info.Item1;
                                ni.SeriesName = info.Item1;
                                ni.ImdbLink = item2.Item2;
                                ni.SeriesIMDB = ni.ImdbLink;
                                ni.ImdbId = item2.Item3;
                                ni.Year = info.Item2;
                                ni.Tag = "TVSeasonList";
                                ni.IsTv = true;
                                list.Add(ni);
                            }

                            NavList.ItemsSource = list;
                            MainPage.Instance.SetNavText(new ResourceLoader().GetString("SearchAllTelevision"));
                            ItemSourceChanged();
                            return;
                        }

                        if (item.Tag == "PerformSearchMovies")
                        {
                            var imdb = new Imdb();
                            var results = await imdb.SearchMovies(item.Name);

                            var list = new List<NavigationItem>();
                            foreach (var item2 in results)
                            {
                                var ni = new NavigationItem();
                                var info = Utilities.GetTitle(item2.Item1);
                                ni.Name = info.Item1;
                                ni.SeriesName = info.Item1;
                                ni.ImdbLink = item2.Item2;
                                ni.SeriesIMDB = ni.ImdbLink;
                                ni.ImdbId = item2.Item3;
                                ni.Year = info.Item2;
                                ni.Tag = "MoviesSearch";
                                ni.IsTv = false;
                                list.Add(ni);
                            }

                            NavList.ItemsSource = list;
                            MainPage.Instance.SetNavText(new ResourceLoader().GetString("SearchAllMovies"));
                            ItemSourceChanged();
                            return;
                        }

                        if (item.Tag == "Movies")
                        {
                            LoadFirstItems();
                            MainPage.Instance.SetNavText("");
                            return;
                        }

                        if (item.Tag == "EpisodeList")
                        {
                            var imdb = new Imdb();
                            var results = await imdb.GetTvSeasonEpisodeList(item.ImdbLink);

                            if (results.Count == 0)
                            {
                                var md = new MessageDialog(
                                    "The season you've requested is sometime in the future. Until we overcome physics and can travel faster than the speed of light, viewing this is impossible at the moment. Sorry!");
                                await md.ShowAsync();
                                Frame.GoBack();
                                return;
                            }

                            var items = new List<NavigationItem>();
                            var episode = 1;
                            foreach (var result in results)
                            {
                                var ni = new NavigationItem();
                                ni.ImdbLink = result.Item2;
                                ni.SeriesName = item.SeriesName;
                                ni.SeriesIMDB = item.SeriesIMDB;
                                ni.Name = result.Item1;
                                ni.IsTv = true;
                                ni.Year = item.Year;
                                ni.Tag = "PlayTV";
                                ni.Season = item.Season;
                                ni.ImdbId = item.ImdbId;
                                ni.Episode = episode;
                                episode++;
                                items.Add(ni);
                            }

                            NavList.ItemTemplate = Resources["MovieTemplate"] as DataTemplate;

                            if (items.Count > 0)
                                MainPage.Instance.SetNavText(items[0]?.SeriesName);
                            NavList.ItemsSource = items;
                            ItemSourceChanged();
                            return;
                        }


                        if (item.Tag == "TVSeasonList")
                        {
                            var imdb = new Imdb();
                            var results = await imdb.GetTvSeasons(item.ImdbLink);

                            var items = new List<NavigationItem>();
                            var season = 1;
                            foreach (var result in results)
                            {
                                var ni = new NavigationItem();
                                ni.ImdbLink = result.Item2;
                                ni.ImdbId = item.ImdbId;
                                ni.SeriesName = item.Name;
                                ni.SeriesIMDB = item.SeriesIMDB;
                                ni.Name = result.Item1;
                                ni.Year = item.Year;
                                ni.IsTv = true;
                                ni.Tag = "EpisodeList";
                                ni.Season = season;
                                season++;
                                items.Add(ni);
                            }
                            NavList.ItemsSource = items;


                            if (items.Count > 0)
                                MainPage.Instance.SetNavText(items[0]?.SeriesName);

                            ItemSourceChanged();
                            return;
                        }

                        if (item.Tag == "TVShows")
                        {
                            LoadFirstItems();
                            return;
                        }

                        if (item.Tag.Equals("MostPopular"))
                        {
                            LoadMostPopularMovies(item.PageIndex == 0 ? 1 : item.PageIndex, item.IsTv);
                            MainPage.Instance.SetNavText(new ResourceLoader().GetString(item.IsTv
                                ? "MostPopularTelevision"
                                : "MostPopularMovies"));
                            return;
                        }

                        if (item.Tag.Equals("NewMovies"))
                        {
                            LoadNewMovies(item.PageIndex == 0 ? 1 : item.PageIndex, item.IsTv);
                            MainPage.Instance.SetNavText(new ResourceLoader().GetString("NewMovies"));
                            return;
                        }

                        if (item.Tag.Equals("BestPicture"))
                        {
                            LoadBestPicture(item.PageIndex == 0 ? 1 : item.PageIndex, item.IsTv);
                            MainPage.Instance.SetNavText(new ResourceLoader().GetString("BestPicture"));
                            return;
                        }

                        if (item.Tag.Equals("MovieGenres"))
                        {
                            LoadGenres();
                            MainPage.Instance.SetNavText(new ResourceLoader().GetString("MovieGenres"));
                            return;
                        }

                        if (item.Tag.Equals("MostVoted"))
                        {
                            LoadMostVoted(item.PageIndex == 0 ? 1 : item.PageIndex);
                            MainPage.Instance.SetNavText(new ResourceLoader().GetString("MostVotedMovies"));
                            return;
                        }

                        if (item.Tag.StartsWith("GenreList:"))
                        {
                            var tok = item.Tag.Split(':');

                            if (tok.Length >= 2)
                                LoadGenre(tok[1], item.IsTv, item.PageIndex);
                        }
                    }
                }
            }
            finally
            {
                MainPage.Instance.FocusContentFrame();
                ToggleLoadingControl();
            }
        }

        private void Instance_XPressed(object e, KeyRoutedEventArgs x)
        {
            NavList_KeyDown(e, x);
        }

        private void Instance_LoadingChanged(object sender, LoadingChangedEventArgs e)
        {
            NavList.IsEnabled = !e.Loading;
        }

        private async void NavList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as NavigationItem;


            var item2 = e.ClickedItem as IResolver;

            if (null != item2)
            {
                ToggleLoadingControl();

                if (item2 is IPairedAuthorizationSource)
                {
                    var pair = item2 as IPairedAuthorizationSource;

                    if (!await pair.CheckAuthorization())
                    {
                        if (!App.Current.GetBoolSetting("enablepairing"))
                        {
                            var md = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("NoSources"));
                            await md.ShowAsync();
                            ToggleLoadingControl();
                            return;
                        }

                        var pd = new PairDialog(pair);
                        var button = await pd.ShowAsync();

                        if (button == ContentDialogResult.Secondary)
                        {
                            var md = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("NoSources"));
                            await md.ShowAsync();
                            ToggleLoadingControl();
                            return;
                        }
                    }
                }

                StorageFile subtitleFile = null;

                if (!item2.isTV)
                {
                    var subsEnabled =
                        ApplicationData.Current.LocalSettings.Values["subtitlesenabled"] as bool?;


                    if (null != subsEnabled)
                        if ((bool) subsEnabled)
                            try
                            {
                                var rl = ResourceLoader.GetForCurrentView();

                                var language =
                                    (string) ApplicationData.Current.LocalSettings.Values["subtitlelanguage"];

                                MainPage.Instance.UpdateLoadingText(rl.GetString("LoadingSubtitles"));
                                var ss = new Subscene();
                                language = ss.SupportedLanguages[language]["2let"] as string;
                                var subs = await ss.SearchMovie(item2.item.Name, item2.item.Year, language, "");

                                if (subs.Count > 0)
                                {
                                    var sd = new SubtitleSelectionDialog(subs);
                                    await sd.ShowAsync();
                                    if (null != sd.SelectedSub)
                                        subtitleFile = await ss.DownloadSubtitle(sd.SelectedSub?.Link);
                                }
                            }
                            catch (Exception)
                            {
                            }
                }

                var mediaUri = await item2.GetMediaUrl();

                if (string.IsNullOrWhiteSpace(mediaUri) || mediaUri.Contains(".flv"))
                {
                    var md = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("NoSources"));
                    await md.ShowAsync();
                    ToggleLoadingControl();
                    return;
                }
                try
                {
                    var movieUri = new Uri(mediaUri);
                }
                catch (Exception)
                {
                    var md = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("NoSources"));
                    await md.ShowAsync();
                    ToggleLoadingControl();
                    return;
                }

                try
                {
                    var httpClient = Utilities.GetHttpClient();
                    var result = await httpClient.GetAsync(mediaUri, HttpCompletionOption.ResponseHeadersRead);
                    if (!result.IsSuccessStatusCode)
                    {
                        var md = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("NoSources"));
                        await md.ShowAsync();
                        ToggleLoadingControl();
                        return;
                    }
                }
                catch (Exception)
                {
                    var md = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("NoSources"));
                    await md.ShowAsync();
                    ToggleLoadingControl();
                    return;
                }

                if (mediaUri.Length > 0)
                    MainPage.Instance.PlayMovie(mediaUri, item2.imdb, subtitleFile);

                ToggleLoadingControl();
                return;
            }


            var indexers = new List<IIndexer>();

            //indexers.Add(new Putlocker());
            indexers.Add(new Onseries());
            indexers.Add(new WatchFree());
            indexers.Add(new XMovies());
            indexers.Add(new YMovies());
            indexers.Add(new Onemovies());
            indexers.Add(new Solarmovies());
            indexers.Add(new Dayt());
            indexers.Add(new Playbox());


            if (null != item)
            {
                if (item.Tag == "PlayTV")
                    try
                    {
                        ToggleLoadingControl();
                        Uri movieUri = null;

                        var resolvers = new List<IResolver>();
                        var rl = new ResourceLoader();
                        await indexers.ParallelForEachAsync(async indexer =>
                        {
                            movieUri = null;
                            MainPage.Instance.UpdateLoadingText("Loading Sources");
                            var resolver =
                                await indexer.GetTvLink(item.SeriesName, item.Year, item.Season, item.Episode);

                            if (resolver.Count == 0)
                                return;

                            resolvers.AddRange(resolver);
                        });

                        resolvers = resolvers.OrderByDescending(x => x.VideoQuality).ThenBy(OrderByType).ToList();


                        if (resolvers.Count == 0)
                        {
                            var md = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("NoSources"));
                            await md.ShowAsync();
                            ToggleLoadingControl();
                            return;
                        }

                        item.Tag = "ShowResolvers";
                        item.Resolvers = resolvers;
                        ToggleLoadingControl();
                        MainPage.Instance.Navigate(typeof(NavigationPage), item);
                        return;

                        foreach (var resolver in resolvers)
                        {
                            if (resolver is IPairedAuthorizationSource)
                            {
                                var pair = resolver as IPairedAuthorizationSource;

                                if (!await pair.CheckAuthorization())
                                {
                                    if (!App.Current.GetBoolSetting("enablepairing"))
                                        continue;

                                    var pd = new PairDialog(pair);
                                    var button = await pd.ShowAsync();

                                    if (button == ContentDialogResult.Secondary)
                                        continue;
                                }
                            }

                            var uri = await resolver.GetMediaUrl();

                            if (string.IsNullOrWhiteSpace(uri) || uri.Contains(".flv"))
                            {
                                uri = "";
                                continue;
                            }
                            try
                            {
                                movieUri = new Uri(uri);
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                            try
                            {
                                var httpClient = Utilities.GetHttpClient();
                                var result = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                                if (!result.IsSuccessStatusCode)
                                    continue;
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            break;
                        }

                        if (movieUri == null)
                        {
                            var md = new MessageDialog(rl.GetString("NoSources"));
                            await md.ShowAsync();
                            MainPage.Instance.ToggleLoadingControl();
                            MainPage.Instance.UpdateLoadingText(rl.GetString("Loading.Text"));
                            return;
                        }
                        MainPage.Instance.SetCurrentMovie(item);
                        var ts = App.Current.GetMoviePosition(item);

                        if (ts != TimeSpan.MinValue && ts != TimeSpan.Zero)
                        {
                            var mf = new MenuFlyout();
                            var mfi = new MenuFlyoutItem();
                            mfi.Text = "Resume from " + ts.ToString("g");
                            mfi.Click += (x, y) =>
                            {
                                MainPage.Instance.PlayMovie(movieUri.AbsoluteUri, item.ImdbId, null, ts);
                            };
                            mf.Items.Add(mfi);
                            mfi = new MenuFlyoutItem();
                            mfi.Text = "Play from beginning";
                            mfi.Click += (x, y) =>
                            {
                                App.Current.SavePosition(item, TimeSpan.Zero);
                                MainPage.Instance.PlayMovie(movieUri.AbsoluteUri, item.ImdbId);
                            };
                            mf.Items.Add(mfi);
                            var shit = NavList.ContainerFromItem(NavList.SelectedItem) as ListViewItem;

                            mf.ShowAt(shit, shit.TransformToVisual(shit).TransformPoint(new Point()));
                            return;
                        }
                        MainPage.Instance.PlayMovie(movieUri.AbsoluteUri, item.ImdbId);
                        return;
                    }
                    finally
                    {
                    }
                if (item.Tag.Equals("MoviesSearch"))
                    try
                    {
                        ToggleLoadingControl();
                        Uri movieUri = null;

                        var resolvers = new List<IResolver>();
                        var rl = new ResourceLoader();
                        await indexers.ParallelForEachAsync(async indexer =>
                        {
                            movieUri = null;
                            MainPage.Instance.UpdateLoadingText("Loading Sources");
                            var resolver =
                                await indexer.GetMovieLink(item.Name, item.Year);

                            if (resolver.Count == 0)
                                return;

                            resolvers.AddRange(resolver);
                        });

                        resolvers = resolvers.OrderByDescending(x => x.VideoQuality).ThenBy(OrderByType).ToList();


                        if (resolvers.Count == 0)
                        {
                            var md = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("NoSources"));
                            await md.ShowAsync();
                            ToggleLoadingControl();
                            return;
                        }
                        item.Tag = "ShowResolvers";

                        item.Resolvers = resolvers;
                        ToggleLoadingControl();
                        MainPage.Instance.Navigate(typeof(NavigationPage), item);
                        return;

                        foreach (var resolver in resolvers)
                        {
                            if (resolver is IPairedAuthorizationSource)
                            {
                                var pair = resolver as IPairedAuthorizationSource;

                                if (!await pair.CheckAuthorization())
                                {
                                    if (!App.Current.GetBoolSetting("enablepairing"))
                                        continue;

                                    var pd = new PairDialog(pair);
                                    var button = await pd.ShowAsync();

                                    if (button == ContentDialogResult.Secondary)
                                        continue;
                                }
                            }

                            var uri = await resolver.GetMediaUrl();

                            if (string.IsNullOrWhiteSpace(uri) || uri.Contains(".flv"))
                            {
                                uri = "";
                                continue;
                            }
                            try
                            {
                                movieUri = new Uri(uri);
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                            try
                            {
                                var httpClient = Utilities.GetHttpClient();
                                var result = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                                if (!result.IsSuccessStatusCode)
                                    continue;
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            break;
                        }

                        if (movieUri == null)
                        {
                            var md = new MessageDialog(rl.GetString("NoSources"));
                            MainPage.Instance.ToggleLoadingControl();
                            MainPage.Instance.UpdateLoadingText(rl.GetString("Loading.Text"));
                            await md.ShowAsync();
                            return;
                        }
                        MainPage.Instance.SetCurrentMovie(item);
                        MainPage.Instance.UpdateLoadingText(rl.GetString("Loading.Text"));

                        var subsEnabled =
                            ApplicationData.Current.LocalSettings.Values["subtitlesenabled"] as bool?;

                        StorageFile subtitleFile = null;
                        if (null != subsEnabled)
                            if ((bool) subsEnabled)
                                try
                                {
                                    var language =
                                        (string) ApplicationData.Current.LocalSettings.Values["subtitlelanguage"];

                                    MainPage.Instance.UpdateLoadingText(rl.GetString("LoadingSubtitles"));
                                    var ss = new Subscene();
                                    language = ss.SupportedLanguages[language]["2let"] as string;
                                    var subs = await ss.SearchMovie(item.Name, item.Year, language, "");

                                    if (subs.Count > 0)
                                    {
                                        var sd = new SubtitleSelectionDialog(subs);
                                        await sd.ShowAsync();
                                        if (null != sd.SelectedSub)
                                            subtitleFile = await ss.DownloadSubtitle(sd.SelectedSub?.Link);
                                    }
                                }
                                catch (Exception)
                                {
                                }
                        MainPage.Instance.UpdateLoadingText(rl.GetString("Loading.Text"));

                        var ts = App.Current.GetMoviePosition(item);

                        if (ts != TimeSpan.MinValue && ts != TimeSpan.Zero)
                        {
                            var mf = new MenuFlyout();
                            var mfi = new MenuFlyoutItem();
                            mfi.Text = "Resume from " + ts.ToString("g");
                            mfi.Click += (x, y) =>
                            {
                                MainPage.Instance.PlayMovie(movieUri.AbsoluteUri, item.ImdbId, subtitleFile, ts);
                            };
                            mf.Items.Add(mfi);
                            mfi = new MenuFlyoutItem();
                            mfi.Text = "Play from beginning";
                            mfi.Click += (x, y) =>
                            {
                                App.Current.SavePosition(item, TimeSpan.Zero);

                                MainPage.Instance.PlayMovie(movieUri.AbsoluteUri, item.ImdbId, subtitleFile);
                            };
                            mf.Items.Add(mfi);
                            var shit = NavList.ContainerFromItem(NavList.SelectedItem) as ListViewItem;

                            mf.ShowAt(shit, shit.TransformToVisual(shit).TransformPoint(new Point()));
                            return;
                        }
                        MainPage.Instance.PlayMovie(movieUri.AbsoluteUri, item.ImdbId, subtitleFile);
                        return;
                    }
                    finally
                    {
                    }
                if (item.Tag == "SearchTv")
                {
                    var sd = new SearchDialog();
                    var result = await sd.ShowAsync();


                    if (!string.IsNullOrWhiteSpace(sd.Query))
                    {
                        item.Name = sd.Query;
                        if (result == ContentDialogResult.Secondary)
                            return;
                        item.Tag = "PerformSearchTV";
                        MainPage.Instance.Navigate(typeof(NavigationPage), item);
                    }

                    return;
                }

                if (item.Tag == "SearchMovies")
                {
                    var sd = new SearchDialog();
                    var result = await sd.ShowAsync();
                    if (result == ContentDialogResult.Secondary)
                        return;
                    if (!string.IsNullOrWhiteSpace(sd.Query))
                    {
                        item.Name = sd.Query;
                        item.Tag = "PerformSearchMovies";
                        MainPage.Instance.Navigate(typeof(NavigationPage), item);
                    }
                }

                MainPage.Instance.Navigate(typeof(NavigationPage), item);
            }
        }

        private int OrderByType(IResolver item)
        {
            if (item is GoogleVideo)
                return 0;
            return 1;
        }

        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = NavList.SelectedItem as NavigationItem;

            if (null != item)
            {
                if (string.IsNullOrWhiteSpace(item.ImdbId))
                    MainPage.Instance.ToggleWatchStack(false);
                else
                    MainPage.Instance.ToggleWatchStack(true, item.IsWatchlistItem);

                if (string.IsNullOrWhiteSpace(item.Tag)
                    || item.Tag == "MostPopular" || item.Tag == "MostVoted" || item.Tag == "SearchMovies" ||
                    item.Tag == "TelevisionNetworks" || item.Tag == "TelevisionNetwork" || item.Tag == "SearchTv" ||
                    item.Tag == "BestPicture" || item.Tag == "MovieGenres" ||
                    item.Tag == "MoviesPage" || item.Tag == "TelevisionPage" || item.Tag == "Watchlist" ||
                    item.Tag == "NewMovies" ||
                    string.IsNullOrEmpty(item.ImdbId))
                    return;

                MainPage.Instance.ChangeBackgroundImage(item.ImdbId, item.IsTv);

                var o = new OmdbApi();
                var title = item.Name;

                if (item.Season <= 0)
                {
                    if (title.LastIndexOf("(", StringComparison.Ordinal) != -1)
                    {
                        var replace = title.Substring(title.LastIndexOf("(", StringComparison.Ordinal));
                        title = title.Replace(replace, "");
                    }

                    var year = item.Name.Substring(item.Name.LastIndexOf("(", StringComparison.Ordinal) + 1);
                    year = year.Replace(")", "");
                    year = year.Replace("-", "");
                    var yearDate = 0;

                    int.TryParse(year, out yearDate);

                    var omdb = new OmdbResponse();
                    omdb.Title = item.Name;
                    omdb.imdbID = item.ImdbId;
                    omdb.Year = item.Year.ToString();

                    if (!item.IsTv)
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            try
                            {
                                MainPage.Instance.SetSidebarInformation(omdb, item.IsTv);
                            }
                            catch (Exception)
                            {
                            }
                        });
                    else
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            try
                            {
                                MainPage.Instance.SetSidebarInformation(omdb, item.IsTv, item.Season);
                            }
                            catch (Exception)
                            {
                            }
                        });
                }
                else
                {
                    var response = new OmdbResponse();
                    response.imdbID = item.ImdbId;
                    MainPage.Instance.SetSidebarInformation(response, item.IsTv, item.Season);
                }
            }
            else
            {
                MainPage.Instance.ToggleWatchStack(false);
            }
        }

        private async void NavList_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == VirtualKey.GamepadX)
            {
                var selectedIndex = NavList.SelectedIndex;
                if (selectedIndex > -1)
                {
                    var item = NavList?.Items?[selectedIndex] as NavigationItem;

                    if (item == null)
                        return;

                    if (item.IsWatchlistItem)
                    {
                        await App.Current.RemoveFromWatchlist(item);
                        var item2 = new NavigationItem();
                        item2.Tag = item.IsTv ? "TVWatchlist" : "MoviesWatchlist";
                        MainPage.Instance.Navigate(typeof(NavigationPage), item2);
                        return;
                    }

                    if (item.Tag.Equals("PlayTV") || item.Tag.Equals("TVSeasonList") || item.Tag.Equals("EpisodeList"))
                    {
                        item.Tag = "TVSeasonList";
                        item.ImdbLink = item.SeriesIMDB;

                        if (string.IsNullOrWhiteSpace(item.Name))
                            item.Name = string.IsNullOrWhiteSpace(item.SeriesName) ? item.Name : item.SeriesName;

                        item.IsWatchlistItem = true;
                        App.Current.AddToTVWatchlist(item);
                        var md =
                            new MessageDialog(ResourceLoader.GetForCurrentView().GetString("AddedWatchlist"));
                        await md.ShowAsync();
                    }
                    else if (item.Tag.Equals("MoviesSearch"))
                    {
                        item.IsWatchlistItem = true;
                        App.Current.AddToMovieWatchlist(item);
                        var md =
                            new MessageDialog(ResourceLoader.GetForCurrentView().GetString("AddedWatchlist"));
                        await md.ShowAsync();
                    }
                }
            }
        }

        private void NavList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
        }
    }

    public sealed class UpperFormatter : IValueConverter
    {
        // This converts the DateTime object to the string to display.
        public object Convert(object value, Type targetType,
            object parameter, string language)
        {
            var v = value as string;

            if (null != v)
                return v.ToUpper();

            return value;
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}