using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _13_TransparentOrigami
{
	class Paper
	{
		int height;
		int width;
		List<Tuple<int, int>> dotList;

		public Paper(int _height, int _width, List<Tuple<int, int>> _dotList)
		{
			height = _height;
			width = _width;
			dotList = _dotList;
		}

		public void FoldVertical(int foldIndex)
		{
			Console.WriteLine("\n--- Folding along x=" + foldIndex + " ---");

			List<Tuple<int, int>> newDots = new List<Tuple<int, int>>();
			
			// Fold bottom half upwards
			foreach(var dot in dotList)
			{
				int currentRow = dot.Item1;
				if (currentRow > foldIndex)
				{
					// Needs to fold upwards
					Console.WriteLine("Dot (" + dot.Item1 + "," + dot.Item2 + ") needs folding");
					
					int newRow = foldIndex - (currentRow - foldIndex);

					Console.WriteLine("\tFolding to: (" + newRow + "," + dot.Item2 + ")");

					// Check if dot is already in the list
					if (newDots.FirstOrDefault(x => x.Item1 == newRow && x.Item2 == dot.Item2) == null)
					{
						newDots.Add(new Tuple<int, int>(newRow, dot.Item2));
						Console.WriteLine("\tDot added!");
					}
					else
					{
						Console.WriteLine("\tDot already exists - no need to add!");
					}	
				}
				else
				{
					// No need to fold, just needs adding to new list (if not already in there)
					if (newDots.FirstOrDefault(x => x.Item1 == dot.Item1 && x.Item2 == dot.Item2) == null)
					{
						newDots.Add(dot);
						Console.WriteLine("Dot (" + dot.Item1 + "," + dot.Item2 + ") added - not folded");
					}
				}
			}

			dotList.Clear();
			dotList = newDots;
			height = foldIndex;
		}

		public void FoldHorizontal(int foldIndex)
		{
			Console.WriteLine("\n--- Folding along y=" + foldIndex + " ---");

			List<Tuple<int, int>> newDots = new List<Tuple<int, int>>();
			
			// Fold right half across to the left
			foreach (var dot in dotList)
			{
				int currentCol = dot.Item2;
				if (currentCol > foldIndex)
				{
					// Needs to fold left
					Console.WriteLine("Dot (" + dot.Item1 + "," + dot.Item2 + ") needs folding");

					int newCol = foldIndex - (currentCol - foldIndex);

					Console.WriteLine("\tFolding to: (" + dot.Item1 + "," + newCol + ")");

					// Check if dot is already in the list
					if (newDots.FirstOrDefault(x => x.Item1 == dot.Item1 && x.Item2 == newCol) == null)
					{
						newDots.Add(new Tuple<int, int>(dot.Item1, newCol));
						Console.WriteLine("\tDot added!");
					}
					else
					{
						Console.WriteLine("\tDot already exists - no need to add!");
					}
				}
				else
				{
					// No need to fold, just needs adding to new list (if not already in there)
					if (newDots.FirstOrDefault(x => x.Item1 == dot.Item1 && x.Item2 == dot.Item2) == null)
					{
						newDots.Add(dot);
						Console.WriteLine("Dot (" + dot.Item1 + "," + dot.Item2 + ") added - not folded");
					}
				}
			}

			dotList.Clear();
			dotList = newDots;
			width = foldIndex;
		}

		public int GetNumberOfDots()
		{
			return dotList.Count;
		}

		public void PrintPaper()
		{
			Console.WriteLine("\n====Paper===");

			// Convert to 2D array of bools to print
			bool[,] dots = new bool[height, width];
			foreach (var dot in dotList)
			{
				dots[dot.Item1, dot.Item2] = true;
			}

			// '#' = dot
			// '.' = empty
			for (int row = 0; row < height; ++row)
			{
				string paperStr = "";
				for (int col = 0; col < width; ++col)
				{
					paperStr += dots[row, col] ? "#" : ".";
				}
				Console.WriteLine(paperStr);
			}
		}
	}

	class Program
	{
		enum FoldType
		{
			Vertical,
			Horizontal
		}		

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			// Parse input into list of dots and folds
			List<Tuple<int, int>> dotList = new List<Tuple<int, int>>();
			List<Tuple<FoldType, int>> folds = new List<Tuple<FoldType, int>>();

			int height = 0;
			int width = 0;

			bool readingDots = true;
			foreach(var line in lines)
			{
				if (line == "")
				{
					readingDots = false;
					continue;
				}

				if (readingDots)
				{
					// Adding dot
					var coords = line.Split(',');
					int row = int.Parse(coords[1]);
					int col = int.Parse(coords[0]);

					dotList.Add(new Tuple<int, int>(row, col));

					height = Math.Max(height, row+1);
					width = Math.Max(width, col+1);
				}
				else
				{
					// Adding folds
					var tokens = line.Split(' ');
					var foldInfo = tokens[2].Split('=');

					FoldType type = foldInfo[0] == "y" ? FoldType.Vertical : FoldType.Horizontal;
					folds.Add(new Tuple<FoldType, int>(type, int.Parse(foldInfo[1])));
				}
			}

			// Starting paper
			Paper paper = new Paper(height, width, dotList);
			paper.PrintPaper();

			// Part One
			{
				// Do first fold only
				{
					FoldType foldType = folds[0].Item1;
					int foldIndex = folds[0].Item2;

					if (foldType == FoldType.Vertical)
					{
						paper.FoldVertical(foldIndex);
					}
					else
					{
						paper.FoldHorizontal(foldIndex);
					}

					paper.PrintPaper();
				}

				int visibleDots = paper.GetNumberOfDots();
				Console.WriteLine("Part One:");
				Console.WriteLine("Number of visible dots after one fold is: " + visibleDots + "\n");
			}

			// Part Two
			{
				// Do rest of folds
				for (int i = 1; i < folds.Count; ++i)
				{
					FoldType foldType = folds[i].Item1;
					int foldIndex = folds[i].Item2;

					if (foldType == FoldType.Vertical)
					{
						paper.FoldVertical(foldIndex);
					}
					else
					{
						paper.FoldHorizontal(foldIndex);
					}
				}

				Console.WriteLine("Part Two:");
				paper.PrintPaper();
			}
		}
	}
}
