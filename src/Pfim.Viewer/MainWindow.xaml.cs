using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Pfim.Viewer
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Images  (*.tga;*.dds)|*.tga;*.dds|All files (*.*)|*.*",
                Title = "Open Files with Pfim"

            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            ImagePanel.Children.Clear();
            Progress.Visibility = Visibility.Visible;
            Progress.Value = 0;
            Progress.IsIndeterminate = true;

            var images = dialog.FileNames;

            Progress.IsIndeterminate = false;
            Progress.Maximum = images.Length;

            foreach (var file in images)
            {
                IImage image = await Task.Run(() => ParseImage(file));
                ImagePanel.Children.Add(WpfImage(image));
                Progress.Value += 1;
            }
            Progress.Visibility = Visibility.Collapsed;
        }

        private static IImage ParseImage(string file)
        {
            return Pfim.FromFile(file);
        }

        private static Image WpfImage(IImage image)
        {
            return new Image
            {
                Source = LoadImage(image),
                Width = image.Width,
                Height = image.Height,
                MaxHeight = image.Height,
                MaxWidth = image.Width,
                Margin = new Thickness(4)
            };
        }

        private static BitmapSource LoadImage(IImage image)
        {
            PixelFormat format;
            switch (image.Format)
            {
                case ImageFormat.Rgb24:
                    format = PixelFormats.Bgr24;
                    break;

                case ImageFormat.Rgba32:
                    format = PixelFormats.Bgr32;
                    break;

                default:
                    throw new Exception("Format not recognized");
            }

            // Create a WPF ImageSource and then set an Image to our variable.
            // Make sure you notify property changes as appropriate ;)
            return BitmapSource.Create(image.Width, image.Height,
                96.0, 96.0, format, null, image.Data, image.Stride);
        }
    }
}
