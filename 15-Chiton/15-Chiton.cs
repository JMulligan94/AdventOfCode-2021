using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _15_Chiton
{
	class Map
	{
		public class Node
		{
			public int row;
			public int col;
			public int cost;
			public bool visited; // Has this node been visited?

			public int score;	 // The score for the distance between this node and the start node so far
			public Node scoreFrom; // The node that the above score was from

			public bool partOfOptimalPath; // Is this node part of the optimal path between start and end nodes

			public Node(int _row, int _col, int _cost)
			{
				row = _row;
				col = _col;
				cost = _cost;

				score = int.MaxValue;
				visited = false;
			}

			public override string ToString()
			{
				return "(" + row + "," + col + "): " + cost;
			}
		}

		int height;
		int width;
		Node[,] nodes;

		Node startNode;
		Node goalNode;

		public Map(int _height, int _width, int[,] riskLevels)
		{
			height = _height;
			width = _width;

			nodes = new Node[height, width];
			for(int row = 0; row < height; ++row)
			{
				for (int col = 0; col < width; ++col)
				{
					nodes[row, col] = new Node(row, col, riskLevels[row, col]);
				}
			}
		}

		public void SetStart(int row, int col)
		{
			nodes[row, col].visited = true;
			nodes[row, col].score = 0;
			startNode = nodes[row, col];
		}

		public void SetGoal(int row, int col)
		{
			goalNode = nodes[row, col];
		}

		private void CheckNeighbour(Node current, ref Node neighbour, ref List<Node> unvisitedNodes)
		{
			// Score it has taken to get to this node
			int newScore = neighbour.cost + current.score;

			// If the way we got here was faster than what the node has experienced before
			// Overwrite the data in it so its linked to this current one
			if (neighbour.score > newScore)
			{
				neighbour.score = newScore;
				neighbour.scoreFrom = current;
			}

			// And if the neighbour hasn't been checked before, add it into the list of nodes to visit in the future
			if (!neighbour.visited)
			{
				if (!unvisitedNodes.Contains(neighbour))
				{
					unvisitedNodes.Add(neighbour);
				}
			}
		}

		public void FindPath(ref List<Node> path)
		{
			// List of nodes that we haven't checked yet (ordered by score from start node)
			List<Node> unvisitedNodes = new List<Node>();
			unvisitedNodes.Add(startNode);

			while (unvisitedNodes.Count > 0)
			{
				// Grab the cheapest node from the start so far
				Node currentNode = unvisitedNodes[0];

				// If we've hit the end node, we've found the fastest path!
				if (currentNode == goalNode)
					break;

				int row = currentNode.row;
				int col = currentNode.col;

				// Check each neighbour
				if (row != 0)
				{
					// North neighbour
					Node northNode = nodes[row-1, col];
					CheckNeighbour(currentNode, ref northNode, ref unvisitedNodes);
				}

				if (row != height-1)
				{
					// South neighbour
					Node southNode = nodes[row+1, col];
					CheckNeighbour(currentNode, ref southNode, ref unvisitedNodes);
				}

				if (col != 0)
				{
					// West neighbour
					Node westNode = nodes[row, col-1];
					CheckNeighbour(currentNode, ref westNode, ref unvisitedNodes);
				}
				

				if (col != width-1)
				{
					// East neighbour
					Node eastNode = nodes[row, col+1];
					CheckNeighbour(currentNode, ref eastNode, ref unvisitedNodes);
				}

				// Now we've checked this nodes neighbours, mark this as visited so we don't add it back to the list of nodes to check
				currentNode.visited = true;

				// Remove from list of nodes
				unvisitedNodes.RemoveAt(0);

				// Sort the array of unvisited nodes by the score, so we're grabbing the shortest path next time
				unvisitedNodes.Sort((x, y) => x.score.CompareTo(y.score));
			}

			// Trace back from goalNode to determine path
			path = new List<Node>();
			Node previousNode = goalNode;
			while (previousNode != null)
			{
				path.Insert(0, previousNode);
				previousNode.partOfOptimalPath = true;
				previousNode = previousNode.scoreFrom;
			}
		}

		public void PrintMap()
		{
			Console.WriteLine("=== Map ===");

			for (int row = 0; row < height; ++row)
			{
				string rowStr = "";
				for (int col = 0; col < width; ++col)
				{
					rowStr += nodes[row, col].partOfOptimalPath
						? "[" + nodes[row, col].cost + "]"
						: "." + nodes[row, col].cost + ".";
				}
				Console.WriteLine(rowStr);
			}
		}
	}

	class Program
	{
		// Dijkstra's Algorithm = https://www.redblobgames.com/pathfinding/a-star/introduction.html
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");
			
			// Part One - Find optimal path cost from top left to bottom right on input
			{
				int height = lines.Length;
				int width = lines[0].Length;

				// Parse input
				int[,] costs = new int[lines.Length, lines[0].Length];
				for (int row = 0; row < lines.Length; ++row)
				{
					for (int col = 0; col < lines[row].Length; ++col)
					{
						costs[row, col] = int.Parse(lines[row][col] + "");
					}
				}

				// Create map of nodes
				Map riskLevelMap = new Map(height, width, costs);

				// Set start and goal nodes
				riskLevelMap.SetStart(0, 0);
				riskLevelMap.SetGoal(height - 1, width - 1);

				// Find shortest path
				List<Map.Node> path = new List<Map.Node>();
				riskLevelMap.FindPath(ref path);

				riskLevelMap.PrintMap();

				Console.WriteLine("Part One:");
				Console.WriteLine("The cost of the path is: " + path.Last().score);
			}

			// Part Two - Find optimal path from top left to bottom right using input as a 1x1 tile in a 5x5 grid
			// where risk increases per tile
			{
				int tileHeight = lines.Length;
				int tileWidth = lines[0].Length;

				int height = tileHeight * 5;
				int width = tileWidth * 5;

				int[,] costs = new int[height, width];

				// Grow 5x5 map from single 1x1 tile horizontally while reading in tile
				for (int row = 0; row < tileHeight; ++row)
				{
					for (int col = 0; col < tileWidth; ++col)
					{
						int risk = int.Parse(lines[row][col] + "");
						for (int i = 0; i < 5; ++i)
						{
							costs[row, (tileWidth * i) + col] = risk;

							risk++;
							if (risk > 9)
								risk = 1; // Wrap around to 1 once we hit 10
						}
					}
				}

				// Grow 5x5 map from single 1x1 tile vertically
				for (int col = 0; col < width; ++col)
				{
					for (int row = 0; row < tileHeight; ++row)
					{
						int risk = costs[row,col];
						for (int i = 1; i < 5; ++i)
						{
							risk++;
							if (risk > 9)
								risk = 1; // Wrap around to 1 once we hit 10

							costs[row + (tileHeight * i), col] = risk;
						}
					}
				}

				Map riskLevelMap = new Map(height, width, costs);
				riskLevelMap.PrintMap();

				// Set start and goal nodes
				riskLevelMap.SetStart(0, 0);
				riskLevelMap.SetGoal(height - 1, width - 1);

				// Find shortest path
				List<Map.Node> path = new List<Map.Node>();
				riskLevelMap.FindPath(ref path);

				riskLevelMap.PrintMap();

				Console.WriteLine("\nPart Two:");
				Console.WriteLine("The cost of the path is: " + path.Last().score);
			}
		}
	}
}
