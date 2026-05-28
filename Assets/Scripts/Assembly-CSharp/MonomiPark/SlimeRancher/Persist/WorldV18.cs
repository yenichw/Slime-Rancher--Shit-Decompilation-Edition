using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonomiPark.SlimeRancher.Persist
{
	public class WorldV18 : VersionedPersistedDataSet<WorldV17>
	{
		public double worldTime;

		public float econSeed;

		public double dailyOfferCreateTime;

		public List<string> lastOfferRancherIds;

		public List<string> pendingOfferRancherIds;

		public double weatherUntil;

		public AmbianceDirector.Weather weather;

		public Dictionary<ExchangeDirector.OfferType, ExchangeOfferV03> offers = new Dictionary<ExchangeDirector.OfferType, ExchangeOfferV03>();

		public Dictionary<Identifiable.Id, float> econSaturations;

		public Dictionary<string, bool> teleportNodeActivations;

		public Dictionary<Vector3V02, double> animalSpawnerTimes;

		public Dictionary<Vector3V02, float> liquidSourceUnits;

		public Dictionary<Vector3V02, double> spawnerTriggerTimes;

		public Dictionary<string, GordoV01> gordos;

		public Dictionary<Vector3V02, ResourceWaterV03> resourceSpawnerWater;

		public Dictionary<string, PlacedGadgetV04> placedGadgets;

		public Dictionary<string, TreasurePodV01> treasurePods;

		public Dictionary<string, SwitchHandler.State> switches;

		public Dictionary<string, bool> puzzleSlotsFilled;

		public Dictionary<string, bool> occupiedPhaseSites;

		public FirestormV01 firestorm = new FirestormV01();

		public Dictionary<string, bool> oasisStates;

		public List<string> activeGingerPatches;

		public override string Identifier => "SRW";

		public override uint Version => 18u;

		public WorldV18()
		{
		}

		public WorldV18(WorldV17 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			worldTime = reader.ReadDouble();
			econSeed = reader.ReadSingle();
			dailyOfferCreateTime = reader.ReadDouble();
			lastOfferRancherIds = PersistedDataSet.LoadList(reader, (string s) => s);
			pendingOfferRancherIds = PersistedDataSet.LoadList(reader, (string s) => s);
			weatherUntil = reader.ReadDouble();
			weather = (AmbianceDirector.Weather)reader.ReadInt32();
			offers = LoadDictionary(reader, (BinaryReader r) => (ExchangeDirector.OfferType)r.ReadInt32(), (BinaryReader r) => ExchangeOfferV03.Load(reader));
			econSaturations = LoadDictionary(reader, (BinaryReader r) => (Identifiable.Id)r.ReadInt32(), (BinaryReader r) => r.ReadSingle());
			teleportNodeActivations = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => r.ReadBoolean());
			animalSpawnerTimes = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => r.ReadDouble());
			liquidSourceUnits = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => r.ReadSingle());
			spawnerTriggerTimes = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => r.ReadDouble());
			gordos = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => GordoV01.Load(r));
			resourceSpawnerWater = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => ResourceWaterV03.Load(r));
			placedGadgets = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => PlacedGadgetV04.Load(r));
			treasurePods = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => TreasurePodV01.Load(r));
			switches = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => (SwitchHandler.State)r.ReadInt32());
			puzzleSlotsFilled = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => r.ReadBoolean());
			occupiedPhaseSites = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => r.ReadBoolean());
			firestorm = FirestormV01.Load(reader);
			oasisStates = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => r.ReadBoolean());
			activeGingerPatches = PersistedDataSet.LoadList(reader, (string s) => s);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(worldTime);
			writer.Write(econSeed);
			writer.Write(dailyOfferCreateTime);
			PersistedDataSet.WriteList(writer, lastOfferRancherIds, (string s) => s);
			PersistedDataSet.WriteList(writer, pendingOfferRancherIds, (string s) => s);
			writer.Write(weatherUntil);
			writer.Write((int)weather);
			WriteDictionary(writer, offers, delegate(BinaryWriter w, ExchangeDirector.OfferType ot)
			{
				w.Write((int)ot);
			}, delegate(BinaryWriter w, ExchangeOfferV03 off)
			{
				off.Write(w.BaseStream);
			});
			WriteDictionary(writer, econSaturations, delegate(BinaryWriter w, Identifiable.Id k)
			{
				w.Write((int)k);
			}, delegate(BinaryWriter w, float v)
			{
				w.Write(v);
			});
			WriteDictionary(writer, teleportNodeActivations, delegate(BinaryWriter w, string k)
			{
				w.Write(k);
			}, delegate(BinaryWriter w, bool v)
			{
				w.Write(v);
			});
			WriteDictionary(writer, animalSpawnerTimes, delegate(BinaryWriter w, Vector3V02 key)
			{
				key.Write(w.BaseStream);
			}, delegate(BinaryWriter w, double val)
			{
				w.Write(val);
			});
			WriteDictionary(writer, liquidSourceUnits, delegate(BinaryWriter w, Vector3V02 key)
			{
				key.Write(w.BaseStream);
			}, delegate(BinaryWriter w, float val)
			{
				w.Write(val);
			});
			WriteDictionary(writer, spawnerTriggerTimes, delegate(BinaryWriter w, Vector3V02 key)
			{
				key.Write(w.BaseStream);
			}, delegate(BinaryWriter w, double val)
			{
				w.Write(val);
			});
			WriteDictionary(writer, gordos, delegate(BinaryWriter w, string key)
			{
				w.Write(key);
			}, delegate(BinaryWriter w, GordoV01 val)
			{
				val.Write(w.BaseStream);
			});
			WriteDictionary(writer, resourceSpawnerWater, delegate(BinaryWriter w, Vector3V02 key)
			{
				key.Write(w.BaseStream);
			}, delegate(BinaryWriter w, ResourceWaterV03 val)
			{
				val.Write(w.BaseStream);
			});
			WriteDictionary(writer, placedGadgets, delegate(BinaryWriter w, string s)
			{
				w.Write(s);
			}, delegate(BinaryWriter w, PlacedGadgetV04 pg)
			{
				pg.Write(w.BaseStream);
			});
			WriteDictionary(writer, treasurePods, delegate(BinaryWriter w, string s)
			{
				w.Write(s);
			}, delegate(BinaryWriter w, TreasurePodV01 tp)
			{
				tp.Write(w.BaseStream);
			});
			WriteDictionary(writer, switches, delegate(BinaryWriter w, string s)
			{
				w.Write(s);
			}, delegate(BinaryWriter w, SwitchHandler.State val)
			{
				w.Write((int)val);
			});
			WriteDictionary(writer, puzzleSlotsFilled, delegate(BinaryWriter w, string s)
			{
				w.Write(s);
			}, delegate(BinaryWriter w, bool val)
			{
				w.Write(val);
			});
			WriteDictionary(writer, occupiedPhaseSites, delegate(BinaryWriter w, string s)
			{
				w.Write(s);
			}, delegate(BinaryWriter w, bool val)
			{
				w.Write(val);
			});
			firestorm.Write(writer.BaseStream);
			WriteDictionary(writer, oasisStates, delegate(BinaryWriter w, string s)
			{
				w.Write(s);
			}, delegate(BinaryWriter w, bool val)
			{
				w.Write(val);
			});
			PersistedDataSet.WriteList(writer, activeGingerPatches, (string s) => s);
		}

		public static WorldV18 Load(BinaryReader reader)
		{
			WorldV18 worldV = new WorldV18();
			worldV.Load(reader.BaseStream);
			return worldV;
		}

		public static void AssertAreEqual(WorldV18 expected, WorldV18 actual)
		{
			TestUtil.AssertAreEqual(expected.lastOfferRancherIds, actual.lastOfferRancherIds, "lastOfferRancherIds");
			TestUtil.AssertAreEqual(expected.pendingOfferRancherIds, actual.pendingOfferRancherIds, "pendingOfferRancherIds");
			TestUtil.AssertAreEqual(expected.offers, actual.offers, delegate(ExchangeOfferV03 e, ExchangeOfferV03 a)
			{
				ExchangeOfferV03.AssertAreEqual(e, a);
			}, "offers");
			TestUtil.AssertAreEqual(expected.econSaturations, actual.econSaturations, delegate
			{
			}, "econSaturations");
			TestUtil.AssertAreEqual(expected.teleportNodeActivations, actual.teleportNodeActivations, delegate
			{
			}, "teleportNodeActivations");
			TestUtil.AssertAreEqual(expected.animalSpawnerTimes, actual.animalSpawnerTimes, delegate
			{
			}, "animalSpawnerTimes");
			TestUtil.AssertAreEqual(expected.liquidSourceUnits, actual.liquidSourceUnits, delegate
			{
			}, "liquidSourceUnits");
			TestUtil.AssertAreEqual(expected.spawnerTriggerTimes, actual.spawnerTriggerTimes, delegate
			{
			}, "spawnerTriggerTimes");
			TestUtil.AssertAreEqual(expected.gordos, actual.gordos, delegate(GordoV01 e, GordoV01 a)
			{
				GordoV01.AssertAreEqual(e, a);
			}, "gordos");
			TestUtil.AssertAreEqual(expected.resourceSpawnerWater, actual.resourceSpawnerWater, delegate(ResourceWaterV03 e, ResourceWaterV03 a)
			{
				ResourceWaterV03.AssertAreEqual(e, a);
			}, "resourceSpawnerWater");
			TestUtil.AssertAreEqual(expected.placedGadgets, actual.placedGadgets, delegate(PlacedGadgetV04 e, PlacedGadgetV04 a)
			{
				PlacedGadgetV04.AssertAreEqual(e, a);
			}, "placedGadgets");
			TestUtil.AssertAreEqual(expected.treasurePods, actual.treasurePods, delegate(TreasurePodV01 e, TreasurePodV01 a)
			{
				TreasurePodV01.AssertAreEqual(e, a);
			}, "treasurePods");
			TestUtil.AssertAreEqual(expected.switches, actual.switches, delegate
			{
			}, "switches");
			TestUtil.AssertAreEqual(expected.puzzleSlotsFilled, actual.puzzleSlotsFilled, delegate
			{
			}, "puzzleSlotsFilled");
			TestUtil.AssertAreEqual(expected.occupiedPhaseSites, actual.occupiedPhaseSites, delegate
			{
			}, "occupiedPhaseSites");
			FirestormV01.AssertAreEqual(expected.firestorm, actual.firestorm);
			TestUtil.AssertAreEqual(expected.oasisStates, actual.oasisStates, delegate
			{
			}, "oasisStates");
			TestUtil.AssertAreEqual(expected.activeGingerPatches, actual.activeGingerPatches, "activeGingerPatches");
		}

		public static void AssertAreEqual(WorldV17 expected, WorldV18 actual)
		{
			TestUtil.AssertAreEqual(expected.offers, actual.offers, delegate(ExchangeOfferV03 e, ExchangeOfferV03 a)
			{
				ExchangeOfferV03.AssertAreEqual(e, a);
			}, "offers");
			TestUtil.AssertAreEqual(expected.econSaturations, actual.econSaturations, delegate
			{
			}, "econSaturations");
			TestUtil.AssertAreEqual(expected.teleportNodeActivations, actual.teleportNodeActivations, delegate
			{
			}, "teleportNodeActivations");
			TestUtil.AssertAreEqual(expected.animalSpawnerTimes, actual.animalSpawnerTimes, delegate
			{
			}, "animalSpawnerTimes");
			TestUtil.AssertAreEqual(expected.liquidSourceUnits, actual.liquidSourceUnits, delegate
			{
			}, "liquidSourceUnits");
			TestUtil.AssertAreEqual(expected.spawnerTriggerTimes, actual.spawnerTriggerTimes, delegate
			{
			}, "spawnerTriggerTimes");
			TestUtil.AssertAreEqual(expected.gordos, actual.gordos, delegate(GordoV01 e, GordoV01 a)
			{
				GordoV01.AssertAreEqual(e, a);
			}, "gordos");
			TestUtil.AssertAreEqual(expected.resourceSpawnerWater, actual.resourceSpawnerWater, delegate(ResourceWaterV03 e, ResourceWaterV03 a)
			{
				ResourceWaterV03.AssertAreEqual(e, a);
			}, "resourceSpawnerWater");
			TestUtil.AssertVersionedAreEqual(expected.placedGadgets, actual.placedGadgets, delegate(PlacedGadgetV04 e, PlacedGadgetV04 a)
			{
				PlacedGadgetV04.AssertAreEqual(e, a);
			}, "placedGadgets");
			TestUtil.AssertAreEqual(UpgradeFrom(expected.treasurePods), actual.treasurePods, delegate(TreasurePodV01 e, TreasurePodV01 a)
			{
				TreasurePodV01.AssertAreEqual(e, a);
			}, "treasurePods");
			TestUtil.AssertAreEqual(expected.switches, actual.switches, delegate
			{
			}, "switches");
			TestUtil.AssertAreEqual(expected.puzzleSlotsFilled, actual.puzzleSlotsFilled, delegate
			{
			}, "puzzleSlotsFilled");
			TestUtil.AssertAreEqual(expected.occupiedPhaseSites, actual.occupiedPhaseSites, delegate
			{
			}, "occupiedPhaseSites");
			FirestormV01.AssertAreEqual(expected.firestorm, actual.firestorm);
			TestUtil.AssertAreEqual(expected.oasisStates, actual.oasisStates, delegate
			{
			}, "oasisStates");
			TestUtil.AssertAreEqual(expected.activeGingerPatches, actual.activeGingerPatches, "activeGingerPatches");
		}

		protected override void UpgradeFrom(WorldV17 legacyData)
		{
			worldTime = legacyData.worldTime;
			econSeed = legacyData.econSeed;
			dailyOfferCreateTime = legacyData.dailyOfferCreateTime;
			lastOfferRancherIds = legacyData.lastOfferRancherIds;
			pendingOfferRancherIds = legacyData.pendingOfferRancherIds;
			weatherUntil = legacyData.weatherUntil;
			weather = legacyData.weather;
			offers = legacyData.offers;
			econSaturations = legacyData.econSaturations;
			teleportNodeActivations = legacyData.teleportNodeActivations;
			animalSpawnerTimes = legacyData.animalSpawnerTimes;
			liquidSourceUnits = legacyData.liquidSourceUnits;
			spawnerTriggerTimes = legacyData.spawnerTriggerTimes;
			gordos = legacyData.gordos;
			resourceSpawnerWater = legacyData.resourceSpawnerWater;
			placedGadgets = legacyData.placedGadgets;
			switches = legacyData.switches;
			puzzleSlotsFilled = legacyData.puzzleSlotsFilled;
			occupiedPhaseSites = legacyData.occupiedPhaseSites;
			firestorm = legacyData.firestorm;
			oasisStates = legacyData.oasisStates;
			activeGingerPatches = legacyData.activeGingerPatches;
			treasurePods = UpgradeFrom(legacyData.treasurePods);
		}

		private static Dictionary<string, TreasurePodV01> UpgradeFrom(Dictionary<string, TreasurePod.State> legacy)
		{
			return legacy.ToDictionary((KeyValuePair<string, TreasurePod.State> pair) => pair.Key, (KeyValuePair<string, TreasurePod.State> pair) => new TreasurePodV01
			{
				state = pair.Value
			});
		}
	}
}
