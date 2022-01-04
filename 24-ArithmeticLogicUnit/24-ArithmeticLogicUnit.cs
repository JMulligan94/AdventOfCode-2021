using System;
using System.Collections.Generic;
using System.IO;

namespace _24_ArithmeticLogicUnit
{
	class ALU
	{
		string modelNumber;
		int[] values = new int[4] { 0,0,0,0 };
		int readIndex = 0;

		public ALU(string _modelNumber, int initZ)
		{
			modelNumber = _modelNumber;
			values[GetValueIndex('z')] = initZ;
		}

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

		public int GetZ() { return values[GetValueIndex('z')]; }

		private int GetValueIndex(char valueChar)
		{
			return valueChar - 'w';
		}
	}


	class Program
	{
		static bool RecurseFindInitialZForTargetZ(int targetZ, int modelNumberDigit, ref List<string>[] instructions)
		{
			if (modelNumberDigit == 0)
			{
				// Found valid model number
				return true;
			}

			Console.WriteLine("\n=== Checking digit " + modelNumberDigit + " for z=" + targetZ + " ===");

			// Try every digit from 9 to 1 (0 is not allowed in the model number)
			for (int digitToCheck = 9; digitToCheck >= 1; --digitToCheck)
			{
				// Brute force the z value needed to be passed in to get the target z value for this digit
				for (int z = 0; z < 100; ++z)
				{
					ALU alu = new ALU(digitToCheck.ToString(), z);
					foreach (var line in instructions[modelNumberDigit-1])
					{
						alu.ReadInstruction(line);
					}

					if (alu.GetZ() == targetZ)
					{
						Console.WriteLine("Digit " + modelNumberDigit + ": '" + digitToCheck + "' with z: " + z + " - valid");

						if (RecurseFindInitialZForTargetZ(z, modelNumberDigit - 1, ref instructions))
						{
							return true;
						}
					}
				}
			}

			Console.WriteLine("=== Couldn't find anything for digit " + modelNumberDigit + " for z=" + targetZ + " ===");
			return false;
		}

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");
			int digitIndex = 0;
			List<string>[] instructions = new List<string>[14];

			List<string> digitInstructions = new List<string>();
			foreach(var line in lines)
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

			RecurseFindInitialZForTargetZ(0, 14, ref instructions);

			// Part One
			{
				Console.WriteLine("\nPart One:");
				Console.WriteLine("Largest valid 14-digit model number is: " );
			}
		}
	}
}
