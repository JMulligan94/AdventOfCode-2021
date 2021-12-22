using System;
using System.IO;

namespace _21_DiracDice
{
	class QuantumDiceGame
	{
		int p1Start;
		int p2Start;

		int p1Score;
		int p2Score;

		static int[] s_possibleRolls = new int[27];
		static int[] s_rollCount = new int[10];

		static QuantumDiceGame()
		{
			int rollIndex = 0;
			for (int rollA = 1; rollA <= 3; ++rollA)
			{
				for (int rollB = 1; rollB <= 3; ++rollB)
				{
					for (int rollC = 1; rollC <= 3; ++rollC)
					{
						int possibleRoll = rollA + rollB + rollC;
						s_possibleRolls[rollIndex++] = possibleRoll;
						s_rollCount[possibleRoll]++;
						Console.WriteLine(rollA + "," + rollB + "," + rollC + " -> " + possibleRoll);
					}
				}
			}
		}

		public QuantumDiceGame(int _p1Start, int _p2Start, int _p1Score, int _p2Score)
		{
			p1Start = _p1Start;
			p2Start = _p2Start;

			p1Score = _p1Score;
			p2Score = _p2Score;
		}

		public void RunGame(out UInt64 p1Wins, out UInt64 p2Wins)
		{
			p1Wins = 0;
			p2Wins = 0;

			int p1Score = 0;
			int p2Score = 0;

			int p1Index = p1Start - 1;
			int p2Index = p2Start - 1;

			// Roll 3 sided dice 3 times
			// 3^3 -> 27 possible outcomes
			// Will create 27 games from player 1's roll
				


			int p1Movement = 3;
			p1Index += p1Movement;
			p1Index %= 10;
			p1Score += p1Index + 1;

			if (p1Score >= 21)
				p1Wins++;

			// Will create 27 games from player 2's roll
			int p2Movement = 3;
			p2Index += p2Movement;
			p2Index %= 10;
			p2Score += p2Index + 1;

			if (p2Score >= 21)
				p2Wins++;
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("test.txt");

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
				QuantumDiceGame diceGame = new QuantumDiceGame(p1Start, p2Start, 0, 0);
				UInt64 p1Wins = 0;
				UInt64 p2Wins = 0;
				diceGame.RunGame(out p1Wins, out p2Wins);
			}
		}
	}
}
