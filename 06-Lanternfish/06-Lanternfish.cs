using System;
using System.Collections.Generic;
using System.IO;

namespace _06_Lanternfish
{
	class Program
	{
		static void PrintFish(ref UInt64[] fishAtTime, string prefix)
		{
			string fishLine = prefix + " ";
			for(int i = 0; i < fishAtTime.Length; ++i)
			{
				fishLine += i + ":" + fishAtTime[i] + ", ";
			}

			Console.WriteLine(fishLine);
		}

		static void ProgressDay(ref UInt64[] fishAtTime, int day)
		{
			// Each day, move the number of fish at each countdown time down by 1 in the array
			// i.e. number of fish with 5 days left becomes number of fish with 4 days left
			UInt64 fishToAdd = fishAtTime[0];
			for (int fishIndex = 1; fishIndex < fishAtTime.Length; ++fishIndex)
			{
				fishAtTime[fishIndex-1] = fishAtTime[fishIndex];
			}

			// The number of fish that were at [0] moves to [6]
			fishAtTime[6] += fishToAdd;

			// We also duplicate the number for [8] for the fish that have spawned
			fishAtTime[8] = fishToAdd;

			PrintFish(ref fishAtTime, "After " + day + " day[s]:");
		}

		static void Main(string[] args)
		{
			var initialFish = File.ReadAllLines("input.txt")[0].Split(',');

			// Part One
			{ 
				// Number of fish at each time interval left
				// i.e. those at [0] are on their final day
				UInt64[] fishAtTime = new UInt64[9];

				for(int i = 0; i < initialFish.Length; ++i)
				{
					int index = int.Parse(initialFish[i]);
					fishAtTime[index]++;
				}

				PrintFish(ref fishAtTime, "Initial state:");

				// Progress days - 80 days
				for (int day = 1; day <= 80; ++day)
				{
					ProgressDay(ref fishAtTime, day);
				}

				// Calculate total number of fish
				UInt64 fishTotal = 0;
				for (int i = 0; i < fishAtTime.Length; ++i)
					fishTotal += fishAtTime[i];

				Console.WriteLine("Part One");
				Console.WriteLine("Total number of fish after 80 days: " + fishTotal + "\n");
			}

			// Part Two
			{
				// 256 days!
				UInt64[] fishAtTime = new UInt64[9];

				for (int i = 0; i < initialFish.Length; ++i)
				{
					int index = int.Parse(initialFish[i]);
					fishAtTime[index]++;
				}

				PrintFish(ref fishAtTime, "Initial state:");

				// Progress days - 256 days
				for (int day = 1; day <= 256; ++day)
				{
					ProgressDay(ref fishAtTime, day);
				}

				// Calculate total number of fish
				UInt64 fishTotal = 0;
				for (int i = 0; i < fishAtTime.Length; ++i)
					fishTotal += fishAtTime[i];

				Console.WriteLine("Part Two");
				Console.WriteLine("Total number of fish after 256 days: " + fishTotal);
			}
		}
	}
}
