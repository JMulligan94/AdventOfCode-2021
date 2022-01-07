using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _24_ArithmeticLogicUnit
{
	class ALU
	{
		string modelNumber;
		int[] values = new int[4] { 0,0,0,0 };
		int readIndex = 0;

		enum ValueIndex
		{
			W,X,Y,Z
		}

		public ALU(string _modelNumber, int initZ)
		{
			modelNumber = _modelNumber;
			values[(int)ValueIndex.Z] = initZ;
		}

		// Read instructions line by line
		public void ReadInstruction(string instruction)
		{
			var tokens = instruction.Split(' ');

			// [0] is the type of instruction
			var operand = tokens[0];
			
			// [1] will always be a variable
			char variableA = tokens[1][0];
			int variableAIndex = GetValueIndex(variableA);

			// [2] could be a variable or an integer
			int variableB = 0;
			if (tokens.Length > 2)
			{
				if (!int.TryParse(tokens[2], out variableB))
				{
					variableB = values[GetValueIndex(tokens[2][0])];
				}
			}

			if (operand == "inp")
			{
				// Read an input value and write it to variable [1]
				int inputValue = int.Parse(modelNumber[readIndex] + "");
				readIndex++;

				values[variableAIndex] = inputValue;
			}
			else if (operand == "add")
			{
				// Add the value of [1] to [2], then store the result in variable [1].
				values[variableAIndex] += variableB;
			}
			else if (operand == "mul")
			{
				// Multiply the value of [1] by the value of [2], then store the result in variable [1].
				values[variableAIndex] *= variableB;
			}
			else if (operand == "div")
			{
				// Divide the value of [1] by the value of [2], truncate the result to an integer, then store the result in variable [1]. (Here, "truncate" means to round the value toward zero.)
				values[variableAIndex] /= variableB;
			}
			else if (operand == "mod")
			{
				// Divide the value of [1] by the value of [2], then store the remainder in variable [1]. (This is also called the modulo operation.)
				values[variableAIndex] %= variableB;
			}
			else if (operand == "eql")
			{
				// If the value of [1] and [2] are equal, then store the value 1 in variable [1]. Otherwise, store the value 0 in variable [1].
				values[variableAIndex] = (values[variableAIndex] == variableB) ?  1 : 0;
			}
			else
			{
				Console.WriteLine("Couldn't parse instruction:\t" + instruction);
			}
		}

		// Quicker method by just passing in parameters for each step
		public void ReadInstructionAlt(int divZ, int addX, int addY)
		{ 
			// Read an input value and write it to variable [1]
			int w = int.Parse(modelNumber[readIndex] + "");
			int x = 0;
			int y = 0;
			int z = values[(int)ValueIndex.Z];

			readIndex++;

			if ((z % 26) + addX == w)
			{
				z = (26 * (z / divZ)) + w + addY;
			}
			else
			{
				z = (z / divZ);
			}

			values = new int[]{ w,x,y,z };
		}

		public int GetZ() { return values[GetValueIndex('z')]; }

		private int GetValueIndex(char valueChar)
		{
			return valueChar - 'w';
		}
	}


	class Program
	{ 
		static void RecurseFindValidModelNumbers(int digitNum, long z, long zCap, ref int[,] parameters, string numberSoFar, ref List<string> validNumbers)
		{
			int divisor = parameters[digitNum, 0];

			// divide zCap by divisor for the new maximum z value allowed at this point
			zCap /= divisor;

			int p1 = parameters[digitNum, 1];
			int p2 = parameters[digitNum, 2];

			// Try every digit for this place in the model number from 9 down to 1
			for (int w = 9; w >= 1; --w)
			{
				// Instructions condensed down into shorter form
				long localZ = z;

				// Test for whether x=1
				bool xIs1 = ((localZ % 26) + p1) != w;
				if (xIs1)
				{
					localZ = (26 * (localZ / divisor)) + w + p2;
				}
				else
				{
					localZ /= divisor;
				}

				// This zCap is the thing that allows brute force to work quickly
				// If z is currently greater than the product of all the divisors left in the coming digits, there's no way for it to hit 0 again in time for the 14th digit place
				// so we can early out..
				if (localZ >= zCap)
					continue;

				string newNumber = numberSoFar.ToString() + w.ToString();

				// We've hit the final digit of the model number and z=0
				// therefore this is a valid number
				if (digitNum == 13 && localZ == 0)
				{
					Console.WriteLine("Found valid number: " + newNumber);
					validNumbers.Add(newNumber);
					continue;
				}

				// Recurse into the next digit place
				RecurseFindValidModelNumbers(digitNum + 1, localZ, zCap, ref parameters, newNumber, ref validNumbers);
			}
		}

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			// Parse input into 14 sets of instructions
			int digitIndex = 0;
			List<string>[] instructions = new List<string>[14];
			List<string> digitInstructions = new List<string>();
			foreach (var line in lines)
			{
				if (line.StartsWith("inp"))
				{
					if (digitInstructions?.Count > 0)
					{
						instructions[digitIndex] = digitInstructions;
						digitIndex++;
					}
					digitInstructions = new List<string>();
				}
				digitInstructions.Add(line);
			}
			instructions[digitIndex] = digitInstructions;

			// Extract the 3 parameter differences between the sets - the rest of the instructions are identical between digit places
			int[,] parameters = new int[14, 3];
			for (int i = 0; i < instructions.Length; ++i)
			{
				parameters[i, 0] = int.Parse(instructions[i][4].Split(' ')[2]);
				parameters[i, 1] = int.Parse(instructions[i][5].Split(' ')[2]);
				parameters[i, 2] = int.Parse(instructions[i][15].Split(' ')[2]);
			}

			// Recurse through all 14 digit permutations with 1-9 in each digit place
			// and maintain a list of valid numbers found
			List<string> validNumbers = new List<string>();
			long z = 0;

			// There are 7 divisors of 26 in the instructions and 7 divisors of 1
			// Total product of all divisors is therefore 26^7.
			// We can use this information to early out on some recursive paths if the z value is greater than this amount
			long zCap = (long)Math.Pow(26, 7);
			RecurseFindValidModelNumbers(0, z, zCap, ref parameters, "", ref validNumbers);

			// Sort from lowest number to highest
			validNumbers.Sort();

			// Part One - Highest valid model number
			{
				Console.WriteLine("\nPart One:");

				// Just to confirm that this is a valid number by manually reading through instructions
				ALU testAlu = new ALU(validNumbers.Last(), 0);
				for (int i = 0; i < 14; ++i)
					testAlu.ReadInstructionAlt(parameters[i, 0], parameters[i, 1], parameters[i, 2]);

				if (testAlu.GetZ() == 0)
					Console.WriteLine("Largest valid 14-digit model number is: " + validNumbers.Last());
			}

			// Part Two - Lowest valid model number
			{
				Console.WriteLine("\nPart Two:");

				// Just to confirm that this is a valid number by manually reading through instructions
				ALU testAlu = new ALU(validNumbers.First(), 0);
				for (int i = 0; i < 14; ++i)
					testAlu.ReadInstructionAlt(parameters[i, 0], parameters[i, 1], parameters[i, 2]);

				if (testAlu.GetZ() == 0)
					Console.WriteLine("Smallest valid 14-digit model number is: " + validNumbers.First());
			}
		}
	}
}
