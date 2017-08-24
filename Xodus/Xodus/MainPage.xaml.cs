using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using UrlResolver;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Xodus
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public delegate void GamepadXFiredEventHandler(object e, KeyRoutedEventArgs x);

        public delegate void LoadingChangedEventHandler(object e, LoadingChangedEventArgs oe);

        public static MainPage Instance;

        private readonly string _currentPoster = "";
        private readonly CurrentTime _time = new CurrentTime();

        private readonly bool AdPlaying = false;
        private DispatcherTimer _adRefreshTimer = new DispatcherTimer();

        private DispatcherTimer _adTimer = new DispatcherTimer();

        private string _backgroundImdbId = "";
        private DispatcherTimer _positionTimer = new DispatcherTimer();
        private DispatcherTimer _weatherTimer = new DispatcherTimer();
        private NavigationItem currentMovie;
        private CancellationTokenSource navigationTokenSource;
        private CancellationTokenSource sidebarTokenSource;

        public MainPage()
        {
            InitializeComponent();
            KeyDown += MainPage_KeyDown;
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPageXBOX_BackRequested;
            ContentFrame.Navigated += ContentFrame_Navigated;
            LoadingChanged += MainPage_LoadingChanged;

            Instance = this;
        }

        public void SetNavText(string text)
        {
            try
            {
                NavText.Text = text;
            }
            catch (Exception)
            {
            }
        }

        private void MainPage_LoadingChanged(object e, LoadingChangedEventArgs oe)
        {
            if (null != SettingsButton)
                SettingsButton.IsEnabled = !LoadingControl.IsLoading;
        }

        public event LoadingChangedEventHandler LoadingChanged;

        public event GamepadXFiredEventHandler XPressed;

        private async void ConfigureWeather()
        {
            try
            {
                var s = new OpenWeatherMapAPI();
                await s.GetCurrentWeather();

                CurrentTemp.Text = s.GetCurrentTemperatureFarhenheit();
                TempIcon.Source = new BitmapImage(new Uri(s.GetWeatherIcon()));
                CurrentTemp.Visibility = Visibility.Visible;
                TempIcon.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
                CurrentTemp.Visibility = Visibility.Collapsed;
                TempIcon.Visibility = Visibility.Collapsed;
            }
        }

        private string GetTimeFormat(DateTime time)
        {
            return time.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern);
        }

        private async void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LoadingControl.IsLoading = false;
                LoadingChanged?.Invoke(this, new LoadingChangedEventArgs(LoadingControl.IsLoading));
                Player.IsFullWindow = false;
                Player.Visibility = Visibility.Collapsed;
            });
        }

        private async void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            sender.MediaFailed -= MediaPlayer_MediaFailed;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var rl = new ResourceLoader();
                var md = new MessageDialog(rl.GetString("NoSources"));
                try
                {
                    await md.ShowAsync();
                }
                catch (Exception)
                {
                }
            });

            MediaPlayer_MediaEnded(null, null);
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame) sender).CanGoBack
                    ? AppViewBackButtonVisibility.Visible
                    : AppViewBackButtonVisibility.Collapsed;
        }

        public void FocusContentFrame()
        {
            ContentFrame.Focus(FocusState.Programmatic);
        }

        private void MainPageXBOX_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (AdPlaying)
                return;

            if (Player.Visibility == Visibility.Visible)
            {
                Player.MediaPlayer.Pause();

                if (Player?.MediaPlayer?.PlaybackSession?.Position ==
                    Player?.MediaPlayer?.PlaybackSession.NaturalDuration)
                    App.Current.SavePosition(currentMovie, TimeSpan.Zero);
                if (null != _positionTimer)
                {
                    _positionTimer.Tick -= _positionTimer_Tick;
                    _positionTimer.Stop();
                    _positionTimer = null;
                }
                Player.MediaPlayer.MediaFailed -= MediaPlayer_MediaFailed;
                Player.MediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
                Player.MediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
                Player.IsFullWindow = false;
                Player.Visibility = Visibility.Collapsed;
                LoadingControl.IsLoading = false;
                LoadingChanged?.Invoke(this, new LoadingChangedEventArgs(LoadingControl.IsLoading));
                e.Handled = true;
                return;
            }

            if (LoadingControl.IsLoading)
                return;

            BackRequested();

            e.Handled = true;
        }

        public bool BackRequested()
        {
            if (AdPlaying)
                return false;

            if (ContentFrame == null)
                return false;

            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
                return true;
            }

            return false;
        }

        public void Navigate(Type type, object param)
        {
            ContentFrame.Navigate(type, param);
        }

        private void LoadInitialPage()
        {
        }

        public void ToggleLoadingControl(bool forceOff = false)
        {
            if (forceOff)
            {
                LoadingControl.IsLoading = false;
                LoadingChanged?.Invoke(this, new LoadingChangedEventArgs(LoadingControl.IsLoading));
                return;
            }

            LoadingControl.IsLoading = !LoadingControl.IsLoading;
            LoadingChanged?.Invoke(this, new LoadingChangedEventArgs(LoadingControl.IsLoading));
        }

        public async Task ChangeBackgroundImage(string imdb, bool isTv)
        {
            //if (isTv) { return; }

            if (!string.IsNullOrWhiteSpace(imdb))
            {
                var temp = App.Current.GetLinkFromCache(imdb);

                if (!string.IsNullOrWhiteSpace(temp))
                {
                    BackgroundImage.Source = new BitmapImage(new Uri(temp));
                    Debug.WriteLine("Picked Image From Cache");
                    return;
                }

                if (_backgroundImdbId.Equals(imdb))
                    return;

                _backgroundImdbId = imdb;

                try
                {
                    if (null == navigationTokenSource)
                        navigationTokenSource = new CancellationTokenSource();
                    else
                        navigationTokenSource = new CancellationTokenSource();

                    var tmdb = new Tmdb();
                    var imageSource = await tmdb.GetImdbBackdrop(imdb, navigationTokenSource, isTv);

                    App.Current.AddToCache(imdb, imageSource);

                    BackgroundImage.Source = new BitmapImage(new Uri(imageSource));
                }
                catch (Exception)
                {
                    try
                    {
                        BackgroundImage.Source = new BitmapImage(new Uri("http://imgur.com/uomkVIL.png"));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else if (!string.IsNullOrEmpty(_backgroundImdbId))
            {
                try
                {
                    BackgroundImage.Source = new BitmapImage(new Uri("http://imgur.com/uomkVIL.png"));
                }
                catch (Exception)
                {
                }
                _backgroundImdbId = "";
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
#if DEBUG
            AlignmentGrid.Visibility = Visibility.Collapsed;
#else
            AlignmentGrid.Visibility = Visibility.Collapsed;
#endif

            ToggleWatchStack(false);
            if (null != TrialTextStack)
                TrialTextStack.Visibility = App.Current.IsTrial ? Visibility.Visible : Visibility.Collapsed;

            if (App.Current.IsTrial)
            {
                _adRefreshTimer = new DispatcherTimer();
                _adRefreshTimer.Interval = TimeSpan.FromSeconds(30);
                _adRefreshTimer.Tick += _adRefreshTimer_Tick;
                _adRefreshTimer.Start();
            }

            ConfigureWeather();
            _weatherTimer = new DispatcherTimer();
            _weatherTimer.Interval = TimeSpan.FromMinutes(30);
            _weatherTimer.Tick += WeatherTimer_Tick;
            _weatherTimer.Start();

            if (e.Parameter == null)
            {
                LoadInitialPage();
                //NavigationList.SelectedIndex = 0;
                ContentFrame.Navigate(typeof(NavigationPage));
            }

            ContentFrame.Focus(FocusState.Programmatic);
        }

        private void _adRefreshTimer_Tick(object sender, object e)
        {
            if (!App.Current.AdReady)
            {
            }
        }

        private void _adTimer_Tick(object sender, object e)
        {
            /*
            if (App.Current.AdReady)
            {
                if (Player?.MediaPlayer?.PlaybackSession?.PlaybackState == MediaPlaybackState.Playing)
                {
                    AdPlaying = true;
                    App.Current.adControl.Completed += AdControl_Completed;
                    App.Current.adControl.Cancelled += AdControl_Cancelled;
                    App.Current.adControl.ErrorOccurred += AdControl_ErrorOccurred;
                    App.Current.PlayAd();
                    Player?.MediaPlayer?.Pause();
                }
            }
            */
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (null != _weatherTimer)
            {
                _weatherTimer.Stop();
                _weatherTimer.Tick -= WeatherTimer_Tick;
            }
        }

        private void WeatherTimer_Tick(object sender, object e)
        {
            ConfigureWeather();
        }

        public void SetCurrentMovie(NavigationItem item)
        {
            currentMovie = item;
        }

        public async void PlayMovie(string movie, string imdbid = null, StorageFile subtitle = null,
            TimeSpan? span = null)
        {
            Debug.WriteLine("Playing Movie: " + movie);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Visible;
            Player.Visibility = Visibility.Visible;
            var source = MediaSource.CreateFromUri(new Uri(movie));

            if (null != subtitle)
            {
                var timedText = TimedTextSource.CreateFromStream(await subtitle.OpenReadAsync());
                timedText.Resolved += TimedText_Resolved;
                source.ExternalTimedTextSources.Add(timedText);
            }

            if (App.Current.IsTrial)
            {
                if (null != _adTimer)
                {
                    _adTimer.Stop();
                    _adTimer.Tick -= _adTimer_Tick;
                    _adTimer = null;
                }

                _adTimer = new DispatcherTimer();
                _adTimer.Interval = TimeSpan.FromMinutes(7);
                _adTimer.Tick += _adTimer_Tick;
                _adTimer.Start();
            }

            var playbackItem = new MediaPlaybackItem(source);
            Player.Source = playbackItem;
            Player.MediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            Player.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            Player.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            Player.MediaPlayer.SystemMediaTransportControls.IsPauseEnabled = true;
            playbackItem.TimedMetadataTracksChanged += PlaybackItem_TimedMetadataTracksChanged;
            if (App.Current.IsXbox())
                Player.IsFullWindow = true;

            if (null != span)
            {
                var ts = (TimeSpan) span;

                if (span > TimeSpan.Zero && span > TimeSpan.MinValue)
                    Player.MediaPlayer.PlaybackSession.Position = (TimeSpan) span;
            }

            Player.MediaPlayer.Play();

            if (!string.IsNullOrWhiteSpace(imdbid))
            {
                if (_positionTimer != null)
                {
                    _positionTimer.Stop();
                    _positionTimer.Tick -= _positionTimer_Tick;
                    _positionTimer = null;
                }

                _positionTimer = new DispatcherTimer();
                _positionTimer.Interval = TimeSpan.FromSeconds(5);
                _positionTimer.Tick += _positionTimer_Tick;
                // _positionTimer.Start();
            }
        }

        private void _positionTimer_Tick(object sender, object e)
        {
            try
            {
                if (Player != null)
                {
                    var position = Player?.MediaPlayer?.PlaybackSession?.Position;

                    if (null != position)
                        App.Current.SavePosition(currentMovie, (TimeSpan) position);
                }
            }
            catch (Exception ex)
            {
                var y = 0;
            }
        }

        private void TimedText_Resolved(TimedTextSource sender, TimedTextSourceResolveResultEventArgs args)
        {
            if (args.Error != null)
            {
                Debug.WriteLine(args.Error);
                return;
            }
            args.Tracks[0].Label = "Default";
        }

        private void PlaybackItem_TimedMetadataTracksChanged(MediaPlaybackItem sender, IVectorChangedEventArgs args)
        {
            sender.TimedMetadataTracks.SetPresentationMode(0, TimedMetadataTrackPresentationMode.PlatformPresented);
        }

        private async void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            Debug.WriteLine("MediaOpened");

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LoadingControl.IsLoading = false;
                LoadingChanged?.Invoke(this, new LoadingChangedEventArgs(LoadingControl.IsLoading));
            });
        }

        private void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.GamepadMenu)
            {
                e.Handled = true;
                ChangeBackgroundImage("", false);
                ContentFrame.Navigate(typeof(SettingsPage));
                return;
            }
            if (e.Key == VirtualKey.GamepadX)
                XPressed?.Invoke(this, e);
            if (e.Key == VirtualKey.GamepadB)
                if (Player.Visibility == Visibility.Visible)
                {
                    if (null != Player.MediaPlayer)
                    {
                        Player.MediaPlayer.Pause();
                        Player.MediaPlayer.MediaFailed -= MediaPlayer_MediaFailed;
                        Player.MediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
                        Player.MediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
                    }

                    Player.Visibility = Visibility.Collapsed;
                    LoadingControl.IsLoading = false;
                    e.Handled = true;
                    return;
                }

            App.Current.KeydownFired();
        }

        private string GetJsonString(string json)
        {
            if (json == null)
                return "";

            return json;
        }

        public async void UpdateLoadingText(string text)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var texblock = LoadingControl.GetChildrenOfType<TextBlock>();

                foreach (var shit in texblock)
                    shit.Text = text;
            });
        }

        public async void SetSidebarInformation(OmdbResponse omdb, bool IsTv = false, int season = 0)
        {
            TitleText.Text = "";
            DescriptionText.Text = "";

            lock (this)
            {
                ScoreSpan.Children.Clear();
            }

            if (null == omdb)
            {
                SelectionImage.Source = null;
                return;
            }

            if (!string.IsNullOrEmpty(omdb.imdbID))
            {
                var si = App.Current.GetCacheSidebarInfo(omdb.imdbID);

                if (null == si)
                {
                    if (null == sidebarTokenSource)
                        sidebarTokenSource = new CancellationTokenSource();

                    var tmdb = new Tmdb();
                    var test = await tmdb.GetPoster(omdb.imdbID, sidebarTokenSource, IsTv, season);

                    if (!string.IsNullOrWhiteSpace(test.Item2))
                        omdb.Poster = test.Item2;

                    if (!string.IsNullOrWhiteSpace(test.Item1))
                        omdb.Plot = test.Item1;

                    App.Current.AddToSidebarCache(omdb.imdbID, GetJsonString(omdb.Poster), omdb.Title,
                        GetJsonString(omdb.Plot));
                }
                else
                {
                    omdb.Poster = si.link;
                    omdb.Plot = si.description;
                }
            }

            if (!string.IsNullOrEmpty(omdb.Title))
                TitleText.Text = GetJsonString(omdb.Title);

            if (!string.IsNullOrEmpty(omdb.Plot))
                DescriptionText.Text = GetJsonString(omdb.Plot);

            if (null != omdb.Ratings)
                lock (this)
                {
                    ScoreSpan.Children.Clear();
                    foreach (var score in omdb.Ratings)
                    {
                        var tb = new TextBlock();
                        tb.Text = score.Source + ": " + score.Value;
                        tb.Margin = new Thickness(0, 0, 5, 0);
                        ScoreSpan.Children.Add(tb);
                    }
                }

            if (_currentPoster.Equals(omdb.Poster))
                return;

            if (!string.IsNullOrEmpty(omdb.Poster))
                try
                {
                    var uri = new Uri(omdb.Poster);
                    SelectionImage.Source = new BitmapImage(uri);
                }
                catch (Exception)
                {
                }
            else
                SelectionImage.Source = null;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeBackgroundImage(null, false);
            ContentFrame.Navigate(typeof(SettingsPage));
        }

        public void ToggleWatchStack(bool enable, bool remove = false)
        {
            if (WatchText == null)
                return;

            if (remove)
                WatchText.Text = ResourceLoader.GetForCurrentView().GetString("RemoveFromWatchlist");
            else
                WatchText.Text = ResourceLoader.GetForCurrentView().GetString("AddToWatchlistReal");
            if (WatchStack != null)
                WatchStack.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}