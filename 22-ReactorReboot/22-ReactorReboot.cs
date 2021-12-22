using System;
using System.Collections.Generic;
using System.IO;

namespace _22_ReactorReboot
{
	class Instruction
	{
		public int xMin, xMax;
		public int yMin, yMax;
		public int zMin, zMax;
		public bool turnOn;
	}

	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");
			List<Instruction> instructions = new List<Instruction>();

			foreach(var line in lines)
			{
				Instruction instruction = new Instruction();

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

				instructions.Add(instruction);
			}

			// Part One - Only in the -50 to 50 range on all 2 axes
			{
				// Initialise 101x101x101 3D cube array
				// Emulates -50 to 50 range in all axes (extra 1 for cube 0)
				bool[,,] cubes = new bool[101, 101, 101];

				foreach (var instruction in instructions)
				{
					//Console.WriteLine("\n===Parsing next instruction===");

					for (int x = instruction.xMin; x <= instruction.xMax; ++x)
					{
						int transposedX = x + 50; // move into 0 to 101 range
						if (transposedX < 0 || transposedX > 100)
							continue;

						for (int y = instruction.yMin; y <= instruction.yMax; ++y)
						{
							int transposedY = y + 50; // move into 0 to 101 range
							if (transposedY < 0 || transposedY > 100)
								continue;

							for (int z = instruction.zMin; z <= instruction.zMax; ++z)
							{
								int transposedZ = z + 50; // move into 0 to 101 range
								if (transposedZ < 0 || transposedZ > 100)
									continue;

								//Console.WriteLine("Turning " + (instruction.turnOn ? "on" : "off") + " - (" + transposedX + "," + transposedY + "," + transposedZ + ")");

								cubes[transposedX, transposedY, transposedZ] = instruction.turnOn;
							}
						}
					}
				}

				// Count how many cubes are now lit
				UInt64 litCubeCount = 0;
				for (int x = 0; x < 101; ++x)
				{
					for (int y = 0; y < 101; ++y)
					{
						for (int z = 0; z < 101; ++z)
						{
							if (cubes[x, y, z])
								litCubeCount++;
						}
					}
				}

				Console.WriteLine("Part One");
				Console.WriteLine("Number of lit cubes after instructions is: " + litCubeCount);
			}

			// Part Two
			{


			}
		}
	}
}
