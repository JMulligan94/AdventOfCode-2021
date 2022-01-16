using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _19_BeaconScanner
{
	class Point
	{
		public int x;
		public int y;
		public int z;

		public long SqrMagnitude
		{
			get { return (x * x) + (y * y) + (z * z); }
		}

		public Point(int _x, int _y, int _z)
		{
			x = _x;
			y = _y;
			z = _z;
		}

		public override bool Equals(object obj)
		{
			return obj is Point coord &&
				   x == coord.x &&
				   y == coord.y &&
				   z == coord.z;
		}

		public bool IsEquivalent(Point other)
		{
			return (x == other.x && y == other.y && z == other.z) || (x == -other.x && y == -other.y && z == -other.z);
		}

		public override string ToString()
		{
			return $"({x},{y},{z})";
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
		public List<Point> orientatedLocations = new List<Point>();
		public List<long> sqrDistances = new List<long>();

		// Indexer for accessing location orientated to a certain index
		public Point this[int i]
		{
			get { return orientatedLocations[i]; }
		}

		public Beacon(Scanner _owningScanner, int _id, Point location, bool _is2D = false)
		{
			owningScanner = _owningScanner;
			id = _id;
			is2D = _is2D;
			if (is2D)
				GenerateAllOrientations_2D(location, ref orientatedLocations);
			else
				GenerateAllOrientations(location, ref orientatedLocations);
		}

		public static void GenerateAllOrientations(Point vectorIn, ref List<Point> vectorOut)
		{
			int x = vectorIn.x;
			int y = vectorIn.y;
			int z = vectorIn.z;

			// "up" is +y (up)
			vectorOut.Add(vectorIn);
			vectorOut.Add(new Point(-z, y, x));
			vectorOut.Add(new Point(-x, y, -z));
			vectorOut.Add(new Point(z, y, x));

			// "up" is -y (down)
			vectorOut.Add(new Point(-x, -y, z));
			vectorOut.Add(new Point(-z, -y, -x));
			vectorOut.Add(new Point(x, -y, -z));
			vectorOut.Add(new Point(z, -y, x));

			// "up" is +x (right)
			vectorOut.Add(new Point(y, -x, z));
			vectorOut.Add(new Point(y, -z, x));
			vectorOut.Add(new Point(y, x, -z));
			vectorOut.Add(new Point(y, z, x));

			// "up" is -x (left)
			vectorOut.Add(new Point(-y, x, z));
			vectorOut.Add(new Point(-y, -z, x));
			vectorOut.Add(new Point(-y, -x, -z));
			vectorOut.Add(new Point(-y, z, -x));

			// "up" is -z (front)
			vectorOut.Add(new Point(x, z, -y));
			vectorOut.Add(new Point(z, -x, -y));
			vectorOut.Add(new Point(-x, -z, -y));
			vectorOut.Add(new Point(-z, x, -y));

			// "up" is +z (back)
			vectorOut.Add(new Point(x, -z, y));
			vectorOut.Add(new Point(-z, -x, y));
			vectorOut.Add(new Point(-x, z, y));
			vectorOut.Add(new Point(z, x, y));
		}

		public static void GenerateAllOrientations_2D(Point vectorIn, ref List<Point> vectorOut)
		{
			int x = vectorIn.x;
			int y = vectorIn.y;
			int z = vectorIn.z;

			vectorOut.Add(vectorIn);
			vectorOut.Add(new Point(y, -x, z));
			vectorOut.Add(new Point(-x, -y, z));
			vectorOut.Add(new Point(-y, x, z));
		}

		public override string ToString()
		{
			return $"S({owningScanner.name}) B({id}) {orientatedLocations.First().ToString()}";
		}
	}

	class BeaconOverlap
	{
		public Scanner scannerA;
		public Scanner scannerB;
		public (int srcIndex, int destIndex) scannerAIndices;
		public (int srcIndex, int destIndex) scannerBIndices;
		public long sqrDist;
	}

	class Scanner
	{
		public string name;
		public bool is2D;

		public List<Beacon> beacons = new List<Beacon>();
		public long[,] beaconSqrDistances;

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

		public void GetBeaconsAtOrientationIndex(int index, ref List<Point> orientations)
		{
			foreach (var beacon in beacons)
			{
				orientations.Add(beacon[index]);
			}
		}

		public void CacheDistancesBetweenBeacons()
		{
			Console.WriteLine($"\n  CACHING DISTANCES BETWEEN BEACONS IN SCANNER \"{name}\"");
			beaconSqrDistances = new long[beacons.Count, beacons.Count];
			for (int i = 0; i < beacons.Count; ++i)
			{
				beacons[i].sqrDistances.Clear();
				Point locationA = beacons[i].orientatedLocations[0];
				for (int j = 0; j < beacons.Count; ++j)
				{
					if (i == j)
					{
						beaconSqrDistances[i, j] = 0;
						continue;
					}

					Point locationB = beacons[j].orientatedLocations[0];

					Point relativeDistance = locationB - locationA;
					long squareDist = ((relativeDistance.x * relativeDistance.x)
						+ (relativeDistance.y * relativeDistance.y)
						+ (relativeDistance.z * relativeDistance.z));
					beaconSqrDistances[i, j] = squareDist;
					if (beacons[i].sqrDistances.Contains(squareDist))
					{
						Console.WriteLine($"   Another sqr distance is already found from beacon {beacons[i]} with distance {squareDist}. This is going to be an issue.");
					}
					else
					{
						Console.WriteLine($"   {i}->{j} = {squareDist}");
					}
					beacons[i].sqrDistances.Add(squareDist);

					int count = 0;
					foreach(var test in beaconSqrDistances)
					{
						if (test == squareDist)
							count++;
					}

					if (count > 2)
					{
						int k = 0;
					}
				}
				beacons[i].sqrDistances.Sort();
			}
		}

		public override string ToString()
		{
			return $"--- Scanner {name} ---";
		}

		internal void Consume(Scanner scanner, Dictionary<Beacon, int> beaconMatches)
		{
			Beacon srcBeaconA = beaconMatches.Keys.ToList()[0];
			Beacon destBeaconA = beaconMatches.Keys.ToList()[1];

			Point srcBeaconALoc = srcBeaconA.orientatedLocations[0];
			Point destBeaconALoc = destBeaconA.orientatedLocations[0];

			Point relativePositionA = destBeaconALoc - srcBeaconALoc;


			Beacon srcBeaconB = scanner.beacons[beaconMatches[srcBeaconA]];
			Beacon destBeaconB = scanner.beacons[beaconMatches[destBeaconA]];

			Point srcBeaconBLoc = srcBeaconB.orientatedLocations[0];
			Point destBeaconBLoc = destBeaconB.orientatedLocations[0];

			Point relativePositionB = destBeaconBLoc - srcBeaconBLoc;

			// Work out orientation
			bool foundOrientation = false;
			int correctOrientation = -1;

			List<Point> orientations = new List<Point>();
			if (is2D)
				Beacon.GenerateAllOrientations_2D(relativePositionB, ref orientations);
			else
				Beacon.GenerateAllOrientations(relativePositionB, ref orientations);

			// How does B have to be rotated to fit A?
			for (int orientationIndex = 0; orientationIndex < NumOrientations; ++orientationIndex)
			{
				if (relativePositionA.Equals(orientations[orientationIndex]))
				{
					if (!foundOrientation)
					{
						correctOrientation = orientationIndex;
						foundOrientation = true;
					}
					else if (orientationIndex != correctOrientation)
					{
						Console.WriteLine("Something's gone wrong when calculating the orientation");
					}
					break;
				}
			}

			if (!foundOrientation)
			{
				Console.WriteLine("......ERROR! - COULDN'T FIND A VALID ORIENTATION");
			}

			Console.WriteLine($"\n.....Scanner {scanner.name} fits at orientation {correctOrientation}");

			Point scannerRelativeOffset = srcBeaconB[correctOrientation] - srcBeaconA[0];

			Console.WriteLine($".....Scanner {scanner.name} must be at {scannerRelativeOffset} (relative to scanner 0)");

			Console.WriteLine($"\n......ALL MATCHES:");
			foreach (var matchingPair in beaconMatches)
			{
				Console.WriteLine($"......{matchingPair.Key[0]} -> {scanner.beacons[matchingPair.Value][0]}");
			}

			for (int beaconIndex = 0; beaconIndex < scanner.beacons.Count; ++beaconIndex)
			{
				// Beacon is already known to the composite scanner
				if (beaconMatches.Values.Contains(beaconIndex))
				{
					continue;
				}

				Beacon otherBeacon = scanner.beacons[beaconIndex];

				// The beacon is not know to the composite scanner - add it now at the correct orientation
				Point translatedBeaconLocation = scannerRelativeOffset + otherBeacon[correctOrientation];

				Console.WriteLine($"......Adding {otherBeacon} to composite scanner as coord {translatedBeaconLocation}");
				AddBeacon(translatedBeaconLocation);
			}

			// Re-calculate beacon distances since new ones have been added
			CacheDistancesBetweenBeacons();
		}
	}

	class Program
	{
		private static bool CheckCompositeAgainstScanner(Scanner compositeScanner, Scanner otherScanner, bool is2D)
		{
			int minOverlapNeeded = 2;// is2D ? 3 : 12;

			Dictionary<Beacon, int> foundBeacons = new Dictionary<Beacon, int>();
			for (int beaconIndexA1 = 0; beaconIndexA1 < compositeScanner.beacons.Count - 1; ++beaconIndexA1)
			{
				Beacon srcBeaconA = compositeScanner.beacons[beaconIndexA1];
				for (int beaconIndexA2 = beaconIndexA1+1; beaconIndexA2 < compositeScanner.beacons.Count; ++beaconIndexA2)
				{
					Beacon dstBeaconA = compositeScanner.beacons[beaconIndexA2];
					bool foundMatch = false;
					long relDistA = compositeScanner.beaconSqrDistances[beaconIndexA1, beaconIndexA2];

					//Console.WriteLine($"\nFINDING  Comp({beaconIndexA1}) -> Comp({beaconIndexA2})  Dist = {relDistA}");

					for (int beaconIndexB1 = 0; beaconIndexB1 < otherScanner.beacons.Count - 1; ++beaconIndexB1)
					{
						for (int beaconIndexB2 = beaconIndexB1+1; beaconIndexB2 < otherScanner.beacons.Count; ++beaconIndexB2)
						{
							long relDistB = otherScanner.beaconSqrDistances[beaconIndexB1, beaconIndexB2];

							if (relDistA == relDistB) // Also check inverse direction
							{
								Console.WriteLine($"\nFINDING  Comp({beaconIndexA1}) -> Comp({beaconIndexA2})  Dist = {relDistA}");

								Console.WriteLine($"..CHECKING  S{otherScanner.name}({beaconIndexB1})->" +
								   $"S{otherScanner.name}({beaconIndexB2})  Dist = {relDistB}");
								Console.WriteLine("....MATCH!");
								foundMatch = true;

								Console.WriteLine($"....EITHER S{otherScanner.name}({beaconIndexB1}) == Comp({beaconIndexA1})");
								Console.WriteLine($"....    OR S{otherScanner.name}({beaconIndexB1}) == Comp({beaconIndexA2})");

								Beacon beaconB1 = otherScanner.beacons[beaconIndexB1];
								Beacon beaconB2 = otherScanner.beacons[beaconIndexB2];

								int srcIndexB = beaconIndexB1;
								int destIndexB = beaconIndexB2;
								int srcIsB1Probability = 0;
								int srcIsB2Probability = 0;
								foreach (var sqrDist in srcBeaconA.sqrDistances)
								{
									if (beaconB1.sqrDistances.Contains(sqrDist))
										srcIsB1Probability++;

									if (beaconB2.sqrDistances.Contains(sqrDist))
										srcIsB2Probability++;
								}
								
								//foreach (var sqrDist in dstBeaconA.sqrDistances)
								//{
								//	if (beaconB1.sqrDistances.Contains(sqrDist))
								//		srcIsB1Probability--;

								//	if (beaconB2.sqrDistances.Contains(sqrDist))
								//		srcIsB2Probability--;
								//}

								if (srcIsB2Probability > srcIsB1Probability)
								{
									srcIndexB = beaconIndexB2;
									destIndexB = beaconIndexB1;
								}

								Console.WriteLine($"....  CALC Comp({beaconIndexA1}) == S{otherScanner.name}({srcIndexB})");
								Console.WriteLine($"....    SO Comp({beaconIndexA2}) == S{otherScanner.name}({destIndexB})");
								Console.WriteLine($"\n....===Comp {beaconIndexA1}->{beaconIndexA2} == S{otherScanner.name} {srcIndexB}->{destIndexB}===");

								if (!foundBeacons.ContainsKey(srcBeaconA))
								{
									foundBeacons.Add(srcBeaconA, srcIndexB);
								}
								else if (foundBeacons[srcBeaconA] != srcIndexB)
								{
									Console.WriteLine($"\n....ERROR! - There's already a different index stored for Comp({beaconIndexA1})  ({foundBeacons[srcBeaconA]})");
								}

								if (!foundBeacons.ContainsKey(dstBeaconA))
								{
									foundBeacons.Add(dstBeaconA, destIndexB);
								}
								else if (foundBeacons[dstBeaconA] != destIndexB)
								{
									Console.WriteLine($"\n....ERROR! - There's already a different index stored for Comp({beaconIndexA2})  ({foundBeacons[dstBeaconA]})");
								}

								//Console.WriteLine($"{srcBeaconA} -> {dstBeaconA}\nmatches distance ({relDistA}) for:\n{otherScanner.beacons[srcIndexB]} -> {otherScanner.beacons[destIndexB]}\n");

								break;
							}
						}
						if (foundMatch)
							break;
					}

					if (!foundMatch)
					{
						//Console.WriteLine($"NO MATCH FOUND FOR Comp({beaconIndexA1}) -> Comp({beaconIndexA2})");
					}
				}
			}

			if (foundBeacons.Count >= minOverlapNeeded)
			{
				// Can stop checking scanner B and the composite scanner can CONSUME it and its beacons using this orientation
				Console.WriteLine($"Scanner {otherScanner.name} is being consumed");
				compositeScanner.Consume(otherScanner, foundBeacons);
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

			List<Scanner> scanners = new List<Scanner>();
			bool newScannerInfo = true;
			Scanner currentScanner = null;
			foreach (var line in lines)
			{
				if (newScannerInfo)
				{
					var idLineTokens = line.Split(' ');
					currentScanner = new Scanner(idLineTokens[2], is2D);
					newScannerInfo = false;

					scanners.Add(currentScanner);
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
			
			foreach (Scanner scanner in scanners)
				scanner.CacheDistancesBetweenBeacons();

			Scanner compositeScanner = new Scanner("Composite", is2D);

			Queue<Scanner> scannersToCheck = new Queue<Scanner>(scanners);
			Scanner startScanner = scannersToCheck.Dequeue();

			List<Point> startBeaconLocations = new List<Point>();
			startScanner.GetBeaconsAtOrientationIndex(0, ref startBeaconLocations);
			foreach (var startBeaconLocation in startBeaconLocations)
				compositeScanner.AddBeacon(startBeaconLocation);

			compositeScanner.CacheDistancesBetweenBeacons();

			Console.WriteLine("\n\n=== Checking for overlaps ===");

			while (scannersToCheck.Count > 0)
			{
				Scanner scannerToCheck = scannersToCheck.Dequeue();
				
				if(!CheckCompositeAgainstScanner(compositeScanner, scannerToCheck, is2D))
				{
					// No overlap found, add it back to the bottom of the queue
					scannersToCheck.Enqueue(scannerToCheck);
				}
			}

			Console.WriteLine("\nPart One:");
			Console.WriteLine("Number of unique beacons found is: " + compositeScanner.beacons.Count);
		}
	}
}
