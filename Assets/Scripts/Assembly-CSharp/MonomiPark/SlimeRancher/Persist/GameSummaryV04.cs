using System;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GameSummaryV04 : VersionedPersistedDataSet<GameSummaryV03>
	{
		public Identifiable.Id iconId;

		public PlayerState.GameMode gameMode;

		public string version;

		public int currency;

		public int pediaCount;

		public double worldTime;

		public bool isGameOver;

		public ulong saveNumber;

		public DateTimeOffset saveTimestamp;

		public override string Identifier => "SRGSUMM";

		public override uint Version => 4u;

		protected override void LoadData(BinaryReader reader)
		{
			version = reader.ReadString();
			gameMode = (PlayerState.GameMode)reader.ReadInt32();
			iconId = (Identifiable.Id)reader.ReadInt32();
			currency = reader.ReadInt32();
			pediaCount = reader.ReadInt32();
			worldTime = reader.ReadDouble();
			saveTimestamp = ReadDateTimeOffset(reader);
			isGameOver = reader.ReadBoolean();
			saveNumber = reader.ReadUInt64();
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
			writer.Write(isGameOver);
			writer.Write(saveNumber);
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

		public static GameSummaryV04 Load(BinaryReader reader)
		{
			GameSummaryV04 gameSummaryV = new GameSummaryV04();
			gameSummaryV.Load(reader.BaseStream);
			return gameSummaryV;
		}

		public static void AssertAreEqual(GameSummaryV04 expected, GameSummaryV04 actual)
		{
		}

		public static void AssertAreEqual(GameSummaryV03 expected, GameSummaryV04 actual)
		{
		}

		protected override void UpgradeFrom(GameSummaryV03 legacyData)
		{
			version = legacyData.version;
			gameMode = legacyData.gameMode;
			iconId = legacyData.iconId;
			currency = legacyData.currency;
			pediaCount = legacyData.pediaCount;
			worldTime = legacyData.worldTime;
			saveTimestamp = legacyData.saveTimestamp;
			isGameOver = legacyData.isGameOver;
			saveNumber = 0uL;
		}
	}
}
