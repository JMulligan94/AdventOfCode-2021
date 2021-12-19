using System;
using System.IO;

namespace _18_Snailfish
{
	class NumberNode
	{
		public NumberNode parent;

		public NumberNode left;
		public NumberNode right;

		public int value;

		public NumberNode(NumberNode _parent, int _value)
		{
			parent = _parent;
			value = _value;
		}

		public NumberNode(NumberNode lhs, NumberNode rhs)
		{
			left = lhs;
			lhs.parent = this;
			right = rhs;
			rhs.parent = this;
		}

		public NumberNode(string line, ref int readIndex, NumberNode parentNode)
		{
			parent = parentNode;

			// Pare from string into SnailfishNumber
			// Is a pair if starts with [
			if (line[readIndex] == '[')
			{
				readIndex++;
				left = new NumberNode(line, ref readIndex, this);
				if (line[readIndex] == ',')
				{
					readIndex++;
					right = new NumberNode(line, ref readIndex, this);
					readIndex++; // Past the ']'
				}
			}
			else
			{
				string valueStr = "";
				while (line[readIndex] != ',' && line[readIndex] != ']')
				{
					valueStr += line[readIndex];
					readIndex++;
				}
				value = int.Parse(valueStr);
			}
		}

		public static NumberNode operator +(NumberNode lhs, NumberNode rhs)
		{
			return new NumberNode(lhs, rhs);
		}

		public bool IsPairNode() { return left != null && right != null; }
		public void ReduceNumber()
		{
			// Until no more exploding or splitting needs doing
			while (true)
			{
				bool exploded = HasExploded(0);
				bool split = !exploded && HasSplit();
				/*
				if (exploded)
					Console.WriteLine("after explode:\t" + this);
				
				if (split)
					Console.WriteLine("after split:\t" + this);
				*/
				if (!exploded && !split)
					break;
			}
		}

		public void SplitNumber()
		{
			// Left element = value / 2 rounded down
			left = new NumberNode(this, value / 2);

			// Right element = value / 2 rounded up
			right = new NumberNode(this, (int)Math.Ceiling(value / 2.0f));

			value = 0;
		}

		public void ExplodeNumber()
		{
			NumberNode leftNumber = FindImmediateLeftNode();
			if (leftNumber != null)
				leftNumber.value += left.value;

			NumberNode rightNumber = FindImmediateRightNode();
			if (rightNumber != null)
				rightNumber.value += right.value;

			// This pair becomes 0
			if (parent.left == this)
			{
				parent.left = null;
				parent.left = new NumberNode(parent, 0);
			}
			else
			{
				parent.right = null;
				parent.right = new NumberNode(parent, 0);
			}
		}

		public NumberNode FindImmediateLeftNode()
		{
			if (parent?.parent == null)
				return null;

			// Traverse up to parent to then go right on
			NumberNode traverseNode = this;
			while (true)
			{
				NumberNode nextNode = traverseNode.parent;
				if (nextNode == null)
					return null;

				if (nextNode.left == traverseNode)
				{
					traverseNode = nextNode;
					continue;
				}
				else
				{
					traverseNode = nextNode;
					break;
				}
			}

			// Found common parent - go far down to the right
			NumberNode immediateLeftNode = traverseNode.left;
			while (immediateLeftNode.left != null)
				immediateLeftNode = immediateLeftNode.right;

			return immediateLeftNode;
		}

		public NumberNode FindImmediateRightNode()
		{
			if (parent?.parent == null)
				return null;

			// Traverse up to parent to then go left on
			NumberNode traverseNode = this;
			while (true)
			{
				NumberNode nextNode = traverseNode.parent;
				if (nextNode == null)
					return null;

				if (nextNode.right == traverseNode)
				{
					traverseNode = nextNode;
					continue;
				}
				else
				{
					traverseNode = nextNode;
					break;
				}
			}

			// Found common parent - go far down to the left
			NumberNode immediateRightNode = traverseNode.right;
			while (immediateRightNode.left != null)
				immediateRightNode = immediateRightNode.left;

			return immediateRightNode;
		}

		public bool HasSplit()
		{
			// If any regular number is 10 or greater, the left number splits
			if (value >= 10 && !IsPairNode())
			{
				SplitNumber();
				return true;
			}

			if (left?.HasSplit() ?? false)
				return true;

			if (right?.HasSplit() ?? false)
				return true;

			return false;
		}

		public bool HasExploded(int depth)
		{
			// If any pair is nested inside four parts, the leftmost pair explodes
			if (depth == 4 && IsPairNode())
			{
				ExplodeNumber();
				return true;
			}

			if (left?.HasExploded(depth+1) ?? false)
				return true;

			if (right?.HasExploded(depth+1) ?? false)
				return true;

			return false;
		}

		public UInt64 CalculateMagnitude()
		{
			// The magnitude of a pair is 3 times the magnitude of its left element plus 2 times the magnitude of its right element.
			// The magnitude of a regular number is just that number.
			UInt64 magnitude = 0;
			if (IsPairNode())
			{
				magnitude += left.CalculateMagnitude() * 3;
				magnitude += right.CalculateMagnitude() * 2;
			}
			else
			{
				magnitude += (UInt64)value;
			}
			return magnitude;
		}

		public override string ToString()
		{
			if (left == null && right == null)
				return value.ToString();

			return ("[" + left + "," + right + "]");
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			// Part One
			{
				Console.WriteLine("\nParsing:\t" + lines[0]);
				int lineIndex = 0;
				NumberNode sumNumber = new NumberNode(lines[0], ref lineIndex, null);
				Console.WriteLine("Parsed into:\t" + sumNumber);

				for (int i = 1; i < lines.Length; ++i)
				{
					var line = lines[i];

					Console.WriteLine("\nParsing:\t" + line);
					lineIndex = 0;
					NumberNode number = new NumberNode(line, ref lineIndex, null);

					Console.WriteLine("Parsed into:\t" + number);

					sumNumber += number;
					Console.WriteLine("\nafter addition:\t" + sumNumber);

					sumNumber.ReduceNumber();
					Console.WriteLine("\nReduced to:\t" + sumNumber);
				}

				Console.WriteLine("\nPart One:");
				Console.WriteLine("Magnitude of final number is: " + sumNumber.CalculateMagnitude());
			}

			// Part Two - Find combination of numbers that produces greatest magnitude
			{
				int largestXIndex = 0;
				int largestYIndex = 0;
				UInt64 largestMagnitude = 0;

				for (int x = 0; x < lines.Length; ++x)
				{
					var lineX = lines[x];
					for (int y = 0; y < lines.Length; ++y)
					{
						if (x == y)
							continue;

						int lineIndex = 0;
						NumberNode xNumber = new NumberNode(lineX, ref lineIndex, null);

						var lineY = lines[y];
						lineIndex = 0;
						NumberNode yNumber = new NumberNode(lineY, ref lineIndex, null);

						NumberNode sumNumber = xNumber + yNumber;
						sumNumber.ReduceNumber();

						UInt64 magnitude = sumNumber.CalculateMagnitude();
						if (magnitude > largestMagnitude)
						{
							largestXIndex = x;
							largestYIndex = y;
							largestMagnitude = magnitude;
						}
					}
				}

				Console.WriteLine("\nPart Two:");
				Console.WriteLine("Largest magnitude is from (x,y): (" + largestXIndex + "," + largestYIndex + ")");
				Console.WriteLine("Largest magnitude is: " + largestMagnitude);

			}
		}
	}
}
