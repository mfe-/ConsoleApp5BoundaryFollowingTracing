using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Reflection;
using Xunit;

namespace ConsoleApp5BoundaryFollowingTracing
{

#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    public class BoundaryTest : IDisposable
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
    {
        public Image<Rgba32> _image;
        public List<Point> _expectedPoints = new();
        public BoundaryTest()
        {
            var assembly = Assembly.GetExecutingAssembly();
            //load expected contour result
            using Stream? stream = assembly?.GetManifestResourceStream("ConsoleApp5BoundaryFollowingTracing.Acer_Campestre_01.ab.jpg.result.txt");
            if (stream == null) throw new InvalidOperationException("Stream was null!");
            using StreamReader reader = new StreamReader(stream);
            while (reader.EndOfStream == false)
            {
                var lines = reader.ReadLine()?.Split(";");
                if (lines != null)
                {
                    var x = int.Parse(lines[0]);
                    var y = int.Parse(lines[1]);
                    _expectedPoints.Add(new Point(x, y));
                }
            }
            var bytes = File.ReadAllBytes("Acer_Campestre_01.ab.jpg");
            _image = Image.Load<Rgba32>(new MemoryStream(bytes));
        }
        [Fact]
        public void DoesBoundaryProcessingTraceAsync_return_correct_positions()
        {
            var list = BoundaryProcessingTrace.BoundaryProcessingTraceAsync(_image);
            Assert.Equal(_expectedPoints, list);
        }
        [Fact]
        public void DoesBoundaryProcessingTraceAsyncAggressiveInliningv_return_correct_positions()
        {
            var list = BoundaryProcessingTraceAggressiveInliningv2.BoundaryProcessingTraceAsync(_image);
            Assert.Equal(_expectedPoints, list);
        }
        [Fact]
        public void DoesBoundaryProcessingTraceAsyncImageSharp3_return_correct_positions()
        {
            var list = BoundaryProcessingTracevInlineMethodv3.BoundaryProcessingTraceAsync(_image);
            Assert.Equal(_expectedPoints, list);
        }
        //[Fact]
        //public void DoesBoundaryProcessingTraceInliningMethod_return_correct_positions()
        //{
        //    var list = BoundaryProcessingTracevInliningMethodv4.BoundaryProcessingTraceAsync(_image);
        //    Assert.Equal(_expectedPoints, list);
        //}
        public void Dispose()
        {
            _image.Dispose();
        }
    }
}
