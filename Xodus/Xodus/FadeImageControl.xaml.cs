using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Xodus
{
    public sealed partial class FadeImageControl : UserControl
    {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(FadeImageControl),
                new PropertyMetadata(default(ImageSource), SourceChanged));

        public static readonly DependencyProperty PlaceHolderProperty =
            DependencyProperty.Register("PlaceHolder", typeof(ImageSource), typeof(FadeImageControl),
                new PropertyMetadata(default(ImageSource)));

        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(Stretch), typeof(FadeImageControl),
                new PropertyMetadata(default(Stretch)));

        public FadeImageControl()
        {
            InitializeComponent();
        }

        public ImageSource Source
        {
            get => (ImageSource) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public ImageSource PlaceHolder
        {
            get => (ImageSource) GetValue(PlaceHolderProperty);
            set => SetValue(PlaceHolderProperty, value);
        }

        public Stretch Stretch
        {
            get => (Stretch) GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        private static void SourceChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = (FadeImageControl) dependencyObject;
            var newSource = (ImageSource) dependencyPropertyChangedEventArgs.NewValue;


            Debug.WriteLine("Image source changed: {0}", ((BitmapImage) newSource)?.UriSource?.AbsolutePath);


            if (newSource != null)
            {
                var image = (BitmapImage) newSource;

                // If the image is not a local resource or it was not cached
                if (image?.UriSource?.Scheme != "ms-appx" && image?.UriSource?.Scheme != "ms-resource" &&
                    image?.PixelHeight * image?.PixelWidth == 0)
                {
                    image.ImageOpened += (sender, args) => control.LoadImage(image);
                    control.Staging.Source = image;
                }
                else
                {
                    control.LoadImage(newSource);
                }
            }
        }

        private void LoadImage(ImageSource source)
        {
            ImageFadeOut.Completed += (s, e) =>
            {
                Image.Source = source;
                ImageFadeIn.Begin();
            };
            ImageFadeOut.Begin();
        }
    }
}