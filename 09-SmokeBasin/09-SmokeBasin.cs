using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _09_SmokeBasin
{
	class Program
	{

		// Print grid with "[X]" if co-ordinate has been considered up to this point
		static void PrintGrid(ref List<List<int>> heights, ref List<List<bool>> consideredSoFar)
		{
			string gridStr = "";
			for (int row = 0; row < heights.Count; ++row)
			{
				for (int col = 0; col < heights[row].Count; ++col)
				{
					gridStr += consideredSoFar[row][col] 
						? "[" + heights[row][col] + "]\t" : heights[row][col] + "\t";
				}
				gridStr += "\n";
			}
			Console.Write(gridStr);
		}

		// From the given co-ord, spread outwards until hitting a height of "9", keeping count of the area size 
		static int SpreadFromCoordToBasinEdge(int row, int col, ref List<List<int>> heights, ref List<List<bool>> consideredSoFar)
		{
			int area = 1;

			if (consideredSoFar[row][col])
			{
				// Already considered this co-ord, return early
				return 0;
			}

			// Mark this co-ord as considered
			consideredSoFar[row][col] = true;

			//Console.WriteLine("\nSpreading from (" + row + "," + col + ")...");
			//PrintGrid(ref heights, ref consideredSoFar);

			if (heights[row][col] == 9)
			{
				// Hit the edge - no longer in basin - don't add anything to area
				return 0;
			}

			if (row > 0)
			{
				// Spread above
				area += SpreadFromCoordToBasinEdge(row-1, col, ref heights, ref consideredSoFar);
			}

			if (row < heights.Count-1)
			{
				// Spread below
				area += SpreadFromCoordToBasinEdge(row+1, col, ref heights, ref consideredSoFar);
			}

			if (col > 0)
			{
				// Spread left
				area += SpreadFromCoordToBasinEdge(row, col-1, ref heights, ref consideredSoFar);
			}

			if (col < heights[row].Count-1)
			{
				// Spread below
				area += SpreadFromCoordToBasinEdge(row, col+1, ref heights, ref consideredSoFar);
			}

			return area;
		}

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			// Height values for each co-ord
			List<List<int>> heights = new List<List<int>>();
			
			// Has this co-ord been considered in part 2 yet?
			List<List<bool>> consideredCoord = new List<List<bool>>();
			
			// Store the lowest points of the heightmap
			List<Tuple<int, int>> lowPoints = new List<Tuple<int, int>>();

			for (int row = 0; row < lines.Length; ++row)
			{
				heights.Add(new List<int>());
				consideredCoord.Add(new List<bool>());

				string line = lines[row];
				for (int col = 0; col < line.Length; ++col)
				{
					int point = int.Parse(line[col] + "");
					
					heights.Last().Add(point);
					consideredCoord.Last().Add(false);

					if (row != 0)
					{
						// Check above
						int other = int.Parse(lines[row - 1][col] + "");
						if (other <= point)
							continue;
					}

					if (row != lines.Length - 1)
					{
						// Check below
						int other = int.Parse(lines[row + 1][col] + "");
						if (other <= point)
							continue;
					}


					if (col != 0)
					{
						// Check left
						int other = int.Parse(lines[row][col - 1] + "");
						if (other <= point)
							continue;
					}

					if (col != line.Length - 1)
					{
						// Check right
						int other = int.Parse(lines[row][col + 1] + "");
						if (other <= point)
							continue;
					}

					// If we got here, this is the lowest point of all surrounding tiles (note: not diagonally)
					lowPoints.Add(new Tuple<int, int>(row, col));
				}
			}

			// Part One
			{
				Console.WriteLine("Part One:");

				// Risk level = Height of low point + 1
				int riskLevelSum = 0;
				foreach (var lowPoint in lowPoints)
				{
					int height = heights[lowPoint.Item1][lowPoint.Item2];
					int riskLevel = height + 1;
					Console.WriteLine("Low point: (" + lowPoint.Item1 + "," + lowPoint.Item2 + ") -> " + height);
					riskLevelSum += riskLevel;
				}

				Console.WriteLine("Sum of Risk levels is: " + riskLevelSum + "\n");
			}

			// Part Two
			{
				// List of all basin areas so we can later grab largest 3 and multiply them
				List<int> basinAreas = new List<int>();
				foreach (var lowPoint in lowPoints)
				{
					int height = int.Parse(lines[lowPoint.Item1][lowPoint.Item2] + "");
					Console.WriteLine("Low point: (" + lowPoint.Item1 + "," + lowPoint.Item2 + ") -> " + height);

					// Spread out recursively from low point to find area before we hit a 9
					int basinArea = SpreadFromCoordToBasinEdge(lowPoint.Item1, lowPoint.Item2, ref heights, ref consideredCoord);

					Console.WriteLine("Basin area = " + basinArea);

					basinAreas.Add(basinArea);
				}

				// Sort by area from highest to lowest
				basinAreas.Sort();
				basinAreas.Reverse();

				int basinAreaProduct = basinAreas[0] * basinAreas[1] * basinAreas[2];
				Console.WriteLine("Part Two: ");
				Console.WriteLine("Basin Area Product is: " + basinAreaProduct);
			}
		}
	}
}
