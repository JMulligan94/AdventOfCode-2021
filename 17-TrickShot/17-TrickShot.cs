using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _17_TrickShot
{
	using IntVec2 = Tuple<int, int>;

	// Class for holding information about the information for a projectile including its flight path
	class Projectile
	{
		public IntVec2 initialVelocity = new IntVec2(0,0); 
		public List<IntVec2> path = new List<IntVec2>();
		public void AddToPath(IntVec2 coord)
		{
			path.Add(coord);
		}

		// Get upper/lower and left/right bounds of the flight path
		public void GetBounds(ref IntVec2 xBounds, ref IntVec2 yBounds)
		{
			int minHeight = int.MaxValue;
			int maxHeight = int.MinValue;

			int minLength = int.MaxValue;
			int maxLength = int.MinValue;

			foreach (var coord in path)
			{
				if (coord.Item1 > maxLength)
					maxLength = coord.Item1;
				if (coord.Item1 < minLength)
					minLength = coord.Item1;

				if (coord.Item2 > maxHeight)
					maxHeight = coord.Item2;
				if (coord.Item2 < minHeight)
					minHeight = coord.Item2;
			}

			xBounds = new IntVec2(minLength, maxLength);
			yBounds = new IntVec2(minHeight, maxHeight);
		}

		// Is this coord in the flight path
		public bool IsInPath(IntVec2 coord)
		{
			return path.Where(x => x.Item1 == coord.Item1 && x.Item2 == coord.Item2).FirstOrDefault() != null;
		}

		// Are the coords within the target range
		public static bool IsInRange(IntVec2 xRange, IntVec2 yRange, IntVec2 coords)
		{
			if (coords.Item1 < xRange.Item1 || coords.Item1 > xRange.Item2)
				return false;

			if (coords.Item2 < yRange.Item1 || coords.Item2 > yRange.Item2)
				return false;

			return true;
		}

		// Has projectile gone past the range - useful to know when to stop checking a velocity
		public static bool IsPastTargetRange(IntVec2 xRange, IntVec2 yRange, IntVec2 coords)
		{
			if (coords.Item1 > xRange.Item2) // Farther than furthest end of xRange
				return true;

			if (coords.Item2 < yRange.Item1) // Lower than lowest end of yRange
				return true;

			return false;
		}

		public void PrintPath(IntVec2 xRange, IntVec2 yRange)
		{
			Console.WriteLine("\n-=-=- Flight Path (" + initialVelocity.Item1 + "," + initialVelocity.Item2 + ") -=-=-");

			IntVec2 xBounds = new IntVec2(0, 0);
			IntVec2 yBounds = new IntVec2(0, 0);

			GetBounds(ref xBounds, ref yBounds);

			int minX = Math.Min(Math.Min(0, xBounds.Item1), xRange.Item1);
			int maxX = Math.Max(Math.Max(0, xBounds.Item2), xRange.Item2);

			int minY = Math.Min(Math.Min(0, yBounds.Item1), yRange.Item1);
			int maxY = Math.Max(Math.Max(0, yBounds.Item2), yRange.Item2);

			for (int y = maxY; y >= minY; --y)
			{
				string lineStr = "";
				for (int x = minX; x <= maxX; ++x)
				{
					if (y == 0 && x == 0) // Origin point = S
					{
						lineStr += "S";
					}
					else if (IsInPath(new IntVec2(x, y))) // Point in trajectory = #
					{
						lineStr += "#";
					}
					else if (IsInRange(xRange, yRange, new IntVec2(x, y))) // Target Range points = T
					{
						lineStr += "T";
					}
					else // All empty points = .
					{
						lineStr += ".";
					}
				}
				Console.WriteLine(lineStr);
			}
		}

	}

	class Program
	{
		static bool TestVelocity(IntVec2 velocity, IntVec2 xRange, IntVec2 yRange, out int maxHeight)
		{
			// Firing from origin
			IntVec2 currentCoord = new IntVec2(0, 0);
			Projectile projectile = new Projectile();
			projectile.AddToPath(currentCoord);
			projectile.initialVelocity = velocity;

			bool inRange = Projectile.IsInRange(xRange, yRange, currentCoord);
			bool isPastTargetRange = Projectile.IsPastTargetRange(xRange, yRange, currentCoord);

			IntVec2 currentVelocity = velocity;

			// Iterate until we're in range of the target
			// OR we've gone past the target
			while (!inRange && !isPastTargetRange)
			{
				// Move to new co-ord using velocity
				currentCoord = new IntVec2(currentCoord.Item1 + currentVelocity.Item1,
					currentCoord.Item2 + currentVelocity.Item2);

				// Add to projectile's flight path
				projectile.AddToPath(currentCoord);

				// Add drag to velocity in x-axis
				int newX = currentVelocity.Item1;
				if (currentVelocity.Item1 != 0)
					newX += currentVelocity.Item1 > 0 ? -1 : 1;

				// Add gravity to velocity in y-axis
				int newY = currentVelocity.Item2 - 1;

				currentVelocity = new IntVec2(newX, newY);

				// Check if we're within range OR gone past the valid range
				inRange = Projectile.IsInRange(xRange, yRange, currentCoord);
				isPastTargetRange = Projectile.IsPastTargetRange(xRange, yRange, currentCoord);
			}

			//projectile.PrintPath(xRange, yRange);

			if (!inRange)
			{
				maxHeight = 0;
				return false;
			}

			IntVec2 xBounds = new IntVec2(0, 0);
			IntVec2 yBounds = new IntVec2(0, 0);
			projectile.GetBounds(ref xBounds, ref yBounds);
			maxHeight = yBounds.Item2;
			return true;
		}

		static void Main(string[] args)
		{
			var line = File.ReadAllLines("input.txt")[0];

			// Parse input into target range
			var lineTokens = line.Split(' ');

			var xRangeStr = lineTokens[2].TrimEnd(',').Substring(2);
			int xRangeMin = int.Parse(xRangeStr.Split('.')[0]);
			int xRangeMax = int.Parse(xRangeStr.Split('.')[2]);
			IntVec2 xRange = new IntVec2(xRangeMin, xRangeMax);

			var yRangeStr = lineTokens[3].Substring(2);
			int yRangeMin = int.Parse(yRangeStr.Split('.')[0]);
			int yRangeMax = int.Parse(yRangeStr.Split('.')[2]);
			IntVec2 yRange = new IntVec2(yRangeMin, yRangeMax);

			// Try to find all valid velocities that make the projectile fall into range
			List<IntVec2> velocitiesOnTarget = new List<IntVec2>();
			IntVec2 maxHeightVelocity = new IntVec2(0, 0);
			int maxHeight = int.MinValue;
			
			int xLowest = 0; // Must have a positive velocity in the x axis - range is always to the right
			int xHighest = xRangeMax + 1; // Can't have a velocity greater than the right edge of the range

			int yLowest = yRangeMin - 1; // Can't have a velocity lower than the bottom of the range

			// Now we have (mostly) sensible ranges - brute force!
			for (int x = xLowest; x < xHighest; ++x)
			{
				for (int y = yLowest; y < 500; ++y)
				{
					int maxPathHeight = 0;
					bool success = TestVelocity(new IntVec2(x, y), xRange, yRange, out maxPathHeight);

					// If successfully landing in target range, add to list
					if (success)
					{
						velocitiesOnTarget.Add(new IntVec2(x, y));
					}

					// If largest height so far, record info
					if (success && 
						maxPathHeight > maxHeight)
					{
						maxHeightVelocity = new IntVec2(x, y);
						maxHeight = maxPathHeight;
					}
				}
			}

			// Part One
			{
				Console.WriteLine("\nPart One:");
				Console.WriteLine("The highest height is: " + maxHeight);
				Console.WriteLine("From initial velocity: " + maxHeightVelocity);
			}

			// Part Two
			{
				Console.WriteLine("\nPart Two:");
				Console.WriteLine("The total number of valid initial velocities is: " + velocitiesOnTarget.Count);
			}
		}
	}
}
