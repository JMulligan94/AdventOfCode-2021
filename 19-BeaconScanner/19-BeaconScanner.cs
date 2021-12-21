using System;
using System.Collections.Generic;
using System.IO;

namespace _19_BeaconScanner
{
	using IntVec3 = Tuple<int,int,int>;

	class Scanner
	{
		public int id;
		public List<IntVec3> relativeBeaconLocations = new List<IntVec3>();
	}

	class Program
	{
		static void GenerateAllOrientations(IntVec3 vectorIn, ref List<IntVec3> vectorOut)
		{
			vectorOut.Add(vectorIn);															// x,y,z
			vectorOut.Add(new IntVec3(vectorIn.Item1, vectorIn.Item3, -vectorIn.Item2));		// x,z,-y
			vectorOut.Add(new IntVec3(vectorIn.Item1, -vectorIn.Item2, -vectorIn.Item3));		// x,-y,-z
			vectorOut.Add(new IntVec3(vectorIn.Item1, -vectorIn.Item3, vectorIn.Item2));		// x,-z,y

			vectorOut.Add(new IntVec3(-vectorIn.Item1, -vectorIn.Item2, -vectorIn.Item3));		// -x,-y,-z
			vectorOut.Add(new IntVec3(-vectorIn.Item1, vectorIn.Item3, -vectorIn.Item2));		// -x,z,-y
			vectorOut.Add(new IntVec3(-vectorIn.Item1, -vectorIn.Item2, -vectorIn.Item3));		// -x,-y,-z
			vectorOut.Add(new IntVec3(-vectorIn.Item1, -vectorIn.Item3, vectorIn.Item2));		// -x,-z,y

		}

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("testA.txt");

			List<Scanner> scanners = new List<Scanner>();
			bool newScannerInfo = true;
			Scanner currentScanner = null;
			foreach (var line in lines)
			{
				if (newScannerInfo)
				{
					currentScanner = new Scanner();
					var idLineTokens = line.Split(' ');
					currentScanner.id = int.Parse(idLineTokens[2]);
					newScannerInfo = false;

					scanners.Add(currentScanner);
					continue;
				}
				if (line == "")
				{
					newScannerInfo = true;
					continue;
				}
				else
				{
					var tokens = line.Split(',');
					currentScanner.relativeBeaconLocations.Add(new IntVec3(int.Parse(tokens[0]), int.Parse(tokens[1]), 0));
				}
			}
			int i = 0;

			IntVec3 vectorIn = new IntVec3(2, 3, 1);

			List<IntVec3> orientations = new List<IntVec3>();
			GenerateAllOrientations(vectorIn, ref orientations);
			foreach(var orientation in orientations)
			{
				Console.WriteLine("Orientation: (" + orientation.Item1 + "," + orientation.Item2 + "," + orientation.Item3 + ")");
			}

		}
	}
}
