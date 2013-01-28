using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace wpfScope
{
    public class DimensionsAnalyzer : INotifyPropertyChanged
    {
        #region Properties

        public static readonly string ScreenshotPropertyName = "Screenshot";
        private BitmapSource _screenshot;
        public BitmapSource Screenshot
        { 
            get { return _screenshot; }
            set
            {
                if (_screenshot != value)
                {
                    _screenshot = value;
                    NotifyPropertyChanged(ScreenshotPropertyName);
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

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
}
