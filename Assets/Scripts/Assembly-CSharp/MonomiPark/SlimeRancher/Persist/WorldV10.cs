using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonomiPark.SlimeRancher.Persist
{
	public class WorldV10 : VersionedPersistedDataSet<WorldV09>
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

		public Dictionary<string, PlacedGadgetV01> placedGadgets;

		public override string Identifier => "SRW";

		public override uint Version => 10u;

		public WorldV10()
		{
		}

		public WorldV10(WorldV09 legacyData)
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
			}, delegate(BinaryWriter w, PlacedGadgetV01 pg)
			{
				pg.Write(w.BaseStream);
			});
		}

		public static WorldV10 Load(BinaryReader reader)
		{
			WorldV10 worldV = new WorldV10();
			worldV.Load(reader.BaseStream);
			return worldV;
		}

		public static void AssertAreEqual(WorldV10 expected, WorldV10 actual)
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
			TestUtil.AssertAreEqual(expected.placedGadgets, actual.placedGadgets, delegate(PlacedGadgetV01 e, PlacedGadgetV01 a)
			{
				PlacedGadgetV01.AssertAreEqual(e, a);
			}, "placedGadgets");
		}

		public static void AssertAreEqual(WorldV09 expected, WorldV10 actual)
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
			TestUtil.AssertAreEqual(UpgradeGordoEatsFrom(expected.gordoEatenCounts), actual.gordoEatenCounts, delegate
			{
			}, "gordoEatenCounts");
			TestUtil.AssertAreEqual(expected.resourceSpawnerWater, actual.resourceSpawnerWater, delegate(ResourceWaterV03 e, ResourceWaterV03 a)
			{
				ResourceWaterV03.AssertAreEqual(e, a);
			}, "resourceSpawnerWater");
			TestUtil.AssertAreEqual(expected.placedGadgets, actual.placedGadgets, delegate(PlacedGadgetV01 e, PlacedGadgetV01 a)
			{
				PlacedGadgetV01.AssertAreEqual(e, a);
			}, "placedGadgets");
		}

		protected override void UpgradeFrom(WorldV09 legacyData)
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
			gordoEatenCounts = UpgradeGordoEatsFrom(legacyData.gordoEatenCounts);
			resourceSpawnerWater = legacyData.resourceSpawnerWater;
			placedGadgets = legacyData.placedGadgets;
		}

		public static Dictionary<string, int> UpgradeGordoEatsFrom(Dictionary<Vector3V02, int> legacyData)
		{
			Dictionary<Vector3, string> dictionary = new Dictionary<Vector3, string>();
			dictionary[new Vector3(286.8f, -4.6f, 219.2f)] = "gordo0769818715";
			dictionary[new Vector3(-172.2f, -4.6f, 124.4f)] = "gordo1173217994";
			dictionary[new Vector3(-235.4f, -1.4f, -178.5f)] = "gordo0806598363";
			dictionary[new Vector3(-108.9f, -0.6f, 14.6f)] = "gordo1686430858";
			dictionary[new Vector3(228.2f, 5f, 147f)] = "gordo1831887310";
			dictionary[new Vector3(414.7f, 4.4f, 395.7f)] = "gordo0983207065";
			dictionary[new Vector3(-253.9f, -1.2f, 163.7f)] = "gordo0966530436";
			dictionary[new Vector3(-445f, 9f, 392f)] = "gordo2083992877";
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			foreach (KeyValuePair<Vector3V02, int> legacyDatum in legacyData)
			{
				Vector3 value = legacyDatum.Key.value;
				bool flag = false;
				foreach (KeyValuePair<Vector3, string> item in dictionary)
				{
					if ((value - item.Key).sqrMagnitude < 100f)
					{
						dictionary2[item.Value] = legacyDatum.Value;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Log.Warning("Failed to find gordo match during upgrade: " + value);
				}
			}
			return dictionary2;
		}
	}
}
