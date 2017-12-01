using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StellarCartography.Model
{
    class PictureRect
    {
        public Double X { get; set; }
        public Double Y { get; set; }
        public Double Width
        {
            get
            {
                return Image.Width;
            }
        }
        public Double Height
        {
            get
            {
                return Image.Height;
            }
        }
        public Bitmap Image { get; set; }
        public BitmapSource Source { get; set; }

        public PictureRect(Double x, Double y, Bitmap image)
        {
            this.X = x;
            this.Y = y;
            this.Image = image;
            this.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                         image.GetHbitmap(),
                         IntPtr.Zero,
                         Int32Rect.Empty,
                         BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
