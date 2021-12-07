using System;
using System.IO;

namespace _07_TheTreacheryOfWhales
{
	class Program
	{
		static void Main(string[] args)
		{
			var crabsInput = File.ReadAllLines("input.txt")[0].Split(',');

			// Parse string array into int array of horizontal positions
			int furthestCrab = int.MinValue; // Get the upper bounds of the horizontal distance while we're iterating
			int[] crabs = new int[crabsInput.Length];
			for (int i = 0; i < crabsInput.Length; ++i)
			{
				crabs[i] = int.Parse(crabsInput[i]);
				
				furthestCrab = Math.Max(furthestCrab, crabs[i]);
			}

			// Part One
			{

				int lowestTotalMovement = int.MaxValue;
				int cheapestAlignment = 0;
				
				// Iterate through each available horizontal position - checking if it is the cheapest alignment option
				for (int aligningPos = 0; aligningPos < furthestCrab; ++aligningPos)
				{
					int totalMovement = 0;
					foreach (var crab in crabs)
					{
						int distance = Math.Abs(crab - aligningPos);

						// For part 1, Cost is 1 fuel per unit
						int fuel = distance;
						totalMovement += fuel; 
					}

					if (totalMovement < lowestTotalMovement)
					{
						lowestTotalMovement = totalMovement;
						cheapestAlignment = aligningPos;
					}
				}

				Console.WriteLine("Part One:");
				Console.WriteLine("Cheapest alignment is: " + cheapestAlignment);
				Console.WriteLine("Cost is: " + lowestTotalMovement + "\n");
			}

			// Part Two
			{
				int lowestTotalMovement = int.MaxValue;
				int cheapestAlignment = 0;

				// Iterate through each available horizontal position - checking if it is the cheapest alignment option
				for (int aligningPos = 0; aligningPos < furthestCrab; ++aligningPos)
				{
					int totalMovement = 0;
					foreach (var crab in crabs)
					{
						int distance = Math.Abs(crab - aligningPos);

						// For part 2, Cost is an increasing amount of fuel per unit
						// 1=1, 2=2+1, 3=3+2+1,...
						// n=(n(n+1))/2
						int fuel = (int)((distance * (distance + 1)) * 0.5);
						totalMovement += fuel;
					}

					if (totalMovement < lowestTotalMovement)
					{
						lowestTotalMovement = totalMovement;
						cheapestAlignment = aligningPos;
					}
				}

				Console.WriteLine("Part Two:");
				Console.WriteLine("Cheapest alignment is: " + cheapestAlignment);
				Console.WriteLine("Cost is: " + lowestTotalMovement + "\n");
			}
		}
	}
}
