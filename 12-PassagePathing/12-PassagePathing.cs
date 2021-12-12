using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _12_PassagePathing
{
	// Class to hold a route of caves
	class Route
	{
		// String representation of caves visited so far (delimited by commas)
		public string routeStr = "";

		// Has this route already visited a small cave twice? (We're allowed one revisit in Part 2)
		public bool hasRevisitedSmallCave = false;

		public Route(string route, bool _hasRevisited = false)
		{
			routeStr = route;
			hasRevisitedSmallCave = _hasRevisited;
		}

		// Add a cave to the route (append string with comma and cave name)
		public void AddToRoute(Cave cave)
		{
			routeStr += "," + cave.name;
		}

		// Create and return a full copy of this route
		public Route Clone()
		{
			return new Route(routeStr, hasRevisitedSmallCave);
		}

		// Has this route seen this cave before?
		public bool HasBeenTo(Cave cave)
		{
			return routeStr.Contains("," + cave.name + ",");
		}

		// Is this route allowed to visit this cave?
		public bool CanVisit(Cave cave, bool checkRevisits)
		{
			// Always allowed to visit big caves 
			if (cave.isBigCave)
				return true;

			// Never allowed to visit start cave again
			if (cave.name == "start")
				return false;

			// This must be a small cave
			// we can't go back/add to route if we've already been there before
			// UNLESS we're allowed one revisit
			if (checkRevisits)
			{
				// We're allowed a revisit this small cave (even if we've been here before) if we haven't revisited any small caves yet
				if (!hasRevisitedSmallCave)
				{
					return true;
				}
			}

			// Otherwise, we can only visit this cave if we haven't been here before
			return !HasBeenTo(cave);
		}

		public bool IsValidRoute()
		{
			return routeStr.EndsWith("end");
		}

		public override string ToString()
		{
			return routeStr;
		}
	}

	// Class to hold information for a single cave
	class Cave
	{
		public int id; // unique int id
		public string name; // Name of cave (determines big or small cave)
		public List<Cave> connectedTo = new List<Cave>(); // Which caves is this connected to
		public bool isBigCave; // Is this a big cave (i.e. can we revisit this cave any time we want?)

		public Cave(string _name)
		{
			id = s_uniqueId++;
			name = _name;
			isBigCave = _name[0] >= 'A' && _name[0] <= 'Z'; // Is a big cave if name is in capitals
		}

		// Find all routes from this cave, using the information from the route that was taken to get here
		public void FindRoutes(ref Route currentRoute, ref List<Route> routes, bool allowOneRevisit)
		{
			// Check if we've hit the end of the cave network
			if (name == "end")
			{
				return;
			}

			// Get all valid caves to iterate over
			List<Cave> adjoinedCaves = new List<Cave>();
			GetVisitableAdjoiningCaves(ref currentRoute, ref adjoinedCaves, allowOneRevisit);

			//Console.WriteLine("From " + currentRoute + ": ");
			// Create a new route with each new cave appended
			foreach (var cave in adjoinedCaves)
			{
				// Create a copy of the route that was taken to get here and append
				Route newRoute = currentRoute.Clone();
				newRoute.AddToRoute(cave);
				
				// Check if we need to mark this route as having revisited a small cave if we'd already visited this small cave once before
				if (!cave.isBigCave && currentRoute.HasBeenTo(cave))
					newRoute.hasRevisitedSmallCave = true;
				
				//Console.WriteLine("    Cave " + cave.name + "...");
				
				// Recurse with this connected cave and search the cave network from there
				cave.FindRoutes(ref newRoute, ref routes, allowOneRevisit);

				// Add to routes through the cave network
				routes.Add(newRoute);
			}
		}

		// Create a list of all visitable adjoining caves
		public void GetVisitableAdjoiningCaves(ref Route route, ref List<Cave> connectedCaves, bool allowOneRevisit)
		{
			foreach (var cave in connectedTo)
			{
				if (route.CanVisit(cave, allowOneRevisit))
				{
					connectedCaves.Add(cave);
				}
			}
		}

		private static int s_uniqueId = 0;

		public override string ToString()
		{
			return id + ": " + name;
		}
	}

	class Program
	{

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			// Create map of caves with their names as key
			Dictionary<string, Cave> caves = new Dictionary<string, Cave>();
			foreach(var line in lines)
			{
				var connection = line.Split('-');
				string from = connection[0];
				string to = connection[1];

				if (!caves.ContainsKey(from))
					caves.Add(from, new Cave(from));

				if (!caves.ContainsKey(to))
					caves.Add(to, new Cave(to));

				caves[from].connectedTo.Add(caves[to]);
				caves[to].connectedTo.Add(caves[from]);
			}

			Cave startCave = caves["start"];
			Route startRoute = new Route("start");

			// Part One
			{
				// Get a list of visitable caves from the start cave
				List<Cave> adjoinedCaves = new List<Cave>();
				startCave.GetVisitableAdjoiningCaves(ref startRoute, ref adjoinedCaves, false);

				// Get list of valid routes through the cave network
				List<Route> routes = new List<Route>();
				startCave.FindRoutes(ref startRoute, ref routes, false);

				// Remove any routes that don't stop at the end of the network
				for (int i = routes.Count - 1; i >= 0; --i)
				{
					if (!routes[i].IsValidRoute())
						routes.RemoveAt(i);
				}

				Console.WriteLine("\nPart One:");

				foreach (var route in routes)
				{
					Console.WriteLine(route);
				}

				Console.WriteLine("The amount of valid routes is: " + routes.Count);
			}

			// Part Two
			{
				// Get a list of visitable caves from the start cave
				List<Cave> adjoinedCaves = new List<Cave>();
				startCave.GetVisitableAdjoiningCaves(ref startRoute, ref adjoinedCaves, true);

				// Get list of valid routes through the cave network
				List<Route> routes = new List<Route>();
				startCave.FindRoutes(ref startRoute, ref routes, true);

				// Remove any routes that don't stop at the end of the network
				for (int i = routes.Count - 1; i >= 0; --i)
				{
					if (!routes[i].IsValidRoute())
						routes.RemoveAt(i);
				}

				Console.WriteLine("\nPart Two:");

				foreach (var route in routes)
				{
					Console.WriteLine(route);
				}

				Console.WriteLine("The amount of valid routes (allowing 1 revisit) is: " + routes.Count);
			}
		}
	}
}
