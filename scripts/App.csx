#! "netcoreapp2.0"
#r "nuget:NetStandard.Library,2.0"
#r "nuget:Magick.NET-Q16-AnyCPU,7.4.3"

using ImageMagick;

var image = new MagickImage(new FileInfo("scripts/images/jw1.png"));
var size = new MagickGeometry(100, 100);
image.Resize(size);

image.Write("scripts/images/jw1-100x100.png");

