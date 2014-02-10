// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Kinect
{
    public static class ImageFrameExtensions
    {

    	public static short[] ToDepthArray(this DepthImageFrame image)
		{
			return ImageFrameCommonExtensions.ToDepthArray(image);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y")]
		public static int GetDistance(this DepthImageFrame image, int x, int y)
		{
			return ImageFrameCommonExtensions.GetDistance(image, x, y);
		}

    	public static Point GetMidpoint(this short[] depthData, int width, int height, int startX, int startY, int endX, int endY, int minimumDistance)
        {
            double x;
            double y;
            depthData.GetMidpoint(width, height, startX, startY, endX, endY, minimumDistance, out x, out y);

            return new Point(x, y);
        }

		public static BitmapSource ToBitmapSource(this short[] depthData, int width, int height, int minimumDistance, Color highlightColor)
        {
            if (depthData == null)
            {
                return null; 
            }
                //depthData must be array of distances already

                var depthColors = new byte[depthData.Length * 4];

                for (int colorIndex = 0, depthIndex = 0; colorIndex < depthColors.Length; colorIndex += 4, depthIndex++)
                {

                    //get the depth, then calculate the intensity (0-255 based on the depth)
                    //depth of -1 = dark brown                

                    if (depthData[depthIndex] == -1)
                    {
                        // dark brown
                        depthColors[colorIndex + ImageFrameCommonExtensions.RedIndex] = 66;
                        depthColors[colorIndex + ImageFrameCommonExtensions.GreenIndex] = 66;
                        depthColors[colorIndex + ImageFrameCommonExtensions.BlueIndex] = 33;
                    }
                    else
                    {
                        
                        var intensity = ImageFrameCommonExtensions.CalculateIntensityFromDepth(depthData[depthIndex]);

                        depthColors[colorIndex + ImageFrameCommonExtensions.RedIndex] = intensity;
                        depthColors[colorIndex + ImageFrameCommonExtensions.GreenIndex] = intensity;
                        depthColors[colorIndex + ImageFrameCommonExtensions.BlueIndex] = intensity;

                        //change color to highlight color
                        if (depthData[depthIndex] <= minimumDistance && depthData[depthIndex] > 0)
                        {
                            var color = Color.Multiply(highlightColor, intensity / 255f);

                            depthColors[colorIndex + ImageFrameCommonExtensions.RedIndex] = color.R;
                            depthColors[colorIndex + ImageFrameCommonExtensions.GreenIndex] = color.G;
                            depthColors[colorIndex + ImageFrameCommonExtensions.BlueIndex] = color.B;
                        }
                    }


 
                }

				return depthColors.ToBitmapSource(width, height);

        }


        public static BitmapSource ToBitmapSource(this DepthImageFrame image)
        {

            if (image == null)
            {
                return null; 
            }

            var bytes = ImageFrameCommonExtensions.ConvertDepthFrameToBitmap(image);
            return bytes.ToBitmapSource(image.Width, image.Height); 

        }
        
        public static BitmapSource ToBitmapSource(this ColorImageFrame image)
        {
            if (image == null)
            {
                return null; 
            }

            byte[] colorData = new byte[image.PixelDataLength];
            image.CopyPixelDataTo(colorData);

            return colorData.ToBitmapSource(image.Width, image.Height); 

            
            
        }
    }
}
