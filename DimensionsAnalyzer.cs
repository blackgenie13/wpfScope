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

        public static readonly string DimensionsStringPropertyName = "DimensionsString";
        private string _dimensionsString;
        public string DimensionsString
        {
            get { return _dimensionsString; }
            set
            {
                if (_dimensionsString != value)
                {
                    _dimensionsString = value;
                    NotifyPropertyChanged(DimensionsStringPropertyName);
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
                    IntPtr hbitmap = IntPtr.Zero;

                    try
                    {
                        if (ScreenshotBitmap != null) { ScreenshotBitmap.Dispose(); } // Kill memory leaks.

                        ScreenshotBitmap = bmp;
                        hbitmap = bmp.GetHbitmap();

                        ScreenshotImage = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    }
                    catch (Exception e)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(e);
#endif
                    }
                    finally
                    {
                        if (ScreenshotImage != null)
                        {
                            ScreenshotImage.Freeze();
                        }

                        if (hbitmap != IntPtr.Zero)
                        {
                            Win32API.DeleteObject(hbitmap);
                        }
                    }
                }

            }
        }

        #endregion

        #region Helper Methods

        private void UpdateDimensionString(int left, int right, int top, int bottom)
        {
            DimensionsString = String.Format("{0} x {1} px", Math.Max(right - left + 1, 0), Math.Max(bottom - top + 1, 0));
        }

        private void UpdateGuidelines()
        {
            bool shouldUpdate = false;
            int bw = 0;
            int bh = 0;
            int cx = (int)CursorPosition.X;
            int cy = (int)CursorPosition.Y;
            int left = 0;
            int right = 0;
            int top = 0;
            int bottom = 0;

            lock (_bitmapLock)
            {
                if (ScreenshotBitmap != null)
                {
                    try
                    {
                        bw = ScreenshotBitmap.Width;
                        bh = ScreenshotBitmap.Height;

                        if (cx < bw && cy < bh)
                        {
                            Color color = ScreenshotBitmap.GetPixel(cx, cy);

                            // Get the horizontal guideline bounds.
                            right = bw - 1;
                            for (int i = cx; i > 0; i--) { if (ScreenshotBitmap.GetPixel(i, cy) != color) { left = i + 1; break; } }
                            for (int i = cx; i < bw; i++) { if (ScreenshotBitmap.GetPixel(i, cy) != color) { right = i - 1; break; } }

                            // Get the vertical guideline bounds.
                            bottom = bh - 1;
                            for (int j = cy; j > 0; j--) { if (ScreenshotBitmap.GetPixel(cx, j) != color) { top = j + 1; break; } }
                            for (int j = cy; j < bh; j++) { if (ScreenshotBitmap.GetPixel(cx, j) != color) { bottom = j - 1; break; } }

                            shouldUpdate = true;
                        }
                    }
                    catch (Exception e)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(e);
#endif
                    }
                }
            }

            if (shouldUpdate)
            {
                // Set all the guideline points.
                GuidelineCoordinates.UpdateHorizontalGuideline(left, cy,
                                                               right, cy);
                GuidelineCoordinates.UpdateVerticalGuideline(cx, top,
                                                             cx, bottom);
                NotifyPropertyChanged(GuidelineCoordinatesPropertyName);

                UpdateDimensionString(left, right, top, bottom);
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

    public class Win32API
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
