using System;
using System.Diagnostics;
using Foundation;
using ImageIO;
using VS4Mac.AssetStudio.Models;

namespace VS4Mac.AssetStudio.Helpers
{
    public static class ImageHelper
    {
        public static Metadata GetImageMetadata(string path)
        {
            try
            {
                Metadata result = new Metadata();
                NSUrl url = new NSUrl(path: path, isDir: false);
                CGImageSource myImageSource = CGImageSource.FromUrl(url, null);
                var ns = new NSDictionary();

                //Dimensions
                NSObject width;
                NSObject height;

                using (NSDictionary imageProperties = myImageSource.CopyProperties(ns, 0))
                {
                    var tiff = imageProperties.ObjectForKey(CGImageProperties.TIFFDictionary) as NSDictionary;
                    width = imageProperties[CGImageProperties.PixelWidth];
                    height = imageProperties[CGImageProperties.PixelHeight];
                }

                result.Height = Convert.ToDouble(height.ToString());
                result.Width = Convert.ToDouble(width.ToString());

                //FileSize
                var res = url.GetResourceValues(new NSString[] { NSUrl.FileSizeKey }, out NSError fileSizeError);

                if (fileSizeError == null)
                {
                    result.FileSize = long.Parse((res[NSUrl.FileSizeKey]).ToString());
                }

                return result;
            }
            catch
            {
                Debug.WriteLine("An error occurred getting the image information.");

                return null;
            }
        }

        public static string GetImageSize(Metadata metadata)
        {
            var imageSizeKB = Math.Round(metadata.FileSize / 1024f, 2);
            var imageSizeMB = Math.Round(imageSizeKB / 1024f, 2);

            if(imageSizeMB < 1)
            {
                return string.Format("{0} KB", imageSizeKB);
            }

            return string.Format("{0} MB", imageSizeMB);
        }

        public static string GetImageSize(long imageSize)
        {
            var imageSizeKB = Math.Round(imageSize / 1024f, 2);
            var imageSizeMB = Math.Round(imageSizeKB / 1024f, 2);

            if (imageSizeMB < 1)
            {
                return string.Format("{0} KB", imageSizeKB);
            }

            return string.Format("{0} MB", imageSizeMB);
        }
    }
}