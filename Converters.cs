using System;
using System.Windows.Data;

namespace wpfScope
{
    public class LineCoordinatesToValueConverter : IValueConverter
    {
        public enum PointOption
        {
            StartX,
            StartY,
            EndX,
            EndY
        }

        #region IValueConverter Members

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

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
