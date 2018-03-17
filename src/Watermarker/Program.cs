using System;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Brushes;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using IO = System.IO;

namespace Watermarker {
    static class Program {
        static async Task Main(string[] args) {

            if (args.Length != 2) return;
            var url = args[1];
            var text = args[0];

            Console.WriteLine($@"> downloading ""{url}""");
            var image = await DownloadImage(url);
            var fileName = IO.Path.GetFileName(image);
            var newName = "watermarker-" + IO.Path.ChangeExtension(fileName, ".png");

            using (var img = Image.Load(image)) {
                Font font = SystemFonts.CreateFont("Arial", 30);
                using (Image<Rgba32> image1 = img.Clone(x => x.ConvertToAvatar(new Size(400, 300), 25))) {
                    using (var img2 = image1.Clone(ctx => ctx.ApplyScalingWaterMark(font, text, Rgba32.HotPink, 50, false))) {
                        Console.WriteLine($@"> saving ""{newName}""");
                        img2.Save(newName);
                    }
                }
            }
        }

        static async Task<string> DownloadImage(string url) {
            using (var client = new HttpClient()) {
                var rs = await client.GetAsync(url);
                var content = await rs.Content.ReadAsByteArrayAsync();
                var localPath = new Uri(url).LocalPath;
                var fileName = IO.Path.GetFileName(localPath);
                var temp = IO.Path.Combine(IO.Path.GetTempPath(), fileName);
                IO.File.WriteAllBytes(temp, content);
                return temp;
            }
        }

        public static IImageProcessingContext<TPixel> ApplyScalingWaterMark<TPixel>(this IImageProcessingContext<TPixel> processingContext, Font font, string text, TPixel color, float padding, bool wordwrap)
           where TPixel : struct, IPixel<TPixel> {
            if (wordwrap) {
                return processingContext.ApplyScalingWaterMarkWordWrap(font, text, color, padding);
            } else {
                return processingContext.ApplyScalingWaterMarkSimple(font, text, color, padding);
            }
        }

        public static IImageProcessingContext<TPixel> ApplyScalingWaterMarkSimple<TPixel>(this IImageProcessingContext<TPixel> processingContext, Font font, string text, TPixel color, float padding)
            where TPixel : struct, IPixel<TPixel> {
            return processingContext.Apply(img => {
                float targetWidth = img.Width - (padding * 2);
                float targetHeight = img.Height - (padding * 2);

                SizeF size = TextMeasurer.Measure(text, new RendererOptions(font));

                // float scalingFactor = Math.Min(img.Width / size.Width, img.Height / size.Height);
                float scalingFactor = Math.Min(targetWidth / size.Width, targetHeight / size.Height);

                Font scaledFont = new Font(font, scalingFactor * font.Size);

                var center = new PointF(img.Width / 2, img.Height / 2);

                img.Mutate(i => i.DrawText(text, scaledFont, color, center, new TextGraphicsOptions(true) {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }));
            });
        }

        public static IImageProcessingContext<TPixel> ApplyScalingWaterMarkWordWrap<TPixel>(this IImageProcessingContext<TPixel> processingContext, Font font, string text, TPixel color, float padding)
            where TPixel : struct, IPixel<TPixel> {
            return processingContext.Apply(img => {
                float targetWidth = img.Width - (padding * 2);
                float targetHeight = img.Height - (padding * 2);

                float targetMinHeight = img.Height - (padding * 3);

                var scaledFont = font;
                SizeF s = new SizeF(float.MaxValue, float.MaxValue);

                float scaleFactor = (scaledFont.Size / 2);
                int trapCount = (int)scaledFont.Size * 2;
                if (trapCount < 10) {
                    trapCount = 10;
                }

                bool isTooSmall = false;

                while ((s.Height > targetHeight || s.Height < targetMinHeight) && trapCount > 0) {
                    if (s.Height > targetHeight) {
                        if (isTooSmall) {
                            scaleFactor = scaleFactor / 2;
                        }

                        scaledFont = new Font(scaledFont, scaledFont.Size - scaleFactor);
                        isTooSmall = false;
                    }

                    if (s.Height < targetMinHeight) {
                        if (!isTooSmall) {
                            scaleFactor = scaleFactor / 2;
                        }
                        scaledFont = new Font(scaledFont, scaledFont.Size + scaleFactor);
                        isTooSmall = true;
                    }
                    trapCount--;

                    s = TextMeasurer.Measure(text, new RendererOptions(scaledFont) {
                        WrappingWidth = targetWidth
                    });
                }

                var center = new PointF(padding, img.Height / 2);
                img.Mutate(i => i.DrawText(text, scaledFont, color, center, new TextGraphicsOptions(true) {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    WrapTextWidth = targetWidth
                }));
            });
        }
    }
}
