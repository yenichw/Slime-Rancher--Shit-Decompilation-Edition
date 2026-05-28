using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GameSummaryV01 : PersistedDataSet
	{
		public Identifiable.Id iconId;

		public PlayerState.GameMode gameMode;

		public string version;

		public int currency;

		public int pediaCount;

		public double worldTime;

		public override string Identifier => "SRGSUMM";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			version = reader.ReadString();
			gameMode = (PlayerState.GameMode)reader.ReadInt32();
			iconId = (Identifiable.Id)reader.ReadInt32();
			currency = reader.ReadInt32();
			pediaCount = reader.ReadInt32();
			worldTime = reader.ReadDouble();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(version);
			writer.Write((int)gameMode);
			writer.Write((int)iconId);
			writer.Write(currency);
			writer.Write(pediaCount);
			writer.Write(worldTime);
		}

		public static GameSummaryV01 Load(BinaryReader reader)
		{
			GameSummaryV01 gameSummaryV = new GameSummaryV01();
			gameSummaryV.Load(reader.BaseStream);
			return gameSummaryV;
		}

		public static void AssertAreEqual(GameSummaryV01 expected, GameSummaryV01 actual)
		{
		}
	}
}
