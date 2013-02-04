using System.Windows;

namespace wpfScope
{
    /// <summary>
    /// Holds the coordinates for a single line.
    /// </summary>
    public class LineCoordinates
    {
        #region Properties

        /// <summary>
        /// The first point of the line.
        /// </summary>
        private Point _start;
        public Point Start { get { return _start; } }

        /// <summary>
        /// The second point of the line.
        /// </summary>
        private Point _end;
        public Point End { get { return _end; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LineCoordinates() : this(new Point(), new Point()) { }

        /// <summary>
        /// Constructor that initializes a line for given points.
        /// </summary>
        /// <param name="start">The first point of the line.</param>
        /// <param name="end">The second point of the line.</param>
        public LineCoordinates(Point start, Point end)
        {
            _start = start;
            _end = end;
        }

        #endregion

        #region API

        /// <summary>
        /// Updates the internal points.
        /// 
        /// Use this so that new points are not being created all the time.
        /// </summary>
        /// <param name="x1">The first point's x.</param>
        /// <param name="y1">The first point's y.</param>
        /// <param name="x2">The second point's x.</param>
        /// <param name="y2">The second point's y.</param>
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
