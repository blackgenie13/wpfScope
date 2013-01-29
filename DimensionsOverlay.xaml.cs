using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
        private const bool _enableActivation = false;
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
                _startUpdatingButton.Visibility = Visibility.Hidden;
                _stopUpdatingButton.Visibility = Visibility.Hidden;
                _windowFrameTextBlock.Visibility = Visibility.Hidden;
            }
        }

        #endregion

        #region Window Overrides

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (_enableActivation) { EnableAnalysisUpdate(); }
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            if (_enableActivation) { DisableAnalysisUpdate(); }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _startUpdatingButton.Click += (sStart, eStart) => { EnableAnalysisUpdate(); };
            _stopUpdatingButton.Click += (sStop, eStop) => { DisableAnalysisUpdate(); };

            _overlayCanvas.MouseEnter += _overlayCanvas_MouseEnter;
            _overlayCanvas.MouseLeave += _overlayCanvas_MouseLeave;
            _overlayCanvas.MouseMove += _overlayCanvas_MouseMove;
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
            _horizontalAxisLeftCap.Visibility = System.Windows.Visibility.Hidden;
            _horizontalAxis.Visibility = System.Windows.Visibility.Hidden;
            _horizontalAxisRightCap.Visibility = System.Windows.Visibility.Hidden;
            _verticalAxisTopCap.Visibility = System.Windows.Visibility.Hidden;
            _verticalAxis.Visibility = System.Windows.Visibility.Hidden;
            _verticalAxisBottomCap.Visibility = System.Windows.Visibility.Hidden;
        }

        private void EnableGuides()
        {
            if (_mouseOver)
            {
                _horizontalAxisLeftCap.Visibility = System.Windows.Visibility.Visible;
                _horizontalAxis.Visibility = System.Windows.Visibility.Visible;
                _horizontalAxisRightCap.Visibility = System.Windows.Visibility.Visible;
                _verticalAxisTopCap.Visibility = System.Windows.Visibility.Visible;
                _verticalAxis.Visibility = System.Windows.Visibility.Visible;
                _verticalAxisBottomCap.Visibility = System.Windows.Visibility.Visible;
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
                        // TODO.byip: Figure out a better way to do this, because the guides flicker.
                        //
                        // Hides the guides.
                        this.Dispatcher.BeginInvoke((Action)delegate(){ DisableGuides(); });

                        // Take the screenshot.
                        Analyzer.UpdateScreenshot(ScreenshotUtility.ScreenshotRegion((int)Analyzer.Location.X, (int)Analyzer.Location.Y,
                                                                                     (int)(0.5 * Analyzer.Size.Width), (int)Analyzer.Size.Height));
                        // Show the guides.
                        this.Dispatcher.BeginInvoke((Action)delegate(){ EnableGuides(); });
                    }
                }

                System.Threading.Thread.Sleep(_updateFrequency);
            }
        }

        #endregion
    }
}
