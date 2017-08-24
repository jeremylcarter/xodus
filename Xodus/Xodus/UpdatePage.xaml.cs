using System;
using System.Diagnostics;
using Windows.Services.Store;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.HockeyApp;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Xodus
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UpdatePage : Page
    {
        public UpdatePage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var app = Application.Current as App;

            if (app == null)
                return;

            var downloadOperation =
                StoreContext.GetDefault().RequestDownloadAndInstallStorePackageUpdatesAsync(app.StoreUpdates);

            downloadOperation.Progress = async (asyncInfo, progress) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { ProgressBar.Value = progress.PackageDownloadProgress; });
            };

            var result = await downloadOperation.AsTask();


            Debug.WriteLine(result.OverallState);


            if (result.OverallState == StorePackageUpdateState.Completed)
            {
                HockeyClient.Current.TrackEvent("Update Completed");
                app.Exit();
            }
            else
            {
                HockeyClient.Current.TrackEvent("Download " + result.OverallState);
                var frame = Window.Current.Content as Frame;
                frame?.Navigate(typeof(MainPage), null);
            }
        }
    }
}