using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _23_Amphipod
{
	class Program
	{
		// A class for storing the state of the room/hallway configuration
		class State
		{
			public string hallway;
			public List<string> rooms;
			public UInt64 energyCostSoFar;
			public State from;
			public List<State> to;

			public static int[] s_roomToHallwayIndices = new int[] { 2, 4, 6, 8 };
			public static int[] s_validHallwayIndices = new int[] { 0, 1, 3, 5, 7, 9, 10 }; // Amphipods can't stop outside of a room
			public static int[] s_typeCosts = new int[] { 1, 10, 100, 1000 };

			public State()
			{
				hallway = "";
				rooms = new List<string>();
				energyCostSoFar = 0;

				from = null;
				to = new List<State>();
			}

			public State(State other)
			{
				other.to.Add(this);
				hallway = other.hallway;
				rooms = new List<string>(other.rooms);
				energyCostSoFar = other.energyCostSoFar;

				from = other;
				to = new List<State>();
			}

			// Outputs in format:
			//   "<Hallway>,[<Room 0>][<Room 1>][<Room 2>][<Room 3>],Energy"
			public override string ToString()
			{
				string output = "";
				for (int i = 0; i < hallway.Length; ++i)
				{
					output += hallway[i];
				}
				output += ',';

				for (int i = 0; i < rooms.Count; ++i)
				{
					output += '[';
					output += rooms[i];
					output += ']';
				}

				output += ",";
				output += energyCostSoFar.ToString();
				return output;
			}

			public string GetUniqueString()
			{
				return hallway + GetRoomsString();
			}

			public string GetRoomsString()
			{
				string roomStr = "";
				for (int i = 0; i < rooms.Count; ++i)
					roomStr += rooms[i];

				return roomStr;
			}

			public int GetFirstSpaceIndex(int roomIndex)
			{
				for (int i = rooms[roomIndex].Length - 1; i >= 0; --i)
				{
					if (rooms[roomIndex][i] == '.')
						return i;
				}
				return -1;
			}

			public int GetTopAmphipodIndex(int roomIndex)
			{
				for (int i = 0; i < rooms[roomIndex].Length; ++i)
				{
					if (rooms[roomIndex][i] != '.')
						return i;
				}
				return -1;
			}

			public void AddToRoom(int roomIndex, char type, ref int spacesMoved)
			{
				int indexToAddTo = GetFirstSpaceIndex(roomIndex);
				if (indexToAddTo >= 0)
				{
					rooms[roomIndex] = rooms[roomIndex].Remove(indexToAddTo, 1).Insert(indexToAddTo, type + "");

					spacesMoved += indexToAddTo + 1;
				}
			}

			public char PopFromRoom(int roomIndex, ref int spacesMoved)
			{
				char poppedType = '.';

				int indexToPopFrom = GetTopAmphipodIndex(roomIndex);
				if (indexToPopFrom >= 0)
				{
					poppedType = rooms[roomIndex][indexToPopFrom];
					rooms[roomIndex] = rooms[roomIndex].Remove(indexToPopFrom, 1).Insert(indexToPopFrom, ".");
					spacesMoved += indexToPopFrom + 1;
				}
				return poppedType;
			}

			public void AddToHallway(int hallwayIndex, char type)
			{
				if (hallway[hallwayIndex] == '.')
				{
					hallway = hallway.Remove(hallwayIndex, 1).Insert(hallwayIndex, type + "");
				}
			}
			public char RemoveFromHallway(int hallwayIndex)
			{
				char removedType = hallway[hallwayIndex];
				if (hallway[hallwayIndex] != '.')
					hallway = hallway.Remove(hallwayIndex, 1).Insert(hallwayIndex, ".");
				return removedType;
			}

			public void IncrementEnergy(int spacesMoved, char typeMoved)
			{
				energyCostSoFar += (UInt64)spacesMoved * (UInt64)s_typeCosts[typeMoved - 'A'];
			}

			public bool RoomCanAccept(int roomIndex, char type)
			{
				// Wrong room for this type
				if (type - 'A' != roomIndex)
					return false;

				// Otherwise, the room needs to have a free space
				//  and all other amphipods in there must be the correct type
				bool roomHasSpace = false;
				bool roomHasWrongAmphipod = false;
				for (int i = 0; i < rooms[roomIndex].Length; ++i)
				{
					if (rooms[roomIndex][i] == '.')
						roomHasSpace = true;
					else if (rooms[roomIndex][i] != type)
						roomHasWrongAmphipod = true;
				}

				return roomHasSpace && !roomHasWrongAmphipod;
			}

			public bool IsHallwayClear(int startIndex, int endIndex)
			{
				int minRange = Math.Min(startIndex, endIndex);
				int maxRange = Math.Max(startIndex, endIndex);

				for (int i = 0; i < s_validHallwayIndices.Length; ++i)
				{
					// Out of range
					if (s_validHallwayIndices[i] <= minRange || s_validHallwayIndices[i] >= maxRange)
						continue;

					if (hallway[s_validHallwayIndices[i]] != '.')
						return false;
				}
				return true;
			}

			public bool IsFinished()
			{
				return GetRoomsString() == "AABBCCDD"
					|| GetRoomsString() == "AAAABBBBCCCCDDDD";
			}

			public void PrintPathToState()
			{
				Stack<State> stateStack = new Stack<State>();
				stateStack.Push(this);
				State currentState = this;
				while (currentState.from != null)
				{
					stateStack.Push(currentState.from);
					currentState = currentState.from;
				}

				int i = 0;
				while (stateStack.Count > 0)
				{
					Console.WriteLine(i + ":\t" + stateStack.Pop());
					i++;
				}
			}
		}

		static State SolveAmphipods(string inputFile)
		{
			var lines = File.ReadAllLines(inputFile);

			// Parse input into Room information
			var hallwayLine = lines[1].Trim('#');

			State initialState = new State();
			for (int i = 0; i < hallwayLine.Length; ++i)
				initialState.hallway += '.';

			var topRoomsLine = lines[2].Substring(1, lines[2].Length - 2);
			var topRooms = topRoomsLine.Split('#');
			for (int i = 0; i < topRooms.Length; ++i)
			{
				if (topRooms[i] != "")
					initialState.rooms.Add(topRooms[i]);
			}

			for (int lineIndex = 3; lineIndex < lines.Length - 1; ++lineIndex)
			{
				var lowerRoomsLine = lines[lineIndex].Trim(' ').Trim('#');
				var lowerRooms = lowerRoomsLine.Split('#');

				for (int i = 0; i < lowerRooms.Length; ++i)
				{
					if (lowerRooms[i] != "")
						initialState.rooms[i] += lowerRooms[i];
				}
			}

			Console.WriteLine("Starting config:");
			Console.WriteLine(initialState);

			UInt64 lowestCost = UInt64.MaxValue;
			State lowestCostState = null;

			Queue<State> statesToCheck = new Queue<State>();
			statesToCheck.Enqueue(initialState);
			Dictionary<string, State> visitedStates = new Dictionary<string, State>();
			List<State> finishedStates = new List<State>();
			while (statesToCheck.Count > 0)
			{
				State currentState = statesToCheck.Dequeue();
				string currentStateString = currentState.GetUniqueString();

				if (currentStateString == "A........BDBDDACCBD.BAC..CA")
				{
					int z = 0;
				}

				if (!visitedStates.ContainsKey(currentStateString))
				{
					visitedStates.Add(currentStateString, currentState);
				}
				else if (visitedStates[currentStateString].energyCostSoFar > currentState.energyCostSoFar)
				{
					visitedStates[currentStateString] = currentState;
				}

				if (currentState.IsFinished())
				{
					finishedStates.Add(currentState);
					if (lowestCost > currentState.energyCostSoFar)
					{
						lowestCost = currentState.energyCostSoFar;
						lowestCostState = currentState;
					}
					continue;
				}

				// HALLWAY -> ROOM
				// Check any hallway amphipods for movement into their dest room
				for (int hallwayIndex = 0; hallwayIndex < currentState.hallway.Length; ++hallwayIndex)
				{
					if (currentState.hallway[hallwayIndex] != '.')
					{
						char typeToMove = currentState.hallway[hallwayIndex];

						// Can dest room accept this hallway amphipod?
						int destRoomIndex = typeToMove - 'A';

						if (currentState.RoomCanAccept(destRoomIndex, typeToMove))
						{
							// Room will accept the amphipod - but is hallway clear?
							if (currentState.IsHallwayClear(hallwayIndex, State.s_roomToHallwayIndices[destRoomIndex]))
							{
								// Hallway is also clear, so this is a valid move
								State newState = new State(currentState);

								char poppedType = newState.RemoveFromHallway(hallwayIndex);
								int spacesMoved = Math.Abs(hallwayIndex - State.s_roomToHallwayIndices[destRoomIndex]);
								newState.AddToRoom(destRoomIndex, poppedType, ref spacesMoved);
								newState.IncrementEnergy(spacesMoved, poppedType);

								// If we haven't checked this state before, queue it up to be checked
								if (!visitedStates.ContainsKey(newState.GetUniqueString())
										&& newState.energyCostSoFar < lowestCost)
									statesToCheck.Enqueue(newState);
							}
						}
					}
				}

				// For all rooms..
				for (int roomIndex = 0; roomIndex < 4; ++roomIndex)
				{
					string room = currentState.rooms[roomIndex];
					int roomTopIndex = 0;
					bool allCorrect = true;
					bool containsAmphipod = false;
					for (int spaceIndex = room.Length - 1; spaceIndex >= 0; --spaceIndex)
					{
						if (room[spaceIndex] != '.')
						{
							containsAmphipod = true;
							if ((room[spaceIndex] - 'A') != roomIndex)
								allCorrect = false;

							roomTopIndex = spaceIndex;
						}
					}

					if (!containsAmphipod)
					{
						// Room has nothing to move
						continue;
					}

					if (allCorrect)
					{
						// The amphipod is in its dest room
						continue;
					}

					char typeToMove = room[roomTopIndex];

					int hallwayEntranceIndex = State.s_roomToHallwayIndices[roomIndex];

					// ROOM -> HALLWAY
					// Check possible steps into hallway to the left
					for (int j = State.s_validHallwayIndices.Length - 1; j >= 0; --j)
					{
						int checkHallwayIndex = State.s_validHallwayIndices[j];
						// Check if this hallway index is to the right of the current room
						if (checkHallwayIndex > hallwayEntranceIndex)
							continue;

						// Check if hallway is blocked - amphipod can't go any further
						if (currentState.hallway[checkHallwayIndex] != '.')
							break;

						// Found valid place for amphipod to move to
						State newState = new State(currentState);

						int spacesMoved = 0;
						char poppedType = newState.PopFromRoom(roomIndex, ref spacesMoved);
						newState.AddToHallway(checkHallwayIndex, poppedType);
						spacesMoved += Math.Abs(checkHallwayIndex - State.s_roomToHallwayIndices[roomIndex]);
						newState.IncrementEnergy(spacesMoved, poppedType);

						// If we haven't checked this state before, queue it up to be checked
						if (!visitedStates.ContainsKey(newState.GetUniqueString())
							&& newState.energyCostSoFar < lowestCost)
							statesToCheck.Enqueue(newState);
					}

					// Check possible steps into hallway to the right
					for (int j = 0; j < State.s_validHallwayIndices.Length; ++j)
					{
						int checkHallwayIndex = State.s_validHallwayIndices[j];
						// Check if this hallway index is to the left of the current room
						if (checkHallwayIndex < hallwayEntranceIndex)
							continue;

						// Check if hallway is blocked - amphipod can't go any further
						if (currentState.hallway[checkHallwayIndex] != '.')
							break;

						// Found valid place for amphipod to move to
						State newState = new State(currentState);

						int spacesMoved = 0;
						char poppedType = newState.PopFromRoom(roomIndex, ref spacesMoved);
						newState.AddToHallway(checkHallwayIndex, poppedType);
						spacesMoved += Math.Abs(checkHallwayIndex - State.s_roomToHallwayIndices[roomIndex]);
						newState.IncrementEnergy(spacesMoved, poppedType);

						// If we haven't checked this state before, queue it up to be checked
						if (!visitedStates.ContainsKey(newState.GetUniqueString())
							&& newState.energyCostSoFar < lowestCost)
							statesToCheck.Enqueue(newState);
					}

					// ROOM -> ROOM
					// Check possible movements from room directly into destination room
					int destRoomIndex = typeToMove - 'A';
					string destRoom = currentState.rooms[destRoomIndex];
					if (currentState.RoomCanAccept(destRoomIndex, typeToMove))
					{
						// Is hallway clear?
						if (currentState.IsHallwayClear(State.s_roomToHallwayIndices[roomIndex], State.s_roomToHallwayIndices[destRoomIndex]))
						{
							// Found valid place for amphipod to move to
							State newState = new State(currentState);
							int spacesMoved = 0;
							char poppedType = newState.PopFromRoom(roomIndex, ref spacesMoved);
							spacesMoved += Math.Abs(State.s_roomToHallwayIndices[roomIndex] - State.s_roomToHallwayIndices[destRoomIndex]);
							newState.AddToRoom(destRoomIndex, typeToMove, ref spacesMoved);
							newState.IncrementEnergy(spacesMoved, poppedType);

							// If we haven't checked this state before, queue it up to be checked
							if (!visitedStates.ContainsKey(newState.GetUniqueString())
								&& newState.energyCostSoFar < lowestCost)
								statesToCheck.Enqueue(newState);
						}
					}
				}
			}

			return lowestCostState;
		}

		static void Main(string[] args)
		{
			// Part One
			{
				Console.WriteLine("\nPart One:");
				State lowestCostState = SolveAmphipods("input.txt");
				lowestCostState.PrintPathToState();
				Console.WriteLine("Lowest energy amount spent arranging amphipods is: " + lowestCostState.energyCostSoFar);
			}

			// Part Two
			{
				Console.WriteLine("\nPart Two:");
				State lowestCostState = SolveAmphipods("input2.txt");
				lowestCostState.PrintPathToState();
				Console.WriteLine("Lowest energy amount spent arranging amphipods is: " + lowestCostState.energyCostSoFar);
			}
		}
	}
}
