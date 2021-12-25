using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp5BoundaryFollowingTracing;

internal class FourierDescriptors
{

    /// <summary>
    /// The input to the function is a list of boundary points and the ouput is a 1D vector representing the signature.
    /// </summary>
    /// <remarks>
    /// The shape in this particular context is expected to be a binary image
    /// Centroid Distance Function
    /// Since this shape signature is only dependent on the location of the centroid and the points on the 
    /// boundary, it is invariant to the translation of the shape and also the rotation
    /// </remarks>
    public static (IList<double> CentroidDistanceSignature, Point Centroid) boundary2signature(IList<Point> boundaryPoints)
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
}

