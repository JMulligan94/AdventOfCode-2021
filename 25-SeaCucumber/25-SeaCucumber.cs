using System;
using System.Collections.Generic;
using System.IO;

namespace _25_SeaCucumber
{
	class Program
	{
		static int PerformStep(ref char[,] grid, int width, int height)
		{
			int totalToMove = 0;
			List<Tuple<int, int>> toMove = new List<Tuple<int, int>>();

			// EASTWARD
			// Get which cucumbers need moving
			for (int row = 0; row < height; ++row)
			{
				for (int col = 0; col < width; ++col)
				{
					if (grid[row, col] == '>')
					{
						bool needsWrapping = col == width - 1;
						int targetCol = needsWrapping ? 0 : col + 1;
						if (grid[row, targetCol] == '.')
							toMove.Add(new Tuple<int, int>(row, col));
					}
				}
			}

			// Move eastward cucumbers forward
			foreach(var coord in toMove)
			{
				bool needsWrapping = coord.Item2 == width - 1;
				int targetCol = needsWrapping ? 0 : coord.Item2 + 1;
				grid[coord.Item1, coord.Item2] = '.';
				grid[coord.Item1, targetCol] = '>';
			}

			totalToMove += toMove.Count;
			toMove.Clear();

			// SOUTHWARD
			// Get which cucumbers need moving
			for (int row = 0; row < height; ++row)
			{
				for (int col = 0; col < width; ++col)
				{
					if (grid[row, col] == 'v')
					{
						bool needsWrapping = row == height - 1;
						int targetRow = needsWrapping ? 0 : row + 1;
						if (grid[targetRow, col] == '.')
							toMove.Add(new Tuple<int, int>(row, col));
					}
				}
			}

			// Move southward cucumbers forward
			foreach (var coord in toMove)
			{
				bool needsWrapping = coord.Item1 == height - 1;
				int targetRow = needsWrapping ? 0 : coord.Item1 + 1;
				grid[coord.Item1, coord.Item2] = '.';
				grid[targetRow, coord.Item2] = 'v';
			}

			totalToMove += toMove.Count;

			return totalToMove;
		}

		static void PrintGrid(ref char[,] grid, int width, int height)
		{
			for (int row = 0; row < height; ++row)
			{
				string output = "";
				for (int col = 0; col < width; ++col)
				{
					output += grid[row, col];
				}
				Console.WriteLine(output);
			}
			Console.WriteLine();
		}

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			// Read cucumbers into grid
			int height = lines.Length;
			int width = lines[0].Length;
			char[,] grid = new char[height, width];
			for(int row = 0; row < height; ++row)
			{
				for (int col = 0; col < width; ++col)
				{
					grid[row, col] = lines[row][col];
				}
			}

			Console.WriteLine("Initial state:");
			PrintGrid(ref grid, width, height);

			int stepCount = 0;
			while(true)
			{
				int totalMoved = PerformStep(ref grid, width, height);
				stepCount++;

				Console.WriteLine("After " + stepCount + " step[s]:");
				PrintGrid(ref grid, width, height);

				if (totalMoved == 0)
				{
					Console.WriteLine("Cucumbers have stopped moving");
					break;
				}	
			}

			// Part One
			{
				Console.WriteLine("\nPart One:");
				Console.WriteLine("Number of steps before cucumbers stopped moving is: " + stepCount);
			}

		}
	}
}
