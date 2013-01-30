using System.Windows;

namespace wpfScope
{
    public class LineCoordinates
    {
        #region Properties

        private Point _start;
        public Point Start { get { return _start; } }

        private Point _end;
        public Point End { get { return _end; } }

        #endregion

        #region Constructors

        public LineCoordinates() : this(new Point(), new Point()) { }

        public LineCoordinates(Point start, Point end)
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
