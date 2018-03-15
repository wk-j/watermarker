#r "../packages/Magick.NET-Q8-x64/lib/net40/Magick.NET-Q8-x64.dll"

open ImageMagick
open System.IO

let image = new MagickImage(new FileInfo("scripts/images/jw1.png"))
let size = new MagickGeometry(100, 100)
image.Resize(size)

image.Write("scripts/images/jw1-100x100.png")