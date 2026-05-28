using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class WorldV08 : VersionedPersistedDataSet<WorldV07>
	{
		private const float MAX_DIST_MATCH = 5f;

		private const float MAX_DIST_MATCH_SQR = 25f;

		private const float MAX_DIST_CLOSE_MATCH = 0.1f;

		private const float MAX_DIST_CLOSE_MATCH_SQR = 0.010000001f;

		public float worldTime;

		public float econSeed;

		public float dailyOfferCreateTime;

		public string lastRancherOfferId;

		public float weatherUntil;

		public AmbianceDirector.Weather weather;

		public ExchangeOfferV02 offer = new ExchangeOfferV02();

		public Dictionary<Identifiable.Id, float> econSaturations;

		public Dictionary<string, bool> teleportNodeActivations;

		public Dictionary<Vector3V02, float> animalSpawnerTimes;

		public Dictionary<Vector3V02, float> liquidSourceUnits;

		public Dictionary<Vector3V02, float> spawnerTriggerTimes;

		public Dictionary<Vector3V02, int> gordoEatenCounts;

		public Dictionary<Vector3V02, ResourceWaterV02> resourceSpawnerWater;

		public Dictionary<string, PlacedGadgetV01> placedGadgets;

		public override string Identifier => "SRW";

		public override uint Version => 8u;

		public WorldV08()
		{
		}

		public WorldV08(WorldV07 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			worldTime = reader.ReadSingle();
			econSeed = reader.ReadSingle();
			dailyOfferCreateTime = reader.ReadSingle();
			if (reader.ReadBoolean())
			{
				lastRancherOfferId = reader.ReadString();
			}
			else
			{
				lastRancherOfferId = null;
			}
			weatherUntil = reader.ReadSingle();
			weather = (AmbianceDirector.Weather)reader.ReadInt32();
			offer = new ExchangeOfferV02();
			offer.Load(reader.BaseStream);
			econSaturations = LoadDictionary(reader, (BinaryReader r) => (Identifiable.Id)r.ReadInt32(), (BinaryReader r) => r.ReadSingle());
			teleportNodeActivations = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => r.ReadBoolean());
			animalSpawnerTimes = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => r.ReadSingle());
			liquidSourceUnits = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => r.ReadSingle());
			spawnerTriggerTimes = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => r.ReadSingle());
			gordoEatenCounts = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => r.ReadInt32());
			resourceSpawnerWater = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => ResourceWaterV02.Load(r));
			placedGadgets = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => PlacedGadgetV01.Load(r));
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
			}, delegate(BinaryWriter w, float val)
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
			}, delegate(BinaryWriter w, float val)
			{
				w.Write(val);
			});
			WriteDictionary(writer, gordoEatenCounts, delegate(BinaryWriter w, Vector3V02 key)
			{
				key.Write(w.BaseStream);
			}, delegate(BinaryWriter w, int val)
			{
				w.Write(val);
			});
			WriteDictionary(writer, resourceSpawnerWater, delegate(BinaryWriter w, Vector3V02 key)
			{
				key.Write(w.BaseStream);
			}, delegate(BinaryWriter w, ResourceWaterV02 val)
			{
				val.Write(w.BaseStream);
			});
			WriteDictionary(writer, placedGadgets, delegate(BinaryWriter w, string s)
			{
				w.Write(s);
			}, delegate(BinaryWriter w, PlacedGadgetV01 pg)
			{
				pg.Write(w.BaseStream);
			});
		}

		public static WorldV08 Load(BinaryReader reader)
		{
			WorldV08 worldV = new WorldV08();
			worldV.Load(reader.BaseStream);
			return worldV;
		}

		public static void AssertAreEqual(WorldV08 expected, WorldV08 actual)
		{
			ExchangeOfferV02.AssertAreEqual(expected.offer, actual.offer);
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
			TestUtil.AssertAreEqual(expected.resourceSpawnerWater, actual.resourceSpawnerWater, delegate(ResourceWaterV02 e, ResourceWaterV02 a)
			{
				ResourceWaterV02.AssertAreEqual(e, a);
			}, "resourceSpawnerWater");
			TestUtil.AssertAreEqual(expected.placedGadgets, actual.placedGadgets, delegate(PlacedGadgetV01 e, PlacedGadgetV01 a)
			{
				PlacedGadgetV01.AssertAreEqual(e, a);
			}, "placedGadgets");
		}

		public static void AssertAreEqual(WorldV07 expected, WorldV08 actual)
		{
			ExchangeOfferV02.AssertAreEqual(expected.offer, actual.offer);
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
			TestUtil.AssertAreEqual(expected.resourceSpawnerWater, actual.resourceSpawnerWater, delegate(ResourceWaterV02 e, ResourceWaterV02 a)
			{
				ResourceWaterV02.AssertAreEqual(e, a);
			}, "resourceSpawnerWater");
		}

		protected override void UpgradeFrom(WorldV07 legacyData)
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
			placedGadgets = new Dictionary<string, PlacedGadgetV01>();
		}
	}
}
