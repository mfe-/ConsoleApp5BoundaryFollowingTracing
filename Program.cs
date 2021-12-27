// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using ConsoleApp5BoundaryFollowingTracing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

Console.WriteLine("Hello, World!");

#if !DEBUG
var summary = BenchmarkRunner.Run<BoundaryBenchmark>();
summary = BenchmarkRunner.Run<FourierDescriptorsBenchmark>();
#endif


//string rootleavesplantspeciesPath = @"C:\Users\marti\OneDrive\Dokumente\IPA\lab3\100 leaves plant species\data";
string rootleavesplantspeciesPath = @"C:\Users\U01K1RM\Downloads\100 leaves plant species\100 leaves plant species\data";
var database = FourierDescriptors.BuildFourierDecriptorsAsync(rootleavesplantspeciesPath);


do
{
    Console.WriteLine($"Enter a reference image, so we can look up similar images from {Environment.NewLine}{rootleavesplantspeciesPath}");
    string? imgName = Console.ReadLine();

    var searchresult = database.SelectMany(a => a.Value).Where(b => String.Equals(b.ImageName, imgName, StringComparison.OrdinalIgnoreCase));
    if (searchresult.Any())
    {
        FourierDescriptors.FindSimilarImages(database, searchresult.First().NormalizedFourierDescriptor);
    }
    else
    {
        Console.WriteLine("Nothing found. Enter an other image");
    }

} while (true);


var bytes = await File.ReadAllBytesAsync("Acer_Campestre_01.ab.jpg");
//var bytes = await File.ReadAllBytesAsync("acer_pictum_03.ab.jpg");
using var image = await Image.LoadAsync<Rgba32>(new MemoryStream(bytes));

var contour = BoundaryProcessingTracevInlineMethodv3.BoundaryProcessingTraceAsync(image);
using var onlyCountourImage = new Image<Rgba32>(image.Width, image.Height);
foreach (var point in contour)
{
    onlyCountourImage[point.X, point.Y] = new Rgba32(217, 30, 24, 255);
}

var centerSignature = FourierDescriptors.Boundary2CentroidSignature(contour);

onlyCountourImage[centerSignature.Centroid.X, centerSignature.Centroid.Y] = new Rgba32(217, 30, 24, 255);

onlyCountourImage.SaveAsPng("Acer_Campestre_01_BoundaryProcessingTrace.ab.png");