using System;
using System.Collections.Generic;
using System.IO;

namespace _14_ExtendedPolymerization
{
	class Program
	{
		static string PerformStep(string template, ref char[,] rules)
		{
			string newTemplate = template;

			List<Tuple<int, char>> toInsert = new List<Tuple<int, char>>();
			for(int i = 0; i < template.Length-1; ++i)
			{
				char thisChar = template[i];
				char nextChar = template[i+1];
				char insertionChar = rules[thisChar - 'A', nextChar - 'A'];
				if (insertionChar != 0)
				{
					toInsert.Add(new Tuple<int, char>(i+1, insertionChar));
				}
			}

			// Iterate backwards and insert new chars (since it is done simultaneously)
			for (int i = toInsert.Count - 1; i >= 0; --i)
			{
				int insertIndex = toInsert[i].Item1;
				string insertChar = toInsert[i].Item2 + "";

				newTemplate = newTemplate.Insert(insertIndex, insertChar);
				//Console.WriteLine("\tInserted " + insertChar + " at " + insertIndex);
			}

			return newTemplate;
		}

		static UInt64 GetQuantityRange(string template)
		{
			UInt64[] charCounts = new UInt64[26];
			foreach (var character in template)
			{
				charCounts[character - 'A']++;
			}

			UInt64 highestCount = UInt64.MinValue;
			int highestIndex = 0;
			UInt64 lowestCount = UInt64.MaxValue;
			int lowestIndex = 0;

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

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("test.txt");

			string initialTemplate = lines[0];
			List<Tuple<string, string>> stringInsertionRules = new List<Tuple<string, string>>();

			for (int lineIndex = 2; lineIndex < lines.Length; ++lineIndex)
			{
				var ruleTokens = lines[lineIndex].Split(' ');
				stringInsertionRules.Add(new Tuple<string, string>(ruleTokens[0], ruleTokens[2]));
			}

			char[,] insertionRules = new char[26, 26];

			foreach (var rule in stringInsertionRules)
			{
				int row = (rule.Item1[0] - 'A');
				int col = (rule.Item1[1] - 'A');
				insertionRules[row, col] = rule.Item2[0];
			}

			string template = initialTemplate;
			Console.WriteLine("Template:\t\t" + template);

			// Perform 10 steps
			for (int i = 0; i < 10; ++i)
			{
				template = PerformStep(template, ref insertionRules);
				//Console.WriteLine("Completed step " + i);
				Console.WriteLine("After step " + i + ":\t\t" + template);
			}

			// Part One
			{
				Console.WriteLine("\nPart One:");

				// Get the difference between most common char and least common
				UInt64 quantityDiff = GetQuantityRange(template);
				Console.WriteLine("Quantity difference after 10 steps is: " + quantityDiff + "\n");
			}

			// Perform 30 more steps - 40 total
			for (int i = 10; i < 40; ++i)
			{
				template = PerformStep(template, ref insertionRules);
				Console.WriteLine("Completed step " + i);
				//Console.WriteLine("After step " + i + ":\t\t" + template);
			}

			// Part Two
			{
				Console.WriteLine("\nPart Two:");

				// Get the difference between most common char and least common
				UInt64 quantityDiff = GetQuantityRange(template);
				Console.WriteLine("Quantity difference after 40 steps is: " + quantityDiff + "\n");
			}
		}
	}
}
