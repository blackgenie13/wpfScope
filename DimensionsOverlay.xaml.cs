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
using System.Windows.Shapes;

namespace wpfScope
{
    /// <summary>
    /// Interaction logic for DimensionsOverlay.xaml
    /// </summary>
    public partial class DimensionsOverlay : Window
    {
        public DimensionsOverlay()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            CloseButton.Click += CloseButton_Click;
            TitleBar.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
