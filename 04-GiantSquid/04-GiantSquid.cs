using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _04_GiantSquid
{
	// Class for Bingo card storing a grid of 5x5 numbers
	class Card
	{
		// Class for a single number on the bingo card
		class CardNumber
		{
			public int value; // Number
			public bool marked; // Has the number been called

			public CardNumber(int number)
			{
				value = number;
				marked = false;
			}

			public void SetMarked()
			{
				marked = true;
			}

			public override string ToString()
			{
				// "[X]" if marked, "X" if unmarked
				return marked ? ("[" + value + "]") : value.ToString();
			}
		}

		public int id; // Card ID
		private List<List<CardNumber>> m_numbers = new List<List<CardNumber>>(5); // 5 rows of numbers
		public bool isComplete = false; // Has card been completed?

		public Card(int _id, string[] numberStrings)
		{
			id = _id;

			int rowIdx = 0;

			// Parse number strings into grid of CardNumbers
			foreach(var numberStr in numberStrings)
			{
				var numbers = numberStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				foreach(var number in numbers)
				{
					if (m_numbers.Count <= rowIdx)
						m_numbers.Add(new List<CardNumber>(5));
					m_numbers[rowIdx].Add(new CardNumber(int.Parse(number)));
				}
				rowIdx++;
			}
		}

		// Mark called number on the card if applicable
		// return true if match is found on the card
		public bool MarkNumber(int calledNumber)
		{
			for(int row = 0; row < 5; ++row)
			{
				for(int col = 0; col < 5; ++col)
				{
					if (m_numbers[row][col].value == calledNumber)
					{
						// Found a match for the called number!
						m_numbers[row][col].SetMarked();

						// Check if this new marked number creates a winner and cache
						isComplete = CheckCardComplete(row, col);

						Console.WriteLine("\nChecking card " + id + " - " + calledNumber + " found at (" + row + "," + col + ")");

						return true;
					}
				}
			}
			return false;
		}

		// Checks given row and column for completion (i.e. all 5 numbers marked)
		private bool CheckCardComplete(int rowToCheck, int colToCheck)
		{
			// Check row
			{
				bool allMarked = true;
				foreach(CardNumber number in m_numbers[rowToCheck])
				{
					allMarked &= number.marked;
					if (!allMarked)
						break;
				}

				if (allMarked)
					return true;
			}

			// Check col
			{
				bool allMarked = true;

				for (int row = 0; row < 5; ++row)
				{
					allMarked &= m_numbers[row][colToCheck].marked;
					if (!allMarked)
						break;
				}

				if (allMarked)
					return true;
			}

			return false;
		}

		public void PrintCard()
		{
			Console.WriteLine("============== Card " + id + " ==============");
			for (int row = 0; row < 5; ++row)
			{
				for (int col = 0; col < 5; ++col)
				{
					Console.Write(m_numbers[row][col] + "\t");
				}
				Console.Write("\n");
			}
			Console.WriteLine("====================================");
		}

		// returns sum of all numbers left unmarked on this card
		public int GetSumOfUnmarked()
		{
			int sum = 0;
			for (int row = 0; row < 5; ++row)
			{
				for (int col = 0; col < 5; ++col)
				{
					if (!m_numbers[row][col].marked)
						sum += m_numbers[row][col].value;
				}
			}
			return sum;
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");
			var callingNumbers = lines[0].Split(','); // First line contains calling numbers

			// Create bingo cards!
			List<Card> cards = new List<Card>();
			int lineIdx = 2;
			string[] currentCardNumbers = new string[5];
			int tokenIdx = 0;
			while (lineIdx < lines.Length)
			{
				var line = lines[lineIdx];

				if (line == "") // At empty line, we can store all previous lines onto a new card and reset
				{
					tokenIdx = 0;
					cards.Add(new Card(cards.Count, currentCardNumbers));
				}
				else
				{
					currentCardNumbers[tokenIdx++] = line;
				}
				lineIdx++;
			}

			cards.Add(new Card(cards.Count, currentCardNumbers)); // Add final card

			bool[] cardsFinished = new bool[cards.Count]; // Which cards have already won
			int cardsFinishedCount = 0; // How many have already won?

			int firstWinningNumber = 0; // The called number that the first winning card won with
			Card firstWinningCard = null;

			int lastWinningNumber = 0; // The called number that the final winning card won with
			Card lastWinningCard = null;

			int sumOfUnmarkedAtFirstWinner = 0; 
			int firstWinnerScore = 0; 

			// Call the numbers!
			foreach (int calledNumber in callingNumbers.Select(x => int.Parse(x)))
			{
				Console.WriteLine("\n== Calling " + calledNumber + " ==");

				// Check each card against this called number
				foreach (Card card in cards)
				{
					// Mark number on card if applicable (returns true if found a number to mark on the card)
					if (card.MarkNumber(calledNumber))
					{
						card.PrintCard();

						// Number found on card!
						if (card.isComplete) // Has the card won?
						{
							// If this is the first time the card has won
							if (!cardsFinished[card.id])
							{
								Console.WriteLine("Winner!");
								cardsFinished[card.id] = true; // Keep track of which cards have already won

								if (cardsFinishedCount == 0)
								{
									// If this is the first winner
									firstWinningNumber = calledNumber;
									firstWinningCard = card;
									sumOfUnmarkedAtFirstWinner = firstWinningCard.GetSumOfUnmarked();
									firstWinnerScore = sumOfUnmarkedAtFirstWinner * firstWinningNumber;
								}

								cardsFinishedCount++;

								if (cardsFinishedCount == cards.Count)
								{
									// If this is the final winner
									lastWinningNumber = calledNumber;
									lastWinningCard = card;
								}
							}
						}
					}
				}
				if (lastWinningCard != null)
					break;
			}

			// Part One
			{
				Console.WriteLine("\nPart One:");
				Console.WriteLine("First winning number is " + firstWinningNumber);
				Console.WriteLine("First winning card is " + firstWinningCard.id);
				Console.WriteLine("Sum of unmarked numbers is " + sumOfUnmarkedAtFirstWinner);
				Console.WriteLine("Score: " + firstWinnerScore + "\n");
			}

			// Part Two
			{
				int sumOfUnmarkedAtFinalWinner = lastWinningCard.GetSumOfUnmarked();
				int finalWinnerScore = sumOfUnmarkedAtFinalWinner * lastWinningNumber;

				Console.WriteLine("\nPart Two:");
				Console.WriteLine("Final winning number is " + lastWinningNumber);
				Console.WriteLine("Final winning card is " + lastWinningCard.id);
				Console.WriteLine("Sum of unmarked numbers is " + sumOfUnmarkedAtFinalWinner);
				Console.WriteLine("Score: " + finalWinnerScore + "\n");
			}
		}
	}
}
