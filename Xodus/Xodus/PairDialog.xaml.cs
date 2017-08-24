using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using UrlResolver;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Xodus
{
    public sealed partial class PairDialog : ContentDialog
    {
        private readonly DispatcherTimer _checkAuthTimer;
        private readonly IPairedAuthorizationSource _pairSource;
        private bool _isAuthorized;

        public PairDialog(IPairedAuthorizationSource source)
        {
            InitializeComponent();
            Closing += PairDialog_Closing;
            _pairSource = source;
            var rl = new ResourceLoader();
            Title = rl.GetString("AuthorizationRequired");
            PairLink.NavigateUri = new Uri(source.GetPairUri());
            PrimaryButtonText = rl.GetString("Retry");
            IsPrimaryButtonEnabled = true;
            IsSecondaryButtonEnabled = true;
            SecondaryButtonText = rl.GetString("Cancel");
            var text = new TextBlock();
            text.Margin = new Thickness(5);
            text.Text = source.GetPairUri();
            PairLink.Content = text;
            _checkAuthTimer = new DispatcherTimer();
            _checkAuthTimer.Tick += CheckAuthTimer_Tick;
            _checkAuthTimer.Interval = TimeSpan.FromSeconds(10);
            // _checkAuthTimer.Start();
        }

        private void PairDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (_checkAuthTimer.IsEnabled)
                //_checkAuthTimer.Stop();

                _checkAuthTimer.Tick -= CheckAuthTimer_Tick;
        }

        public bool IsAuthorized()
        {
            return _isAuthorized;
        }

        private async void CheckAuthTimer_Tick(object sender, object e)
        {
            if (await _pairSource.CheckAuthorization())
            {
                _isAuthorized = true;
                //_checkAuthTimer.Stop();
                Hide();
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender,
            ContentDialogButtonClickEventArgs args)
        {
            if (await _pairSource.CheckAuthorization())
            {
                _isAuthorized = true;
                //_checkAuthTimer.Stop();
                Hide();
            }
            else
            {
                args.Cancel = true;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}