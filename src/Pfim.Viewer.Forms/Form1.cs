using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Pfim.Viewer.Forms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Images  (*.tga;*.dds)|*.tga;*.dds|All files (*.*)|*.*",
                Title = "Open File with Pfim"

            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var image = Pfim.FromFile(dialog.FileName);
            image.ApplyColorMap();

            PixelFormat format;
            switch (image.Format)
            {
                case ImageFormat.Rgb24:
                    format = PixelFormat.Format24bppRgb;
                    break;

                case ImageFormat.Rgba32:
                    format = PixelFormat.Format32bppArgb;
                    break;

                case ImageFormat.R5g5b5:
                    format = PixelFormat.Format16bppRgb555;
                    break;

                case ImageFormat.R5g6b5:
                    format = PixelFormat.Format16bppRgb565;
                    break;

                case ImageFormat.R5g5b5a1:
                    format = PixelFormat.Format16bppArgb1555;
                    break;

                case ImageFormat.Rgb8:
                    format = PixelFormat.Format8bppIndexed;
                    break;

                default:
                    var msg = $"{image.Format} is not recognized for Bitmap on Windows Forms. " +
                               "You'd need to write a conversion function to convert the data to known format";
                    var caption = "Unrecognized format";
                    MessageBox.Show(msg, caption, MessageBoxButtons.OK);
                    return;
            }

            unsafe
            {
                fixed (byte* p = image.Data)
                {
                    var bitmap = new Bitmap(image.Width, image.Height, image.Stride, format, (IntPtr) p);
                    if (format == PixelFormat.Format8bppIndexed)
                    {
                        var palette = bitmap.Palette;
                        for (int i = 0; i < 256; i++)
                        {
                            palette.Entries[i] = Color.FromArgb((byte)i, (byte)i, (byte)i);
                        }
                        bitmap.Palette = palette;
                    }

                    pictureBox.Image = bitmap;
                }
            }
        }
    }
}
