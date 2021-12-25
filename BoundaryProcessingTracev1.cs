using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp5BoundaryFollowingTracing;

internal class BoundaryProcessingTracev1
{

    public static IList<Point> BoundaryProcessingTraceAsync(Image<Rgba32> image)
    {
        List<Point> boundaryPoints = new();
        Point? initb0 = null;
        var b0 = GetUppermostLeftmostPointAsync(image);
        //Denote by c0 the west neighbor of b0[see Fig. 11.1(b)].
        //Clearly, c0 is always a background point.
        var c0 = new Point(b0.X - 1, b0.Y);
        boundaryPoints.Add(c0);
        //Examine the 8 - neighbors of b0, starting at c0 and proceeding in a clockwise direction.
        if (initb0 == null)
        {
            initb0 = new Point(b0.X, b0.Y);
        }
        //Let b=b0 and c=c0.
        var b = b0;
        var c = c0;
        boundaryPoints.Add(b);
        do
        {
            //Let the 8 - neighbors of b, starting at c and proceeding in a clockwise direction, 
            //be denoted by nn n 12 8 ,,,. … Find the first neighbor labeled 1 and denote it by nk.
            var nkNK = ExamineNeighborsAsync(image, c, b);
            //Let b n = k and c n = k– .
            b = nkNK.b1;
            c = nkNK.c1;
            boundaryPoints.Add(b);
        }
        while (!(b.X == initb0.Value.X && b.Y == initb0.Value.Y));
        //Repeat Steps 3 and 4 until b b = 0. The sequence of b points found when the 
        //algorithm stops is the set of ordered boundary points.
        return boundaryPoints;
    }

    static Point GetUppermostLeftmostPointAsync(Image<Rgba32> image)
    {
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];
                if (pixel is Rgba32 { R: >= 254, G: >= 254, B: >= 254 }
                 //check for a "single" pixel (quercus_crassifolia_16.ab.jpg x=265,y=23)
                 && !(image[x + 1, y] is Rgba32 { R: 0, G: 0, B: 0 } && image[x + 1, y - 1] is Rgba32 { R: 0, G: 0, B: 0 } && image[x, y - 1] is Rgba32 { R: 0, G: 0, B: 0 }))
                {

                    return new Point(x, y);
                }
            }
        }
        throw new InvalidOperationException("No point ==1 found!");
    }
    static (Point b1, Point c1) ExamineNeighborsAsync(Image<Rgba32> image, Point c0, Point b0)
    {
        Point point = c0;
        Point lastPoint = new Point();
        while (image[point.X, point.Y] is { R: < 254, G: < 254, B: < 254 })
        {
            lastPoint = point;
            if (point.X < b0.X && point.Y == b0.Y)
            {
                //c0|
                //  |b0|xx
                //von links eins rauf gehen bei zustand 
                point = new Point(point.X, point.Y - 1);
            }
            else if (point.X < b0.X && point.Y == (b0.Y - 1))
            {
                //  |c0 
                //  |b0|xx
                point = new Point(point.X + 1, point.Y);
            }
            else if (point.X == b0.X && point.Y == (b0.Y - 1))
            {
                //  |  |c0
                //  |b0|xx
                point = new Point(point.X + 1, point.Y);
            }
            else if (point.X > b0.X && point.Y == (b0.Y - 1))
            {
                //  |  |
                //  |b0|c0
                point = new Point(point.X, point.Y + 1);
            }
            else if (point.X > b0.X && point.Y == b0.Y)
            {
                //  |  |
                //  |b0|
                //  |  |c0
                point = new Point(point.X, point.Y + 1);
            }
            else if (point.X > b0.X && point.Y > b0.Y)
            {
                //  |  |
                //  |b0|
                //  |c0|
                point = new Point(point.X - 1, point.Y);
            }
            else if (point.X == b0.X && point.Y > b0.Y)
            {
                //  |  |
                //  |b0|
                //c0|  |
                point = new Point(point.X - 1, point.Y);
            }
            else if (point.X < b0.X && point.Y > b0.Y)
            {
                //  |  |
                //c0|b0|
                //  |  |
                point = new Point(point.X, point.Y - 1);
            }
        }

        return new(point, lastPoint);
    }
}

