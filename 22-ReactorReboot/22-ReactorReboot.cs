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
		public bool turnOn;

		public override string ToString()
		{
			return originalLine;
		}
	}

	class Program
	{

		static void CheckInRange_Partitions(Tuple<int, int> xRange, Tuple<int, int> yRange, Tuple<int,int> zRange, 
			int instructionStep, ref List<Instruction> instructions, out ulong turnOnCount, out ulong turnOffCount)
		{
			Console.WriteLine("\nParsing instruction: \"" + instructions[instructionStep] + "\"...");

			ulong xSize = (ulong)Math.Abs(xRange.Item1 - xRange.Item2) + 1; // +1 since upper boundary is also considered part of the range
			ulong ySize = (ulong)Math.Abs(yRange.Item1 - yRange.Item2) + 1;
			ulong zSize = (ulong)Math.Abs(zRange.Item1 - zRange.Item2) + 1;

			turnOnCount = 0;
			turnOffCount = 0;

			// Partition into cubes of set amount
			const int c_cubeSplitSize = 1000;
			int numXPartitions = (int)(xSize / c_cubeSplitSize) + 1;
			int numYPartitions = (int)(ySize / c_cubeSplitSize) + 1;
			int numZPartitions = (int)(zSize / c_cubeSplitSize) + 1;

			for (int xPartition = 0; xPartition < numXPartitions; ++xPartition)
			{
				int partitionLowerX = xRange.Item1 + (xPartition * c_cubeSplitSize);
				int partitionHigherX = Math.Min(xRange.Item2, xRange.Item1 + c_cubeSplitSize);
				for (int yPartition = 0; yPartition < numYPartitions; ++yPartition)
				{
					int partitionLowerY = yRange.Item1 + (yPartition * c_cubeSplitSize);
					int partitionHigherY = Math.Min(yRange.Item2, yRange.Item1 + c_cubeSplitSize);
					for (int zPartition = 0; zPartition < numZPartitions; ++zPartition)
					{
						int partitionLowerZ = zRange.Item1 + (zPartition * c_cubeSplitSize);
						int partitionHigherZ = Math.Min(zRange.Item2, zRange.Item1 + c_cubeSplitSize);

						bool[,,] lights = new bool[c_cubeSplitSize+1, c_cubeSplitSize+1, c_cubeSplitSize+1];

						// Recreate state for all steps beforehand
						for (int i = 0; i < instructionStep; ++i)
						{
							Instruction previousInstruction = instructions[i];

							if (previousInstruction.xMin > partitionHigherX || previousInstruction.xMax < partitionLowerX)
								continue;

							if (previousInstruction.yMin > partitionHigherY || previousInstruction.yMax < partitionLowerY)
								continue;

							if (previousInstruction.zMin > partitionHigherZ || previousInstruction.zMax < partitionLowerZ)
								continue;

							int lowerX = Math.Max(partitionLowerX, previousInstruction.xMin);
							int upperX = Math.Min(partitionHigherX, previousInstruction.xMax);
							int lowerY = Math.Max(partitionLowerY, previousInstruction.yMin);
							int upperY = Math.Min(partitionHigherY, previousInstruction.yMax);
							int lowerZ = Math.Max(partitionLowerZ, previousInstruction.zMin);
							int upperZ = Math.Min(partitionHigherZ, previousInstruction.zMax);

							// Turn on/off lights for this step in the given section
							for (int x = lowerX; x <= upperX; ++x)
							{
								int transposedX = x - xRange.Item1 - (xPartition * c_cubeSplitSize);
								for (int y = lowerY; y <= upperY; ++y)
								{
									int transposedY = y - yRange.Item1 - (yPartition * c_cubeSplitSize);
									for (int z = lowerZ; z <= upperZ; ++z)
									{
										int transposedZ = z - zRange.Item1 - (zPartition * c_cubeSplitSize);
										lights[transposedX, transposedY, transposedZ] = previousInstruction.turnOn;
									}
								}
							}
						}

						Instruction currentInstruction = instructions[instructionStep];
						int xStart = Math.Max(xRange.Item1, xRange.Item1 + (xPartition * c_cubeSplitSize));
						int xEnd = Math.Min(xRange.Item2, xStart + c_cubeSplitSize);
						int yStart = Math.Max(yRange.Item1, yRange.Item1 + (yPartition * c_cubeSplitSize));
						int yEnd = Math.Min(yRange.Item2, yStart + c_cubeSplitSize);
						int zStart = Math.Max(zRange.Item1, zRange.Item1 + (zPartition * c_cubeSplitSize));
						int zEnd = Math.Min(zRange.Item2, zStart + c_cubeSplitSize);

						// Turn on/off lights for this step in the given section
						for (int x = xStart; x <= xEnd; ++x)
						{
							int transposedX = x - xStart;
							for (int y = yStart; y <= yEnd; ++y)
							{
								int transposedY = y - yStart;
								for (int z = zStart; z <= zEnd; ++z)
								{
									int transposedZ = z - zStart;
									bool lightOn = lights[transposedX, transposedY, transposedZ];
									if (currentInstruction.turnOn)
									{
										if (!lightOn)
											turnOnCount++;
									}
									else
									{
										if (lightOn)
											turnOffCount++;
									}
								}
							}
						}
						//Console.WriteLine("Partition (" + xPartition + "," + yPartition + "," + zPartition + ") - on: " + turnOnCount + ", off: " + turnOffCount);
					}
				}
			}
			Console.WriteLine("New on: " + turnOnCount + ", New off: " + turnOffCount);
		}

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("test.txt");

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

				instructions.Add(instruction);

				xLower = Math.Min(xLower, instruction.xMin);
				yLower = Math.Min(yLower, instruction.yMin);
				zLower = Math.Min(zLower, instruction.zMin);
				
				xUpper = Math.Max(xUpper, instruction.xMax);
				yUpper = Math.Max(yUpper, instruction.yMax);
				zUpper = Math.Max(zUpper, instruction.zMax);
			}

			// Part One - Only consider ones in -50 -> 50 range on all axes
			{
				ulong litCubes = 0;
				//for (int i = 0; i < instructions.Count; ++i)
				//{
				//	Instruction instruction = instructions[i];

				//	// Limit ranges to -50 -> 50
				//	if (instruction.xMax < -50 || instruction.xMin > 50)
				//		continue;

				//	if (instruction.yMax < -50 || instruction.yMin > 50)
				//		continue;

				//	if (instruction.zMax < -50 || instruction.zMin > 50)
				//		continue;

				//	Tuple<int, int> xRange = new Tuple<int, int>(Math.Max(-50, instruction.xMin), Math.Min(50, instruction.xMax));
				//	Tuple<int, int> yRange = new Tuple<int, int>(Math.Max(-50, instruction.yMin), Math.Min(50, instruction.yMax));
				//	Tuple<int, int> zRange = new Tuple<int, int>(Math.Max(-50, instruction.zMin), Math.Min(50, instruction.zMax));

				//	ulong turnOnChange = 0;
				//	ulong turnOffChange = 0;
				//	CheckInRange_Partitions(xRange, yRange, zRange, i, ref instructions, out turnOnChange, out turnOffChange);
					
				//	litCubes += turnOnChange;
				//	litCubes -= turnOffChange;
				//}

				Console.WriteLine("Part One");
				Console.WriteLine("Number of lit cubes after instructions is: " + litCubes);
			}

			// Part Two
			{
				ulong litCubes = 0;

				// Recreate state for all steps beforehand
				//for (int i = 0; i < instructions.Count; ++i)
				//{
				//	Instruction instruction = instructions[i];
				//	Tuple<int, int> xRange = new Tuple<int, int>(instruction.xMin, instruction.xMax);
				//	Tuple<int, int> yRange = new Tuple<int, int>(instruction.yMin, instruction.yMax);
				//	Tuple<int, int> zRange = new Tuple<int, int>(instruction.zMin, instruction.zMax);

				//	ulong turnOnChange = 0;
				//	ulong turnOffChange = 0;
				//	CheckInRange_Partitions(xRange, yRange, zRange, i, ref instructions, out turnOnChange, out turnOffChange);

				//	litCubes += turnOnChange;
				//	litCubes -= turnOffChange;
				//}


				Console.WriteLine("Part Two");
				Console.WriteLine("Number of lit cubes after instructions is: " + litCubes);
			}
		}
	}
}
