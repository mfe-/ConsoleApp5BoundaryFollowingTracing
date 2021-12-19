// See https://aka.ms/new-console-template for more information
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Diagnostics;
//var bytes = await File.ReadAllBytesAsync("acer_pictum_03.ab.jpg");
//using var image = await Image.LoadAsync<Rgba32>(new MemoryStream(bytes));

//var contour = BoundaryProcessingTraceAsync(image);
//using var onlyCountourImage = new Image<Rgba32>(image.Width, image.Height);
//foreach (var point in contour)
//{
//    onlyCountourImage[point.X, point.Y] = new Rgba32(217, 30, 24, 255);
//}

//var centerSignature = boundary2signature(contour);

//onlyCountourImage[centerSignature.Centroid.X, centerSignature.Centroid.Y] = new Rgba32(217, 30, 24, 255);

//onlyCountourImage.SaveAsPng("Acer_Campestre_01_BoundaryProcessingTrace.ab.png");
[DebuggerDisplay("ImageName={ImageName},Signature={Signature.Count} BoundaryPoints={BoundaryPoints.Count}")]
public class ImageData
{
    public IList<Point>? BoundaryPoints { get; set; }
    public string ImageName { get; set; } = "";
    public IList<double> Signature { get; set; } = new List<double>();
    public IList<double> NormalizedFourierDescriptor { get; set; } = new List<double>();
}
