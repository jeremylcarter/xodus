using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Xodus
{
    public sealed partial class SubtitleSelectionDialog : ContentDialog
    {
        public SubtitleItem SelectedSub;
        private List<Dictionary<string, object>> subList;

        public StorageFile SubtitleFile;


        public SubtitleSelectionDialog(List<Dictionary<string, object>> subs)
        {
            var rl = new ResourceLoader();
            InitializeComponent();
            IsSecondaryButtonEnabled = true;
            IsPrimaryButtonEnabled = true;
            PrimaryButtonText = rl.GetString("Use");
            SecondaryButtonText = rl.GetString("Cancel");
            Title = rl.GetString("SubsTitle");

            subList = subs.OrderByDescending(x => x["rating"]).ToList();

            var subtitles = new List<SubtitleItem>();
            foreach (var sub in subs)
            {
                var item = new SubtitleItem();
                item.Name = sub["filename"] as string;
                item.Rating = (int) sub["rating"];
                item.Link = sub["link"] as string;
                subtitles.Add(item);
            }
            SubsList.ItemsSource = subtitles;
            if (SubsList?.Items?.Count > 0)
                SubsList.SelectedIndex = 0;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender,
            ContentDialogButtonClickEventArgs args)
        {
            var si = SubsList?.Items?[SubsList.SelectedIndex] as SubtitleItem;
            SelectedSub = si;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void SubsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var si = e.ClickedItem as SubtitleItem;
            SelectedSub = si;
            Hide();
        }

        public class SubtitleItem
        {
            public string Name { get; set; }
            public string Link { get; set; }
            public int Rating { get; set; }
        }
    }
}