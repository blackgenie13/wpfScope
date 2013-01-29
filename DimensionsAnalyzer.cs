using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace wpfScope
{
    public class DimensionsAnalyzer : INotifyPropertyChanged
    {
        #region Properties

        public static readonly string ScreenshotBitmapPropertyName = "ScreenshotBitmap";
        private Bitmap _screenshotBitmap;
        public Bitmap ScreenshotBitmap
        { 
            get { return _screenshotBitmap; }
            set
            {
                if (_screenshotBitmap != value)
                {
                    _screenshotBitmap = value;
                    NotifyPropertyChanged(ScreenshotBitmapPropertyName);
                }
            }
        }
        
        public static readonly string ScreenshotImagePropertyName = "ScreenshotImage";
        private BitmapSource _screenshotImage;
        public BitmapSource ScreenshotImage
        {
            get { return _screenshotImage; }
            set
            {
                if (_screenshotImage != value)
                {
                    _screenshotImage = value;
                    NotifyPropertyChanged(ScreenshotImagePropertyName);
                }
            }
        }

        public System.Windows.Point Location { get; set; }

        public System.Windows.Size Size { get; set; }

        #endregion

        #region API

        public void UpdateScreenshot(Bitmap bmp)
        {
            if (bmp != null)
            {
                ScreenshotBitmap = bmp;
                ScreenshotImage = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                ScreenshotImage.Freeze();
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
