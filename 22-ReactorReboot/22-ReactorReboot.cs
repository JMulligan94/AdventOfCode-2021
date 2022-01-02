using System;
using System.Collections.Generic;
using System.IO;

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

		public int CompareXMinTo(Instruction other)
		{
			return xMin.CompareTo(other.xMin);
		}

		public int CompareYMinTo(Instruction other)
		{
			return yMin.CompareTo(other.yMin);
		}

		public int CompareZMinTo(Instruction other)
		{
			return zMin.CompareTo(other.zMin);
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("testA.txt");

			// Parse lines into instructions
			List<Instruction> instructionsOn = new List<Instruction>();
			List<Instruction> instructionsOff = new List<Instruction>();
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

				if (instruction.turnOn)
					instructionsOn.Add(instruction);
				else
					instructionsOff.Add(instruction);
			}

			// Part One - Only consider ones in -50 -> 50 range on all axes
			{
				List<Instruction> xRangeAsc = new List<Instruction>(instructionsOn);
				xRangeAsc.Sort((x, y) => x.CompareXMinTo(y));

				List<Instruction> yRangeAsc = new List <Instruction>(instructionsOn);
				yRangeAsc.Sort((x, y) => x.CompareYMinTo(y));

				List<Instruction> zRangeAsc = new List<Instruction>(instructionsOn);
				zRangeAsc.Sort((x, y) => x.CompareZMinTo(y));


				//foreach (var instruction in instructions)
				//{
				//	Tuple<int, int> newRange = new Tuple<int, int>(instruction.xMin, instruction.xMax);

				//	if (instruction.turnOn)
				//	{
				//		for (int i = xRangesOn.Count - 1; i >= 0; --i)
				//		{
				//			// Check if this new range overlaps others
				//			int j = 0;
				//		}
				//		xRangesOn.Add(newRange);
				//		xRangesOn.Sort((x,y) => x.Item1.CompareTo(y.Item1));
				//	}
				//	else
				//	{
				//		for (int i = xRangesOff.Count - 1; i >= 0; --i)
				//		{
				//			// Check if this new range overlaps others
				//			int j = 0;
				//		}
				//		xRangesOff.Add(newRange);
				//		xRangesOff.Sort((x, y) => x.Item1.CompareTo(y.Item1));
				//	}
				//}

				Console.WriteLine("Part One");
				Console.WriteLine("Number of lit cubes after instructions is: ");
			}

			// Part Two
			{


			}
		}
	}
}
