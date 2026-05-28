using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class WorldV11 : VersionedPersistedDataSet<WorldV10>
	{
		private const float MAX_DIST_MATCH = 5f;

		private const float MAX_DIST_MATCH_SQR = 25f;

		private const float MAX_DIST_CLOSE_MATCH = 0.1f;

		private const float MAX_DIST_CLOSE_MATCH_SQR = 0.010000001f;

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

		public Dictionary<string, int> gordoEatenCounts;

		public Dictionary<Vector3V02, ResourceWaterV03> resourceSpawnerWater;

		public Dictionary<string, PlacedGadgetV02> placedGadgets;

		public Dictionary<string, TreasurePod.State> treasurePods;

		public override string Identifier => "SRW";

		public override uint Version => 11u;

		public WorldV11()
		{
		}

		public WorldV11(WorldV10 legacyData)
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
			gordoEatenCounts = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => r.ReadInt32());
			resourceSpawnerWater = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => ResourceWaterV03.Load(r));
			placedGadgets = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => PlacedGadgetV02.Load(r));
			treasurePods = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => (TreasurePod.State)r.ReadInt32());
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
			WriteDictionary(writer, gordoEatenCounts, delegate(BinaryWriter w, string key)
			{
				w.Write(key);
			}, delegate(BinaryWriter w, int val)
			{
				w.Write(val);
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
			}, delegate(BinaryWriter w, PlacedGadgetV02 pg)
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
		}

		public static WorldV11 Load(BinaryReader reader)
		{
			WorldV11 worldV = new WorldV11();
			worldV.Load(reader.BaseStream);
			return worldV;
		}

		public static void AssertAreEqual(WorldV11 expected, WorldV11 actual)
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
			TestUtil.AssertAreEqual(expected.gordoEatenCounts, actual.gordoEatenCounts, delegate
			{
			}, "gordoEatenCounts");
			TestUtil.AssertAreEqual(expected.resourceSpawnerWater, actual.resourceSpawnerWater, delegate(ResourceWaterV03 e, ResourceWaterV03 a)
			{
				ResourceWaterV03.AssertAreEqual(e, a);
			}, "resourceSpawnerWater");
			TestUtil.AssertAreEqual(expected.placedGadgets, actual.placedGadgets, delegate(PlacedGadgetV02 e, PlacedGadgetV02 a)
			{
				PlacedGadgetV02.AssertAreEqual(e, a);
			}, "placedGadgets");
			TestUtil.AssertAreEqual(expected.treasurePods, actual.treasurePods, delegate
			{
			}, "treasurePods");
		}

		public static void AssertAreEqual(WorldV10 expected, WorldV11 actual)
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
			TestUtil.AssertAreEqual(expected.gordoEatenCounts, actual.gordoEatenCounts, delegate
			{
			}, "gordoEatenCounts");
			TestUtil.AssertAreEqual(expected.resourceSpawnerWater, actual.resourceSpawnerWater, delegate(ResourceWaterV03 e, ResourceWaterV03 a)
			{
				ResourceWaterV03.AssertAreEqual(e, a);
			}, "resourceSpawnerWater");
			TestUtil.AssertAreEqual(UpgradeFrom(expected.placedGadgets), actual.placedGadgets, delegate(PlacedGadgetV02 e, PlacedGadgetV02 a)
			{
				PlacedGadgetV02.AssertAreEqual(e, a);
			}, "placedGadgets");
		}

		protected override void UpgradeFrom(WorldV10 legacyData)
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
			gordoEatenCounts = legacyData.gordoEatenCounts;
			resourceSpawnerWater = legacyData.resourceSpawnerWater;
			placedGadgets = UpgradeFrom(legacyData.placedGadgets);
			treasurePods = new Dictionary<string, TreasurePod.State>();
		}

		private static Dictionary<string, PlacedGadgetV02> UpgradeFrom(Dictionary<string, PlacedGadgetV01> legacyData)
		{
			Dictionary<string, PlacedGadgetV02> dictionary = new Dictionary<string, PlacedGadgetV02>();
			foreach (KeyValuePair<string, PlacedGadgetV01> legacyDatum in legacyData)
			{
				dictionary[legacyDatum.Key] = new PlacedGadgetV02(legacyDatum.Value);
			}
			return dictionary;
		}
	}
}
