using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GameV09 : GamePersistedDataSet<GameV08>
	{
		private class Placeholder : PersistedDataSet
		{
			public enum Enum
			{
				NONE = 0
			}

			public override string Identifier => "SRDLC";

			public override uint Version => 1u;

			protected override void LoadData(BinaryReader reader)
			{
				PersistedDataSet.LoadList(reader, (int i) => Enum.NONE);
			}

			protected override void WriteData(BinaryWriter writer)
			{
				PersistedDataSet.WriteList(writer, new List<Enum>(), (Enum i) => 0);
			}

			public static Placeholder Load(BinaryReader reader)
			{
				Placeholder placeholder = new Placeholder();
				placeholder.Load(reader.BaseStream);
				return placeholder;
			}
		}

		public string gameName;

		public string displayName;

		public GameSummaryV04 summary = new GameSummaryV04();

		public WorldV22 world = new WorldV22();

		public PlayerV13 player = new PlayerV13();

		public RanchV07 ranch = new RanchV07();

		public List<ActorDataV08> actors = new List<ActorDataV08>();

		public PediaV03 pedia = new PediaV03();

		public GameAchieveV03 achieve = new GameAchieveV03();

		public HolidayDirectorV02 holiday = new HolidayDirectorV02();

		public DLCV01 dlc = new DLCV01();

		public override string Identifier => "SRGAME";

		public override uint Version => 9u;

		protected override void LoadSummaryData(BinaryReader reader)
		{
			gameName = reader.ReadString();
			displayName = reader.ReadString();
			summary = GameSummaryV04.Load(reader);
		}

		protected override void WriteSummaryData(BinaryWriter writer)
		{
			writer.Write(gameName);
			writer.Write(displayName);
			summary.Write(writer.BaseStream);
		}

		protected override void LoadGameData(BinaryReader reader)
		{
			world = WorldV22.Load(reader);
			player = PlayerV13.Load(reader);
			ranch = RanchV07.Load(reader);
			ReadSectionSeparator(reader);
			actors = PersistedDataSet.LoadList<ActorDataV08>(reader);
			ReadSectionSeparator(reader);
			pedia = PediaV03.Load(reader);
			achieve = GameAchieveV03.Load(reader);
			holiday = HolidayDirectorV02.Load(reader);
			dlc = DLCV01.Load(reader);
		}

		protected override void WriteGameData(BinaryWriter writer)
		{
			world.Write(writer.BaseStream);
			player.Write(writer.BaseStream);
			ranch.Write(writer.BaseStream);
			WriteSectionSeparator(writer);
			PersistedDataSet.WriteList(writer, actors);
			WriteSectionSeparator(writer);
			pedia.Write(writer.BaseStream);
			achieve.Write(writer.BaseStream);
			holiday.Write(writer.BaseStream);
			dlc.Write(writer.BaseStream);
		}

		protected override void UpgradeFrom(GameV08 legacyData)
		{
			gameName = legacyData.gameName;
			displayName = legacyData.displayName;
			summary = legacyData.summary;
			world = new WorldV22(new WorldV21(new WorldV20(legacyData.world)));
			achieve = legacyData.achieve;
			player = legacyData.player;
			ranch = legacyData.ranch;
			actors = UpgradeFrom(legacyData.actors, world.worldTime);
			pedia = legacyData.pedia;
			holiday = new HolidayDirectorV02(legacyData.holiday);
			dlc = legacyData.dlc;
		}

		private static List<ActorDataV08> UpgradeFrom(List<ActorDataV05> legacyData, double worldTime)
		{
			int num = 1;
			List<ActorDataV08> list = new List<ActorDataV08>();
			foreach (ActorDataV05 legacyDatum in legacyData)
			{
				ActorDataV08 actorDataV = new ActorDataV08(new ActorDataV07(new ActorDataV06(legacyDatum)));
				actorDataV.actorId = num++;
				if (actorDataV.typeId == 7)
				{
					actorDataV.destroyTime = worldTime + 21600.0;
				}
				else if (Identifiable.IsPlort((Identifiable.Id)actorDataV.typeId))
				{
					actorDataV.destroyTime = worldTime + 86400.0;
				}
				list.Add(actorDataV);
			}
			return list;
		}

		public static void AssertAreEqual(GameV09 expected, GameV09 actual, bool allowActorMovement = false)
		{
			GameSummaryV04.AssertAreEqual(expected.summary, actual.summary);
			WorldV22.AssertAreEqual(expected.world, actual.world);
			GameAchieveV03.AssertAreEqual(expected.achieve, actual.achieve);
			PediaV03.AssertAreEqual(expected.pedia, actual.pedia);
			PlayerV13.AssertAreEqual(expected.player, actual.player);
			RanchV07.AssertAreEqual(expected.ranch, actual.ranch);
			for (int i = 0; i < expected.actors.Count; i++)
			{
				ActorDataV08.AssertAreEqual(expected.actors[i], actual.actors[i], allowActorMovement);
			}
			HolidayDirectorV02.AssertAreEqual(expected.holiday, actual.holiday);
			DLCV01.AssertAreEqual(expected.dlc, actual.dlc);
		}

		public static void AssertAreEqual(GameV08 expected, GameV09 actual)
		{
			GameSummaryV04.AssertAreEqual(expected.summary, actual.summary);
			WorldV22.AssertAreEqual(new WorldV21(new WorldV20(expected.world)), actual.world);
			GameAchieveV03.AssertAreEqual(expected.achieve, actual.achieve);
			PediaV03.AssertAreEqual(expected.pedia, actual.pedia);
			PlayerV13.AssertAreEqual(expected.player, actual.player);
			RanchV07.AssertAreEqual(expected.ranch, actual.ranch);
			TestUtil.AssertAreEqual(UpgradeFrom(expected.actors, expected.world.worldTime), actual.actors, delegate(ActorDataV08 e, ActorDataV08 a, string r)
			{
				ActorDataV08.AssertAreEqual(e, a);
			}, "actors");
			HolidayDirectorV02.AssertAreEqual(expected.holiday, actual.holiday);
			DLCV01.AssertAreEqual(expected.dlc, actual.dlc);
		}
	}
}
