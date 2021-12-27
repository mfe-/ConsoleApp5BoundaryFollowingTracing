using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ConsoleApp5BoundaryFollowingTracing;

[MinColumn, MaxColumn, MeanColumn, MedianColumn, MemoryDiagnoser]
public class FourierDescriptorsBenchmark
{
    IList<Point> _points;
    [GlobalSetup]
    public void Setup()
    {
        var bytes = File.ReadAllBytes("acer_pictum_03.ab.jpg");
        using var image = Image.Load<Rgba32>(new MemoryStream(bytes));
        _points = BoundaryProcessingTracevInlineMethodv3.BoundaryProcessingTraceAsync(image);
    }
    [Benchmark]
    public (IList<double> CentroidDistanceSignature, Point Centroid) BoundaryImpl1() =>
        FourierDescriptors.Boundary2CentroidSignature(_points);

}

