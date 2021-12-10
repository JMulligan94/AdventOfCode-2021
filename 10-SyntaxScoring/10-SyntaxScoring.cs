using System;
using System.Collections.Generic;
using System.IO;

namespace _10_SyntaxScoring
{
	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			// Pairs opening char to closing char
			Dictionary<char, char> delimiterPairs = new Dictionary<char, char>();
			delimiterPairs.Add('(', ')');
			delimiterPairs.Add('[', ']');
			delimiterPairs.Add('{', '}');
			delimiterPairs.Add('<', '>');

			List<char> illegalCharacters = new List<char>();
			List<Tuple<string, Stack<char>>> incompleteLines = new List<Tuple<string, Stack<char>>>();
			foreach (var line in lines)
			{
				Stack<char> openedChunks = new Stack<char>();
				foreach (var character in line)
				{
					if (delimiterPairs.ContainsKey(character))
					{
						// Opening character
						openedChunks.Push(character);
						continue;
					}
					else
					{
						// Must be a closing character
						// Check against last opened chunk to determine if it's valid
						char lastOpened = openedChunks.Peek();
						char validClosingChar = delimiterPairs[lastOpened];

						if (character == validClosingChar)
						{
							// This is a valid pair, remove last opened chunk info and continue
							openedChunks.Pop();
							continue;
						}
						else
						{
							// Not valid, stop here and log illegal character
							illegalCharacters.Add(character);
							Console.WriteLine("Expected " + validClosingChar + ", but found " + character + " instead.");

							// Clear out all opened chunks so it doesn't register as an incomplete line
							openedChunks.Clear();
							break;
						}
					}
				}

				if (openedChunks.Count > 0)
				{
					// Line valid so far but incomplete
					incompleteLines.Add(new Tuple<string, Stack<char>>(line, openedChunks));
				}
			}

			// Part One
			{
				Console.WriteLine("Part One");

				// Pairs closing chars to their scores if found to be included illegally
				Dictionary<char, int> illegalCharacterScores = new Dictionary<char, int>();
				illegalCharacterScores.Add(')', 3);
				illegalCharacterScores.Add(']', 57);
				illegalCharacterScores.Add('}', 1197);
				illegalCharacterScores.Add('>', 25137);

				int totalSyntaxErrorScore = 0;
				foreach (var illegalChar in illegalCharacters)
				{
					totalSyntaxErrorScore += illegalCharacterScores[illegalChar];
				}

				Console.WriteLine("Total error score is: " + totalSyntaxErrorScore + "\n");
			}

			// Part Two
			{
				Console.WriteLine("Part Two");

				// Pairs closing chars to their scores if they are autocompleted
				Dictionary<char, UInt64> autocompleteCharacterScores = new Dictionary<char, UInt64>();
				autocompleteCharacterScores.Add(')', 1);
				autocompleteCharacterScores.Add(']', 2);
				autocompleteCharacterScores.Add('}', 3);
				autocompleteCharacterScores.Add('>', 4);

				List<UInt64> autocompleteScores = new List<UInt64>();
				foreach (var incompleteLineInfo in incompleteLines)
				{
					UInt64 autocompleteScore = 0;
					string outputStr = "Complete by adding ";
					while (incompleteLineInfo.Item2.Count > 0)
					{
						char incompleteOpeningChar = incompleteLineInfo.Item2.Pop();
						char closingCharRequired = delimiterPairs[incompleteOpeningChar];

						// Determine autocomplete score
						autocompleteScore *= 5; // Multiply by 5
						autocompleteScore += autocompleteCharacterScores[closingCharRequired]; // Add autocomplete score for closing char added

						outputStr += closingCharRequired;
					}

					Console.WriteLine(outputStr);
					autocompleteScores.Add(autocompleteScore);
				}

				autocompleteScores.Sort(); // Sort from lowest to highest

				//Should always be an odd amount
				UInt64 middleScore = autocompleteScores[autocompleteScores.Count / 2];

				Console.WriteLine("Total autocomplete score is: " + middleScore + "\n");
			}
		}
	}
}
