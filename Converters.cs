using System;
using System.Windows.Data;

namespace wpfScope
{
    /// <summary>
    /// Converts LineCoordinates objects to a single value given a parameter for which point is needed.
    /// 
    /// Allows multiple Line controls to be bound to a single LineCoordinates object.
    /// </summary>
    [ValueConversion(typeof(LineCoordinates), (typeof(double)))]
    public class LineCoordinatesToValueConverter : IValueConverter
    {
        /// <summary>
        /// Specifies which point in the LineCoordinates object to get.
        /// </summary>
        public enum PointOption
        {
            StartX,
            StartY,
            EndX,
            EndY
        }

        #region IValueConverter Members

        /// <summary>
        /// Takes a LineCoordinates object and grabs a value for a given PointOption.
        /// </summary>
        /// <param name="value">LineCoordinates object.</param>
        /// <param name="parameter">A string that can be cast to which PointOption is requested.</param>
        /// <returns>The double of the PointOption for the given LineCoordinates object.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var coordinates = value as LineCoordinates;
            var parameterString = parameter as string;
            double i = 0;

            if (coordinates != null && parameter != null)
            {
                PointOption option = (PointOption)Enum.Parse(typeof(PointOption), parameterString);

                switch (option)
                {
                    case PointOption.StartX:
                        i = coordinates.Start.X;
                        break;
                    case PointOption.EndX:
                        i = coordinates.End.X;
                        break;
                    case PointOption.StartY:
                        i = coordinates.Start.Y;
                        break;
                    case PointOption.EndY:
                        i = coordinates.End.Y;
                        break;
                }
            }

            return i;
        }

        /// <summary>
        /// This is never used.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
