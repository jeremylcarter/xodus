using System;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using Windows.Services.Store;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Xodus
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrialExpired : Page
    {
        public TrialExpired()
        {
            InitializeComponent();
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
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
    }
}