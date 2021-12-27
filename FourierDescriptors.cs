﻿using MathNet.Numerics.IntegralTransforms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp5BoundaryFollowingTracing;

internal class FourierDescriptors
{
    public static Dictionary<string, IList<ImageData>> BuildFourierDecriptorsAsync(string dictionary)
    {
        string rootleavesplantspeciesPath = dictionary;
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
                var bytes = File.ReadAllBytes(img.FullName);
                using var image = Image.Load<Rgba32>(new MemoryStream(bytes));

                var boundaryPoints = BoundaryProcessingTrace.BoundaryProcessingTraceAsync(image);
                var signature = Boundary2CentroidSignature(boundaryPoints);
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

                plantdata.NormalizedFourierDescriptor = ToFourier(plantdata.Signature);

            }
        }

        return plantspecies;
    }
    public static void FindSimilarImages(IDictionary<string, IList<ImageData>> plantSpeciesvaluePairs, IList<double> normalizedFourierDescriptor)
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

    /// <summary>
    /// The input to the function is a list of boundary points and the ouput is a 1D vector representing the signature.
    /// </summary>
    /// <remarks>
    /// The shape in this particular context is expected to be a binary image
    /// Centroid Distance Function
    /// Since this shape signature is only dependent on the location of the centroid and the points on the 
    /// boundary, it is invariant to the translation of the shape and also the rotation
    /// </remarks>
    public static (IList<double> CentroidDistanceSignature, Point Centroid) Boundary2CentroidSignature(IList<Point> boundaryPoints)
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
    public static Point CalculateCentroid(IList<Point> boundaryPoints)
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
    public static IList<double> ToFourier(IList<double> signature)
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
}

