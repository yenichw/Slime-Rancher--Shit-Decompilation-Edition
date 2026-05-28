using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class PlayerV06 : VersionedPersistedDataSet<PlayerV05>
	{
		public int health;

		public int energy;

		public int radiation;

		public int currency;

		public int keys;

		public int currencyEverCollected;

		public string version = "0.3.0";

		public PlayerState.GameMode gameMode;

		public Identifiable.Id gameIconId = Identifiable.Id.CARROT_VEGGIE;

		public Vector3V02 playerPos;

		public Vector3V02 playerRotEuler;

		public List<PlayerState.Upgrade> upgrades;

		public List<AmmoDataV02> ammo;

		public List<MailV02> mail;

		public Dictionary<PlayerState.Upgrade, double> upgradeLocks;

		public Dictionary<ProgressDirector.ProgressType, int> progress;

		public Dictionary<ProgressDirector.ProgressType, List<double>> delayedProgress;

		public List<Gadget.Id> blueprints;

		public List<Gadget.Id> availBlueprints;

		public Dictionary<Gadget.Id, GadgetDirector.BlueprintLockData> blueprintLocks;

		public Dictionary<Gadget.Id, int> gadgets;

		public Dictionary<Identifiable.Id, int> craftMatCounts;

		public override string Identifier => "SRPL";

		public override uint Version => 6u;

		public PlayerV06()
		{
		}

		public PlayerV06(PlayerV05 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			health = reader.ReadInt32();
			energy = reader.ReadInt32();
			radiation = reader.ReadInt32();
			currency = reader.ReadInt32();
			keys = reader.ReadInt32();
			currencyEverCollected = reader.ReadInt32();
			version = reader.ReadString();
			gameMode = (PlayerState.GameMode)reader.ReadInt32();
			gameIconId = (Identifiable.Id)reader.ReadInt32();
			playerPos = new Vector3V02();
			playerPos.Load(reader.BaseStream);
			playerRotEuler = new Vector3V02();
			playerRotEuler.Load(reader.BaseStream);
			upgrades = PersistedDataSet.LoadList(reader, (int x) => (PlayerState.Upgrade)x);
			ammo = PersistedDataSet.LoadList<AmmoDataV02>(reader);
			mail = PersistedDataSet.LoadList<MailV02>(reader);
			upgradeLocks = LoadDictionary(reader, (BinaryReader r) => (PlayerState.Upgrade)r.ReadInt32(), (BinaryReader r) => r.ReadDouble());
			progress = LoadDictionary(reader, (BinaryReader r) => (ProgressDirector.ProgressType)r.ReadInt32(), (BinaryReader r) => r.ReadInt32());
			delayedProgress = LoadDictionary(reader, (BinaryReader r) => (ProgressDirector.ProgressType)r.ReadInt32(), (BinaryReader r) => PersistedDataSet.LoadList(r, (double val) => val));
			blueprints = PersistedDataSet.LoadList(reader, (int x) => (Gadget.Id)x);
			availBlueprints = PersistedDataSet.LoadList(reader, (int x) => (Gadget.Id)x);
			blueprintLocks = LoadDictionary(reader, (BinaryReader r) => (Gadget.Id)r.ReadInt32(), (BinaryReader r) => new GadgetDirector.BlueprintLockData(r.ReadBoolean(), r.ReadDouble()));
			gadgets = LoadDictionary(reader, (BinaryReader r) => (Gadget.Id)r.ReadInt32(), (BinaryReader r) => r.ReadInt32());
			craftMatCounts = LoadDictionary(reader, (BinaryReader r) => (Identifiable.Id)r.ReadInt32(), (BinaryReader r) => r.ReadInt32());
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(health);
			writer.Write(energy);
			writer.Write(radiation);
			writer.Write(currency);
			writer.Write(keys);
			writer.Write(currencyEverCollected);
			writer.Write(version);
			writer.Write((int)gameMode);
			writer.Write((int)gameIconId);
			playerPos.Write(writer.BaseStream);
			playerRotEuler.Write(writer.BaseStream);
			PersistedDataSet.WriteList(writer, upgrades, (PlayerState.Upgrade item) => (int)item);
			PersistedDataSet.WriteList(writer, ammo);
			PersistedDataSet.WriteList(writer, mail);
			WriteDictionary(writer, upgradeLocks, delegate(BinaryWriter w, PlayerState.Upgrade v)
			{
				w.Write((int)v);
			}, delegate(BinaryWriter w, double v)
			{
				w.Write(v);
			});
			WriteDictionary(writer, progress, delegate(BinaryWriter w, ProgressDirector.ProgressType v)
			{
				w.Write((int)v);
			}, delegate(BinaryWriter w, int v)
			{
				w.Write(v);
			});
			WriteDictionary(writer, delayedProgress, delegate(BinaryWriter w, ProgressDirector.ProgressType v)
			{
				w.Write((int)v);
			}, delegate(BinaryWriter w, List<double> v)
			{
				PersistedDataSet.WriteList(w, v, (double item) => item);
			});
			PersistedDataSet.WriteList(writer, blueprints, (Gadget.Id item) => (int)item);
			PersistedDataSet.WriteList(writer, availBlueprints, (Gadget.Id item) => (int)item);
			WriteDictionary(writer, blueprintLocks, delegate(BinaryWriter w, Gadget.Id v)
			{
				w.Write((int)v);
			}, delegate(BinaryWriter w, GadgetDirector.BlueprintLockData v)
			{
				w.Write(v.timedLock);
				w.Write(v.lockedUntil);
			});
			WriteDictionary(writer, gadgets, delegate(BinaryWriter w, Gadget.Id v)
			{
				w.Write((int)v);
			}, delegate(BinaryWriter w, int v)
			{
				w.Write(v);
			});
			WriteDictionary(writer, craftMatCounts, delegate(BinaryWriter w, Identifiable.Id v)
			{
				w.Write((int)v);
			}, delegate(BinaryWriter w, int v)
			{
				w.Write(v);
			});
		}

		public static PlayerV06 Load(BinaryReader reader)
		{
			PlayerV06 playerV = new PlayerV06();
			playerV.Load(reader.BaseStream);
			return playerV;
		}

		public static void AssertAreEqual(PlayerV06 expected, PlayerV06 actual)
		{
			Vector3V02.AssertAreEqual(expected.playerPos, actual.playerPos);
			Vector3V02.AssertAreEqual(expected.playerRotEuler, actual.playerRotEuler);
			for (int i = 0; i < expected.ammo.Count; i++)
			{
				AmmoDataV02.AssertAreEqual(expected.ammo[i], actual.ammo[i]);
			}
			for (int j = 0; j < expected.mail.Count; j++)
			{
				MailV02.AssertAreEqual(expected.mail[j], actual.mail[j]);
			}
			for (int k = 0; k < expected.upgrades.Count; k++)
			{
			}
			foreach (KeyValuePair<ProgressDirector.ProgressType, int> item in expected.progress)
			{
				_ = item;
			}
			foreach (KeyValuePair<PlayerState.Upgrade, double> upgradeLock in expected.upgradeLocks)
			{
				_ = upgradeLock;
			}
			foreach (KeyValuePair<ProgressDirector.ProgressType, List<double>> item2 in expected.delayedProgress)
			{
				TestUtil.AssertAreEqual(item2.Value, actual.delayedProgress[item2.Key], $"Delayed Progress: {item2.Key}");
			}
			for (int l = 0; l < expected.blueprints.Count; l++)
			{
			}
			for (int m = 0; m < expected.availBlueprints.Count; m++)
			{
			}
			foreach (KeyValuePair<Gadget.Id, GadgetDirector.BlueprintLockData> blueprintLock in expected.blueprintLocks)
			{
				_ = blueprintLock;
			}
			foreach (KeyValuePair<Gadget.Id, int> gadget in expected.gadgets)
			{
				_ = gadget;
			}
			foreach (KeyValuePair<Identifiable.Id, int> craftMatCount in expected.craftMatCounts)
			{
				_ = craftMatCount;
			}
		}

		public static void AssertAreEqual(PlayerV05 expected, PlayerV06 actual)
		{
			Vector3V02.AssertAreEqual(expected.playerPos, actual.playerPos);
			Vector3V02.AssertAreEqual(expected.playerRotEuler, actual.playerRotEuler);
			for (int i = 0; i < expected.ammo.Count; i++)
			{
				AmmoDataV02.AssertAreEqual(expected.ammo[i], actual.ammo[i]);
			}
			for (int j = 0; j < expected.mail.Count; j++)
			{
				MailV02.AssertAreEqual(expected.mail[j], actual.mail[j]);
			}
			for (int k = 0; k < expected.upgrades.Count; k++)
			{
			}
			foreach (KeyValuePair<ProgressDirector.ProgressType, int> item in expected.progress)
			{
				_ = item;
			}
			foreach (KeyValuePair<PlayerState.Upgrade, float> upgradeLock in expected.upgradeLocks)
			{
				_ = upgradeLock;
			}
			foreach (KeyValuePair<ProgressDirector.ProgressType, List<float>> item2 in expected.delayedProgress)
			{
				TestUtil.AssertAreEqual(UpgradeFrom(item2.Value), actual.delayedProgress[item2.Key], $"Delayed Progress: {item2.Key}");
			}
			for (int l = 0; l < expected.blueprints.Count; l++)
			{
			}
			for (int m = 0; m < expected.availBlueprints.Count; m++)
			{
			}
			foreach (KeyValuePair<Gadget.Id, GadgetDirector.BlueprintLockData> blueprintLock in expected.blueprintLocks)
			{
				_ = blueprintLock;
			}
			foreach (KeyValuePair<Gadget.Id, int> gadget in expected.gadgets)
			{
				_ = gadget;
			}
			foreach (KeyValuePair<Identifiable.Id, int> craftMatCount in expected.craftMatCounts)
			{
				_ = craftMatCount;
			}
		}

		protected override void UpgradeFrom(PlayerV05 legacyData)
		{
			health = legacyData.health;
			energy = legacyData.energy;
			radiation = legacyData.radiation;
			currency = legacyData.currency;
			keys = legacyData.keys;
			currencyEverCollected = legacyData.currencyEverCollected;
			version = legacyData.version;
			gameMode = legacyData.gameMode;
			gameIconId = legacyData.gameIconId;
			playerPos = legacyData.playerPos;
			playerRotEuler = legacyData.playerRotEuler;
			upgrades = legacyData.upgrades;
			ammo = legacyData.ammo;
			mail = legacyData.mail;
			upgradeLocks = UpgradeFrom(legacyData.upgradeLocks);
			progress = legacyData.progress;
			delayedProgress = UpgradeFrom(legacyData.delayedProgress);
			blueprints = legacyData.blueprints;
			availBlueprints = legacyData.availBlueprints;
			blueprintLocks = legacyData.blueprintLocks;
			gadgets = legacyData.gadgets;
			craftMatCounts = legacyData.craftMatCounts;
		}

		private static List<double> UpgradeFrom(List<float> legacyList)
		{
			List<double> list = new List<double>();
			foreach (float legacy in legacyList)
			{
				list.Add(legacy);
			}
			return list;
		}

		public static Dictionary<PlayerState.Upgrade, double> UpgradeFrom(Dictionary<PlayerState.Upgrade, float> legacyDict)
		{
			Dictionary<PlayerState.Upgrade, double> dictionary = new Dictionary<PlayerState.Upgrade, double>();
			foreach (KeyValuePair<PlayerState.Upgrade, float> item in legacyDict)
			{
				dictionary[item.Key] = item.Value;
			}
			return dictionary;
		}

		public static Dictionary<ProgressDirector.ProgressType, List<double>> UpgradeFrom(Dictionary<ProgressDirector.ProgressType, List<float>> legacyDict)
		{
			Dictionary<ProgressDirector.ProgressType, List<double>> dictionary = new Dictionary<ProgressDirector.ProgressType, List<double>>();
			foreach (KeyValuePair<ProgressDirector.ProgressType, List<float>> item in legacyDict)
			{
				dictionary[item.Key] = UpgradeFrom(item.Value);
			}
			return dictionary;
		}
	}
}
