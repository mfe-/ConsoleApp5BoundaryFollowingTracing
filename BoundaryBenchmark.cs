using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ConsoleApp5BoundaryFollowingTracing;

//[SimpleJob(RunStrategy.ColdStart, targetCount: 10_000)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn, MemoryDiagnoser]
public class BoundaryBenchmark
{
    Image<Rgba32> image;
    [GlobalSetup]
    public void Setup()
    {
        var bytes = File.ReadAllBytes("acer_pictum_03.ab.jpg");
        image = Image.Load<Rgba32>(new MemoryStream(bytes));
    }
    [Benchmark]
    public IList<Point> BoundaryImpl1() => BoundaryProcessingTracevInlineMethodv3.BoundaryProcessingTraceAsync(image);
    [Benchmark]
    public IList<Point> BoundaryImpl_AggressiveInlining() => BoundaryProcessingTraceAggressiveInliningv2.BoundaryProcessingTraceAsync(image);
    [Benchmark]
    public IList<Point> BoundaryImpl_ImageSharpRowSpan() => BoundaryProcessingTracevInlineMethodv3.BoundaryProcessingTraceAsync(image);

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        image.Dispose();
    }
}

