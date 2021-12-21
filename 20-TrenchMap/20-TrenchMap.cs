using System;
using System.Collections.Generic;
using System.IO;

namespace _20_TrenchMap
{
	class Image
	{
		List<string> pixels = new List<string>();
		
		int width = 0;
		int height = 0;

		int timesEnhanced = 0;		// The amount of times this image has been enhanced
		char infinitePixel = '.';	// The current state of all infinite pixels

		public void AddPixels(string pixelLine)
		{
			pixels.Add(pixelLine);

			width = pixelLine.Length;
			height = pixels.Count;
		}

		public void EnhanceImage(string enhancementAlgorithm)
		{
			// Grow image by 2 in all directions (since image is infinite)
			{
				int newWidth = width + 2;
				int newHeight = height + 2;

				// Grow using the current state of the infinite pixels
				string emptyLine = "";
				for (int i = 0; i < newWidth; ++i)
					emptyLine += infinitePixel;

				// Add left and right pixels
				for (int i = 0; i < height; ++i)
					pixels[i] = infinitePixel + pixels[i] + infinitePixel;

				// Add top and bottom line
				pixels.Insert(0, emptyLine);
				pixels.Add(emptyLine);

				width = newWidth;
				height = newHeight;
			}

			List<List<int>> enhancementIndices = new List<List<int>>();
			// Check each pixel to get enhancement indices
			{
				for(int row = 0; row < height; ++row)
				{
					enhancementIndices.Add(new List<int>());
					for (int col = 0; col < width; ++col)
					{
						enhancementIndices[row].Add(CalculateEnhancementIndex(row, col));
					}
				}
			}

			// Enhance pixels from indices calculated simultaneously
			{
				for (int row = 0; row < height; ++row)
				{
					string newLine = "";
					for (int col = 0; col < width; ++col)
					{
						newLine += enhancementAlgorithm[enhancementIndices[row][col]];
					}
					pixels[row] = newLine;
				}
			}

			timesEnhanced++;

			// Flip the infinite pixels to their inverse states if needed
			// (i.e. if all surrounding pixels are unlit, we take index 0 -> lit pixel
			//   and if all surrounding pixels are lit, we take index 255 -> unlit pixel)
			if (enhancementAlgorithm[0] == '#' && enhancementAlgorithm[255] == '.')
				infinitePixel = timesEnhanced % 2 != 0 ? '#' : '.';
		}

		public int CalculateEnhancementIndex(int row, int col)
		{
			// Create binary string from surrounding pixels and convert to int index for the enhancement algorithm
			string binaryString = "";

			bool isTop = row == 0;
			bool isLeft = col == 0;
			bool isBottom = row == pixels.Count - 1;
			bool isRight = col == pixels[0].Length - 1;

			char infiniteBit = infinitePixel == '#' ? '1' : '0';

			// Top left
			bool hasTopLeft = !isTop && !isLeft;
			if (hasTopLeft)
				binaryString += pixels[row - 1][col - 1] == '#' ? "1" : "0";
			else
				binaryString += infiniteBit;

			// Top
			bool hasTop = !isTop;
			if (hasTop)
				binaryString += pixels[row - 1][col] == '#' ? "1" : "0";
			else
				binaryString += infiniteBit;

			// Top right
			bool hasTopRight = !isTop && !isRight;
			if (hasTopRight)
				binaryString += pixels[row - 1][col + 1] == '#' ? "1" : "0";
			else
				binaryString += infiniteBit;

			// Left
			bool hasLeft = !isLeft;
			if (hasLeft)
				binaryString += pixels[row][col-1] == '#' ? "1" : "0";
			else
				binaryString += infiniteBit;

			// Middle
			binaryString += pixels[row][col] == '#' ? "1" : "0";

			// Right
			bool hasRight = !isRight;
			if (hasRight)
				binaryString += pixels[row][col+1] == '#' ? "1" : "0";
			else
				binaryString += infiniteBit;

			// Bottom left
			bool hasBottomLeft = !isBottom && !isLeft;
			if (hasBottomLeft)
				binaryString += pixels[row+1][col - 1] == '#' ? "1" : "0";
			else
				binaryString += infiniteBit;

			// Bottom
			bool hasBottom = !isBottom;
			if (hasBottom)
				binaryString += pixels[row+1][col] == '#' ? "1" : "0";
			else
				binaryString += infiniteBit;

			// Bottom right
			bool hasBottomRight = !isBottom && !isRight;
			if (hasBottomRight)
				binaryString += pixels[row+1][col+1] == '#' ? "1" : "0";
			else
				binaryString += infiniteBit;

			return Convert.ToInt32(binaryString, 2);
		}

		public int GetNumLitPixels()
		{
			int numberLit = 0;
			for (int row = 0; row < height; ++row)
			{
				for (int col = 0; col < width; ++col)
				{
					if (pixels[row][col] == '#')
						numberLit++;
				}
			}
			return numberLit;
		}

		public void PrintImage()
		{
			Console.WriteLine("\n==== Image ====");
			foreach (var line in pixels)
			{
				string output = "";
				foreach (var pixel in line)
				{
					output += pixel + " ";
				}
				Console.WriteLine(output);
			}
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");
			string enhancementAlgorithm = lines[0];

			// Part One
			{
				Console.WriteLine("\nPart One:");
				Image image = new Image();
				for (int i = 2; i < lines.Length; ++i)
				{
					image.AddPixels(lines[i]);
				}

				Console.WriteLine("\nBefore:");
				image.PrintImage();

				// Enhance twice
				image.EnhanceImage(enhancementAlgorithm);
				image.EnhanceImage(enhancementAlgorithm);

				Console.WriteLine("\nAfter:");
				image.PrintImage();

				Console.WriteLine("Number of lit pixels after 2 enhancements is: " + image.GetNumLitPixels());
			}

			// Part Two
			{
				Console.WriteLine("\nPart Two:");
				Image image = new Image();
				for (int i = 2; i < lines.Length; ++i)
				{
					image.AddPixels(lines[i]);
				}

				Console.WriteLine("\nBefore:");
				image.PrintImage();

				// Enhance 50 times
				for (int i = 0; i < 50; ++i)
				{
					image.EnhanceImage(enhancementAlgorithm);
				}
				Console.WriteLine("\nAfter:");
				image.PrintImage();

				Console.WriteLine("Number of lit pixels after 50 enhancements is: " + image.GetNumLitPixels());
			}
		}
	}
}
