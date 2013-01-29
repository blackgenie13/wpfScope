using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace wpfScope
{
    public partial class DimensionsOverlay : MetroWindow
    {
        #region Private Fields

        private object _lock = new object(); // NO HEISENBUGS.
        private bool _shouldUpdateAnalysis;
        private BackgroundWorker _updateAnalysisWorker;

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

            Analyzer = new DimensionsAnalyzer();
            DataContext = this;

            EnableAnalysisUpdate();

            _updateAnalysisWorker = new BackgroundWorker();
            _updateAnalysisWorker.DoWork += _updateAnalysisWorker_DoWork;
            _updateAnalysisWorker.RunWorkerAsync();

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

        private void UpdateFrame()
        {
            Analyzer.Location = new Point(Left + 1, Top + TitlebarHeight + 1);
            Analyzer.Size = new Size(ActualWidth - 2, ActualHeight - TitlebarHeight - 2);

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
                        // Actually update the analysis.
                        System.Diagnostics.Debug.WriteLine(String.Format("Updating analysis at ({0}) for ({1}).", Analyzer.Location, Analyzer.Size));

                        Analyzer.UpdateScreenshot(ScreenshotUtility.ScreenshotRegion((int)Analyzer.Location.X, (int)Analyzer.Location.Y,
                                                                                     (int)(0.5 * Analyzer.Size.Width), (int)Analyzer.Size.Height));
                    }
                }

                System.Threading.Thread.Sleep(_updateFrequency);
            }
        }

        #endregion
    }
}
