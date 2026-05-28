using System;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GameSummaryV02 : VersionedPersistedDataSet<GameSummaryV01>
	{
		public Identifiable.Id iconId;

		public PlayerState.GameMode gameMode;

		public string version;

		public int currency;

		public int pediaCount;

		public double worldTime;

		public DateTimeOffset saveTimestamp;

		public override string Identifier => "SRGSUMM";

		public override uint Version => 2u;

		protected override void LoadData(BinaryReader reader)
		{
			version = reader.ReadString();
			gameMode = (PlayerState.GameMode)reader.ReadInt32();
			iconId = (Identifiable.Id)reader.ReadInt32();
			currency = reader.ReadInt32();
			pediaCount = reader.ReadInt32();
			worldTime = reader.ReadDouble();
			saveTimestamp = ReadDateTimeOffset(reader);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(version);
			writer.Write((int)gameMode);
			writer.Write((int)iconId);
			writer.Write(currency);
			writer.Write(pediaCount);
			writer.Write(worldTime);
			WriteDateTimeOffset(writer, saveTimestamp);
		}

		private DateTimeOffset ReadDateTimeOffset(BinaryReader reader)
		{
			long ticks = reader.ReadInt64();
			double value = reader.ReadDouble();
			return new DateTimeOffset(ticks, TimeSpan.FromMinutes(value));
		}

		private void WriteDateTimeOffset(BinaryWriter writer, DateTimeOffset dateTimeOffset)
		{
			writer.Write(dateTimeOffset.Ticks);
			writer.Write(dateTimeOffset.Offset.TotalMinutes);
		}

		public static GameSummaryV02 Load(BinaryReader reader)
		{
			GameSummaryV02 gameSummaryV = new GameSummaryV02();
			gameSummaryV.Load(reader.BaseStream);
			return gameSummaryV;
		}

		public static void AssertAreEqual(GameSummaryV02 expected, GameSummaryV02 actual)
		{
		}

		public static void AssertAreEqual(GameSummaryV01 expected, GameSummaryV02 actual)
		{
		}

		protected override void UpgradeFrom(GameSummaryV01 legacyData)
		{
			version = legacyData.version;
			gameMode = legacyData.gameMode;
			iconId = legacyData.iconId;
			currency = legacyData.currency;
			pediaCount = legacyData.pediaCount;
			worldTime = legacyData.worldTime;
			saveTimestamp = DateTimeOffset.MinValue;
		}
	}
}
