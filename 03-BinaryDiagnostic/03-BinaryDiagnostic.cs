using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _03_BinaryDiagnostic
{
	class Program
	{
		static string SolveCommonNumber(ref List<string> numbers, bool matchMostCommon)
		{
			for (int i = 0; i < numbers[0].Length; ++i)
			{
				int bitCounter = 0;
				foreach (var line in numbers)
				{
					if (line[i] == '1')
						bitCounter++;
				}

				bool mostCommonIs1 = (bitCounter * 2) >= numbers.Count;

				for (int lineIdx = numbers.Count - 1; lineIdx >= 0; --lineIdx)
				{
					bool validBit = false;
					if (matchMostCommon)
						validBit = mostCommonIs1 == (numbers[lineIdx][i] == '1'); // Valid if 1 is majority and is 1 or 0 is majority and is 0
					else
						validBit = mostCommonIs1 != (numbers[lineIdx][i] == '1'); // Valid if 1 is majority and is 0 or 0 is majority and is 1 

					if (!validBit)
					{
						// Remove from list
						numbers.RemoveAt(lineIdx);
					}
				}

				// Found the number!
				if (numbers.Count == 1)
					return numbers[0];
			}
			return null;
		}

		static int ConvertBitStringToInt(string bitString)
		{
			// Iterate bit places and increment int value with relevant digit
			int intValue = 0;
			for (int i = 0; i < bitString.Length; ++i)
			{
				int digit = (bitString.Length - i - 1); // 0-index is highest bit 

				if (bitString[i] == '1')
					intValue += 1 << digit;
			}
			return intValue;
		}

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			int amountOfNumbers = lines.Length;
			int numberOfBits = lines[0].Length;

			// Part One
			{
				// Count number of 1s in each bit place 
				int[] bitCounters = new int[numberOfBits];
				foreach (var line in lines)
				{
					for (int i = 0; i < numberOfBits; ++i)
					{
						if (line[i] == '1')
							bitCounters[i]++;
					}
				}

				// Gamma rate built from series of bits for whichever digit was most common for that bit place
				int gammaRate = 0;
				for (int i = 0; i < numberOfBits; ++i)
				{
					if ((bitCounters[i] * 2) > amountOfNumbers)
					{
						// Over half are 1s for this digit
						int digit = (numberOfBits - i - 1); // 0-index is highest bit 
						gammaRate += 1 << digit;
					}
				}


				// Epsilon rate built from series of bits for whichever digit was least common for that bit place
				int totalRate = ((1 << numberOfBits) - 1); 
				int epsilonRate = totalRate - gammaRate; // Can be calculated as the inverse of the gamma rate

				int powerConsumption = gammaRate * epsilonRate;

				Console.WriteLine("Part One:");
				Console.WriteLine("Gamma Rate: " + gammaRate + ", Epsilon Rate: " + epsilonRate);
				Console.WriteLine("Power consumption (product) is: " + powerConsumption + "\n");
			}

			// Part Two
			{
				// Find oxygen value
				List<string> mostCommonLines = new List<string>();
				mostCommonLines.AddRange(lines);
				string mostCommon = SolveCommonNumber(ref mostCommonLines, true);
				int oxygenSupportRating = ConvertBitStringToInt(mostCommon);

				// Find CO2 value
				List<string> leastCommonLines = new List<string>();
				leastCommonLines.AddRange(lines);
				string leastCommon = SolveCommonNumber(ref leastCommonLines, false);
				int co2ScrubberRating = ConvertBitStringToInt(leastCommon);

				int lifeSupportRating = oxygenSupportRating * co2ScrubberRating;

				Console.WriteLine("Part Two:");
				Console.WriteLine("Oxygen Support Rating: " + oxygenSupportRating + ", CO2 Scrubber Rating: " + co2ScrubberRating);
				Console.WriteLine("Life support rating (product) is: " + lifeSupportRating + "\n");
			}
		}
	}
}
