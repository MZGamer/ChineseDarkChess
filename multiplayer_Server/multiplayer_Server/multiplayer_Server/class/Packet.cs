using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ChineseDarkChess;

namespace multiplayer_Server {
	[Serializable]
	public class Packet {
		public Command command { get; set; }
		public MoveData moveData { get; set; }
		public int[,] board { get; set; }
		public List<int> redPiecesTaken {
			get; set;
		}
		public List<int> blackPiecesTaken {
			get; set;
		}

		public byte[] Data { get; set; }
		public Packet(Command command, MoveData moveData,int[,]board = null, List<int> redPiecesTaken = null, List<int> blackPiecesTaken = null) {
			this.command = command;
			this.moveData = moveData;
			this.board = board;
			this.redPiecesTaken = redPiecesTaken;
			this.blackPiecesTaken = blackPiecesTaken;
		}

		public Packet() {

		}
		public static Packet Serialize(object anySerializableObject) {
			using (var memoryStream = new MemoryStream()) {
				(new BinaryFormatter()).Serialize(memoryStream, anySerializableObject);
				return new Packet { Data = memoryStream.ToArray() };
			}
		}

		public static object Deserialize(byte[] Data) {
			using (var memoryStream = new MemoryStream(Data))
				return (new BinaryFormatter()).Deserialize(memoryStream);
		}

	}
	public enum Command {
		FLIP,
		MOVE,
		SURRENDER,
		UPDATE_BOARD

    }
}
