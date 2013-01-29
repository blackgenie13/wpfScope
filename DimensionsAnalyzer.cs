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

        public static readonly string CursorPositionPropertyName = "CursorPosition";
        private System.Windows.Point _cursorPosition;
        public System.Windows.Point CursorPosition
        {
            get { return _cursorPosition; }
            set
            {
                if (_cursorPosition != value)
                {
                    _cursorPosition = value;
                    NotifyPropertyChanged(CursorPositionPropertyName);
                }
            }
        }

        public static readonly string HorizontalLineCoordinatesPropertyName = "HorizontalLineCoordinates";
        private LineCoordinates _horizontalLineCoordinates;
        public LineCoordinates HorizontalLineCoordinates
        {
            get { return _horizontalLineCoordinates; }
            set
            {
                if (_horizontalLineCoordinates != value)
                {
                    _horizontalLineCoordinates = value;
                    NotifyPropertyChanged(HorizontalLineCoordinatesPropertyName);
                }
            }
        }

        public System.Windows.Point Location { get; set; }

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

        public System.Windows.Size Size { get; set; }

        public static readonly string VerticalLineCoordinatesPropertyName = "VerticalLineCoordinates";
        private LineCoordinates _verticalLineCoordinates;
        public LineCoordinates VerticalLineCoordinates
        {
            get { return _verticalLineCoordinates; }
            set
            {
                if (_verticalLineCoordinates != value)
                {
                    _verticalLineCoordinates = value;
                    NotifyPropertyChanged(VerticalLineCoordinatesPropertyName);
                }
            }
        }

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

    public struct LineCoordinates
    {
        public System.Windows.Point Start, End;

        public LineCoordinates(System.Windows.Point start, System.Windows.Point end)
        {
            Start = start;
            End = end;
        }

        public override bool Equals(object obj)
        {
            return obj is LineCoordinates && this == (LineCoordinates)obj;
        }

        public static bool operator ==(LineCoordinates lc1, LineCoordinates lc2)
        {
            return lc1.Start == lc2.Start && lc1.End == lc2.End;
        }

        public static bool operator !=(LineCoordinates lc1, LineCoordinates lc2)
        {
            return !(lc1 == lc2);
        }
    }

}
