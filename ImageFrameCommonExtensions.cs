// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Kinect;
using System.Diagnostics; 

[assembly: CLSCompliant(true)]
namespace Microsoft.Kinect
{
	internal static class ImageFrameCommonExtensions
	{
		public const int RedIndex = 2;
		public const int GreenIndex = 1;
		public const int BlueIndex = 0;

		const float MaxDepthDistance = 4000; // 4000 seemed to be max value returned
		const float MinDepthDistance = 800; // 800 seemed to be the min value returned
		const float MaxDepthDistanceOffset = MaxDepthDistance - MinDepthDistance;

		public static int GetDistance(this DepthImageFrame depthFrame, int x, int y)
		{
		   		
			var width = depthFrame.Width;

			if (x > width)
				throw new ArgumentOutOfRangeException("x", "x is larger than the width");

			if (y > depthFrame.Height)
				throw new ArgumentOutOfRangeException("y", "y is larger than the height");

			if (x < 0)
				throw new ArgumentOutOfRangeException("x", "x is smaller than zero");

			if (y < 0)
				throw new ArgumentOutOfRangeException("y", "y is smaller than zero");

			
			var index = (width * y) + x;
	
            short[] allData = new short[depthFrame.PixelDataLength];
            
            depthFrame.CopyPixelDataTo(allData);
            
            return GetDepth(allData[index]); 

		}

		public static void GetMidpoint(this short[] depthData, int width, int height, int startX, int startY, int endX, int endY, int minimumDistance, out double xLocation, out double yLocation)
		{
			if (depthData == null)
				throw new ArgumentNullException("depthData");

			if (width * height != depthData.Length)
				throw new ArgumentOutOfRangeException("depthData", "Depth Data length does not match target height and width");

			if (endX > width)
				throw new ArgumentOutOfRangeException("endX", "endX is larger than the width");

			if (endY > height)
				throw new ArgumentOutOfRangeException("endY", "endY is larger than the height");

			if (startX < 0)
				throw new ArgumentOutOfRangeException("startX", "startX is smaller than zero");

			if (startY < 0)
				throw new ArgumentOutOfRangeException("startY", "startY is smaller than zero");

			xLocation = 0;
			yLocation = 0;

			var counter = 0;
			for (var x = startX; x < endX; x++)
			{
				for (var y = startY; y < endY; y++)
				{
					var depth = depthData[x + width * y];
					if (depth > minimumDistance || depth <= 0)
						continue;

					xLocation += x;
					yLocation += y;

					counter++;
				}
			}

			if (counter <= 0)
				return;

			xLocation /= counter;
			yLocation /= counter;
		}

	   
		public static short[] ToDepthArray(this DepthImageFrame image)
		{
			if (image == null)
				throw new ArgumentNullException("image");

			var width = image.Width;
			var height = image.Height;
			//var depthArray = new short[width * image.Height];

			short[] rawDepthData = new short[image.PixelDataLength]; 
			image.CopyPixelDataTo(rawDepthData); 

			//loop through rawDepthData and add the depth
			
			short[] allPoints = new short[image.PixelDataLength];

			for (int i = 0; i < rawDepthData.Length; i++)
			{
				allPoints[i] = (short) GetDepth(rawDepthData[i]); 
			}
		 
			return allPoints;
		}


		public static byte CalculateIntensityFromDepth(int distance)
		{
		
			return (byte)(255 - (255 * Math.Max(distance - MinDepthDistance, 0) / (MaxDepthDistanceOffset)));
		}


		public static void SkeletonOverlay(ref byte redFrame, ref byte greenFrame, ref byte blueFrame, int player)
		{
			switch (player)
			{
				default: // case 0:
					break;
				case 1:
					greenFrame = 0;
					blueFrame = 0;
					break;
				case 2:
					redFrame = 0;
					greenFrame = 0;
					break;
				case 3:
					redFrame = 0;
					blueFrame = 0;
					break;
				case 4:
					greenFrame = 0;
					break;
				case 5:
					blueFrame = 0;
					break;
				case 6:
					redFrame = 0;
					break;
				case 7:
					redFrame /= 2;
					blueFrame = 0;
					break;
			}
		}

		public static byte[] ConvertDepthFrameToBitmap(DepthImageFrame depthFrame)
		{
            if (depthFrame == null)
            {
                return null; 
            }

            short[] depthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(depthData); 

			Byte[] depthColors = new Byte[depthData.Length * 4]; //four colors for each pixel

            for (int colorIndex = 0, depthIndex = 0; colorIndex < depthColors.Length;  colorIndex += 4, depthIndex++)
			{

				//get the depth, then calculate the intensity (0-255 based on the depth)
                //depth of -1 = dark brown                

                int depth = GetDepth(depthData[depthIndex]);

                if (depth == -1)
                {
                    // dark brown
                    depthColors[colorIndex + RedIndex] = 66;
                    depthColors[colorIndex + GreenIndex] = 66;
                    depthColors[colorIndex + BlueIndex] = 33;
                }
                else
                {
                    var intensity = ImageFrameCommonExtensions.CalculateIntensityFromDepth(depth);

                    depthColors[colorIndex + RedIndex] = intensity;
                    depthColors[colorIndex + GreenIndex] = intensity;
                    depthColors[colorIndex + BlueIndex] = intensity;
                }

			

                //if the pixel is a player, choose a color
                int player = GetPlayerIndex(depthData[depthIndex]);
                SkeletonOverlay(
                    ref depthColors[colorIndex + RedIndex],
                    ref depthColors[colorIndex + GreenIndex],
                    ref depthColors[colorIndex + BlueIndex], player);
			}
            return depthColors; 

		
		}



		public static int GetPlayerIndex(short depthPoint)
		{            
			return depthPoint & DepthImageFrame.PlayerIndexBitmask;
		}

		public static int GetDepth(short depthPoint)
		{
			return depthPoint >> DepthImageFrame.PlayerIndexBitmaskWidth;
		}


	}
}
