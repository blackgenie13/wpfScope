using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace wpfScope
{
    public partial class DimensionsOverlay : MetroWindow
    {
        #region Private Fields

        // Background Worker
        private object _lock = new object(); // NO HEISENBUGS.
        private bool _shouldUpdateAnalysis;
        private BackgroundWorker _updateAnalysisWorker;

        // Mouse
        private bool _mouseOver;

        // Settings
        private const bool _hideDebugControls = true;
        private const int _updateFrequency = 500;

        #endregion

        #region Properties

        public DimensionsAnalyzer Analyzer { get; set; }

        #endregion

        #region Constructors

        public DimensionsOverlay()
        {
            InitializeComponent();
            DataContext = this;

            Analyzer = new DimensionsAnalyzer();
            EnableAnalysisUpdate();
            _updateAnalysisWorker = new BackgroundWorker();
            _updateAnalysisWorker.DoWork += _updateAnalysisWorker_DoWork;
            _updateAnalysisWorker.RunWorkerAsync();

            _mouseOver = false;

            if (_hideDebugControls)
            {
                _windowFrameTextBlock.Visibility = Visibility.Hidden;
            }
        }

        #endregion

        #region Window Overrides

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _overlayCanvas.MouseEnter += _overlayCanvas_MouseEnter;
            _overlayCanvas.MouseLeave += _overlayCanvas_MouseLeave;
            _overlayCanvas.MouseMove += _overlayCanvas_MouseMove;

            DisableGuides();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            UpdateFrame();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            UpdateFrame();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            switch (WindowState)
            {
                case WindowState.Maximized:
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

        void _overlayCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseOver = false;
            DisableGuides();
        }

        void _overlayCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            _mouseOver = true;
            EnableGuides();
        }

        private void _overlayCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Analyzer.CursorPosition = e.MouseDevice.GetPosition(_overlayCanvas); ;
        }

        #endregion

        #region Helper Methods

        private void DisableAnalysisUpdate()
        {
            lock (_lock)
            {
                _shouldUpdateAnalysis = false;
            }
        }

        private void EnableAnalysisUpdate()
        {
            lock (_lock)
            {
                _shouldUpdateAnalysis = true;
            }
        }

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

        private void UpdateFrame()
        {
            Analyzer.Location = new Point(Left, Top + TitlebarHeight);
            Analyzer.Size = new Size(ActualWidth, ActualHeight - TitlebarHeight);

            _windowFrameTextBlock.Text = String.Format("({0}, {1}) {2} x {3}", Top, Left, ActualWidth, ActualHeight);
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
#if WINDOWS7
                        // TODO.byip: Figure out a better way to do this, because the guides flicker.
                        //
                        // Hides the guides.
                        this.Dispatcher.BeginInvoke((Action)delegate(){ DisableGuides(); });
#endif
                        // Take the screenshot.
                        Analyzer.UpdateScreenshot(ScreenshotUtility.ScreenshotRegion((int)Analyzer.Location.X, (int)Analyzer.Location.Y,
                                                                                     (int)(0.5 * Analyzer.Size.Width), (int)Analyzer.Size.Height));
#if WINDOWS7
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
