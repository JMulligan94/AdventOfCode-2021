using System;
using System.Collections.Generic;
using System.IO;

namespace _21_DiracDice
{
	class Program
	{
		public static (int roll, int frequency)[] s_rollCount = { (3, 1), (4, 3), (5, 6), (6, 7), (7, 6), (8, 3), (9, 1) };

		public static (ulong p1Wins, ulong p2Wins) RecursePlayGame(int p1Pos, int p1Score, int p2Pos, int p2Score, bool isPlayer1, ulong universeCount)
		{
			ulong p1Wins = 0;
			ulong p2Wins = 0;

			for (int rollIndex = 0; rollIndex < s_rollCount.Length; ++rollIndex)
			{
				ulong frequency = (ulong)s_rollCount[rollIndex].frequency;

				int roll = s_rollCount[rollIndex].roll;
				int newSpaceIndex = (roll + (isPlayer1 ? p1Pos : p2Pos)) % 10;
				int newScore = newSpaceIndex + 1 + (isPlayer1 ? p1Score : p2Score);

				ulong identicalUniverseCount = universeCount * frequency;

				if (newScore >= 21)
				{
					if (isPlayer1)
					{
						// P1 wins in X number of universes!
						p1Wins += identicalUniverseCount;
					}
					else
					{
						// P2 wins in X number of universes!
						p2Wins += identicalUniverseCount;
					}

					continue;
				}

				// Otherwise, no one has won this game yet, keep going and pass play back to the other player
				if (isPlayer1)
				{
					var wins = RecursePlayGame(newSpaceIndex, newScore, p2Pos, p2Score, false, identicalUniverseCount);
					p1Wins += wins.p1Wins;
					p2Wins += wins.p2Wins;
				}
				else
				{
					var wins = RecursePlayGame(p1Pos, p1Score, newSpaceIndex, newScore, true, identicalUniverseCount);
					p1Wins += wins.p1Wins;
					p2Wins += wins.p2Wins;
				}
			}

			return (p1Wins, p2Wins);
		}

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			int p1Start = int.Parse(lines[0].Split(' ')[4]);
			int p2Start = int.Parse(lines[1].Split(' ')[4]);
			Console.WriteLine("Player 1 starting at P" + p1Start);
			Console.WriteLine("Player 2 starting at P" + p2Start);

			// Part One
			{
				Console.WriteLine("\nPart One:");
				int deterministicIndex = 1;

				int p1Score = 0;
				int p2Score = 0;

				int p1Index = p1Start - 1;
				int p2Index = p2Start - 1;

				while (true)
				{
					// Roll 100-sided dice 3 times
					// Using deterministic dice for part 1 so we take the next three integers
					// (i.e. rolling 7, 8 and 9)
					int p1Movement = deterministicIndex * 3 + 3;
					deterministicIndex += 3;

					p1Index += p1Movement;
					p1Index %= 10;
					p1Score += p1Index + 1;

					Console.WriteLine("Player 1 moves by " + p1Movement + " to space " + (p1Index + 1) + " for a total score of " + p1Score);

					if (p1Score >= 1000)
						break;

					int p2Movement = deterministicIndex * 3 + 3;
					deterministicIndex += 3;

					p2Index += p2Movement;
					p2Index %= 10;
					p2Score += p2Index + 1;

					Console.WriteLine("Player 2 moves by " + p2Movement + " to space " + (p2Index + 1) + " for a total score of " + p2Score);

					if (p2Score >= 1000)
						break;
				}

				int winningPlayer = p1Score > p2Score ? 1 : 2;
				int winningScore = winningPlayer == 1 ? p1Score : p2Score;
				int losingScore = winningPlayer == 1 ? p2Score : p1Score;
				int dieRollCount = deterministicIndex - 1;

				Console.WriteLine("\nPlayer " + winningPlayer + " wins!");
				Console.WriteLine("Winning score: " + winningScore);
				Console.WriteLine("Die rolled " + dieRollCount + " times!");

				Console.WriteLine("Losing score: " + losingScore);

				Console.WriteLine("Product of losing score and die roll count is: " + (losingScore * dieRollCount));
			}

			// Part Two
			{
				Console.WriteLine("\nPart Two:");

				var wins = RecursePlayGame(p1Start-1, 0, p2Start-1, 0, true, 1);
				ulong p1Wins = wins.p1Wins;
				ulong p2Wins = wins.p2Wins;

				Console.WriteLine("Player 1 wins " + p1Wins + " times");
				Console.WriteLine("Player 2 wins " + p2Wins + " times");
			}
		}
	}
}
