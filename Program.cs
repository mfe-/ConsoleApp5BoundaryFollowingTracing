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