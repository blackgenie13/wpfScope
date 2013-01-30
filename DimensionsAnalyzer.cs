using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace wpfScope
{
    public class DimensionsAnalyzer : INotifyPropertyChanged
    {
        #region Properties

        public static readonly string CursorPositionPropertyName = "CursorPosition";
        private System.Windows.Point _cursorPosition;
        public System.Windows.Point CursorPosition
        {
            get { return _cursorPosition; }
            set
            {
                if (_cursorPosition != value)
                {
                    _cursorPosition = value;
                    NotifyPropertyChanged(CursorPositionPropertyName);

                    UpdateGuidelines();
                }
            }
        }

        public static readonly string GuidelineCoordinatesPropertyName = "GuidelineCooordinates";
        private GuidelineCoordinates _guidelineCoordinates;
        public GuidelineCoordinates GuidelineCoordinates
        {
            get { return _guidelineCoordinates; }
            set
            {
                if (_guidelineCoordinates != value)
                {
                    _guidelineCoordinates = value;
                    NotifyPropertyChanged(GuidelineCoordinatesPropertyName);
                }
            }
        }

        public System.Windows.Point Location { get; set; }

        public static readonly string ScreenshotBitmapPropertyName = "ScreenshotBitmap";
        private Bitmap _screenshotBitmap;
        public Bitmap ScreenshotBitmap
        { 
            get { return _screenshotBitmap; }
            set
            {
                if (_screenshotBitmap != value)
                {
                    _screenshotBitmap = value;
                    NotifyPropertyChanged(ScreenshotBitmapPropertyName);
                }
            }
        }
        
        public static readonly string ScreenshotImagePropertyName = "ScreenshotImage";
        private BitmapSource _screenshotImage;
        public BitmapSource ScreenshotImage
        {
            get { return _screenshotImage; }
            set
            {
                if (_screenshotImage != value)
                {
                    _screenshotImage = value;
                    NotifyPropertyChanged(ScreenshotImagePropertyName);
                }
            }
        }

        public System.Windows.Size Size { get; set; }

        #endregion

        #region Constructor

        public DimensionsAnalyzer()
        {
            GuidelineCoordinates = new GuidelineCoordinates();
        }

        #endregion

        #region API

        public void UpdateGuidelines()
        {
            if (ScreenshotImage != null)
            {
                GuidelineCoordinates.UpdateHorizontalGuideline(0, _cursorPosition.Y, ScreenshotImage.Width, _cursorPosition.Y);
                GuidelineCoordinates.UpdateVerticalGuideline(_cursorPosition.X, 0, _cursorPosition.X, ScreenshotImage.Height);
                NotifyPropertyChanged(GuidelineCoordinatesPropertyName);
            }
        }

        public void UpdateScreenshot(Bitmap bmp)
        {
            if (bmp != null)
            {
                ScreenshotBitmap = bmp;

                ScreenshotImage = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                ScreenshotImage.Freeze();
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

        public LineCoordinates LeftCap { get { return new LineCoordinates(new System.Windows.Point(), new System.Windows.Point()); } }
        public LineCoordinates RightCap { get { return new LineCoordinates(new System.Windows.Point(), new System.Windows.Point()); } }
        public LineCoordinates TopCap { get { return new LineCoordinates(new System.Windows.Point(), new System.Windows.Point()); } }
        public LineCoordinates BottomCap { get { return new LineCoordinates(new System.Windows.Point(), new System.Windows.Point()); } }

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

    public class LineCoordinates
    {
        #region Properties

        private System.Windows.Point _start;
        public System.Windows.Point Start { get { return _start; } }

        private System.Windows.Point _end;
        public System.Windows.Point End { get { return _end; } }

        #endregion

        #region Constructors

        public LineCoordinates() : this(new System.Windows.Point(), new System.Windows.Point()) { }

        public LineCoordinates(System.Windows.Point start, System.Windows.Point end)
        {
            _start = start;
            _end = end;
        }

        #endregion

        #region API

        public void Update(double x1, double y1, double x2, double y2)
        {
            _start.X = x1;
            _start.Y = y1;
            _end.X = x2;
            _end.Y = y2;
        }

        #endregion

        #region Comparison Overrides

        public override bool Equals(object obj)
        {
            return obj is LineCoordinates && this == (LineCoordinates)obj;
        }

        public static bool operator ==(LineCoordinates lc1, LineCoordinates lc2)
        {
            if (object.ReferenceEquals(lc1, null) || object.ReferenceEquals(lc2, null)) { return false; }
            return lc1.Start == lc2.Start && lc1.End == lc2.End;
        }

        public static bool operator !=(LineCoordinates lc1, LineCoordinates lc2)
        {
            return !(lc1 == lc2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
