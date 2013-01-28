using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace wpfScope
{
    /// <summary>
    /// Interaction logic for DimensionsOverlay.xaml
    /// </summary>
    public partial class DimensionsOverlay : MetroWindow
    {
        private object _lock = new object(); // NO HEISENBUGS.
        private bool _shouldUpdateAnalysis;
        private BackgroundWorker _updateAnalysisWorker;

        public DimensionsOverlay()
        {
            InitializeComponent();

            _updateAnalysisWorker = new BackgroundWorker();
            _updateAnalysisWorker.DoWork += _updateAnalysisWorker_DoWork;
            _updateAnalysisWorker.RunWorkerAsync(this);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DisableAnalysisUpdate();

            _startUpdatingButton.Click += (sStart, eStart) => { EnableAnalysisUpdate(); };
            _stopUpdatingButton.Click += (sStop, eStop) => { DisableAnalysisUpdate(); };
        }

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

                    if (window != null)
                    {
                        // Actually update the analysis.
                        System.Diagnostics.Debug.WriteLine("Updating analysis...");
                    }
                }

                System.Threading.Thread.Sleep(500);
            }
        }

        #endregion
    }
}
