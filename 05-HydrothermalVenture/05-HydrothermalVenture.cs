using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _05_HydrothermalVenture
{
	// Class for holding the hydrothermal line information
	class Grid
	{
		// Grid of amount of times a line covers a co-ordinate
		List<List<int>> lineCount = new List<List<int>>();
		
		int height = 0;
		int width = 0;

		// Add a line to the grid
		public void AddLine(int x1, int x2, int y1, int y2)
		{
			int lowY = Math.Min(y1, y2);
			int highY = Math.Max(y1, y2);

			int lowX = Math.Min(x1, x2);
			int highX = Math.Max(x1, x2);

			// Grow the height of the grid if needed
			if (highY >= lineCount.Count)
				GrowGridY(highY);

			// Grow the width of the grid if needed
			if (highX >= lineCount[0].Count)
				GrowGridX(highX);

			if (x1 == x2)
			{
				// Vertical line

				// Fill line from top to bottom
				for (int y = lowY; y <= highY; ++y)
				{
					lineCount[y][x1]++;
				}
			}
			else if (y1 == y2)
			{
				// Horizontal line

				// Fill line from left to right
				for (int x = lowX; x <= highX; ++x)
				{
					lineCount[y1][x]++;
				}
			}
			else
			{
				// Diagonal line

				int directionX = (x1 < x2) ? 1 : -1; // direction x value needs to move in to get from x1 to x2
				int directionY = (y1 < y2) ? 1 : -1; // direction y value needs to move in to get from y1 to y2

				int x = x1;
				int y = y1;
				// Iterate until we've hit x2,y2
				while (true)
				{
					//Console.WriteLine(x + "," + y);

					lineCount[y][x]++;

					x += directionX;
					y += directionY;
					if (y == y2) 
						break;
				}

				// Mark x2,y2 since it is included in range
				lineCount[y2][x2]++;
			}
		}

		// Grow width of grid if needed
		private void GrowGridX(int x)
		{
			if (width < x)
			{
				Console.WriteLine("Growing grid to x = " + x);
				foreach (var line in lineCount)
				{
					while (line.Count <= x)
					{
						line.Add(0);
					}
				}
				width = x;
			}
		}

		// Grow height of grid if needed
		private void GrowGridY(int y)
		{
			if (height < y)
			{
				Console.WriteLine("Growing grid to y = " + y);
				while (lineCount.Count <= y)
				{
					lineCount.Add(new List<int>());

					// Immediately grow new row to required width
					int i = 0;
					while (i++ <= width)
						lineCount.Last().Add(0);
				}
				height = y;
			}
		}

		public void PrintGrid()
		{
			foreach(var row in lineCount)
			{
				foreach(var col in row)
				{
					Console.Write(col == 0 ? "." : col.ToString());
				}
				Console.Write("\n");
			}
			Console.WriteLine();
		}

		// return number of co-ords with value > 1
		public int ComputeNumberOfOverlaps()
		{
			int overlaps = 0;
			foreach (var row in lineCount)
			{
				foreach (var col in row)
				{
					if (col > 1)
						overlaps++;
				}
			}
			return overlaps;
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			Grid lineGridHV = new Grid(); // Grid for just horizontal and vertical lines
			Grid lineGridDiagonals = new Grid(); // Grid for all lines

			foreach (var line in lines)
			{
				// Split line string into relevant info
				var coordTokens = line.Split(' ');

				var coords1 = coordTokens[0].Split(',');
				int x1 = int.Parse(coords1[0]);
				int y1 = int.Parse(coords1[1]);

				var coords2 = coordTokens[2].Split(',');
				int x2 = int.Parse(coords2[0]);
				int y2 = int.Parse(coords2[1]);

				// Only concerned about horizontal or vertical lines for first grid
				if (x1 == x2 || y1 == y2)
				{
					lineGridHV.AddLine(x1, x2, y1, y2);
				}

				Console.WriteLine("Adding: " + line);

				// Take into account diagonals for second grid
				lineGridDiagonals.AddLine(x1, x2, y1, y2);

				//lineGridDiagonals.PrintGrid();
			}

			//lineGrid.PrintGrid();

			//lineGridDiagonals.PrintGrid();

			// Part One
			{
				int numberOfOverlaps = lineGridHV.ComputeNumberOfOverlaps();
				Console.WriteLine("\nPart One:");
				Console.WriteLine("Number of Overlaps: " + numberOfOverlaps + "\n");
			}

			// Part Two
			{
				int numberOfOverlaps = lineGridDiagonals.ComputeNumberOfOverlaps();
				Console.WriteLine("Part Two:");
				Console.WriteLine("Number of Overlaps: " + numberOfOverlaps);
			}
		}
	}
}
