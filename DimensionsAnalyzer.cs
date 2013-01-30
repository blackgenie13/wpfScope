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
        #region Private Fields

        // Sync bitmap updates.
        private object _bitmapLock = new object();

        #endregion

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

                    UpdateGuidelines();
                }
            }
        }

        public static readonly string GuidelineCoordinatesPropertyName = "GuidelineCooordinates";
        private GuidelineCoordinates _guidelineCoordinates;
        public GuidelineCoordinates GuidelineCoordinates
        {
            get { return _guidelineCoordinates; }
            set
            {
                if (_guidelineCoordinates != value)
                {
                    _guidelineCoordinates = value;
                    NotifyPropertyChanged(GuidelineCoordinatesPropertyName);
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

        #endregion

        #region Constructor

        public DimensionsAnalyzer()
        {
            GuidelineCoordinates = new GuidelineCoordinates();
        }

        #endregion

        #region API

        public void UpdateScreenshot(Bitmap bmp)
        {
            if (bmp != null)
            {
                lock (_bitmapLock)
                {
                    ScreenshotBitmap = bmp;
                    ScreenshotImage = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }

                ScreenshotImage.Freeze();
            }
        }

        #endregion

        #region Helper Methods

        private Tuple<double, double> GetHorizontalBounds()
        {
            Tuple<double, double> range = null;

            return range;
        }

        private void UpdateGuidelines()
        {
            if (ScreenshotBitmap != null)
            {
                lock (_bitmapLock)
                {
                    GuidelineCoordinates.UpdateHorizontalGuideline(0, _cursorPosition.Y,
                                                                   ScreenshotBitmap.Width, _cursorPosition.Y);
                    GuidelineCoordinates.UpdateVerticalGuideline(_cursorPosition.X, 0,
                                                                 _cursorPosition.X, ScreenshotBitmap.Height);
                }

                NotifyPropertyChanged(GuidelineCoordinatesPropertyName);
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
