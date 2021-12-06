// See https://aka.ms/new-console-template for more information
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;

Console.WriteLine("Hello, World!");




var bytes = await File.ReadAllBytesAsync("Acer_Campestre_01.ab.jpg");
using var image = await Image.LoadAsync<Rgba32>(new MemoryStream(bytes));

var contour = BoundaryProcessingTraceAsync(image);
contour.SaveAsJpeg("Acer_Campestre_01_BoundaryProcessingTrace.ab.jpg");


Image<Rgba32> BoundaryProcessingTraceAsync(Image<Rgba32> image)
{
    var cp = image.Clone();
    Point? initb0 = null;
    Rgba32 color = new Rgba32(217, 30, 24, 255);
    var b0 = GetUppermostLeftmostPointAsync(image);
    //Denote by c0 the west neighbor of b0[see Fig. 11.1(b)].
    //Clearly, c0 is always a background point.
    if (image[b0.X - 1, b0.Y] is not { R: 0, G: 0, B: 0 }) throw new InvalidOperationException("Background expected!");
    var c0 = new Point(b0.X - 1, b0.Y);
    //Examine the 8 - neighbors of b0, starting at c0 and proceeding in a clockwise direction.
    var b1c1 = ExamineNeighborsAsync(image, c0, b0);
    cp[c0.X, c0.Y] = color;
    if (initb0 == null)
    {
        initb0 = new Point(b0.X, b0.Y);
    }
    //Let b=b0 and c=c0.
    var b = b1c1.b1;
    var c = b1c1.c1;
    while (!(b.X == initb0.Value.X && b.Y == initb0.Value.Y))
    {
        cp[b.X, b.Y] = color;
        //Let the 8 - neighbors of b, starting at c and proceeding in a clockwise direction, 
        //be denoted by nn n 12 8 ,,,. … Find the first neighbor labeled 1 and denote it by nk.
        var nkNK = ExamineNeighborsAsync(image, c, b);
        //Let b n = k and c n = k– .
        b = nkNK.b1;
        c = nkNK.c1;
        cp[b.X, b.Y] = color;
    }
    //Repeat Steps 3 and 4 until b b = 0. The sequence of b points found when the 
    //algorithm stops is the set of ordered boundary points.
    
    return cp;
}

static Point GetUppermostLeftmostPointAsync(Image<Rgba32> image)
{
    for (int y = 0; y < image.Height; y++)
    {
        for (int x = 0; x < image.Width; x++)
        {
            var pixel = image[x, y];
            if (pixel is Rgba32 { R: 255, G: 255, B: 255, A: 255 })
            {
                return new Point(x, y);
            }
        }
    }
    throw new InvalidOperationException("No point ==1 found!");
}
(Point b1, Point c1) ExamineNeighborsAsync(Image<Rgba32> image, Point c0, Point b0)
{
    Point point = c0;
    Point lastPoint = new Point();
    while (image[point.X, point.Y] is { R: 0, G: 0, B: 0 })
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