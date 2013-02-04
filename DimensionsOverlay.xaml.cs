using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace wpfScope
{
    /// <summary>
    /// The Dimensions view. Displays the desktop below the window and some guidelines that show the
    /// dimensions in the space around the cursor.
    /// </summary>
    public partial class DimensionsOverlay : MetroWindow
    {
        #region Private Fields

        // Background Worker
        private object _lock = new object(); // NO HEISENBUGS.
        private bool _shouldUpdateAnalysis;
        private BackgroundWorker _updateAnalysisWorker;

        // Mouse
        private bool _freezeMouse;
        private bool _mouseOver;

        // Settings
        private const bool _showDebugControls = false;
        private const int _updateFrequency = 500;

        #endregion

        #region Properties

        /// <summary>
        /// The analyzer for this view.
        /// </summary>
        public DimensionsAnalyzer Analyzer { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DimensionsOverlay()
        {
            InitializeComponent();
            DataContext = this;

            Analyzer = new DimensionsAnalyzer();
            EnableAnalysisUpdate();
            _updateAnalysisWorker = new BackgroundWorker();
            _updateAnalysisWorker.DoWork += _updateAnalysisWorker_DoWork;
            _updateAnalysisWorker.RunWorkerAsync();

            _freezeMouse = false;
            _mouseOver = false;

            if (_showDebugControls)
            {
#if DEBUG
                _windowFrameTextBlock.Visibility = Visibility.Visible;
#endif
            }
        }

        #endregion

        #region Window Overrides

        /// <summary>
        /// When the window is initialized, add all our handlers and disable the guidelines.
        /// </summary>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _overlayCanvas.MouseEnter += _overlayCanvas_MouseEnter;
            _overlayCanvas.MouseLeave += _overlayCanvas_MouseLeave;
            _overlayCanvas.MouseMove += _overlayCanvas_MouseMove;
            _overlayCanvas.MouseDown += _overlayCanvas_MouseDown;

            DisableGuides();
        }

        /// <summary>
        /// Updates the frame's location and size.
        /// </summary>
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            UpdateFrame();
        }

        /// <summary>
        /// Updates the frame's location and size.
        /// </summary>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            UpdateFrame();
        }

        /// <summary>
        /// When the state of the window changes, we need to make sure to update the size, location and
        /// whether we want to continue updating the analyzer.
        /// </summary>
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            switch (WindowState)
            {
                case WindowState.Maximized:
                    // TODO.byip: THIS DOESN'T ACTUALLY WORK. The size and location of a window don't get updated
                    //            when the window gets maximized.
                    UpdateFrame();
                    EnableAnalysisUpdate();
                    break;
                case WindowState.Minimized:
                    DisableAnalysisUpdate();
                    break;
                case WindowState.Normal:
                    UpdateFrame();
                    EnableAnalysisUpdate();
                    break;
            }
        }

        #endregion

        #region Canvas Mouse Event Handlers

        /// <summary>
        /// When the cursor leaves the canvas, turn off the guidelines unless the cursor has been frozen.
        /// </summary>
        private void _overlayCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseOver = false;

            if (!_freezeMouse) { DisableGuides(); }
        }

        /// <summary>
        /// Turn on guidelines when the user is moving around in the canvas.
        /// </summary>
        private void _overlayCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            _mouseOver = true;
            EnableGuides();
        }

        /// <summary>
        /// Update the guidelines as the user moves around the canvas.
        /// </summary>
        private void _overlayCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_freezeMouse)
            {
                Analyzer.CursorPosition = e.MouseDevice.GetPosition(_overlayCanvas);

                // Update dimensions text box position.
                if (Analyzer.ScreenshotImage != null)
                {
                    // Figure out what quadrant the cursor is in.
                    int mx = (int)(0.5 * Analyzer.ScreenshotImage.Width);
                    int my = (int)(0.5 * Analyzer.ScreenshotImage.Height);
                    int cx = (int)Analyzer.CursorPosition.X;
                    int cy = (int)Analyzer.CursorPosition.Y;

                    int w = (int)_dimensionsTextBlockBorder.ActualWidth;
                    int h = (int)_dimensionsTextBlockBorder.ActualHeight;
                    int x = cx;
                    int y = cy;

                    int margin = 5;

                    if (cx > mx)
                    {
                        x = cx - w - margin;
                        if (cy < my) { y = cy + margin; } // I
                        else { y = cy - h - margin; } // II
                    }
                    else
                    {
                        x = cx + margin;
                        if (cy < my) { x += 2 * margin; y = cy + margin; } // IV
                        else { y = cy - h - margin; } // III
                    }

                    Canvas.SetLeft(_dimensionsTextBlockBorder, x);
                    Canvas.SetTop(_dimensionsTextBlockBorder, y);
                }
            }
        }

        /// <summary>
        /// If the user clicks somewhere, freeze/unfreeze the cursor.
        /// </summary>
        private void _overlayCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _freezeMouse = !_freezeMouse;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Turn off the analyzer.
        /// </summary>
        private void DisableAnalysisUpdate()
        {
            lock (_lock)
            {
                _shouldUpdateAnalysis = false;
            }
        }

        /// <summary>
        /// Turn on the analyzer.
        /// </summary>
        private void EnableAnalysisUpdate()
        {
            lock (_lock)
            {
                _shouldUpdateAnalysis = true;
            }
        }

        /// <summary>
        /// Hide the guidelines.
        /// </summary>
        private void DisableGuides()
        {
            _dimensionsTextBlockBorder.Visibility = System.Windows.Visibility.Hidden;
            _horizontalGuidelineLeftCap.Visibility = System.Windows.Visibility.Hidden;
            _horizontalGuideline.Visibility = System.Windows.Visibility.Hidden;
            _horizontalGuidelineRightCap.Visibility = System.Windows.Visibility.Hidden;
            _verticalGuidelineTopCap.Visibility = System.Windows.Visibility.Hidden;
            _verticalGuideline.Visibility = System.Windows.Visibility.Hidden;
            _verticalGuidelineBottomCap.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// Show the guidelines.
        /// </summary>
        private void EnableGuides()
        {
            if (_mouseOver)
            {
                _dimensionsTextBlockBorder.Visibility = System.Windows.Visibility.Visible;
                _horizontalGuidelineLeftCap.Visibility = System.Windows.Visibility.Visible;
                _horizontalGuideline.Visibility = System.Windows.Visibility.Visible;
                _horizontalGuidelineRightCap.Visibility = System.Windows.Visibility.Visible;
                _verticalGuidelineTopCap.Visibility = System.Windows.Visibility.Visible;
                _verticalGuideline.Visibility = System.Windows.Visibility.Visible;
                _verticalGuidelineBottomCap.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// Update the Analyzer's size and location.
        /// </summary>
        private void UpdateFrame()
        {
            Analyzer.Location = new Point(Left, Top + TitlebarHeight);
            Analyzer.Size = new Size(ActualWidth, ActualHeight - TitlebarHeight);

#if DEBUG
            _windowFrameTextBlock.Text = String.Format("({0}, {1}) {2} x {3}", Top, Left, ActualWidth, ActualHeight);
#endif
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
                    if (Analyzer != null)
                    {
#if WINDOWS8
                        // TODO.byip: Figure out a better way to do this, because the guides flicker.
                        //
                        // Hides the guides.
                        this.Dispatcher.BeginInvoke((Action)delegate(){ DisableGuides(); });
#endif
                        // Take the screenshot.
                        Analyzer.UpdateScreenshot(ScreenshotUtility.ScreenshotRegion((int)Analyzer.Location.X, (int)Analyzer.Location.Y,
                                                                                     (int)Analyzer.Size.Width, (int)Analyzer.Size.Height));
#if WINDOWS8
                        // Show the guides.
                        this.Dispatcher.BeginInvoke((Action)delegate() { EnableGuides(); });
#endif
                    }
                }

                System.Threading.Thread.Sleep(_updateFrequency);
            }
        }

        #endregion
    }
}
