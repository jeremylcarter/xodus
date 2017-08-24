using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Services.Store;
using Windows.Storage;
using Windows.System.Display;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.HockeyApp;
using Newtonsoft.Json;
using UrlResolver;

namespace Xodus
{
    public class SidebarInfo
    {
        public string link { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }

    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private class MoviePosition
        {
            public string ImdbId { get; set; }
            public TimeSpan Position { get; set; }
            public int Season { get; set; }
            public int Episode { get; set; }
        }

        private class WatchList
        {
            public readonly List<NavigationItem> Movies = new List<NavigationItem>();
            public readonly List<NavigationItem> TVShows = new List<NavigationItem>();
        }


        private Dictionary<string, SidebarInfo> sidebarCache = new Dictionary<string, SidebarInfo>();
        private Dictionary<string, string> imageCache = new Dictionary<string, string>();

        private List<MoviePosition> MoviePositions = new List<MoviePosition>();

        private WatchList _watchList;

        private static bool _xboxCheck;
        private const int MAX_POSITIONS = 10;

        private DisplayRequest _displayRequest;
        private DispatcherTimer _idleTimer;
        private bool _isIdle = true;

        private bool _isXbox;
        private StoreAppLicense _storeAppLicense;
        private StoreContext _storeContext;
        public IReadOnlyList<StorePackageUpdate> StoreUpdates;

#if DEBUG
        public static readonly string AppId = "d25517cb-12d4-4699-8bdc-52040c712cab";
        public static readonly string AdUnitId = "test";
#else
        public static readonly string AppId = "9mxvglxqb7sc";
        public static readonly string AdUnitId = "345470";
#endif
        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            HockeyClient.Current.Configure("e0453f2f10fb45ba86ee1302bd2491a8");

            if (IsXbox())
                RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
        }

        public new static App Current => (App) Application.Current;

        public bool AdReady = false;

        public bool IsTrial => false;

        public bool IsIdle
        {
            get => _isIdle;
            set
            {
                if (_isIdle != value)
                {
                    _isIdle = value;
                    IsIdleChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void AddToCache(string imdb, string link)
        {
            while (imageCache.Count > 1000)
                imageCache.Remove(imageCache.ElementAt(0).Key);

            if (!imageCache.ContainsKey(imdb))
                imageCache.Add(imdb, link);
        }


        public void AddToSidebarCache(string imdb, string link, string title, string description)
        {
            while (sidebarCache.Count > 1000)
                sidebarCache.Remove(imageCache.ElementAt(0).Key);

            if (string.IsNullOrWhiteSpace(link))
                return;

            if (!sidebarCache.ContainsKey(imdb))
            {
                var si = new SidebarInfo
                {
                    link = link,
                    title = title,
                    description = description
                };

                sidebarCache.Add(imdb, si);
            }
        }

        public SidebarInfo GetCacheSidebarInfo(string imdb)
        {
            var fixup = false;

            SidebarInfo si = null;

            if (sidebarCache.ContainsKey(imdb))
            {
                si = sidebarCache[imdb];


                if (string.IsNullOrWhiteSpace(si.link))
                {
                    fixup = true;
                    si = null;
                }
            }

            if (fixup)
            {
                sidebarCache.Remove(imdb);
                return null;
            }
            return si;

            return null;
        }

        public string GetLinkFromCache(string imdb)
        {
            if (imageCache.ContainsKey(imdb))
                return imageCache[imdb];

            return "";
        }

        public event EventHandler IsIdleChanged;

        private async Task<bool> InitializeLicense()
        {
            if (_storeContext == null)
                _storeContext = StoreContext.GetDefault();

            _storeContext.OfflineLicensesChanged += StoreContext_OfflineLicensesChanged;

            _storeAppLicense = await _storeContext.GetAppLicenseAsync();

            if (_storeAppLicense.IsActive)
                if (_storeAppLicense.IsTrial)
                    if (DateTime.Now >= _storeAppLicense.ExpirationDate.DateTime)
                        return true;

            return false;
        }

        private void StoreContext_OfflineLicensesChanged(StoreContext sender, object args)
        {
            Frame frame = null;
            if (Window.Current != null && Window.Current.Content != null)
                frame = Window.Current.Content as Frame;

            if (_storeAppLicense.IsActive)
            {
                if (_storeAppLicense.IsTrial)
                    if (DateTime.Now >= _storeAppLicense.ExpirationDate.DateTime)
                        if (Window.Current != null && Window.Current.Content != null)
                            frame?.Navigate(typeof(TrialExpired), null);
            }
            else
            {
                frame?.Navigate(typeof(TrialExpired), null);
            }
        }

        public void KeydownFired()
        {
            Debug.WriteLine("Keydown Fired");
            CoreWindow_PointerMoved(null, null);
        }

        private void CoreWindow_PointerMoved(CoreWindow sender, PointerEventArgs args)
        {
            _idleTimer.Stop();
            _idleTimer.Start();
            IsIdle = false;
        }

        private void IdleTimer_Tick(object sender, object e)
        {
            _idleTimer.Stop();
            IsIdle = true;
        }

        public bool IsXbox()
        {
            if (_xboxCheck)
                return _isXbox;

            _isXbox = AnalyticsInfo.VersionInfo.DeviceFamily.ToLower().Contains("xbox");
            _xboxCheck = true;
            return _isXbox;
        }

        public void SetBoolSetting(string key, bool value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
        }

        public bool GetBoolSetting(string key)
        {
            var setting = ApplicationData.Current.LocalSettings.Values[key] as bool?;

            if (setting == null)
                return false;

            return (bool) setting;
        }

        public void SetIntSetting(string key, int value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
        }

        public int GetIntSetting(string key)
        {
            var setting = ApplicationData.Current.LocalSettings.Values[key] as int?;

            if (setting == null)
                return 0;

            return (int) setting;
        }


        private async Task LoadPersistedItems()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder dataFolder;

            try
            {
                dataFolder = await localFolder.GetFolderAsync("data");
            }
            catch (Exception)
            {
                dataFolder = await localFolder.CreateFolderAsync("data");
            }


            StorageFile sideCacheFile = null;


            try
            {
                sideCacheFile = await dataFolder.GetFileAsync("sidecache.json");
            }
            catch (Exception)
            {
                sideCacheFile = await dataFolder.CreateFileAsync("sidecache.json");
            }

            var sideJson = await FileIO.ReadTextAsync(sideCacheFile);

            sidebarCache = JsonConvert.DeserializeObject<Dictionary<string, SidebarInfo>>(sideJson);

            if (sidebarCache == null)
                sidebarCache = new Dictionary<string, SidebarInfo>();


            StorageFile imageCacheFile = null;


            try
            {
                imageCacheFile = await dataFolder.GetFileAsync("imagecache.json");
            }
            catch (Exception)
            {
                imageCacheFile = await dataFolder.CreateFileAsync("imagecache.json");
            }

            var imageJson = await FileIO.ReadTextAsync(imageCacheFile);

            imageCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(imageJson);

            if (imageCache == null)
                imageCache = new Dictionary<string, string>();

            StorageFile playlistFile = null;

            try
            {
                playlistFile = await dataFolder.GetFileAsync("watchlist.json");
            }
            catch (Exception)
            {
                playlistFile = await dataFolder.CreateFileAsync("watchlist.json");
            }

            var playlistJson = await FileIO.ReadTextAsync(playlistFile);


            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                _watchList = JsonConvert.DeserializeObject<WatchList>(playlistJson, settings);
            }
            catch (Exception)
            {
                _watchList = new WatchList();
            }

            if (_watchList == null)
                _watchList = new WatchList();

            StorageFile moviePositionFile = null;

            try
            {
                moviePositionFile = await dataFolder.GetFileAsync("movieposition.json");
            }
            catch (Exception)
            {
                moviePositionFile = await dataFolder.CreateFileAsync("movieposition.json");
            }

            var positionJson = await FileIO.ReadTextAsync(moviePositionFile);


            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                MoviePositions = JsonConvert.DeserializeObject<List<MoviePosition>>(positionJson, settings);
            }
            catch (Exception)
            {
                MoviePositions = new List<MoviePosition>();
            }

            if (MoviePositions == null)
                MoviePositions = new List<MoviePosition>();
        }

        public ObservableCollection<NavigationItem> GetMoviesWatchList()
        {
            var list = new ObservableCollection<NavigationItem>();

            foreach (var i in _watchList?.Movies)
                list.Add(i);
            return list;
        }

        public ObservableCollection<NavigationItem> GetTVWatchList()
        {
            var list = new ObservableCollection<NavigationItem>();

            foreach (var i in _watchList?.TVShows)
                list.Add(i);
            return list;
        }

        private async Task PersistItems()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder dataFolder;

            try
            {
                dataFolder = await localFolder.GetFolderAsync("data");
            }
            catch (Exception)
            {
                dataFolder = await localFolder.CreateFolderAsync("data");
            }

            if (dataFolder == null)
                return;


            StorageFile sideFile = null;

            try
            {
                sideFile =
                    await dataFolder.CreateFileAsync("sidecache.json", CreationCollisionOption.ReplaceExisting);
            }
            catch (Exception)
            {
                sideFile = null;
            }

            if (sideFile == null)
                return;

            try
            {
                await FileIO.WriteTextAsync(sideFile, JsonConvert.SerializeObject(sidebarCache));
            }
            catch (Exception)
            {
            }

            StorageFile imageFile = null;

            try
            {
                imageFile =
                    await dataFolder.CreateFileAsync("imagecache.json", CreationCollisionOption.ReplaceExisting);
            }
            catch (Exception)
            {
                imageFile = null;
            }

            if (imageFile == null)
                return;

            try
            {
                await FileIO.WriteTextAsync(imageFile, JsonConvert.SerializeObject(imageCache));
            }
            catch (Exception)
            {
            }

            StorageFile playlistsFile = null;

            try
            {
                playlistsFile =
                    await dataFolder.CreateFileAsync("watchlist.json", CreationCollisionOption.ReplaceExisting);
            }
            catch (Exception)
            {
                playlistsFile = null;
            }

            if (playlistsFile == null)
                return;

            try
            {
                await FileIO.WriteTextAsync(playlistsFile, JsonConvert.SerializeObject(_watchList));
            }
            catch (Exception)
            {
            }

            StorageFile moviePositionFile = null;

            try
            {
                moviePositionFile =
                    await dataFolder.CreateFileAsync("movieposition.json", CreationCollisionOption.ReplaceExisting);
            }
            catch (Exception)
            {
                moviePositionFile = null;
            }

            if (moviePositionFile == null)
                return;

            try
            {
                await FileIO.WriteTextAsync(moviePositionFile, JsonConvert.SerializeObject(MoviePositions));
            }
            catch (Exception ex)
            {
                var x = 0;
            }
        }

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (IsXbox())
            {
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode
                    (ApplicationViewBoundsMode.UseCoreWindow);
                Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("ms-appx:///TenFoot.xaml")
                });
            }

            await LoadPersistedItems();

            if (ApplicationData.Current.LocalSettings.Values["enablepairing"] == null)
                Current.SetBoolSetting("enablepairing", true);

            var resourceLoader = new ResourceLoader();
            _displayRequest = new DisplayRequest();
            _displayRequest.RequestActive();


            _idleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };

            _idleTimer.Tick += IdleTimer_Tick;

            Window.Current.CoreWindow.PointerMoved += CoreWindow_PointerMoved;

            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
                rootFrame.Navigated += RootFrame_Navigated;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }


                SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    rootFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                var mandatoryUpdate = false;
                IReadOnlyList<StorePackageUpdate> updates = null;
                try
                {
                    var storeContext = StoreContext.GetDefault();
                    updates = await storeContext.GetAppAndOptionalStorePackageUpdatesAsync();
                    foreach (var update in updates)
                        if (update.Mandatory)
                            mandatoryUpdate = true;
                }
                catch (Exception)
                {
                }

                if (rootFrame.Content == null)
                {
                    var navMainFrame = true;

                    resourceLoader = new ResourceLoader();
                    if (mandatoryUpdate)
                    {
                        var messageDialog = new MessageDialog(resourceLoader.GetString("UpdateText"),
                            resourceLoader.GetString("UpdateCaption"));
                        messageDialog.Commands.Add(new UICommand(resourceLoader.GetString("YES")));
                        var command = await messageDialog.ShowAsync();

                        if (command.Label.Equals(resourceLoader.GetString("YES"),
                            StringComparison.CurrentCultureIgnoreCase))
                        {
                            StoreUpdates = updates;
                            rootFrame.Navigate(typeof(UpdatePage), e.Arguments);
                            navMainFrame = false;
                        }
                    }

                    if (navMainFrame)
                    {
                        var trialExpired = await InitializeLicense();

                        if (trialExpired)
                            rootFrame.Navigate(typeof(TrialExpired), null);
                        else
                            rootFrame.Navigate(typeof(MainPage));
                    }
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;

            if (null == rootFrame)
                return;

            if (rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Each time a navigation event occurs, update the Back button's visibility
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame) sender).CanGoBack
                    ? AppViewBackButtonVisibility.Visible
                    : AppViewBackButtonVisibility.Collapsed;
        }

        /// <summary>
        ///     Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        public async void AddToMovieWatchlist(NavigationItem item)
        {
            _watchList?.Movies?.Add(item);
            await PersistItems();
        }

        public async void AddToTVWatchlist(NavigationItem item)
        {
            _watchList?.TVShows?.Add(item);
            await PersistItems();
        }

        public async Task RemoveFromWatchlist(NavigationItem item)
        {
            _watchList?.TVShows?.Remove(item);
            _watchList?.Movies?.Remove(item);
            await PersistItems();
        }


        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            await PersistItems();

            deferral.Complete();
        }

        public async void SavePosition(NavigationItem item, TimeSpan ts)
        {
            try
            {
                lock (MoviePositions)
                {
                    while (MoviePositions.Count > MAX_POSITIONS)
                        MoviePositions.RemoveAt(0);

                    if (!MoviePositions.Any(x => x.ImdbId == item.ImdbId && x.Season == item.Season &&
                                                 x.Episode == item.Episode))
                    {
                        var mp = new MoviePosition
                        {
                            ImdbId = item.ImdbId,
                            Position = ts,
                            Season = item.Season,
                            Episode = item.Episode
                        };
                        MoviePositions.Add(mp);
                    }
                    else
                    {
                        var mp = MoviePositions.FirstOrDefault(x => x.ImdbId == item.ImdbId &&
                                                                    x.Season == item.Season && x.Episode ==
                                                                    item.Episode);
                        if (null != mp)
                            mp.Position = ts;
                    }

                    PersistItems();
                }
            }
            catch (Exception)
            {
            }
        }

        public TimeSpan GetMoviePosition(NavigationItem item)
        {
            var mp = MoviePositions.FirstOrDefault(x => x.ImdbId == item.ImdbId && x.Season == item.Season &&
                                                        x.Episode == item.Episode);
            if (mp != null)
                return mp.Position;

            return TimeSpan.Zero;
        }
    }
}