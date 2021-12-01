using System;
using System.IO;

namespace _01_SonarSweep
{
	class Program
	{
		static void Main(string[] args)
		{
			// Read in lines and generate measurement array
			var lines = File.ReadAllLines("input.txt");
			int[] measurements = new int[lines.Length];
			for (int i = 0; i < lines.Length; ++i)
			{
				measurements[i] = int.Parse(lines[i]);
			}

			// Part One - single measurement comparison
			{
				int largerMeasurements = 0;
				for (int i = 1; i < measurements.Length; ++i)
				{
					// Compare previous reading to this
					if (measurements[i - 1] < measurements[i])
						largerMeasurements++;
				}

				Console.WriteLine("Part One:\nNumber of increased depth measurements is " + largerMeasurements + "\n");
			}

			// Part Two - Three measurement sliding window
			{
				// Create array of sums for sliding windows
				int[] sums = new int[measurements.Length-2];
				for (int i = 2; i < measurements.Length; ++i)
				{
					sums[i-2] = measurements[i - 2] + measurements[i - 1] + measurements[i];
				}

				// Find amount of increases in depth from sliding window sums
				int largerMeasurementSums = 0;
				for (int i = 1; i < sums.Length; ++i)
				{
					// Compare previous window to this
					if (sums[i-1] < sums[i])
						largerMeasurementSums++;
				}


				Console.WriteLine("Part Two:\nNumber of increased sliding depth measurements is " + largerMeasurementSums);
			}
		}
	}
}
