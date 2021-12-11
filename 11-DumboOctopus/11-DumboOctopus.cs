using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _11_DumboOctopus
{
	class Program
	{
		// Print out current state of grid
		// "[X]" means that it flashed
		static void PrintGrid(string prefix, ref List<List<int>> octopuses)
		{
			Console.WriteLine(prefix);

			string outputStr = "";
			foreach (var row in octopuses)
			{
				foreach (var col in row)
				{
					outputStr += col == 0 ? "[" + col + "]" : col;
					outputStr += "\t";
				}
				outputStr += "\n";
			}
			outputStr += "\n";

			Console.WriteLine(outputStr);
		}

		// Perform flash for specific octopus (increment all adjacent octopuses by 1)
		static void FlashOctopus(int row, int col, ref List<List<int>> octopuses)
		{
			//PrintGrid("Flash! " + row + "," + col, ref octopuses);

			bool isTopmost = row == 0;
			bool isBottommost = row == octopuses.Count - 1;
			bool isLeftmost = col == 0;
			bool isRightmost = col == octopuses[row].Count - 1;

			// North
			if (!isTopmost)
			{
				octopuses[row-1][col]++;

				// North-West
				if (!isLeftmost)
					octopuses[row-1][col-1]++;

				// North-East
				if (!isRightmost)
					octopuses[row-1][col+1]++;
			}

			// East
			if (!isRightmost)
				octopuses[row][col+1]++;

			// West
			if (!isLeftmost)
				octopuses[row][col-1]++;

			// South
			if (!isBottommost)
			{
				octopuses[row+1][col]++;

				// South-West
				if (!isLeftmost)
					octopuses[row+1][col-1]++;

				// South-East
				if (!isRightmost)
					octopuses[row+1][col+1]++;
			}
		}

		// returns number of octopuses that flashed during the step
		static int PerformStep(ref List<List<int>> octopuses)
		{
			// Increase all energy levels by 1
			for (int row = 0; row < octopuses.Count; ++row)
			{
				for (int col = 0; col < octopuses[row].Count; ++col)
				{
					octopuses[row][col]++;
				}
			}

			// All octopuses that have flashed in this step
			List<Tuple<int, int>> flashedOctopuses = new List<Tuple<int, int>>();
			while (true)
			{
				List<Tuple<int, int>> newFlashes = new List<Tuple<int, int>>();
			
				for (int row = 0; row < octopuses.Count; ++row)
				{
					for (int col = 0; col < octopuses[row].Count; ++col)
					{
						// If any are 10 or greater, they FLASH and increment all adjacent tiles, including diagonally (only once per step)
						if (octopuses[row][col] >= 10)
						{
							// Have we already accounted for this octopus flashing in this step?
							bool alreadyFlashedThisStep = false;
							foreach (var flashOctopus in flashedOctopuses)
							{
								if (row == flashOctopus.Item1 && col == flashOctopus.Item2)
								{
									// Already flashed
									alreadyFlashedThisStep = true;
									break;
								}
							}
							
							if (!alreadyFlashedThisStep)
								newFlashes.Add(new Tuple<int, int>(row, col));
						}
					}
				}

				// If there were no more flashes, we can break out of the loop and move onto the next part of the step
				if (newFlashes.Count == 0)
					break;

				// Octopus has flashed, increment all neighbours
				foreach (var flashOctopus in newFlashes)
				{
					FlashOctopus(flashOctopus.Item1, flashOctopus.Item2, ref octopuses);
					flashedOctopuses.Add(flashOctopus);
				}
			}

			// Set all flashed octopuses to 0 energy level
			foreach (var flashOctopus in flashedOctopuses)
			{
				octopuses[flashOctopus.Item1][flashOctopus.Item2] = 0;
			}

			return flashedOctopuses.Count;
		}

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			// Part One
			{
				List<List<int>> octopusLevels = new List<List<int>>();
				foreach (var row in lines)
				{
					octopusLevels.Add(new List<int>());
					foreach (var col in row)
					{
						octopusLevels.Last().Add(int.Parse(col + ""));
					}
				}

				// Perform 100 steps and keep track of amount of flashes from each step
				PrintGrid("Before any steps: ", ref octopusLevels);
				int totalFlashes = 0;
				for (int i = 1; i <= 100; ++i)
				{
					totalFlashes += PerformStep(ref octopusLevels);
					PrintGrid("After step " + i + ": ", ref octopusLevels);
				}

				Console.WriteLine("Part One:");
				Console.WriteLine("Total number of flashes after 100 steps is: " + totalFlashes + "\n");
			}

			// Part Two
			{
				List<List<int>> octopusLevels = new List<List<int>>();
				foreach (var row in lines)
				{
					octopusLevels.Add(new List<int>());
					foreach (var col in row)
					{
						octopusLevels.Last().Add(int.Parse(col + ""));
					}
				}

				// Find first step where all octopuses flash simultaneously
				// i.e. when the number of flashes for that step = total number of octopuses (100)
				PrintGrid("Before any steps: ", ref octopusLevels);
				int firstSimultaneousFlash = -1;
				int step = 1;
				while (firstSimultaneousFlash == -1)
				{
					if (PerformStep(ref octopusLevels) == 100)
					{
						firstSimultaneousFlash = step;
					}
					PrintGrid("After step " + step + ": ", ref octopusLevels);
					step++;
				}


				Console.WriteLine("Part Two:");
				Console.WriteLine("First step during which all octopuses flash is: " + firstSimultaneousFlash);
			}
		}
	}
}
