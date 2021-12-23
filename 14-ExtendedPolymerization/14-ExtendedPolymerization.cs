using System;
using System.Collections.Generic;
using System.IO;

namespace _14_ExtendedPolymerization
{
	class Program
	{
		// Returns the difference between the highest and lowest count in charCounts
		static UInt64 GetQuantityRange(ref UInt64[] charCounts)
		{
			UInt64 highestCount = UInt64.MinValue;
			int highestIndex = 0;
			UInt64 lowestCount = UInt64.MaxValue;
			int lowestIndex = 0;

			// Iterate each char count to determine which is highest or lowest
			for (int countIndex = 0; countIndex < charCounts.Length; ++countIndex)
			{
				UInt64 count = charCounts[countIndex];

				if (count == 0) // Character is not included in template
					continue;

				if (highestCount < count)
				{
					highestCount = count;
					highestIndex = countIndex;
				}

				if (lowestCount > count)
				{
					lowestCount = count;
					lowestIndex = countIndex;
				}
			}

			Console.WriteLine("Most common = " + (char)(highestIndex + 'A') + " (" + highestCount + " times)");
			Console.WriteLine("Least common = " + (char)(lowestIndex + 'A') + " (" + lowestCount + " times)");

			return highestCount - lowestCount;
		}

		private static UInt64 PerformSteps(int numSteps, ref Dictionary<string, UInt64> pairCounts, ref Dictionary<string, Tuple<string, string>> pairsCreated, ref UInt64[] charCounts)
		{
			// Create dictionary to use for simultaneously altering pair count between each step
			Dictionary<string, UInt64> pairsToCreate = new Dictionary<string, UInt64>(pairCounts);
			for (int stepIndex = 0; stepIndex < numSteps; ++stepIndex)
			{
				// Null all values for counts to 0 for this step
				foreach (var pairToNull in pairsToCreate)
				{
					pairsToCreate[pairToNull.Key] = 0;
				}

				// For each step, run through the pairs we have and add to the pairs we will create from them
				foreach (var pair in pairCounts)
				{
					if (pair.Value == 0)
						continue;

					var childPairs = pairsCreated[pair.Key];
					pairsToCreate[childPairs.Item1] += pair.Value;
					pairsToCreate[childPairs.Item2] += pair.Value;

					// Add to char counts array for the newly created letter
					charCounts[childPairs.Item1[1] - 'A'] += pair.Value;
				}

				// Set new pair counts that were created this step - allows it to work simultaneously
				pairCounts = new Dictionary<string, UInt64>(pairsToCreate);
			}

			// Get the difference between most common char and least common
			return GetQuantityRange(ref charCounts);
		}

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			// Parse from file to dictionary of which pairs create which chars
			string initialTemplate = lines[0];
			List<Tuple<string, string>> stringInsertionRules = new List<Tuple<string, string>>();
			for (int lineIndex = 2; lineIndex < lines.Length; ++lineIndex)
			{
				var ruleTokens = lines[lineIndex].Split(' ');
				stringInsertionRules.Add(new Tuple<string, string>(ruleTokens[0], ruleTokens[2]));
			}

			// Map of the amount of pairs currently in the template
			Dictionary<string, UInt64> pairCounts = new Dictionary<string, UInt64>();

			// Map of pairs to the pairs they create
			Dictionary<string, Tuple<string, string>> pairsCreated = new Dictionary<string, Tuple<string, string>>();

			// Create key set for dictionary which tracks number of each pair created
			// Also cache which pairs create which other pairs at this time
			foreach (var rule in stringInsertionRules)
			{
				pairCounts.Add(rule.Item1, 0);
				pairsCreated.Add(rule.Item1, new Tuple<string, string>(rule.Item1[0] + rule.Item2, rule.Item2 + rule.Item1[1]));
			}

			// Keep track of amount of times a char appears in the template
			UInt64[] charCounts = new UInt64[26];

			// Get initial values for amount of characters and amount of each pair type
			for (int i = 0; i < initialTemplate.Length; ++i)
			{
				charCounts[initialTemplate[i] - 'A']++;

				if (i < initialTemplate.Length - 1)
				{
					string pair = "" + initialTemplate[i] + initialTemplate[i + 1];
					pairCounts[pair] = pairCounts[pair] + 1;
				}
			}

			// Part One
			{
				Console.WriteLine("\nPart One:");

				// Perform 10 steps
				UInt64 quantityDiff = PerformSteps(10, ref pairCounts, ref pairsCreated, ref charCounts); 
				Console.WriteLine("Quantity difference after 10 steps is: " + quantityDiff + "\n");
			}


			// Part Two
			{
				Console.WriteLine("\nPart Two:");

				// Perform another 30 steps
				UInt64 quantityDiff = PerformSteps(30, ref pairCounts, ref pairsCreated, ref charCounts);
				Console.WriteLine("Quantity difference after 40 steps is: " + quantityDiff + "\n");
			}
		}
	}
}
