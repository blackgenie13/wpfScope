﻿using System.ComponentModel;
using System.Windows;

namespace wpfScope
{
    /// <summary>
    /// Holds the coordinates for each line that makes up the guidelines for the view.
    /// </summary>
    public class GuidelineCoordinates : INotifyPropertyChanged
    {
        #region Private Fields

        /// <summary>
        /// The length of each side of the cap ends of the guidelines.
        /// </summary>
        private const int _capLength = 4;

        #endregion

        #region Properties

        public static readonly string LeftCapPropertyName = "LeftCap";
        public LineCoordinates LeftCap { get; private set; }

        public static readonly string HorizontalGuidelinePropertyName = "HorizontalGuideline";
        public LineCoordinates HorizontalGuideline { get; private set; }

        public static readonly string RightCapPropertyName = "RightCap";
        public LineCoordinates RightCap { get; private set; }



        public static readonly string TopCapPropertyName = "TopCap";
        public LineCoordinates TopCap { get; private set; }

        public static readonly string VerticalGuidelinePropertyName = "VerticalGuideline";
        public LineCoordinates VerticalGuideline { get; private set; }

        public static readonly string BottomCapPropertyName = "BottomCap";
        public LineCoordinates BottomCap { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GuidelineCoordinates()
        {
            LeftCap = new LineCoordinates();
            HorizontalGuideline = new LineCoordinates();
            RightCap = new LineCoordinates();

            TopCap = new LineCoordinates();
            VerticalGuideline = new LineCoordinates();
            BottomCap = new LineCoordinates();
        }

        #endregion

        #region API

        /// <summary>
        /// Updates the horizontal guideline.
        /// </summary>
        /// <param name="x1">The left x.</param>
        /// <param name="y1">The left y.</param>
        /// <param name="x2">The right x.</param>
        /// <param name="y2">The right y.</param>
        public void UpdateHorizontalGuideline(double x1, double y1, double x2, double y2)
        {
            LeftCap.Update(x1, y1 - _capLength,
                           x1, y1 + _capLength);
            HorizontalGuideline.Update(x1, y1,
                                       x2, y2);
            RightCap.Update(x2, y1 - _capLength,
                           x2, y1 + _capLength);

            NotifyPropertyChanged(LeftCapPropertyName);
            NotifyPropertyChanged(HorizontalGuidelinePropertyName);
            NotifyPropertyChanged(RightCapPropertyName);
        }

        /// <summary>
        /// Updates the vertical guideline.
        /// </summary>
        /// <param name="x1">The top x.</param>
        /// <param name="y1">The top y.</param>
        /// <param name="x2">The bottom x.</param>
        /// <param name="y2">The bottom y.</param>
        public void UpdateVerticalGuideline(double x1, double y1, double x2, double y2)
        {
            TopCap.Update(x1 - _capLength, y1,
                          x1 + _capLength, y1);
            VerticalGuideline.Update(x1, y1,
                                     x2, y2);
            BottomCap.Update(x1 - _capLength, y2,
                             x1 + _capLength, y2);

            NotifyPropertyChanged(TopCapPropertyName);
            NotifyPropertyChanged(VerticalGuidelinePropertyName);
            NotifyPropertyChanged(BottomCapPropertyName);
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <inheritdoc />
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
