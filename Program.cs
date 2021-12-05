// See https://aka.ms/new-console-template for more information
using ConsoleApp5BoundaryFollowingTracing;
using System;
using System.Collections.Generic;

Console.WriteLine("Hello, World!");


var img = new byte[8][]
{
    new byte[]{ 0, 0, 0 ,0, 0, 0, 0, 0, 0 },
    new byte[]{ 0, 0, 1, 1, 1, 1, 0, 0, 0 },
    new byte[]{ 0, 1, 0, 0, 1, 0, 0, 0, 0 },
    new byte[]{ 0, 0, 1, 0, 1, 0, 0, 0, 0 },
    new byte[]{ 0, 1, 0, 0, 1, 0, 0, 0, 0 },
    new byte[]{ 0, 1, 1, 1, 1, 0, 0, 0, 0 },
    new byte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    new byte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0 },
};

//Print(img);

var contours = BoundaryProcessingTrace(img);

Point GetUppermostLeftmostPoint(ref byte[][] cp)
{
    var b0 = new Point();
    for (int y = 0; y < cp.Length && b0.X == 0 && b0.Y == 0; y++)
    {
        for (int x = 0; x < cp[y].Length; x++)
        {
            if (cp[y][x] == 1)
            {
                b0 = new Point(y, x);
                break;
            }
        }
    }
    return b0;
}
(Point b1, Point c1) ExamineNeighbors(ref byte[][] cp, Point c0, Point b0)
{
    Point point = c0;
    Point lastPoint = b0;
    do
    {
        lastPoint = point;
        if (point.X < b0.X && point.Y == b0.Y)
        {
            //c0|
            //  |b0|xx
            //von links eins rauf gehen bei zustand 
            point = new Point(point.Y - 1, point.X);
        }
        else if (point.X < b0.X && point.Y == (b0.Y - 1))
        {
            //  |c0 
            //  |b0|xx
            point = new Point(point.Y, point.X + 1);
        }
        else if (point.X == b0.X && point.Y == (b0.Y - 1))
        {
            //  |  |c0
            //  |b0|xx
            point = new Point(point.Y, point.X + 1);
        }
        else if (point.X > b0.X && point.Y == (b0.Y - 1))
        {
            //  |  |
            //  |b0|c0
            point = new Point(point.Y + 1, point.X);
        }
        else if (point.X > b0.X && point.Y == b0.Y)
        {
            //  |  |
            //  |b0|
            //  |  |c0
            point = new Point(point.Y + 1, point.X);
        }
        else if (point.X > b0.X && point.Y > b0.Y)
        {
            //  |  |
            //  |b0|
            //  |c0|
            point = new Point(point.Y, point.X - 1);
        }
        else if (point.X == b0.X && point.Y > b0.Y)
        {
            //  |  |
            //  |b0|
            //c0|  |
            point = new Point(point.Y, point.X - 1);
        }
        else if (point.X < b0.X && point.Y > b0.Y)
        {
            //  |  |
            //c0|b0|
            //  |  |
            point = new Point(point.Y - 1, point.X);
        }
    }
    while (point.GetValue(ref cp) == 0);

    return new(point, lastPoint);
}

byte[][] BoundaryProcessingTrace(byte[][] img)
{
    List<Point> foundPixels = new List<Point>();
    var cp = Copy(img);
    Point? initb0 = null;
    //Let the starting point, b0, be the uppermost-leftmost point in the image that is labeled 1
    var b0 = GetUppermostLeftmostPoint(ref cp);
    foundPixels.Add(b0);
    //Denote by c0 the west neighbor of b0[see Fig. 11.1(b)].
    //Clearly, c0 is always a background point.
    if (cp[b0.Y][b0.X - 1] != 0) throw new InvalidOperationException("Background expected!");
    var c0 = new Point(b0.Y, b0.X - 1);
    //Examine the 8 - neighbors of b0, starting at c0 and proceeding in a clockwise direction.
    var b1c1 = ExamineNeighbors(ref cp, c0, b0);
    b0.SetValue(ref cp, 2);
    //Print(cp);
    //Store the locations of b0 for use in Step 5.
    if (initb0 == null)
    {
        initb0 = new Point(b0.Y, b0.X);
    }
    //Let b=b0 and c=c0.
    var b = b1c1.b1;
    var c = b1c1.c1;
    //Print(cp);
    do
    {
        b.SetValue(ref cp, 2);
        //Let the 8 - neighbors of b, starting at c and proceeding in a clockwise direction, 
        //be denoted by nn n 12 8 ,,,. … Find the first neighbor labeled 1 and denote it by nk.
        var nkNK = ExamineNeighbors(ref cp, c, b);
        //Let b n = k and c n = k– .
        b = nkNK.b1;
        c = nkNK.c1;
        b.SetValue(ref cp, 2);
    }
    //Repeat Steps 3 and 4 until b b = 0. The sequence of b points found when the 
    //algorithm stops is the set of ordered boundary points.
    while (!(b.X == initb0.X && b.Y == initb0.Y));

    Print(cp);

    return cp;
}


void Print(byte[][] data)
{
    for (int i = 0; i < data.Length; i++)
    {
        for (int j = 0; j < data[i].Length; j++)
        {
            Console.Write($"{data[i][j]} ");
        }
        Console.WriteLine();
    }
}
byte[][] Copy(byte[][] data)
{
    var cp = new byte[data.Length][];
    for (int i = 0; i < data.Length; i++)
    {
        cp[i] = new byte[data[i].Length];
        data[i].CopyTo(cp[i], 0);
    }
    return cp;
}