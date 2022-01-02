using System;
using System.Collections.Generic;
using System.IO;

namespace _23_Amphipod
{
	enum RoomSpace
	{
		None,
		Top,
		Bottom,
		Hallway
	}

	class Amphipod
	{
		public int roomNum;
		public RoomSpace space;
		public char type;

		public int hallWayIndex;

		public Amphipod(int _roomNum, RoomSpace _space, char _type)
		{
			roomNum = _roomNum;
			space = _space;
			type = _type;

			hallWayIndex = -1; // Not in hallway so -1
		}

		public override string ToString()
		{
			return type + " - " + ((space == RoomSpace.Top) ? "Top" : (space == RoomSpace.Bottom ? "Bottom" : "Hallway"));
		}
	}

	class Room
	{
		public char roomType;

		public char bottomSpace;
		public char topSpace;

		public bool bottomLocked;
		public bool topLocked;

		public int roomToHallwayLoc;

		static Dictionary<char, UInt64> s_typeCosts = new Dictionary<char, UInt64>() { { 'A', 1 }, { 'B', 10 }, { 'C', 100 }, { 'D', 1000 } };

		public Room(int _roomIndex, char _bottom, char _top, int _hallwayLoc)
		{
			roomType = (char)('A' + _roomIndex);

			bottomSpace = _bottom;
			if (bottomSpace == roomType)
				bottomLocked = true;

			topSpace = _top;
			if (bottomLocked && topSpace == roomType)
				topLocked = true;

			roomToHallwayLoc = _hallwayLoc;
		}

		public bool AddToRoom(char amphipodType, ref int spacesMoved)
		{
			if (bottomSpace != '.' && topSpace != '.')
				return false;

			if (amphipodType != roomType)
				return false;

			if (bottomLocked)
			{
				topSpace = amphipodType;
				topLocked = true;
				spacesMoved++;
			}
			else
			{
				bottomSpace = amphipodType;
				bottomLocked = true;
				spacesMoved += 2;
			}
			return true;
		}

		public RoomSpace GetFirstSpaceInRoom()
		{
			if (bottomSpace == '.')
				return RoomSpace.Bottom;

			if (topSpace == '.')
				return RoomSpace.Top;

			return RoomSpace.None;
		}

		public bool GetFirstUnlockedAmphipod(out char type, out RoomSpace space)
		{
			type = '.';
			space = RoomSpace.None;

			// Room is locked - no available amphipods to use
			if (IsLocked())
				return false;

			if (!topLocked && topSpace != '.')
			{
				type = topSpace;
				space = RoomSpace.Top;
				return true;
			}
			else if (!bottomLocked && bottomSpace != '.')
			{
				type = bottomSpace;
				space = RoomSpace.Bottom;
				return true;
			}
			return false;
		}

		public bool IsLocked()
		{
			// Both spaces are filled with correct amphipods, room is now locked
			return topLocked;
		}

		public static UInt64 GetMovementCost(char typeToMove, int spacesMoved)
		{
			return s_typeCosts[typeToMove] * (UInt64)spacesMoved;
		}

		public override string ToString()
		{
			return roomToHallwayLoc + ":" + topSpace + bottomSpace;
		}
	}

	class Program
	{
		static void MoveFromRoomToRoom(ref Room from, ref Room to, ref UInt64 energyCost)
		{
			int spacesMoved = 0;
			char typeToMove = '.';
			RoomSpace spaceToMoveFrom = RoomSpace.None;
			if (from.GetFirstUnlockedAmphipod(out typeToMove, out spaceToMoveFrom))
			{
				to.AddToRoom(typeToMove, ref spacesMoved);
				spacesMoved += Math.Abs(from.roomToHallwayLoc - to.roomToHallwayLoc);
				if (spaceToMoveFrom == RoomSpace.Top)
				{
					spacesMoved++;
					from.topSpace = '.';
				}
				else
				{
					spacesMoved += 2;
					from.bottomSpace = '.';
				}
				energyCost += Room.GetMovementCost(typeToMove, spacesMoved);
			}
		}

		static void MoveFromHallwayToRoom(int hallwayIndex, ref Room to, ref char[] hallway, ref UInt64 energyCost)
		{
			int spacesMoved = 0;
			char typeToMove = hallway[hallwayIndex];
			spacesMoved += Math.Abs(hallwayIndex - to.roomToHallwayLoc);
			to.AddToRoom(typeToMove, ref spacesMoved);
			hallway[hallwayIndex] = '.';
			energyCost += Room.GetMovementCost(typeToMove, spacesMoved);
		}

		static bool MoveFromRoomToHallway(ref Room from, int hallwayIndex, ref char[] hallway, ref UInt64 energyCost)
		{
			if (hallway[hallwayIndex] != '.')
				return false;

			int spacesMoved = 0;
			char typeToMove = '.';
			RoomSpace spaceToMoveFrom = RoomSpace.None;
			if (from.GetFirstUnlockedAmphipod(out typeToMove, out spaceToMoveFrom))
			{
				hallway[hallwayIndex] = typeToMove;
				spacesMoved += Math.Abs(hallwayIndex - from.roomToHallwayLoc);
				if (spaceToMoveFrom == RoomSpace.Top)
				{
					from.topSpace = '.';
					spacesMoved++;
				}
				else
				{
					from.bottomSpace = '.';
					spacesMoved += 2;
				}
				energyCost += Room.GetMovementCost(typeToMove, spacesMoved);
				return true;
			}
			return false;
		}

		static bool RearrangementComplete(ref Room[] rooms)
		{
			bool complete = true;
			foreach(var room in rooms)
			{
				complete &= room.IsLocked();
			}
			return complete;
		}

		static void PrintLayout(ref char[] hallway, ref Room[] rooms)
		{
			Console.WriteLine("===============================");

			// Top wall
			for (int i = 0; i < hallway.Length + 2; ++i)
				Console.Write("# ");
			Console.Write('\n');

			// Hallway
			Console.Write("# ");
			for (int i = 0; i < hallway.Length; ++i)
				Console.Write(hallway[i] + " ");
			Console.Write("#\n");

			// Top room
			Console.Write("# # #");
			foreach(var room in rooms)
			{
				if (room.topLocked)
				{
					Console.Write("[" + room.topSpace + "]#");
				}
				else
				{
					Console.Write(" " + room.topSpace + " #");
				}
			}
			Console.Write(" # #\n");

			// Bottom room
			Console.Write("    #");
			foreach (var room in rooms)
			{
				if (room.bottomLocked)
				{
					Console.Write("[" + room.bottomSpace + "]#");
				}
				else
				{
					Console.Write(" " + room.bottomSpace + " #");
				}
			}
			Console.Write("\n");

			// Bottom wall
			Console.Write("    # ");
			foreach (var room in rooms)
			{
				Console.Write("# # ");
			}
			Console.Write("\n");

			Console.WriteLine("===============================");
		}

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");

			// Parse input into Room information
			var hallwayLine = lines[1].Trim('#');

			char[] hallway = new char[hallwayLine.Length];
			
			for (int i = 0; i < hallway.Length; ++i)
				hallway[i] = '.';

			var topRoomsLine = lines[2].Substring(1, lines[2].Length - 2);

			var bottomRoomsLine = lines[3].Trim(' ').Trim('#');
			var bottomRooms = bottomRoomsLine.Split('#');

			Room[] rooms = new Room[bottomRooms.Length];
			Amphipod[,] amphipods = new Amphipod[4, 2];

			int roomIndex = 0;
			for (int i = 0; i < topRoomsLine.Length; ++i)
			{
				if (topRoomsLine[i] != '#')
				{
					char bottom = bottomRooms[roomIndex][0];
					if (amphipods[bottom - 'A', 0] == null)
						amphipods[bottom - 'A', 0] = new Amphipod(roomIndex, RoomSpace.Top, bottom);
					else
						amphipods[bottom - 'A', 1] = new Amphipod(roomIndex, RoomSpace.Top, bottom);

					char top = topRoomsLine[i];
					if (amphipods[top - 'A', 0] == null)
						amphipods[top - 'A', 0] = new Amphipod(roomIndex, RoomSpace.Bottom, top);
					else
						amphipods[top - 'A', 1] = new Amphipod(roomIndex, RoomSpace.Bottom, top);

					rooms[roomIndex] = new Room(roomIndex, bottom, top, i);
					roomIndex++;
				}
			}

			Console.WriteLine("Starting config:");
			PrintLayout(ref hallway, ref rooms);

			UInt64 energyCost = 0;

			// Energy spent per step:
			// A = 1, B = 10, C = 100, D = 1000
			while (!RearrangementComplete(ref rooms))
			{
				bool hasMoved = false;

				// Are any hallway amphipods blocked by a single other amphipod in their dest room?
				// If so, move that one out of the way
				for (int i = 0; i < hallway.Length; ++i)
				{
					if (hallway[i] == '.')
						continue;

					char typeToMove = hallway[i];
					int destRoom = typeToMove - 'A';
					RoomSpace firstAvailiableInDestRoom = rooms[destRoom].GetFirstSpaceInRoom();
					if (firstAvailiableInDestRoom != RoomSpace.None)
					{
						if (firstAvailiableInDestRoom == RoomSpace.Bottom
							|| (firstAvailiableInDestRoom == RoomSpace.Top && rooms[destRoom].bottomLocked))
						{
							// Move it there
							MoveFromHallwayToRoom(i, ref rooms[destRoom], ref hallway, ref energyCost);
							PrintLayout(ref hallway, ref rooms);
							hasMoved = true;
							break;
						}
						else
						{
							// Can't add to dest room yet since the room contains an amphipod that doesn't belong
							// Need to move that one first
							typeToMove = rooms[destRoom].bottomSpace;
							int newDestRoom = typeToMove - 'A';

							// Move out of room in the right direction
							bool moveRight = newDestRoom > destRoom;
							int hallwayIndex = rooms[destRoom].roomToHallwayLoc + (moveRight ? 1 : -1);
							MoveFromRoomToHallway(ref rooms[destRoom], hallwayIndex, ref hallway, ref energyCost);
							PrintLayout(ref hallway, ref rooms);
							hasMoved = true;
							break;
						}
					}
				}

				if (hasMoved)
					continue;

				// Can any amphipods go into another room?
				for (int i = 0; i < 4; ++i)
				{
					if (rooms[i].IsLocked())
						continue;

					char typeToMove = '.';
					RoomSpace spaceToMoveFrom = RoomSpace.None;

					if (rooms[i].GetFirstUnlockedAmphipod(out typeToMove, out spaceToMoveFrom))
					{
						int destRoom = typeToMove - 'A';
						RoomSpace destRoomSpace = rooms[destRoom].GetFirstSpaceInRoom();
						if (destRoomSpace == RoomSpace.Bottom)
						{
							// Can place in bottom of room to start it
							MoveFromRoomToRoom(ref rooms[i], ref rooms[destRoom], ref energyCost);
							PrintLayout(ref hallway, ref rooms);
							hasMoved = true;
							break;
						}
						else if (rooms[destRoom].bottomLocked && destRoomSpace == RoomSpace.Top)
						{
							// Can place in top of room to complete it
							MoveFromRoomToRoom(ref rooms[i], ref rooms[destRoom], ref energyCost);
							PrintLayout(ref hallway, ref rooms);
							hasMoved = true;
							break;
						}
					}
				}

				if (hasMoved)
					continue;

				// Find first available char that needs to move left
				for (int i = 1; i < 4; ++i)
				{
					char typeToMove = '.';
					RoomSpace spaceToMoveFrom = RoomSpace.None;
					if (!rooms[i].GetFirstUnlockedAmphipod(out typeToMove, out spaceToMoveFrom))
						continue;

					int destRoom = typeToMove - 'A';
					if (destRoom < i)
					{
						// Needs to move just left of room if available
						int hallIndex = rooms[destRoom].roomToHallwayLoc - 1;
						if (MoveFromRoomToHallway(ref rooms[i], hallIndex, ref hallway, ref energyCost))
						{
							PrintLayout(ref hallway, ref rooms);
							hasMoved = true;
							break;
						}
					}
				}

				if (hasMoved)
					continue;

				// Are there any amphipods in a room blocking another that doesn't belong?
				for (int i = 1; i < 4; ++i)
				{
					if (rooms[i].IsLocked())
						continue;

					char typeToMove = '.';
					RoomSpace spaceToMoveFrom = RoomSpace.None;
					if (!rooms[i].GetFirstUnlockedAmphipod(out typeToMove, out spaceToMoveFrom))
						continue;

					int destRoom = typeToMove - 'A';
					if (destRoom == i)
					{
						// In the correct room but not locked in, what's underneath?
						char blockedType = rooms[i].bottomSpace;

						// Move blocking amphipod out the way so it can return to the correct room later
						int hallwayIndex = rooms[i].roomToHallwayLoc - 1;
						MoveFromRoomToHallway(ref rooms[i], hallwayIndex, ref hallway, ref energyCost);
						PrintLayout(ref hallway, ref rooms);

						// Move previously blocked one out of the way
						hallwayIndex = rooms[i].roomToHallwayLoc + 1;
						MoveFromRoomToHallway(ref rooms[i], hallwayIndex, ref hallway, ref energyCost);
						PrintLayout(ref hallway, ref rooms);
						
						hasMoved = true;
						
						break;
					}
				}


				if (!hasMoved)
				{
					Console.WriteLine("Nothing happened in this step");
				}
			}

			// Part One
			{
				Console.WriteLine("Part One:");

				Console.WriteLine("Lowest energy amount spent arranging amphipods is: " + energyCost);
			}
		}
	}
}
