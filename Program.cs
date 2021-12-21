// See https://aka.ms/new-console-template for more information
using MathNet.Numerics.IntegralTransforms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Complex = System.Numerics.Complex;
using MathNet.Numerics.LinearAlgebra;

Console.WriteLine("Hello, World!");

string rootleavesplantspeciesPath = @"C:\Users\marti\OneDrive\Dokumente\IPA\lab3\100 leaves plant species\data";

var directoryInfo = new DirectoryInfo(rootleavesplantspeciesPath);
var plantspeciesDirectories = directoryInfo.GetDirectories();
Dictionary<string, IList<ImageData>> plantspecies = new();
int maxItems = 0;
foreach (var plantspeciesDirectory in plantspeciesDirectories)
{
    plantspecies.Add(plantspeciesDirectory.Name, new List<ImageData>());
    var images = plantspeciesDirectory.GetFiles();
    foreach (var img in images)
    {
        System.Console.WriteLine($"Processing {img.FullName}");
        var bytes = await File.ReadAllBytesAsync(img.FullName);
        using var image = await Image.LoadAsync<Rgba32>(new MemoryStream(bytes));

        var boundaryPoints = BoundaryProcessingTraceAsync(image);
        var signature = boundary2signature(boundaryPoints);
        ImageData imageData = new ImageData()
        {
            BoundaryPoints = boundaryPoints,
            Signature = signature.CentroidDistanceSignature,
            ImageName = img.Name
        };
        if (maxItems < imageData.Signature.Count)
        {
            maxItems = imageData.Signature.Count;
        }
        plantspecies[plantspeciesDirectory.Name].Add(imageData);
    }
}
//angleichen
foreach (var planttyp in plantspecies)
{
    foreach (var plantdata in plantspecies[planttyp.Key])
    {
        if (plantdata.Signature.Count < maxItems)
        {
            for (int i = plantdata.Signature.Count; i < maxItems; i++)
            {
                plantdata.Signature.Add(0);
            }
        }

        plantdata.NormalizedFourierDescriptor = CalculateFourierDescriptor(plantdata.Signature);

    }
}
do
{
    Console.WriteLine($"Enter a reference image, so we can look up similar images from {Environment.NewLine}{rootleavesplantspeciesPath}");
    string? imgName = Console.ReadLine();

    var searchresult = plantspecies.SelectMany(a => a.Value).Where(b => String.Equals(b.ImageName, imgName, StringComparison.OrdinalIgnoreCase));
    if (searchresult.Any())
    {
        FindSimilarImages(plantspecies, searchresult.First().NormalizedFourierDescriptor);
    }
    else
    {
        Console.WriteLine("Nothing found. Enter an other image");
    }

} while (true);

void FindSimilarImages(IDictionary<string, IList<ImageData>> plantSpeciesvaluePairs, IList<double> normalizedFourierDescriptor)
{    
    List<Tuple<string, double>> similarImages = new();

    foreach (var keyValue in plantSpeciesvaluePairs)
    {
        Console.WriteLine($"Values for {keyValue.Key}");
        foreach (var imageDatas in plantSpeciesvaluePairs[keyValue.Key])
        {
            double euclidiandistance = 0;
            for (int i = 0; i < 8; i++)
            {
                euclidiandistance = euclidiandistance + Math.Abs(imageDatas.NormalizedFourierDescriptor[i] - normalizedFourierDescriptor[i]);
            }
            Tuple<string, double> tuple = Tuple.Create(keyValue.Key, euclidiandistance);
            similarImages.Add(tuple);
        }
    }

    foreach (var tuple in similarImages.OrderBy(a => a.Item2))
    {
        Console.WriteLine($"{tuple.Item1,30} Distance {tuple.Item2}");
    }

    //return similarImages;
}

IList<double> CalculateFourierDescriptor(IList<double> signature)
{
    //var complexSignature = Vector<double>.Build.DenseOfEnumerable(signature).ToComplex();

    List<double> magnitude = new List<double>();
    var fourier = signature.Select(a => new Complex(a, 0)).ToArray();

    Fourier.Forward(fourier);

    //var vector = Vector<Complex>.Build.DenseOfArray(fourier);
    //var scaled = vector.Multiply(1 / vector[0]);
    //scaled.Norm(2);
    //// sqrt(..^2 + )
    //// sqrt(..^2) + sqrt(..^2) = |.|_1
    var dc = fourier[0];
    for (int i = 0; i < fourier.Length; i++)
    {
        fourier[i] = fourier[i] / dc;
        magnitude.Add(fourier[i].Magnitude);
    }

    return magnitude;
}

IList<Point> BoundaryProcessingTraceAsync(Image<Rgba32> image)
{
    List<Point> boundaryPoints = new();
    Point? initb0 = null;
    var b0 = GetUppermostLeftmostPointAsync(image);
    //Denote by c0 the west neighbor of b0[see Fig. 11.1(b)].
    //Clearly, c0 is always a background point.
    var c0 = new Point(b0.X - 1, b0.Y);
    boundaryPoints.Add(c0);
    //Examine the 8 - neighbors of b0, starting at c0 and proceeding in a clockwise direction.
    var b1c1 = ExamineNeighborsAsync(image, c0, b0);
    if (initb0 == null)
    {
        initb0 = new Point(b0.X, b0.Y);
    }
    //Let b=b0 and c=c0.
    var b = b1c1.b1;
    var c = b1c1.c1;
    boundaryPoints.Add(b);
    while (!(b.X == initb0.Value.X && b.Y == initb0.Value.Y))
    {
        //Let the 8 - neighbors of b, starting at c and proceeding in a clockwise direction, 
        //be denoted by nn n 12 8 ,,,. … Find the first neighbor labeled 1 and denote it by nk.
        var nkNK = ExamineNeighborsAsync(image, c, b);
        //Let b n = k and c n = k– .
        b = nkNK.b1;
        c = nkNK.c1;
        boundaryPoints.Add(b);
    }
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
             //check if pixel is "left alone" in the dark (quercus_crassifolia_16.ab.jpg x=265,y=23)
             && !(image[x + 1, y] is Rgba32 { R: 0, G: 0, B: 0 } && image[x + 1, y - 1] is Rgba32 { R: 0, G: 0, B: 0 } && image[x, y - 1] is Rgba32 { R: 0, G: 0, B: 0 }))
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


/// <summary>
/// The input to the function is a list of boundary points and the ouput is a 1D vector representing the signature.
/// </summary>
/// <remarks>
/// The shape in this particular context is expected to be a binary image
/// Centroid Distance Function
/// Since this shape signature is only dependent on the location of the centroid and the points on the 
/// boundary, it is invariant to the translation of the shape and also the rotation
/// </remarks>
(IList<double> CentroidDistanceSignature, Point Centroid) boundary2signature(IList<Point> boundaryPoints)
{
    var c = CalculateCentroid(boundaryPoints);

    //Centroid Distance Function
    List<double> signature = new();
    foreach (var point in boundaryPoints)
    {
        signature.Add(Math.Sqrt(Math.Pow(point.X - c.X, 2) + Math.Pow(point.Y - c.Y, 2)));
    }
    return (signature, c);
}
/// <summary>
/// Calculates the Centroid of a closed set of boundary points
/// </summary>
/// <remarks>
/// <seealso cref="https://en.wikipedia.org/wiki/Centroid#Of_a_polygon"/> 
/// <seealso cref="https://www.math.uci.edu/icamp/summer/research_11/klinzmann/cdfd.pdf"/>
/// </remarks>
Point CalculateCentroid(IList<Point> boundaryPoints)
{
    //the area of the shape, A, is given by the following equation
    var A = 0d;
    var N = boundaryPoints.Count;
    //0 and N index are the same point on our boundary
    var sum = 0;
    var cx = 0d;
    var cy = 0d;
    for (int i = 0; i < N - 1; i++)
    {
        var xi = boundaryPoints[i].X;
        var xi1 = boundaryPoints[i + 1].X;

        var yi1 = boundaryPoints[i + 1].Y;
        var yi = boundaryPoints[i].Y;

        cx = cx + (xi + xi1) * (xi * yi1 - xi1 * yi);
        cy = cy + (yi + yi1) * (xi * yi1 - xi1 * yi);
        sum = sum + (xi * yi1 - xi1 * yi);
    }
    //A=1/2 Sum_i^(N-1)(x_i*y_(i+1)-x_(i+1)*y_i)
    A = sum / 2d;
    cx = cx / (6 * A);
    cy = cy / (6 * A);




    return new((int)cx, (int)cy);
}