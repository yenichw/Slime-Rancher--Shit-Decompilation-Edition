using System;
using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GameV06 : VersionedPersistedDataSet<GameV05>
	{
		public string gameName;

		public string displayName;

		private bool loadSummaryOnly;

		public GameSummaryV03 summary = new GameSummaryV03();

		public WorldV19 world = new WorldV19();

		public PlayerV13 player = new PlayerV13();

		public RanchV06 ranch = new RanchV06();

		public List<ActorDataV05> actors = new List<ActorDataV05>();

		public PediaV03 pedia = new PediaV03();

		public GameAchieveV03 achieve = new GameAchieveV03();

		public override string Identifier => "SRGAME";

		public override uint Version => 6u;

		public bool LoadSummaryOnly => loadSummaryOnly;

		public void LoadSummary(Stream stream)
		{
			loadSummaryOnly = true;
			Load(stream, skipPayloadEnd: true);
		}

		protected override void LoadData(BinaryReader reader)
		{
			gameName = reader.ReadString();
			displayName = reader.ReadString();
			summary = GameSummaryV03.Load(reader);
			if (!loadSummaryOnly)
			{
				world = WorldV19.Load(reader);
				player = PlayerV13.Load(reader);
				ranch = RanchV06.Load(reader);
				ReadSectionSeparator(reader);
				actors = PersistedDataSet.LoadList<ActorDataV05>(reader);
				ReadSectionSeparator(reader);
				pedia = PediaV03.Load(reader);
				achieve = GameAchieveV03.Load(reader);
			}
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(gameName);
			writer.Write(displayName);
			summary.Write(writer.BaseStream);
			world.Write(writer.BaseStream);
			player.Write(writer.BaseStream);
			ranch.Write(writer.BaseStream);
			WriteSectionSeparator(writer);
			PersistedDataSet.WriteList(writer, actors);
			WriteSectionSeparator(writer);
			pedia.Write(writer.BaseStream);
			achieve.Write(writer.BaseStream);
		}

		protected override void UpgradeFrom(GameV05 legacyData)
		{
			gameName = legacyData.gameName;
			displayName = legacyData.gameName;
			world = legacyData.world;
			achieve = legacyData.achieve;
			player = legacyData.player;
			ranch = legacyData.ranch;
			actors = legacyData.actors;
			pedia = legacyData.pedia;
			summary = new GameSummaryV03();
			summary.currency = player.currency;
			summary.gameMode = player.gameMode;
			summary.iconId = player.gameIconId;
			summary.version = player.version;
			summary.worldTime = world.worldTime;
			summary.pediaCount = pedia.unlockedIds.Count;
			summary.saveTimestamp = DateTimeOffset.MinValue;
		}

		public static void AssertAreEqual(GameV06 expected, GameV06 actual)
		{
			GameSummaryV03.AssertAreEqual(expected.summary, actual.summary);
			WorldV19.AssertAreEqual(expected.world, actual.world);
			GameAchieveV03.AssertAreEqual(expected.achieve, actual.achieve);
			PediaV03.AssertAreEqual(expected.pedia, actual.pedia);
			PlayerV13.AssertAreEqual(expected.player, actual.player);
			RanchV06.AssertAreEqual(expected.ranch, actual.ranch);
			for (int i = 0; i < expected.actors.Count; i++)
			{
				ActorDataV05.AssertAreEqual(expected.actors[i], actual.actors[i]);
			}
		}
	}
}
