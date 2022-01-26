using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _19_BeaconScanner
{
	class Point : IEquatable<Point> // IEquatable for Linq's Distinct() function
	{
		public int x;
		public int y;
		public int z;

		public long DistanceHash
		{
			get 
			{
				var (xSqr, ySqr, zSqr) = ((x * x), (y * y), (z * z));
				return xSqr + ySqr + zSqr;
			}
		}

		public Point(int _x, int _y, int _z)
		{
			x = _x;
			y = _y;
			z = _z;
		}

		public override bool Equals(object obj)
		{
			return obj is Point other 
				&& Equals(other);
		}

		public override string ToString()
		{
			return $"({x},{y},{z})";
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(x, y, z);
		}

		public bool Equals(Point other)
		{
			return x == other.x &&
				   y == other.y &&
				   z == other.z;
		}

		public static Point operator +(Point a, Point b)
		{
			return new Point(b.x + a.x, b.y + a.y, b.z + a.z);
		}

		public static Point operator -(Point a, Point b)
		{
			return new Point(b.x - a.x, b.y - a.y, b.z - a.z);
		}

	}

	class Beacon
	{
		public int id;
		Scanner owningScanner;
		bool is2D;
		public Point localLocation;
		public List<long> siblingDistances = new List<long>();

		public Point WorldLocation
		{
			get
			{
				if (is2D)
				{
					return Beacon.Rotate_2D(localLocation, owningScanner.worldRotation) + owningScanner.worldCentre;
				}
				else
				{
					return Beacon.Rotate(localLocation, owningScanner.worldRotation) + owningScanner.worldCentre;
				}
			}
		}

		public Beacon(Scanner _owningScanner, int _id, Point location, bool _is2D = false)
		{
			owningScanner = _owningScanner;
			id = _id;
			is2D = _is2D;
			localLocation = location;
		}

		// All 24 possible rotations of a point in 3D space
		public static Point Rotate(Point vectorIn, int rotation)
		{
			int x = vectorIn.x;
			int y = vectorIn.y;
			int z = vectorIn.z;

			switch (rotation)
			{
				// "up" is +y (up)
				case 0: return vectorIn;
				case 1: return new Point(-z, y, x);
				case 2: return new Point(-x, y, -z);
				case 3: return new Point(z, y, -x);

				// "up" is -y (down)
				case 4: return new Point(-x, -y, z);
				case 5: return new Point(-z, -y, -x);
				case 6: return new Point(x, -y, -z);
				case 7: return new Point(z, -y, x);

				// "up" is +x (right)
				case 8: return new Point(y, -x, z);
				case 9: return new Point(y, -z, -x);
				case 10: return new Point(y, x, -z);
				case 11: return new Point(y, z, x);

				// "up" is -x (left)
				case 12: return new Point(-y, x, z);
				case 13: return new Point(-y, -z, x);
				case 14: return new Point(-y, -x, -z);
				case 15: return new Point(-y, z, -x);

				// "up" is -z (front)
				case 16: return new Point(x, z, -y);
				case 17: return new Point(z, -x, -y);
				case 18: return new Point(-x, -z, -y);
				case 19: return new Point(-z, x, -y);

				// "up" is +z (back)
				case 20: return new Point(x, -z, y);
				case 21: return new Point(-z, -x, y);
				case 22: return new Point(-x, z, y);
				case 23: return new Point(z, x, y);

				default: return vectorIn;
			}
		}

		// All 4 possible rotation of a point in 2D space
		public static Point Rotate_2D(Point vectorIn, int rotation)
		{
			int x = vectorIn.x;
			int y = vectorIn.y;
			int z = vectorIn.z;

			switch (rotation)
			{
				case 0: return vectorIn;
				case 1: return new Point(y, -x, z);
				case 2: return new Point(-x, -y, z);
				case 3: return new Point(-y, x, z);
			
				default: return vectorIn;
			}
		}

		public override string ToString()
		{
			return $"{{({owningScanner.name}-{id})-{localLocation}}}";
		}
	}
	class Scanner
	{
		public string name;
		public bool is2D;

		public Point worldCentre = new Point(0,0,0);
		public int worldRotation = 0;
		public List<Beacon> beacons = new List<Beacon>();

		public int NumOrientations
		{
			get { return is2D ? 4 : 24; }
		}

		public Scanner(string _name, bool _is2D = false)
		{
			name = _name;
			is2D = _is2D;
		}

		public void AddBeacon(Point location)
		{
			beacons.Add(new Beacon(this, beacons.Count, location, is2D));
		}

		public override string ToString()
		{
			return $"--- Scanner {name} ---";
		}

		public void CalculateDistancesBetweenBeacons()
		{
			// Calculate the distances between sibling beacons in a scanner once
			for (int beaconAIndex = 0; beaconAIndex < beacons.Count - 1; ++beaconAIndex)
			{
				Beacon beaconA = beacons[beaconAIndex];
				for (int beaconBIndex = beaconAIndex + 1; beaconBIndex < beacons.Count; ++beaconBIndex)
				{
					Beacon beaconB = beacons[beaconBIndex];
					Point beaconDiff = beaconB.localLocation - beaconA.localLocation;
					long distance = beaconDiff.DistanceHash;
					beaconA.siblingDistances.Add(distance);
					beaconB.siblingDistances.Add(distance);
				}
			}
		}
	}

	class Program
	{
		// Using knownScanner, attempt to find the queryScanner's location and rotation
		private static bool LocateScanner(Scanner knownScanner, Scanner queryScanner, bool is2D)
		{
			// for 2D - min overlap of 12 before we consider it to be the same beacon
			int minOverlapNeeded = is2D ? 3 : 12;

			List<(Beacon beaconA, Beacon beaconB)> overlappingBeacons = new List<(Beacon beaconA, Beacon beaconB)>();

			// Compare the distances between sibling beacons for both scanners. 
			// If we find there are many matches in distances in a beacon pair - its very likely that both are the same beacon
			//  since beacon distance won't change no matter what rotation or location we're at
			foreach (Beacon knownBeacon in knownScanner.beacons)
			{
				var knownDistances = knownBeacon.siblingDistances;
				foreach(Beacon queryBeacon in queryScanner.beacons)
				{
					// How many distances do both have in common?
					int currentOverlap = 0;
					foreach (var distance in knownDistances)
					{
						if (queryBeacon.siblingDistances.Contains(distance))
							currentOverlap++;
					}

					if (currentOverlap >= (minOverlapNeeded - 1))
					{
						// Very likely to be the same beacon - just at different locations/rotations
						Console.WriteLine($"{knownBeacon}->{queryBeacon} => {currentOverlap}");
						overlappingBeacons.Add((knownBeacon, queryBeacon));
						break;
					}
				}
			}

			// If we found enough beacons that can be potentially paired up, we can start calculating the location/rotation of the unknown scanner
			if (overlappingBeacons.Count >= minOverlapNeeded)
			{
				// Get the relative distance between the first two matching beacons in the located scanner
				Beacon srcBeaconA = overlappingBeacons[0].beaconA;
				Beacon destBeaconA = overlappingBeacons[1].beaconA;
				Point relDistanceA = destBeaconA.WorldLocation - srcBeaconA.WorldLocation;

				// Compare to the relative distance between the first two matching beacons in the unknown scanner
				Beacon srcBeaconB = overlappingBeacons[0].beaconB;
				Beacon destBeaconB = overlappingBeacons[1].beaconB;
				Point relDistanceB = destBeaconB.WorldLocation - srcBeaconB.WorldLocation;

				// Find orientation and then translation
				int correctRotation = -1;
				
				// How do we have to rotate the relative distance B to equal the relative distance A
				for (int rotation = 0; rotation < knownScanner.NumOrientations; ++rotation)
				{
					// Rotate distance by an amount
					Point rotatedDistanceB = is2D ? 
						Beacon.Rotate_2D(relDistanceB, rotation) 
						: Beacon.Rotate(relDistanceB, rotation);

					if (rotatedDistanceB.Equals(relDistanceA))
					{
						// Rotation found! Both relative distances are now the same!
						correctRotation = rotation;
						break;
					}
				}
				
				if (correctRotation == -1) // ERROR - No rotation found
					return false;

				// Calculate world centre in relation to original located scanner using the rotation that was just calculated
				queryScanner.worldCentre = is2D ?
					Beacon.Rotate_2D(srcBeaconB.WorldLocation, correctRotation) - srcBeaconA.WorldLocation
					: Beacon.Rotate(srcBeaconB.WorldLocation, correctRotation) - srcBeaconA.WorldLocation;

				queryScanner.worldRotation = correctRotation;

				return true;
			}

			// Not enough matches found for this scanner to be considered overlapping
			return false;
		}

		static void Main(string[] args)
		{
			var inputFile = "input.txt";
			var lines = File.ReadAllLines(inputFile);
			var is2D = inputFile == "testA.txt";

			// Parse in input file creating a list of "unknown" scanners and the list of beacons
			List<Scanner> unknownScanners = new List<Scanner>();
			bool newScannerInfo = true;
			Scanner currentScanner = null;
			foreach (var line in lines)
			{
				if (newScannerInfo)
				{
					var idLineTokens = line.Split(' ');
					currentScanner = new Scanner(idLineTokens[2], is2D);
					newScannerInfo = false;

					unknownScanners.Add(currentScanner);
					continue;
				}
				if (line == "")
				{
					newScannerInfo = true;
					continue;
				}
				else
				{
					var tokens = line.Split(',');
					if (is2D)
						currentScanner.AddBeacon(new Point(int.Parse(tokens[0]), int.Parse(tokens[1]), 0));
					else
						currentScanner.AddBeacon(new Point(int.Parse(tokens[0]), int.Parse(tokens[1]), int.Parse(tokens[2])));
				}
			}

			// Calculate the distance between each set of beacons for each of the scanners
			foreach (Scanner scanner in unknownScanners)
				scanner.CalculateDistancesBetweenBeacons();

			// A queue of unknown scanners - once this is empty, all should have been located
			Queue<Scanner> unknownScannersQueue = new Queue<Scanner>(unknownScanners);

			// Start by taking the first one, we'll say we've located this with rotation 0 and centre point 0,0,0
			// This will be used as the absolute origin in position and rotation for all other scanners
			Scanner startScanner = unknownScannersQueue.Dequeue();

			List<Scanner> locatedScanners = new List<Scanner>();
			startScanner.worldCentre = new Point(0, 0, 0);
			startScanner.worldRotation = 0;
			locatedScanners.Add(startScanner);

			unknownScanners.Remove(startScanner);

			// Until all scanners have been located...
			while (unknownScannersQueue.Count > 0)
			{
				Scanner unknownScanner = unknownScannersQueue.Dequeue();
				bool foundScanner = false;

				// Try to use each scanner we DO know the location and rotation of to find more scanner locations
				foreach (Scanner locatedScanner in locatedScanners)
				{
					// Try to locate the unknown scanner using this known one
					if (LocateScanner(locatedScanner, unknownScanner, is2D))
					{
						// Found the scanner relative to a previously located one - add to located scanners list to help us find more unknown scanners
						Console.WriteLine($"===Found Scanner {unknownScanner.name} using Scanner {locatedScanner.name} - calculated centre of {unknownScanner.worldCentre} and rotation {unknownScanner.worldRotation}===\n");
						
						foundScanner = true;

						locatedScanners.Add(unknownScanner);
						unknownScanners.Remove(unknownScanner);

						break;
					}
				}

				if (!foundScanner)
				{
					// Couldn't find overlap with any currently located scanners
					// Add it back to the bottom of the queue to try again later when more have been located
					unknownScannersQueue.Enqueue(unknownScanner);
				}
			}

			List<Point> allBeacons = new List<Point>();
			foreach (var scanner in locatedScanners)
			{
				foreach (var beacon in scanner.beacons)
				{
					allBeacons.Add(beacon.WorldLocation);
				}
			}

			// Part One
			{
				int distinctBeaconCount = allBeacons.Distinct().Count();
				Console.WriteLine("\nPart One:");
				Console.WriteLine("Number of unique beacons found is: " + distinctBeaconCount);
			}

			// Part Two
			{
				long largestManhattan = long.MinValue;
				foreach (var scannerA in locatedScanners)
				{
					foreach(var scannerB in locatedScanners)
					{
						if (scannerA.name == scannerB.name)
							continue;

						long manhattanDistance = Math.Abs(scannerA.worldCentre.x - scannerB.worldCentre.x)
							+ Math.Abs(scannerA.worldCentre.y - scannerB.worldCentre.y)
							+ Math.Abs(scannerA.worldCentre.z - scannerB.worldCentre.z);

						largestManhattan = Math.Max(manhattanDistance, largestManhattan);
					}
				}
				Console.WriteLine("\nPart Two:");
				Console.WriteLine("Largest Manhattan distance is: " + largestManhattan);
			}
		}
	}
}
