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

            PixelFormat format;
            switch (image.Format)
            {
                case ImageFormat.Rgb24:
                    format = PixelFormat.Format24bppRgb;
                    break;

                case ImageFormat.Rgba32:
                    format = PixelFormat.Format32bppArgb;
                    break;

                default:
                    throw new Exception("Format not recognized");
            }

            unsafe
            {
                fixed (byte* p = image.Data)
                {
                    var bitmap = new Bitmap(image.Width, image.Height, image.Stride, format, (IntPtr) p);
                    pictureBox.Image = bitmap;
                }
            }
        }
    }
}
