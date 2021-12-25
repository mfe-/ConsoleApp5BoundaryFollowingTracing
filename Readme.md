
# Implementation of BOUNDARY FOLLOWING (TRACING)

by the descriped steps from `Digital Image Processing 4th Edition Rafael C. Gonzalez Richard E. Woods`

See also:
http://www.imageprocessingplace.com/downloads_V3/root_downloads/tutorials/contour_tracing_Abeer_George_Ghuneim/moore.html

Sample images from:
https://archive.ics.uci.edu/ml/machine-learning-databases/00241/

 ##calculate signature of  boundary

- https://www.math.uci.edu/icamp/summer/research_11/klinzmann/cdfd.pdf
- https://users.monash.edu.au/~dengs/resource/papers/accv_fd.pdf
- https://en.wikipedia.org/wiki/Centroid#Of_a_polygon

## Benchmark
|                          Method |     Mean |   Error |   StdDev |      Min |      Max |   Median |   Gen 0 |  Gen 1 | Allocated |
|-------------------------------- |---------:|--------:|---------:|---------:|---------:|---------:|--------:|-------:|----------:|
|                   BoundaryImpl1 | 167.9 μs | 3.34 μs |  9.48 μs | 152.4 μs | 192.2 μs | 166.2 μs | 10.2539 | 1.9531 |     64 KB |
| BoundaryImpl_AggressiveInlining | 385.9 μs | 6.70 μs |  5.94 μs | 377.6 μs | 400.6 μs | 386.3 μs | 10.2539 | 1.9531 |     64 KB |
|  BoundaryImpl_ImageSharpRowSpan | 168.7 μs | 3.42 μs | 10.10 μs | 151.6 μs | 193.2 μs | 166.7 μs | 10.2539 | 1.9531 |     64 KB |

`dotnet run --project ConsoleApp5BoundaryFollowingTracing.csproj -c Release`