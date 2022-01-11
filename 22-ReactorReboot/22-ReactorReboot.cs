using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _22_ReactorReboot
{
	class Instruction
	{
		public string originalLine;
		public int xMin, xMax;
		public int yMin, yMax;
		public int zMin, zMax;

		public Cuboid cuboid;

		public bool turnOn;

		public override string ToString()
		{
			return originalLine;
		}
	}

	class Cuboid
	{
		public Coordinate min;
		public Coordinate max;
		public Cuboid(Coordinate _min, Coordinate _max)
		{
			min = _min;
			max = _max;
		}
		public Cuboid(Cuboid other)
		{
			min = new Coordinate(other.min);
			max = new Coordinate(other.max);
		}

		public override string ToString()
		{
			return $"Min:{min}, Max:{max}, Vol:{GetVolume()}";
		}

		internal ulong GetVolume()
		{
			ulong xRange = (ulong) Math.Abs(max.x - min.x) + 1;
			ulong yRange = (ulong) Math.Abs(max.y - min.y) + 1;
			ulong zRange = (ulong) Math.Abs(max.z - min.z) + 1;
			return xRange * yRange * zRange;
		}
	}

	class Coordinate
	{
		public int x;
		public int y;
		public int z;

		public Coordinate(int _x, int _y, int _z)
		{
			x = _x;
			y = _y;
			z = _z;
		}

		public Coordinate(Coordinate other)
		{
			x = other.x;
			y = other.y;
			z = other.z;
		}

		public override string ToString()
		{
			return $"({x},{y},{z})";
		}
	}

	class Program
	{
		// Brute force implementation of turning cubes on/off in a given range.
		// Useful for debugging the faster method
		static void PartOne_BruteForce(List<Instruction> instructions, int axisLimit)
		{
			// Part One - Only in the -50 to 50 range on all 2 axes
			{
				// Initialise 101x101x101 3D cube array
				// Emulates -50 to 50 range in all axes (extra 1 for cube 0)
				int arrayDimension = (axisLimit * 2) + 1;
				bool[,,] cubes = new bool[arrayDimension, arrayDimension, arrayDimension];

				foreach (var instruction in instructions)
				{
					if (instruction.xMin < -axisLimit || instruction.xMax > axisLimit)
						continue;
					if (instruction.yMin < -axisLimit || instruction.yMax > axisLimit)
						continue;
					if (instruction.zMin < -axisLimit || instruction.zMax > axisLimit)
						continue;


					for (int x = instruction.xMin; x <= instruction.xMax; ++x)
					{
						int transposedX = x + axisLimit; // move into 0 to 101 range
						if (transposedX < 0 || transposedX > axisLimit*2)
							continue;

						for (int y = instruction.yMin; y <= instruction.yMax; ++y)
						{
							int transposedY = y + axisLimit; // move into 0 to 101 range
							if (transposedY < 0 || transposedY > axisLimit*2)
								continue;

							for (int z = instruction.zMin; z <= instruction.zMax; ++z)
							{
								int transposedZ = z + axisLimit; // move into 0 to 101 range
								if (transposedZ < 0 || transposedZ > axisLimit*2)
									continue;

								//Console.WriteLine("Turning " + (instruction.turnOn ? "on" : "off") + " - (" + transposedX + "," + transposedY + "," + transposedZ + ")");

								cubes[transposedX, transposedY, transposedZ] = instruction.turnOn;
							}
						}
					}
					
					UInt64 intervalLitCubeCount = 0;
					for (int x = 0; x < arrayDimension; ++x)
					{
						for (int y = 0; y < arrayDimension; ++y)
						{
							for (int z = 0; z < arrayDimension; ++z)
							{
								if (cubes[x, y, z])
									intervalLitCubeCount++;
							}
						}
					}
					Console.WriteLine($"After instruction: {instruction}, number of lit cubes = {intervalLitCubeCount}");
				}

				// Count how many cubes are now lit
				UInt64 litCubeCount = 0;
				for (int x = 0; x < arrayDimension; ++x)
				{
					for (int y = 0; y < arrayDimension; ++y)
					{
						for (int z = 0; z < arrayDimension; ++z)
						{
							if (cubes[x, y, z])
								litCubeCount++;
						}
					}
				}
				Console.WriteLine($"Brute force finished! Number of lit cubes = {litCubeCount}\n");
			}
		}

		static void CutHoleInCuboids(Cuboid cutCuboid, ref List<Cuboid> cuboidList)
		{
			var newCuboidList = new List<Cuboid>();

			// Which cuboids overlap?
			for (int cuboidIndex = 0; cuboidIndex < cuboidList.Count; ++cuboidIndex)
			{
				Cuboid existingCuboid = cuboidList[cuboidIndex];

				bool overlapsX = false;
				bool overlapsY = false;
				bool overlapsZ = false;

				// Check overlap
				if (existingCuboid.max.x >= cutCuboid.min.x
					&& existingCuboid.min.x <= cutCuboid.max.x)
				{
					// Overlaps x
					overlapsX = true;
				}
				if (existingCuboid.max.y >= cutCuboid.min.y
					&& existingCuboid.min.y <= cutCuboid.max.y)
				{
					// Overlaps y
					overlapsY = true;
				}
				if (existingCuboid.max.z >= cutCuboid.min.z
					&& existingCuboid.min.z <= cutCuboid.max.z)
				{
					// Overlaps z
					overlapsZ = true;
				}

				if (overlapsX && overlapsY && overlapsZ)
				{
					// Overlaps with existing cuboid
					// Break existing cuboid down into potentially 6 cuboids

					// Resolve x axis overlap
					if (existingCuboid.min.x < cutCuboid.min.x
						&& cutCuboid.min.x <= existingCuboid.max.x) // Slicing left side
					{
						Cuboid slicedCuboid = new Cuboid(existingCuboid.min, new Coordinate(cutCuboid.min.x - 1, existingCuboid.max.y, existingCuboid.max.z));
						if (slicedCuboid.GetVolume() > 0)
							newCuboidList.Add(slicedCuboid);
						existingCuboid.min = new Coordinate(cutCuboid.min.x, existingCuboid.min.y, existingCuboid.min.z);
					}

					if (existingCuboid.min.x <= cutCuboid.max.x
						&& cutCuboid.max.x < existingCuboid.max.x) // Slicing right side
					{
						Cuboid slicedCuboid = new Cuboid(new Coordinate(cutCuboid.max.x + 1, existingCuboid.min.y, existingCuboid.min.z), existingCuboid.max);
						if (slicedCuboid.GetVolume() > 0)
							newCuboidList.Add(slicedCuboid);
						existingCuboid.max = new Coordinate(cutCuboid.max.x, existingCuboid.max.y, existingCuboid.max.z);
					}

					// Resolve y axis overlap
					if (existingCuboid.min.y < cutCuboid.min.y
						&& cutCuboid.min.y <= existingCuboid.max.y) // Slicing top side
					{
						Cuboid slicedCuboid = new Cuboid(existingCuboid.min, new Coordinate(existingCuboid.max.x, cutCuboid.min.y - 1, existingCuboid.max.z));
						if (slicedCuboid.GetVolume() > 0)
							newCuboidList.Add(slicedCuboid);
						existingCuboid.min = new Coordinate(existingCuboid.min.x, cutCuboid.min.y, existingCuboid.min.z);
					}
					if (existingCuboid.min.y <= cutCuboid.max.y
						&& cutCuboid.max.y < existingCuboid.max.y) // Slicing bottom side
					{
						Cuboid slicedCuboid = new Cuboid(new Coordinate(existingCuboid.min.x, cutCuboid.max.y + 1, existingCuboid.min.z), existingCuboid.max);
						if (slicedCuboid.GetVolume() > 0)
							newCuboidList.Add(slicedCuboid);
						existingCuboid.max = new Coordinate(existingCuboid.max.x, cutCuboid.max.y, existingCuboid.max.z);
					}

					// Resolve z axis overlap
					if (existingCuboid.min.z < cutCuboid.min.z
						&& cutCuboid.min.z <= existingCuboid.max.z) // Slicing behind
					{
						Cuboid slicedCuboid = new Cuboid(existingCuboid.min, new Coordinate(existingCuboid.max.x, existingCuboid.max.y, cutCuboid.min.z - 1));
						if (slicedCuboid.GetVolume() > 0)
							newCuboidList.Add(slicedCuboid);
						existingCuboid.min = new Coordinate(existingCuboid.min.x, existingCuboid.min.y, cutCuboid.min.z);
					}
					if (existingCuboid.min.z <= cutCuboid.max.z
						&& cutCuboid.max.z < existingCuboid.max.z) // Slicing in front
					{
						Cuboid slicedCuboid = new Cuboid(new Coordinate(existingCuboid.min.x, existingCuboid.min.y, cutCuboid.max.z + 1), existingCuboid.max);
						if (slicedCuboid.GetVolume() > 0)
							newCuboidList.Add(slicedCuboid);
						existingCuboid.max = new Coordinate(existingCuboid.max.x, existingCuboid.max.y, cutCuboid.max.z);
					}
				}
				else
				{
					newCuboidList.Add(existingCuboid);
				}
			}

			cuboidList = newCuboidList;
		}

		static ulong GetTotalVolume(List<Cuboid> cuboidList)
		{
			ulong area = 0;
			foreach(Cuboid cuboid in cuboidList)
			{
				area += cuboid.GetVolume();
			}
			return area;
		}

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			// Parse lines into instructions
			List<Instruction> instructions = new List<Instruction>();

			int xLower = int.MaxValue;
			int yLower = int.MaxValue;
			int zLower = int.MaxValue;
			int xUpper = int.MinValue;
			int yUpper = int.MinValue;
			int zUpper = int.MinValue;

			foreach (var line in lines)
			{
				Instruction instruction = new Instruction();
				
				instruction.originalLine = line;

				instruction.turnOn = line.Split(' ')[0] == "on";

				var tokens = line.Split(' ')[1].Split(',');

				var xRange = tokens[0].Split('=')[1];
				var yRange = tokens[1].Split('=')[1];
				var zRange = tokens[2].Split('=')[1];

				instruction.xMin = int.Parse(xRange.Split('.')[0]);
				instruction.xMax = int.Parse(xRange.Split('.')[2]);

				instruction.yMin = int.Parse(yRange.Split('.')[0]);
				instruction.yMax = int.Parse(yRange.Split('.')[2]);

				instruction.zMin = int.Parse(zRange.Split('.')[0]);
				instruction.zMax = int.Parse(zRange.Split('.')[2]);

				instruction.cuboid = new Cuboid(new Coordinate(instruction.xMin, instruction.yMin, instruction.zMin), new Coordinate(instruction.xMax, instruction.yMax, instruction.zMax));

				instructions.Add(instruction);

				xLower = Math.Min(xLower, instruction.xMin);
				yLower = Math.Min(yLower, instruction.yMin);
				zLower = Math.Min(zLower, instruction.zMin);
				
				xUpper = Math.Max(xUpper, instruction.xMax);
				yUpper = Math.Max(yUpper, instruction.yMax);
				zUpper = Math.Max(zUpper, instruction.zMax);
			}

			const int c_axisLimit = 50;

			// BRUTE FORCE FOR COMPARISON
			//PartOne_BruteForce(instructions, c_axisLimit);

			var cuboidList = new List<Cuboid>();
			foreach (Instruction instruction in instructions)
			{
				Cuboid newCuboid = new Cuboid(instruction.cuboid);

				CutHoleInCuboids(newCuboid, ref cuboidList);

				// If we're turning this section on, fill in the hole we just created now with the new cuboid
				if (instruction.turnOn)
					cuboidList.Add(newCuboid);

				ulong intervalLitCubes = GetTotalVolume(cuboidList);
				Console.WriteLine($"Number of lit cubes after instruction \"{instruction}\" is: {intervalLitCubes}");
			}

			// Cache number of lit cubes before we restrict it to the initialisation region
			ulong partTwoVolume = GetTotalVolume(cuboidList);

			// Part One - Only consider ones in -50 -> 50 range on all axes
			{
				// Cut down to the range -50...50 in all axes
				if (xLower < -c_axisLimit)
				{
					CutHoleInCuboids(new Cuboid(new Coordinate(xLower-1, yLower-1, zLower-1), new Coordinate(-(c_axisLimit+1), yUpper+1, zUpper+1)), ref cuboidList);
				}
				if (xUpper > c_axisLimit)
				{
					CutHoleInCuboids(new Cuboid(new Coordinate(c_axisLimit+1, yLower-1, zLower-1), new Coordinate(xUpper+1, yUpper+1, zUpper+1)), ref cuboidList);
				}
				if (yLower < -c_axisLimit)
				{
					CutHoleInCuboids(new Cuboid(new Coordinate(xLower-1, yLower-1, zLower-1), new Coordinate(xUpper+1, -(c_axisLimit+1), zUpper+1)), ref cuboidList);
				}
				if (yUpper > c_axisLimit)
				{
					CutHoleInCuboids(new Cuboid(new Coordinate(xLower-1, c_axisLimit+1, zLower-1), new Coordinate(xUpper+1, yUpper+1, zUpper+1)), ref cuboidList);
				}
				if (zLower < -c_axisLimit)
				{
					CutHoleInCuboids(new Cuboid(new Coordinate(xLower-1, yLower-1, zLower-1), new Coordinate(xUpper+1, yUpper+1, -(c_axisLimit+1))), ref cuboidList);
				}
				if (zUpper > c_axisLimit)
				{
					CutHoleInCuboids(new Cuboid(new Coordinate(xLower-1, yLower-1, c_axisLimit+1), new Coordinate(xUpper+1, yUpper+1, zUpper+1)), ref cuboidList);
				}

				ulong partOneVolume = GetTotalVolume(cuboidList);

				Console.WriteLine("\nPart One");
				Console.WriteLine("Number of lit cubes after instructions is: " + partOneVolume + "\n");
			}

			// Part Two
			{
				Console.WriteLine("Part Two");
				Console.WriteLine("Number of lit cubes after instructions is: " + partTwoVolume);
			}
		}
	}
}
