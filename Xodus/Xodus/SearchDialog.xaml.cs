using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Xodus
{
    public sealed partial class SearchDialog : ContentDialog
    {
        public SearchDialog()
        {
            InitializeComponent();
            var rl = new ResourceLoader();
            Title = rl.GetString("SearchDialogTitle");
            PrimaryButtonText = rl.GetString("SearchDialogOK");
            SecondaryButtonText = rl.GetString("SearchDialogCancel");
            DefaultButton = ContentDialogButton.Primary;
            SearchBox.QuerySubmitted += SearchBox_QuerySubmitted;
        }

        public string Query { get; set; }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Query = SearchBox.Text;
            Hide();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Query = SearchBox.Text;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Query = "";
        }
    }
}