using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _08_SevenSegmentSearch
{
	class Entry
	{
		public string[] signalPatterns;
		public string[] outputValues;

		public string[] knownPatterns = new string[10]; // stored in alphabetical order - easier to compare

		public string decodedOutput;

		public Entry(string line)
		{
			// Entry given a line in the format:
			// <pattern1> <pattern2> ... <pattern10> | <output1> <output2> <output3> <output4>
			var lineHalves = line.Split('|');

			var signalPatternsStr = lineHalves[0].TrimEnd(' ');
			signalPatterns = signalPatternsStr.Split(' ');

			var outputValuesStr = lineHalves[1].TrimStart(' ');
			outputValues = outputValuesStr.Split(' ');

			DeduceUniqueNumbers();
			DeduceRemainingNumbers();

			DecodeSignal();
		}

		/*
		* 
		*	 aaaa
		*	b    c
		*	b    c
		*	 dddd
		*	e    f
		*	e    f
		*	 gggg
		* 
		*/

		// 0 = 6 segments
		// 1 = 2 segments (unique)
		// 2 = 5 segments
		// 3 = 6 segments
		// 4 = 4 segments (unique)
		// 5 = 5 segments
		// 6 = 6 segments
		// 7 = 3 segments (unique)
		// 8 = 7 segments (unique)
		// 9 = 6 segments

		public void DeduceUniqueNumbers()
		{
			foreach (var pattern in signalPatterns)
			{
				// Easy ones first - these use unique lengths of segments so they can be determined straight away
				if (pattern.Length == 2)
					knownPatterns[1] = OrderString(pattern);
				else if (pattern.Length == 4)
					knownPatterns[4] = OrderString(pattern);
				else if (pattern.Length == 3)
					knownPatterns[7] = OrderString(pattern);
				else if (pattern.Length == 7)
					knownPatterns[8] = OrderString(pattern);
			}
		}

		// Check if subPattern is FULLY contained within pattern
		private bool PatternContainsSubPattern(string pattern, string subPattern)
		{
			foreach (var segmentChar in subPattern)
			{
				if (pattern.IndexOf(segmentChar) == -1)
				{
					// return false if we find a segment that isn't in the pattern
					return false;
				}
			}
			return true;
		}

		public void DeduceRemainingNumbers()
		{
			string leftoverPattern1 = null;
			string leftoverPattern2 = null;

			foreach (var pattern in signalPatterns)
			{
				if (pattern.Length == 6)
				{
					// === 6 segments ====
					// Either 0, 6, or 9

					// Deduce 6
					// 6 doesn't completely contain the segments from 1 within it (unlike 9 and 0)
					bool found1Inside = PatternContainsSubPattern(pattern, knownPatterns[1]);
					if (!found1Inside)
					{
						knownPatterns[6] = OrderString(pattern);
						continue;
					}

					// Deduce 0
					// 0 doesn't completely contain the segments from 4 within it (unlike 9)
					bool found4Inside = PatternContainsSubPattern(pattern, knownPatterns[4]);
					if (!found4Inside)
					{
						knownPatterns[0] = OrderString(pattern);
						continue;
					}

					// Otherwise this has to be 9
					knownPatterns[9] = OrderString(pattern);
				}
				else if (pattern.Length == 5)
				{
					// === 5 segments ====
					// Either 2, 3, or 5

					// Deduce 3
					// 3 completely contains the segments from 1 within it (unlike 2 and 5)
					bool found1Inside = PatternContainsSubPattern(pattern, knownPatterns[1]);
					if (found1Inside)
					{
						knownPatterns[3] = OrderString(pattern);
						continue;
					}

					if (leftoverPattern1 == null)
						leftoverPattern1 = OrderString(pattern);
					else
						leftoverPattern2 = OrderString(pattern);
				}
			}

			// Left with 2 or 5 for leftover patterns

			// Deduce 5
			// 6 should completely contain whichever one is 5
			bool foundInside6 = PatternContainsSubPattern(knownPatterns[6], leftoverPattern1);

			if (!foundInside6)
			{
				// leftoverPattern1 can't be 5 since one of its segments isn't shared with 6
				// Therefore it must be 2 and the other 5
				knownPatterns[2] = leftoverPattern1;
				knownPatterns[5] = leftoverPattern2;
			}
			else
			{
				knownPatterns[5] = leftoverPattern1;
				knownPatterns[2] = leftoverPattern2;
			}
		}

		public void DecodeSignal()
		{
			// Decode the output values from their segments to their numerical value
			decodedOutput = "";
			foreach (var outputValue in outputValues)
			{
				string orderedOutput = OrderString(outputValue);

				bool found = false;
				for (int i = 0; i < 10; ++i)
				{
					if (knownPatterns[i] == orderedOutput)
					{
						decodedOutput += i;
						found = true;
						break;
					}
				}

				if (!found)
					decodedOutput += "?";
			}

			// Print entry
			string str = "";
			foreach (var outputVal in outputValues)
			{
				str += outputVal + " ";
			}
			str += ": " + decodedOutput;
			Console.WriteLine(str);
		}

		private string OrderString(string input)
		{
			return new string(input.OrderBy(c => c).ToArray());
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			List<Entry> entries = new List<Entry>();
			foreach (var line in lines)
			{
				entries.Add(new Entry(line));
			}


			// Part One - Find amount of 1s, 4s, 7s, and 8s, in the output values
			{
				int digitCount = 0;
				foreach (var entry in entries)
				{
					foreach (var outputVal in entry.outputValues)
					{
						int length = outputVal.Length;
						if (length == 2 || length == 4 || length == 3 || length == 7)
							digitCount++;
					}
				}

				Console.WriteLine("\nPart One");
				Console.WriteLine("Total number of 1,4,7,8s in output values is: " + digitCount + "\n");
			}

			// Part Two - calculate the sum of all outputs
			{
				UInt64 sumOutputValues = 0;

				foreach (var entry in entries)
				{
					sumOutputValues += UInt64.Parse(entry.decodedOutput);
				}

				Console.WriteLine("Part Two");
				Console.WriteLine("Sum of decoded output values is: " + sumOutputValues);
			}
		}
	}
}
