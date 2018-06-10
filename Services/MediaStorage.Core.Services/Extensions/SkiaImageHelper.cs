using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;

namespace MediaStorage.Common.Extensions
{
    public static class SkiaImageHelper
    {
        public static bool ResizeImage(byte[] imageData, IEnumerable<Tuple<int, int>> widthHeightTuples, Action<Stream, int, int> actionResizeCallback, SKEncodedImageFormat format = SKEncodedImageFormat.Jpeg, int quality = 75)
        {
            using(var img = SKBitmap.Decode(imageData))
            {
                int width = 0, height = 0;
                foreach(var size in widthHeightTuples)
                {
                    width = size.Item1;
                    height = size.Item2;

                    using(var stream = new MemoryStream())
                    {
                        if(ResizeImageAndEncode(img, width, height, SKBitmapResizeMethod.Lanczos3, format, quality, stream))
                        {
                            stream.Position = 0; // Reset position.
                            actionResizeCallback.Invoke(stream, width, height);
                        }
                    }
                }
            }

            return true;
        }

        static bool ResizeImageAndEncode(SKBitmap originalImg, int width, int height, SKBitmapResizeMethod resizeMethod, SKEncodedImageFormat format, int quality, Stream outStream)
        {            
            using(var scaled = originalImg.Resize(new SKImageInfo(width, height), resizeMethod))
            {
                using(var stream = new SKManagedWStream(outStream))
                {
                   return scaled.Encode(stream, format, quality);
                }
            }
        }
    }
}
