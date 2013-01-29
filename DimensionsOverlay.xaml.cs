﻿using System;
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

        public DimensionsAnalyzer Analyzer { get; set; }

        #endregion

        #region Constructors

        public DimensionsOverlay()
        {
            InitializeComponent();

            Analyzer = new DimensionsAnalyzer();
            DataContext = this;

            DisableAnalysisUpdate();

            _updateAnalysisWorker = new BackgroundWorker();
            _updateAnalysisWorker.DoWork += _updateAnalysisWorker_DoWork;
            _updateAnalysisWorker.RunWorkerAsync();
        }

        #endregion

        #region Window Overrides

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
            Analyzer.Location = new Point(Top, Left);
            Analyzer.Size = new Size(ActualWidth, ActualHeight);

            WindowFrameTextBlock.Text = String.Format("({0}, {1}) {2} x {3}", Top, Left, ActualWidth, ActualHeight);
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
                    if (Analyzer != null)
                    {
                        // Actually update the analysis.
                        System.Diagnostics.Debug.WriteLine("Updating analysis...");

                        Analyzer.Screenshot = ScreenshotUtility.CaptureRegion(
                            (int)Analyzer.Location.X, (int)Analyzer.Location.Y,
                            (int)(0.5 * Analyzer.Size.Width), (int)Analyzer.Size.Height);
                    }
                }

                System.Threading.Thread.Sleep(500);
            }
        }

        #endregion
    }
}
