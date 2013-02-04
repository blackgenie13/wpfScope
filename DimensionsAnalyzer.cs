using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace wpfScope
{
    /// <summary>
    /// This analyzer does all the background work of figuring out guidelines and dimensions.
    /// 
    /// Given a bitmap, calculates the dimensions of the space around a given cursor position.
    /// 
    /// TODO.byip: Decouple this from the view. Move BackgroundWorker code into here.
    /// </summary>
    public class DimensionsAnalyzer : INotifyPropertyChanged
    {
        #region Private Fields

        // Sync bitmap updates.
        private object _bitmapLock = new object();

        #endregion

        #region Properties

        /// <summary>
        /// The current position of the cursor w.r.t. the top-left corner of the screenshot.
        /// </summary>
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

        /// <summary>
        /// A formatted string with the dimensions of the area around the current cursor position.
        /// </summary>
        public static readonly string DimensionsStringPropertyName = "DimensionsString";
        private string _dimensionsString;
        public string DimensionsString
        {
            get { return _dimensionsString; }
            private set
            {
                if (_dimensionsString != value)
                {
                    _dimensionsString = value;
                    NotifyPropertyChanged(DimensionsStringPropertyName);
                }
            }
        }

        /// <summary>
        /// The coordinates of the lines that make up the guidelines in the view.
        /// </summary>
        public static readonly string GuidelineCoordinatesPropertyName = "GuidelineCooordinates";
        private GuidelineCoordinates _guidelineCoordinates;
        public GuidelineCoordinates GuidelineCoordinates
        {
            get { return _guidelineCoordinates; }
            private set
            {
                if (_guidelineCoordinates != value)
                {
                    _guidelineCoordinates = value;
                    NotifyPropertyChanged(GuidelineCoordinatesPropertyName);
                }
            }
        }

        /// <summary>
        /// The location of the view.
        /// 
        /// TODO.byip: This is only used in the BackgroundWorker, so we can get rid of this once that code is moved in.
        /// </summary>
        public System.Windows.Point Location { get; set; }

        /// <summary>
        /// The bitmap of the screenshot. Used for calculating the guidelines.
        /// </summary>
        public static readonly string ScreenshotBitmapPropertyName = "ScreenshotBitmap";
        private Bitmap _screenshotBitmap;
        public Bitmap ScreenshotBitmap
        { 
            get { return _screenshotBitmap; }
            private set
            {
                if (_screenshotBitmap != value)
                {
                    _screenshotBitmap = value;
                    NotifyPropertyChanged(ScreenshotBitmapPropertyName);
                }
            }
        }
        
        /// <summary>
        /// The image source of the bitmap. Used for grabbing non-byte information from the bitmap, like
        /// size, and for displaying in the view.
        /// 
        /// This must be frozen!
        /// </summary>
        public static readonly string ScreenshotImagePropertyName = "ScreenshotImage";
        private BitmapSource _screenshotImage;
        public BitmapSource ScreenshotImage
        {
            get { return _screenshotImage; }
            private set
            {
                if (_screenshotImage != value)
                {
                    _screenshotImage = value;
                    NotifyPropertyChanged(ScreenshotImagePropertyName);
                }
            }
        }

        /// <summary>
        /// The size of the view.
        /// 
        /// TODO.byip: This is only used in the BackgroundWorker, so we can get rid of this once that code is moved in.
        /// </summary>
        public System.Windows.Size Size { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DimensionsAnalyzer()
        {
            GuidelineCoordinates = new GuidelineCoordinates();
        }

        #endregion

        #region API

        /// <summary>
        /// Updates the bitmap and the dimensions calculation.
        /// </summary>
        /// <param name="bmp">The bitmap to use to calculate guidelines.</param>
        public void UpdateScreenshot(Bitmap bmp)
        {
            if (bmp != null)
            {
                lock (_bitmapLock)
                {
                    IntPtr hbitmap = IntPtr.Zero;

                    try
                    {
                        ScreenshotBitmap = bmp;
                        hbitmap = bmp.GetHbitmap();

                        ScreenshotImage = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        if (ScreenshotImage != null) { ScreenshotImage.Freeze(); }
                        UpdateGuidelines();
                    }
                    catch (Exception e)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(e);
#endif
                    }
                    finally
                    {
                        if (hbitmap != IntPtr.Zero) { Win32API.DeleteObject(hbitmap); }
                        GC.Collect();
                    }
                }

            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Formats the given dimensions as a string to be consumed by the view.
        /// </summary>
        /// <param name="left">The leftmost point of the horizontal guideline.</param>
        /// <param name="right">The rightmost point of the horizontal guideline.</param>
        /// <param name="top">The topmost point of the vertical guideline.</param>
        /// <param name="bottom">The bottommost point of the vertical guideline.</param>
        /// <returns>A formatted string with the dimensions of the current guidelines.</returns>
        private string UpdateDimensionString(int left, int right, int top, int bottom)
        {
            return String.Format("{0} x {1} px", Math.Max(right - left + 1, 0), Math.Max(bottom - top + 1, 0));
        }

        /// <summary>
        /// Updates the guidelines using the cursor and bitmap properties.
        /// </summary>
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

                // Manually update, as we are modifying the properties OF the GuidelineCoordinates.
                NotifyPropertyChanged(GuidelineCoordinatesPropertyName);

                DimensionsString = UpdateDimensionString(left, right, top, bottom);
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <inheritdoc />
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

    /// <summary>
    /// Allows access to Win32 API methods.
    /// </summary>
    public class Win32API
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
