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

        // Background Worker
        private object _lock = new object(); // NO HEISENBUGS.
        private bool _shouldUpdateAnalysis;
        private BackgroundWorker _updateAnalysisWorker;
        private const int _updateFrequency = 500;

        private System.Windows.Point _location;
        private System.Windows.Size _size;

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

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DimensionsAnalyzer()
        {
            GuidelineCoordinates = new GuidelineCoordinates();

            _location = new System.Windows.Point();
            _size = new System.Windows.Size();

            _updateAnalysisWorker = new BackgroundWorker();
            _updateAnalysisWorker.DoWork += _updateAnalysisWorker_DoWork;
            _updateAnalysisWorker.RunWorkerAsync();
        }

        #endregion

        #region API

        /// <summary>
        /// Turn off the analyzer.
        /// </summary>
        public void DisableAnalysisUpdate()
        {
            lock (_lock)
            {
                _shouldUpdateAnalysis = false;
            }
        }

        /// <summary>
        /// Turn on the analyzer.
        /// </summary>
        public void EnableAnalysisUpdate()
        {
            lock (_lock)
            {
                _shouldUpdateAnalysis = true;
            }
        }

        /// <summary>
        /// Updates the location.
        /// </summary>
        /// <param name="x">New x.</param>
        /// <param name="y">New y.</param>
        public void UpdateLocation(double x, double y)
        {
            _location.X = x;
            _location.Y = y;
        }

        /// <summary>
        /// Updates the size.
        /// </summary>
        /// <param name="width">New width.</param>
        /// <param name="height">New height.</param>
        public void UpdateSize(double width, double height)
        {
            _size.Width = width;
            _size.Height = height;
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

        /// <summary>
        /// Updates the bitmap and the dimensions calculation.
        /// </summary>
        /// <param name="bmp">The bitmap to use to calculate guidelines.</param>
        private void UpdateScreenshot(Bitmap bmp)
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

        #region Update Background Worker Handlers

        private void _updateAnalysisWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Do this every 0.5s.
            while (true)
            {
                bool shouldUpdateAnalysis;
                lock (_lock)
                {
                    shouldUpdateAnalysis = _shouldUpdateAnalysis;
                }

                if (shouldUpdateAnalysis)
                {
#if WINDOWS8
                    // TODO.byip: Figure out a better way to do this, because the guides flicker.
                    //
                    // Hides the guides.
                    this.Dispatcher.BeginInvoke((Action)delegate(){ DisableGuides(); });
#endif
                    // Take the screenshot.
                    UpdateScreenshot(ScreenshotUtility.ScreenshotRegion((int)_location.X, (int)_location.Y,
                                                                        (int)_size.Width, (int)_size.Height));
#if WINDOWS8
                    // Show the guides.
                    this.Dispatcher.BeginInvoke((Action)delegate() { EnableGuides(); });
#endif
                }

                System.Threading.Thread.Sleep(_updateFrequency);
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
