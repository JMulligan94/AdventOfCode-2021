using System;
using System.IO;

namespace _02_Dive_
{
	enum Instruction
	{
		Forward,
		Up,
		Down
	}
	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			// Create instruction array
			Tuple<Instruction, int>[] instructions = new Tuple<Instruction, int>[lines.Length];
			for(int i = 0; i < lines.Length; ++i)
			{
				var tokens = lines[i].Split(' ');
				Instruction instruction = tokens[0] == "forward" ? Instruction.Forward : (tokens[0] == "down" ? Instruction.Down : Instruction.Up);

				instructions[i] = new Tuple<Instruction, int>(instruction, int.Parse(tokens[1]));
			}

			// Part One
			{
				/*
					- forward X increases the horizontal position by X units.
					- down X increases the depth by X units.
					- up X decreases the depth by X units.
				*/

				int xPos = 0;
				int yPos = 0;

				// Iterate instructions to change horizontal position and depth of submarine
				foreach (var instruction in instructions)
				{
					switch(instruction.Item1)
					{
						case Instruction.Forward:
							{
								xPos += instruction.Item2;
							}
							break;
						case Instruction.Down:
							{
								yPos += instruction.Item2;
							}
							break;
						case Instruction.Up:
							{
								yPos -= instruction.Item2;
							}
							break;
					}
				}

				Console.WriteLine("Part One:");
				Console.WriteLine("End co-ordinates = " + xPos + "," + yPos);
				Console.WriteLine("Product is: " + (xPos * yPos) + "\n");
				
			}
			// Part Two
			{
				/*
					- down X increases your aim by X units.
					- up X decreases your aim by X units.
					- forward X does two things:
						- It increases your horizontal position by X units.
						- It increases your depth by your aim multiplied by X.
				 */

				int xPos = 0;
				int yPos = 0;
				int aim = 0;

				// Iterate instructions to change horizontal position and depth of submarine
				foreach (var instruction in instructions)
				{
					switch (instruction.Item1)
					{
						case Instruction.Forward:
							{
								xPos += instruction.Item2;
								yPos += (aim * instruction.Item2);
							}
							break;
						case Instruction.Down:
							{
								aim += instruction.Item2;
							}
							break;
						case Instruction.Up:
							{
								aim -= instruction.Item2;
							}
							break;
					}
				}

				Console.WriteLine("Part Two:");
				Console.WriteLine("Aim = " + aim);
				Console.WriteLine("End co-ordinates = " + xPos + "," + yPos);
				Console.WriteLine("Product is: " + (xPos * yPos));

			}
		}
	}
}
