// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

namespace ConsoleApp5BoundaryFollowingTracing
{
    [DebuggerDisplay("Y={Y},X={X}")]
    public class Point
    {
        public Point()
        {
            X = 0;
            Y = 0;
        }
        public Point(int y, int x)
        {
            this.Y = y;
            this.X = x;
        }

        public int X { get; }
        public int Y { get; }

        public byte GetValue(ref byte[][] vs)
        {
            return vs[Y][X];
        }

        internal void SetValue(ref byte[][] vs, byte v)
        {
            vs[Y][X] = 3;
        }
    }
}