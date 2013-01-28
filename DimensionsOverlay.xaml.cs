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

        #endregion

        #region Properties

        private DimensionsAnalyzer Analyzer { get; set; }

        #endregion

        #region Constructors

        public DimensionsOverlay()
        {
            InitializeComponent();

            Analyzer = new DimensionsAnalyzer();

            DisableAnalysisUpdate();

            _updateAnalysisWorker = new BackgroundWorker();
            _updateAnalysisWorker.DoWork += _updateAnalysisWorker_DoWork;
            _updateAnalysisWorker.RunWorkerAsync(this);
        }

        #endregion

        #region Public API

        public Point Position()
        {
            return new Point(this.Left, this.Top);
        }

        public Size Size()
        {
            return new Size(this.ActualWidth, this.ActualHeight);
        }

        #endregion

        #region Window Overrides

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _startUpdatingButton.Click += (sStart, eStart) => { EnableAnalysisUpdate(); };
            _stopUpdatingButton.Click += (sStop, eStop) => { DisableAnalysisUpdate(); };
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

        #endregion

        #region Update Background Worker Handlers

        private void _updateAnalysisWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Do this every 500ms.
            while (true)
            {
                bool shouldUpdateAnalysis;
                lock (_lock)
                {
                    shouldUpdateAnalysis = _shouldUpdateAnalysis;
                }

                if (shouldUpdateAnalysis)
                {
                    var window = e.Argument as DimensionsOverlay;

                    if (Analyzer != null && window != null)
                    {
                        // Actually update the analysis.
                        System.Diagnostics.Debug.WriteLine("Updating analysis...");

                        Point windowPosition = window.Position();
                        Size windowSize = window.Size();

                        Analyzer.Screenshot = ScreenshotUtility.CaptureRegion(
                            (int)windowPosition.X, (int)windowPosition.Y,
                            (int)windowSize.Width, (int)windowSize.Height);
                    }
                }

                System.Threading.Thread.Sleep(500);
            }
        }

        #endregion
    }
}
