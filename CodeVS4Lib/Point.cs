using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeVS4
{
    public interface IPoint : IEquatable<IPoint>
    {
        int X { get; }
        int Y { get; }
    }

    // ==で比較するのはNG
    public class Point : IPoint
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Point()
        {
        }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(IPoint other)
        {
            return X == other.X && Y == other.Y;
        }

        public override string ToString()
        {
            return string.Format("{{ X = {0}, Y = {1} }}", X, Y);
        }
    }
}
