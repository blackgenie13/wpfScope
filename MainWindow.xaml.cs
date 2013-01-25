using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpfScope
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DimensionsOverlay DimensionsOverlayWindow { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (DimensionsOverlayWindow != null)
            {
                DimensionsOverlayWindow.Close();
            }

            base.OnClosing(e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        private void OpenDimensionsButton_Click(object sender, RoutedEventArgs e)
        {
            DimensionsOverlayWindow = new DimensionsOverlay();
            DimensionsOverlayWindow.Show();

            OpenDimensionsButton.IsEnabled = false;
            CloseDimensionsButton.IsEnabled = true;
        }

        private void CloseDimensionsButton_Click(object sender, RoutedEventArgs e)
        {
            DimensionsOverlayWindow.Close();

            OpenDimensionsButton.IsEnabled = true;
            CloseDimensionsButton.IsEnabled = false;
        }
    }
}
