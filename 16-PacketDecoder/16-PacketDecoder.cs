//#define _PACKET_DEBUG

using System;
using System.Collections.Generic;
using System.IO;

namespace _16_PacketDecoder
{
	class Program
	{
		// Returns amount of read bits
		static UInt64 ReadLiteralValuePacket(string binaryString, ref int readIndex)
		{
			Console.WriteLine("\n=== Literal Value Packet ===");

			// Read sets of 5 for literal value
			// First bit in set of 5 details if this is the last set in the value 
			// "1" for not last group, keep reading
			// "0" for last group, stop reading, EOP
			bool isLast = binaryString[readIndex] == '0';
			
			string literalValStr = "";
			while (!isLast)
			{
				readIndex += 1;
				literalValStr += binaryString.Substring(readIndex, 4);
				readIndex += 4;
				isLast = binaryString[readIndex] == '0'; // Keep iterating until we find a "0" in the set of 5
			}

			// Read final set of 4
			readIndex += 1;
			literalValStr += binaryString.Substring(readIndex, 4);
			readIndex += 4;

			UInt64 literalValue = Convert.ToUInt64(literalValStr, 2);
			Console.WriteLine("Literal Value:\t" + literalValue);

			return literalValue;
		}

		// return amount of bits read
		static UInt64 ReadPacket(string packet, ref UInt64 versionSum, ref int readIndex)
		{
			UInt64 literalValue = 0;

			// First 3 bits are the packet version
			UInt64 packetVersion = Convert.ToUInt64(packet.Substring(readIndex, 3), 2);
			versionSum += packetVersion;
#if _PACKET_DEBUG
			Console.WriteLine("Packet Version:\t" + packetVersion);
#endif
			readIndex += 3;

			// Next 3 bits are the packet type
			int packetType = Convert.ToInt32(packet.Substring(readIndex, 3), 2);
			Console.WriteLine("Packet Type ID:\t" + packetType);
			readIndex += 3;

			if (packetType == 4)
			{
				// PACKET TYPE 4 = LITERAL VALUE PACKET
				literalValue = ReadLiteralValuePacket(packet, ref readIndex);
			}
			else
			{
				// OPERATOR PACKET TYPE
				Console.WriteLine("\n=== Operator Packet ===");

				// Next bit is length type ID
				int lengthTypeID = Convert.ToInt32(packet[readIndex] + "", 2);
#if _PACKET_DEBUG
				Console.WriteLine("Length Type ID:\t" + lengthTypeID);
#endif
				readIndex += 1;

				List<UInt64> values = new List<UInt64>();
				if (lengthTypeID == 0)
				{
					// LENGTH TYPE 0
					// Next 15 bits are the total length in bits of the sub-packets contained in this packet
					string subPacketLengthStr = packet.Substring(readIndex, 15);
					int subPacketLength = Convert.ToInt32(subPacketLengthStr, 2);
#if _PACKET_DEBUG
					Console.WriteLine("Sub Length:\t" + subPacketLength);
#endif
					readIndex += 15;

					int subPacketLengthRemaining = subPacketLength;
					while (subPacketLengthRemaining > 0)
					{
						int readIndexBefore = readIndex;
						UInt64 subLiteralValue = ReadPacket(packet, ref versionSum, ref readIndex);
						values.Add(subLiteralValue);

						int bitsReadInPacket = readIndex - readIndexBefore;
#if _PACKET_DEBUG
						Console.WriteLine("Decreasing packet length remaining from " + subPacketLengthRemaining + " by " + bitsReadInPacket);
#endif

						subPacketLengthRemaining -= bitsReadInPacket;
					}
				}
				else
				{
					// LENGTH TYPE 1
					// Next 11 bits are the number of sub-packets immediately contained by this packet
					string subPacketQuantityStr = packet.Substring(readIndex, 11);
					int subPacketQuantity = Convert.ToInt32(subPacketQuantityStr, 2);
					readIndex += 11;

					int subPacketsRemaining = subPacketQuantity;
					while (subPacketsRemaining > 0)
					{
						int readIndexBefore = readIndex;
						UInt64 subLiteralValue = ReadPacket(packet, ref versionSum, ref readIndex);
						values.Add(subLiteralValue);

						int bitsReadInPacket = readIndex - readIndexBefore;
#if _PACKET_DEBUG
						Console.WriteLine("Decreasing packet count from " + subPacketsRemaining);
#endif

						subPacketsRemaining--;
					}
				}

				// Use operator type to determine what operation to use on the sub-packet values
				switch (packetType)
				{
					case 0:
						{
							// Sum packet
#if _PACKET_DEBUG
							Console.WriteLine("Operator Type 0 - Sum packet");
#endif
							for (int i = 0; i < values.Count; ++i)
							{
								literalValue += values[i];
							}
						}
						break;
					case 1:
						{
							// Product packet
#if _PACKET_DEBUG
							Console.WriteLine("Operator Type 1 - Product packet");
#endif
							for (int i = 0; i < values.Count; ++i)
							{
								if (i == 0)
								{
									literalValue = values[0];
									continue;
								}

								literalValue *= values[i];
							}
						}
						break;
					case 2:
						{
							// Minimum packet
#if _PACKET_DEBUG
							Console.WriteLine("Operator Type 2 - Minimum packet");
#endif
							UInt64 minimum = UInt64.MaxValue;
							for (int i = 0; i < values.Count; ++i)
							{
								if (values[i] < minimum)
								{
									literalValue = values[i];
									minimum = literalValue;
								}
							}
						}
						break;
					case 3:
						{
							// Maximum packet
#if _PACKET_DEBUG
							Console.WriteLine("Operator Type 3 - Maximum packet");
#endif
							UInt64 maximum = UInt64.MinValue;
							for (int i = 0; i < values.Count; ++i)
							{
								if (values[i] > maximum)
								{
									literalValue = values[i];
									maximum = literalValue;
								}
							}
						}
						break;
					case 5:
						{
							// Greater than packet
#if _PACKET_DEBUG
							Console.WriteLine("Operator Type 5 - Greater than packet");
#endif
							if (values.Count != 2)
							Console.WriteLine("There should only be 2 sub packet values here!");
							
							literalValue = values[0] > values[1] ? (UInt64)1 : (UInt64)0; 
						}
						break;
					case 6:
						{
							// Less than packet
#if _PACKET_DEBUG
							Console.WriteLine("Operator Type 6 - Less than packet");
#endif
							if (values.Count != 2)
								Console.WriteLine("There should only be 2 sub packet values here!");

							literalValue = values[0] < values[1] ? (UInt64)1 : (UInt64)0;
						}
						break;
					case 7:
						{
							// Equal to packet
#if _PACKET_DEBUG
							Console.WriteLine("Operator Type 7 - Equal to packet");
#endif
							if (values.Count != 2)
								Console.WriteLine("There should only be 2 sub packet values here!");

							literalValue = values[0] == values[1] ? (UInt64)1 : (UInt64)0;
						}
						break;
				}

			}
			return literalValue;
		}

		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt");
			string hexValues = "0123456789ABCDEF";

			foreach (var line in lines)
			{
				Console.WriteLine("\n\n~~~PARSING NEW PACKET~~~");

				// Raw Hexadecimal string
				string hexString = line;

				// Convert into binary
				string binaryString = "";
				foreach (var hexChar in hexString)
				{
					int val = hexValues.IndexOf(hexChar);
					string binaryVal = Convert.ToString(val, 2);
					while (binaryVal.Length < 4) // pad with leading 0s if needed
						binaryVal = binaryVal.Insert(0, "0");
					binaryString += binaryVal;
				}

				Console.WriteLine("Hex string:\t" + hexString);
				Console.WriteLine("Binary string:\t" + binaryString);

				UInt64 versionSum = 0;
				int readIndex = 0;
				UInt64 literalValue = ReadPacket(binaryString, ref versionSum, ref readIndex);

				Console.WriteLine("\n######################################################");
				Console.WriteLine("Part One:");
				Console.WriteLine("Sum of version numbers in all packets is: " + versionSum);
				Console.WriteLine("\nPart Two:");
				Console.WriteLine("Literal value of packet is: " + literalValue);
				Console.WriteLine("######################################################");
			}
		}
	}
}
