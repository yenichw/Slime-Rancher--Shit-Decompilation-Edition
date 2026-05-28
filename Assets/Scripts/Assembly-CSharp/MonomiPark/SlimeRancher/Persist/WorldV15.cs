using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class WorldV15 : VersionedPersistedDataSet<WorldV14>
	{
		public double worldTime;

		public float econSeed;

		public double dailyOfferCreateTime;

		public string lastRancherOfferId;

		public double weatherUntil;

		public AmbianceDirector.Weather weather;

		public ExchangeOfferV03 offer = new ExchangeOfferV03();

		public Dictionary<Identifiable.Id, float> econSaturations;

		public Dictionary<string, bool> teleportNodeActivations;

		public Dictionary<Vector3V02, double> animalSpawnerTimes;

		public Dictionary<Vector3V02, float> liquidSourceUnits;

		public Dictionary<Vector3V02, double> spawnerTriggerTimes;

		public Dictionary<string, GordoV01> gordos;

		public Dictionary<Vector3V02, ResourceWaterV03> resourceSpawnerWater;

		public Dictionary<string, PlacedGadgetV04> placedGadgets;

		public Dictionary<string, TreasurePod.State> treasurePods;

		public Dictionary<string, SwitchHandler.State> switches;

		public Dictionary<string, bool> puzzleSlotsFilled;

		public Dictionary<string, bool> occupiedPhaseSites;

		public FirestormV01 firestorm = new FirestormV01();

		public Dictionary<string, bool> oasisStates;

		public override string Identifier => "SRW";

		public override uint Version => 15u;

		public WorldV15()
		{
		}

		public WorldV15(WorldV14 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			worldTime = reader.ReadDouble();
			econSeed = reader.ReadSingle();
			dailyOfferCreateTime = reader.ReadDouble();
			if (reader.ReadBoolean())
			{
				lastRancherOfferId = reader.ReadString();
			}
			else
			{
				lastRancherOfferId = null;
			}
			weatherUntil = reader.ReadDouble();
			weather = (AmbianceDirector.Weather)reader.ReadInt32();
			offer = new ExchangeOfferV03();
			offer.Load(reader.BaseStream);
			econSaturations = LoadDictionary(reader, (BinaryReader r) => (Identifiable.Id)r.ReadInt32(), (BinaryReader r) => r.ReadSingle());
			teleportNodeActivations = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => r.ReadBoolean());
			animalSpawnerTimes = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => r.ReadDouble());
			liquidSourceUnits = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => r.ReadSingle());
			spawnerTriggerTimes = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => r.ReadDouble());
			gordos = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => GordoV01.Load(r));
			resourceSpawnerWater = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => ResourceWaterV03.Load(r));
			placedGadgets = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => PlacedGadgetV04.Load(r));
			treasurePods = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => (TreasurePod.State)r.ReadInt32());
			switches = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => (SwitchHandler.State)r.ReadInt32());
			puzzleSlotsFilled = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => r.ReadBoolean());
			occupiedPhaseSites = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => r.ReadBoolean());
			firestorm = FirestormV01.Load(reader);
			oasisStates = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => r.ReadBoolean());
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(worldTime);
			writer.Write(econSeed);
			writer.Write(dailyOfferCreateTime);
			bool flag = lastRancherOfferId != null;
			writer.Write(flag);
			if (flag)
			{
				writer.Write(lastRancherOfferId);
			}
			writer.Write(weatherUntil);
			writer.Write((int)weather);
			offer.Write(writer.BaseStream);
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
			}, delegate(BinaryWriter w, TreasurePod.State val)
			{
				w.Write((int)val);
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
		}

		public static WorldV15 Load(BinaryReader reader)
		{
			WorldV15 worldV = new WorldV15();
			worldV.Load(reader.BaseStream);
			return worldV;
		}

		public static void AssertAreEqual(WorldV15 expected, WorldV15 actual)
		{
			ExchangeOfferV03.AssertAreEqual(expected.offer, actual.offer);
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
			TestUtil.AssertAreEqual(expected.treasurePods, actual.treasurePods, delegate
			{
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
		}

		public static void AssertAreEqual(WorldV14 expected, WorldV15 actual)
		{
			ExchangeOfferV03.AssertAreEqual(expected.offer, actual.offer);
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
			TestUtil.AssertVersionedAreEqual(expected.placedGadgets, actual.placedGadgets, delegate(PlacedGadgetV03 e, PlacedGadgetV04 a)
			{
				PlacedGadgetV04.AssertAreEqual(e, a);
			}, "placedGadgets");
			TestUtil.AssertAreEqual(expected.treasurePods, actual.treasurePods, delegate
			{
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
			FirestormV01.AssertAreEqual(new FirestormV01(), actual.firestorm);
		}

		protected override void UpgradeFrom(WorldV14 legacyData)
		{
			worldTime = legacyData.worldTime;
			econSeed = legacyData.econSeed;
			dailyOfferCreateTime = legacyData.dailyOfferCreateTime;
			lastRancherOfferId = legacyData.lastRancherOfferId;
			weatherUntil = legacyData.weatherUntil;
			weather = legacyData.weather;
			offer = legacyData.offer;
			econSaturations = legacyData.econSaturations;
			teleportNodeActivations = legacyData.teleportNodeActivations;
			animalSpawnerTimes = legacyData.animalSpawnerTimes;
			liquidSourceUnits = legacyData.liquidSourceUnits;
			spawnerTriggerTimes = legacyData.spawnerTriggerTimes;
			gordos = legacyData.gordos;
			resourceSpawnerWater = legacyData.resourceSpawnerWater;
			placedGadgets = UpgradeGadgets(legacyData.placedGadgets);
			treasurePods = legacyData.treasurePods;
			switches = legacyData.switches;
			puzzleSlotsFilled = legacyData.puzzleSlotsFilled;
			occupiedPhaseSites = legacyData.occupiedPhaseSites;
			firestorm = new FirestormV01();
			oasisStates = new Dictionary<string, bool>();
		}

		protected Dictionary<string, PlacedGadgetV04> UpgradeGadgets(Dictionary<string, PlacedGadgetV03> legacyDict)
		{
			if (legacyDict == null)
			{
				return null;
			}
			Dictionary<string, PlacedGadgetV04> dictionary = new Dictionary<string, PlacedGadgetV04>();
			foreach (KeyValuePair<string, PlacedGadgetV03> item in legacyDict)
			{
				dictionary[item.Key] = new PlacedGadgetV04(item.Value);
			}
			return dictionary;
		}
	}
}
