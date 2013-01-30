using System.ComponentModel;
using System.Windows;

namespace wpfScope
{
    public class GuidelineCoordinates : INotifyPropertyChanged
    {
        #region Private Fields

        private const int _capLength = 2;

        #endregion

        #region Properties

        public static readonly string HorizontalGuidelinePropertyName = "HorizontalGuideline";
        private LineCoordinates _horizontalGuideline;
        public LineCoordinates HorizontalGuideline
        {
            get { return _horizontalGuideline; }
            set
            {
                if (_horizontalGuideline != value)
                {
                    _horizontalGuideline = value;
                    NotifyPropertyChanged(HorizontalGuidelinePropertyName);
                }
            }
        }

        public static readonly string VerticalGuidelinePropertyName = "VerticalGuideline";
        private LineCoordinates _verticalGuideline;
        public LineCoordinates VerticalGuideline
        {
            get { return _verticalGuideline; }
            set
            {
                if (_verticalGuideline != value)
                {
                    _verticalGuideline = value;
                    NotifyPropertyChanged(VerticalGuidelinePropertyName);
                }
            }
        }

        public LineCoordinates LeftCap { get { return new LineCoordinates(new Point(), new Point()); } }
        public LineCoordinates RightCap { get { return new LineCoordinates(new Point(), new Point()); } }
        public LineCoordinates TopCap { get { return new LineCoordinates(new Point(), new Point()); } }
        public LineCoordinates BottomCap { get { return new LineCoordinates(new Point(), new Point()); } }

        #endregion

        #region Constructor

        public GuidelineCoordinates()
        {
            HorizontalGuideline = new LineCoordinates();
            VerticalGuideline = new LineCoordinates();
        }

        #endregion

        #region API

        public void UpdateHorizontalGuideline(double x1, double y1, double x2, double y2)
        {
            HorizontalGuideline.Update(x1, y1, x2, y2);
            NotifyPropertyChanged(HorizontalGuidelinePropertyName);
        }

        public void UpdateVerticalGuideline(double x1, double y1, double x2, double y2)
        {
            VerticalGuideline.Update(x1, y1, x2, y2);
            NotifyPropertyChanged(VerticalGuidelinePropertyName);
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
