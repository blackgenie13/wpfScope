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

        private void UpdateGuidelines()
        {
            lock (_bitmapLock)
            {
                if (ScreenshotBitmap != null)
                {
                    int bh = ScreenshotBitmap.Height;
                    int bw = ScreenshotBitmap.Width;
                    int cx = (int)CursorPosition.X;
                    int cy = (int)CursorPosition.Y;
                    Color color = ScreenshotBitmap.GetPixel(cx, cy);

                    // Get the horizontal guideline bounds.
                    int left = 0;
                    int right = bw-1;
                    for (int i = cx; i > 0; i--) { if (ScreenshotBitmap.GetPixel(i, cy) != color) { left = i; break; } }
                    for (int i = cx; i < bw; i++) { if (ScreenshotBitmap.GetPixel(i, cy) != color) { right = i; break; } }

                    // Get the vertical guideline bounds.
                    int top = 0;
                    int bottom = bh-2;
                    for (int j = cy; j > 0; j--) { if (ScreenshotBitmap.GetPixel(cx, j) != color) { top = j; break; } }
                    for (int j = cy; j < bh; j++) { if (ScreenshotBitmap.GetPixel(cx, j) != color) { bottom = j; break; } }

                    // Set all the guideline points.
                    GuidelineCoordinates.UpdateHorizontalGuideline(left, cy,
                                                                   right, cy);
                    GuidelineCoordinates.UpdateVerticalGuideline(cx, top,
                                                                 cx, bottom);
                    NotifyPropertyChanged(GuidelineCoordinatesPropertyName);
                }
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
