using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;

namespace MonomiPark.SlimeRancher.Persist
{
	public class PlayerV13 : VersionedPersistedDataSet<PlayerV12>
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

		public Dictionary<PlayerState.AmmoMode, List<AmmoDataV02>> ammo;

		public List<MailV02> mail;

		public List<PlayerState.Upgrade> availUpgrades;

		public Dictionary<PlayerState.Upgrade, PlayerState.UpgradeLockData> upgradeLocks;

		public Dictionary<ProgressDirector.ProgressType, int> progress;

		public Dictionary<ProgressDirector.ProgressTrackerId, double> delayedProgress;

		public List<Gadget.Id> blueprints;

		public List<Gadget.Id> availBlueprints;

		public Dictionary<Gadget.Id, GadgetDirector.BlueprintLockData> blueprintLocks;

		public Dictionary<Gadget.Id, int> gadgets;

		public Dictionary<Identifiable.Id, int> craftMatCounts;

		public RegionRegistry.RegionSetId regionSetId;

		public List<ZoneDirector.Zone> unlockedZoneMaps;

		public double? endGameTime;

		public override string Identifier => "SRPL";

		public override uint Version => 13u;

		public PlayerV13()
		{
		}

		public PlayerV13(PlayerV12 legacyData)
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
			ammo = LoadDictionary(reader, (BinaryReader r) => (PlayerState.AmmoMode)r.ReadInt32(), (BinaryReader r) => PersistedDataSet.LoadList<AmmoDataV02>(r));
			mail = PersistedDataSet.LoadList<MailV02>(reader);
			availUpgrades = PersistedDataSet.LoadList(reader, (int x) => (PlayerState.Upgrade)x);
			upgradeLocks = LoadDictionary(reader, (BinaryReader r) => (PlayerState.Upgrade)r.ReadInt32(), (BinaryReader r) => new PlayerState.UpgradeLockData(r.ReadBoolean(), r.ReadDouble()));
			progress = LoadDictionary(reader, (BinaryReader r) => (ProgressDirector.ProgressType)r.ReadInt32(), (BinaryReader r) => r.ReadInt32());
			delayedProgress = LoadDictionary(reader, (BinaryReader r) => (ProgressDirector.ProgressTrackerId)r.ReadInt32(), (BinaryReader r) => r.ReadDouble());
			blueprints = PersistedDataSet.LoadList(reader, (int x) => (Gadget.Id)x);
			availBlueprints = PersistedDataSet.LoadList(reader, (int x) => (Gadget.Id)x);
			blueprintLocks = LoadDictionary(reader, (BinaryReader r) => (Gadget.Id)r.ReadInt32(), (BinaryReader r) => new GadgetDirector.BlueprintLockData(r.ReadBoolean(), r.ReadDouble()));
			gadgets = LoadDictionary(reader, (BinaryReader r) => (Gadget.Id)r.ReadInt32(), (BinaryReader r) => r.ReadInt32());
			craftMatCounts = LoadDictionary(reader, (BinaryReader r) => (Identifiable.Id)r.ReadInt32(), (BinaryReader r) => r.ReadInt32());
			regionSetId = (RegionRegistry.RegionSetId)reader.ReadInt32();
			unlockedZoneMaps = PersistedDataSet.LoadList(reader, (int x) => (ZoneDirector.Zone)x);
			LoadNullable(reader, out endGameTime);
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
			WriteDictionary(writer, ammo, delegate(BinaryWriter w, PlayerState.AmmoMode v)
			{
				w.Write((int)v);
			}, delegate(BinaryWriter w, List<AmmoDataV02> v)
			{
				PersistedDataSet.WriteList(w, v);
			});
			PersistedDataSet.WriteList(writer, mail);
			PersistedDataSet.WriteList(writer, availUpgrades, (PlayerState.Upgrade item) => (int)item);
			WriteDictionary(writer, upgradeLocks, delegate(BinaryWriter w, PlayerState.Upgrade v)
			{
				w.Write((int)v);
			}, delegate(BinaryWriter w, PlayerState.UpgradeLockData v)
			{
				w.Write(v.timedLock);
				w.Write(v.lockedUntil);
			});
			WriteDictionary(writer, progress, delegate(BinaryWriter w, ProgressDirector.ProgressType v)
			{
				w.Write((int)v);
			}, delegate(BinaryWriter w, int v)
			{
				w.Write(v);
			});
			WriteDictionary(writer, delayedProgress, delegate(BinaryWriter w, ProgressDirector.ProgressTrackerId v)
			{
				w.Write((int)v);
			}, delegate(BinaryWriter w, double v)
			{
				w.Write(v);
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
			writer.Write((int)regionSetId);
			PersistedDataSet.WriteList(writer, unlockedZoneMaps, (ZoneDirector.Zone item) => (int)item);
			WriteNullable(writer, endGameTime);
		}

		public static PlayerV13 Load(BinaryReader reader)
		{
			PlayerV13 playerV = new PlayerV13();
			playerV.Load(reader.BaseStream);
			return playerV;
		}

		public static void AssertAreEqual(PlayerV13 expected, PlayerV13 actual)
		{
			Vector3V02.AssertAreEqual(expected.playerPos, actual.playerPos);
			Vector3V02.AssertAreApproximatelyEqual(expected.playerRotEuler, actual.playerRotEuler, 0.1f);
			TestUtil.AssertAreEqual(expected.ammo, actual.ammo, delegate(List<AmmoDataV02> a, List<AmmoDataV02> b)
			{
				AmmoDataV02.AssertAreEqual(a, b);
			}, "ammo");
			for (int i = 0; i < expected.mail.Count; i++)
			{
				MailV02.AssertAreEqual(expected.mail[i], actual.mail[i]);
			}
			for (int j = 0; j < expected.upgrades.Count; j++)
			{
			}
			foreach (KeyValuePair<ProgressDirector.ProgressType, int> item in expected.progress)
			{
				_ = item;
			}
			for (int k = 0; k < expected.availUpgrades.Count; k++)
			{
			}
			foreach (KeyValuePair<PlayerState.Upgrade, PlayerState.UpgradeLockData> upgradeLock in expected.upgradeLocks)
			{
				_ = upgradeLock;
			}
			foreach (KeyValuePair<ProgressDirector.ProgressTrackerId, double> item2 in expected.delayedProgress)
			{
				_ = item2;
			}
			for (int l = 0; l < expected.blueprints.Count; l++)
			{
			}
			AssertAreEqual(expected.availBlueprints, actual.availBlueprints);
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
			TestUtil.AssertAreEqual(expected.unlockedZoneMaps, actual.unlockedZoneMaps, "unlockedZoneMaps");
		}

		private static string UpgradesListStr(List<PlayerState.Upgrade> upgrades)
		{
			string text = "";
			foreach (PlayerState.Upgrade upgrade in upgrades)
			{
				text = string.Concat(text, upgrade, ",");
			}
			return text;
		}

		public static void AssertAreEqual(PlayerV12 expected, PlayerV13 actual)
		{
			Vector3V02.AssertAreEqual(expected.playerPos, actual.playerPos);
			Vector3V02.AssertAreEqual(expected.playerRotEuler, actual.playerRotEuler);
			for (int i = 0; i < expected.mail.Count; i++)
			{
				MailV02.AssertAreEqual(expected.mail[i], actual.mail[i]);
			}
			for (int j = 0; j < expected.upgrades.Count; j++)
			{
			}
			foreach (KeyValuePair<ProgressDirector.ProgressType, int> item in expected.progress)
			{
				_ = item;
			}
			foreach (KeyValuePair<PlayerState.Upgrade, PlayerState.UpgradeLockData> upgradeLock in expected.upgradeLocks)
			{
				_ = upgradeLock;
			}
			foreach (KeyValuePair<ProgressDirector.ProgressTrackerId, double> item2 in expected.delayedProgress)
			{
				_ = item2;
			}
			for (int k = 0; k < expected.blueprints.Count; k++)
			{
			}
			AssertAreEqual(expected.availBlueprints, actual.availBlueprints);
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
			TestUtil.AssertAreEqual(expected.unlockedZoneMaps, actual.unlockedZoneMaps, "unlockedZoneMaps");
			foreach (KeyValuePair<PlayerState.AmmoMode, List<AmmoDataV02>> item3 in expected.ammo)
			{
				AmmoDataV02.AssertAreEqual(expected.ammo[item3.Key], actual.ammo[item3.Key]);
			}
		}

		protected override void UpgradeFrom(PlayerV12 legacyData)
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
			mail = legacyData.mail;
			upgradeLocks = legacyData.upgradeLocks;
			progress = legacyData.progress;
			delayedProgress = legacyData.delayedProgress;
			blueprints = legacyData.blueprints;
			availBlueprints = legacyData.availBlueprints;
			blueprintLocks = legacyData.blueprintLocks;
			gadgets = legacyData.gadgets;
			craftMatCounts = legacyData.craftMatCounts;
			availUpgrades = legacyData.availUpgrades;
			regionSetId = legacyData.regionSetId;
			unlockedZoneMaps = legacyData.unlockedZoneMaps;
			ammo = legacyData.ammo;
			endGameTime = null;
		}

		private static void AssertAreEqual(List<Gadget.Id> expected, List<Gadget.Id> actual)
		{
			expected = expected.Except(GadgetsModel.AVAILABLE_BLUEPRINTS).ToList();
			actual = actual.Except(GadgetsModel.AVAILABLE_BLUEPRINTS).ToList();
			for (int i = 0; i < expected.Count; i++)
			{
			}
		}
	}
}
