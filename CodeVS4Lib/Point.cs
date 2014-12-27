using System;
using System.Diagnostics;

namespace CodeVS4
{
    // ==で比較するのはNG
    public class Point : IEquatable<Point>
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Point(int x, int y)
        {
            Debug.Assert(0 <= x && x <= 99);
            Debug.Assert(0 <= y && y <= 99);
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format("Point {{ X = {0}, Y = {1} }}", X, Y);
        }

        // public bool Equals(object other)を実装しても良さそうだが、
        // Pointを継承したクラスとの比較を考え始めるとメンドイ

        public bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }
    }
}
