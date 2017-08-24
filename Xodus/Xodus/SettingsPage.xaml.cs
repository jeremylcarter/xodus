using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Services.Store;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Xodus
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private readonly List<string> Languages = new List<string>();

        public SettingsPage()
        {
            InitializeComponent();
            var ss = new Subscene();
            Languages.AddRange(ss.SupportedLanguages.Keys.ToList());
            var selectedlanguage = "";

            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;
            VersionText.Text = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            if (ApplicationData.Current.LocalSettings.Values["subtitlelanguage"] != null)
                selectedlanguage = (string) ApplicationData.Current.LocalSettings.Values["subtitlelanguage"];

            LanguageBox.ItemsSource = Languages;

            var selectedIndex = 0;

            if (Languages.Contains(selectedlanguage))
                selectedIndex = Languages.IndexOf(selectedlanguage);
            if (LanguageBox?.Items?.Count > 0)
                LanguageBox.SelectedIndex = selectedIndex;

            if (ApplicationData.Current.LocalSettings.Values["subtitlesenabled"] != null)
                SubtitleEnableBox.IsChecked = (bool) ApplicationData.Current.LocalSettings.Values["subtitlesenabled"];

            PairedProviderBox.IsChecked = App.Current.GetBoolSetting("enablepairing");
        }

        private void SubtitleEnableBox_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["subtitlesenabled"] = true;
        }

        private void SubtitleEnableBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["subtitlesenabled"] = false;
        }

        private void LanguageBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = LanguageBox?.Items?[LanguageBox.SelectedIndex] as string;
            ApplicationData.Current.LocalSettings.Values["subtitlelanguage"] = s;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!App.Current.IsTrial)
                PurchaseStack.Visibility = Visibility.Collapsed;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var productResult = await StoreContext.GetDefault().GetStoreProductForCurrentAppAsync();

            if (productResult.ExtendedError != null)

            {
                // The user may be offline or there might be some other server failure

                Debug.WriteLine($"ExtendedError: {productResult.ExtendedError.Message}");

                var text = new ResourceLoader().GetString("MicrosoftServer");
                text += productResult.ExtendedError.Message;

                var md = new MessageDialog(text);
                await md.ShowAsync();
                return;
            }

            var result = await productResult.Product.RequestPurchaseAsync();

            switch (result.Status)
            {
                case StorePurchaseStatus.Succeeded:
                case StorePurchaseStatus.AlreadyPurchased:
                {
                    var frame = Window.Current.Content as Frame;
                    frame?.Navigate(typeof(MainPage), null);
                }
                    break;
            }
        }

        private void PairedProviderBox_Checked(object sender, RoutedEventArgs e)
        {
            App.Current.SetBoolSetting("enablepairing", true);
        }

        private void PairedProviderBox_Unchecked(object sender, RoutedEventArgs e)
        {
            App.Current.SetBoolSetting("enablepairing", false);
        }
    }
}