using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class PlayerV04 : PersistedDataSet
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

		public Dictionary<PlayerState.Upgrade, float> upgradeLocks;

		public Dictionary<ProgressDirector.ProgressType, int> progress;

		public Dictionary<ProgressDirector.ProgressType, List<float>> delayedProgress;

		public override string Identifier => "SRPL";

		public override uint Version => 4u;

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
			upgradeLocks = LoadDictionary(reader, (BinaryReader r) => (PlayerState.Upgrade)r.ReadInt32(), (BinaryReader r) => r.ReadSingle());
			progress = LoadDictionary(reader, (BinaryReader r) => (ProgressDirector.ProgressType)r.ReadInt32(), (BinaryReader r) => r.ReadInt32());
			delayedProgress = LoadDictionary(reader, (BinaryReader r) => (ProgressDirector.ProgressType)r.ReadInt32(), (BinaryReader r) => PersistedDataSet.LoadList(r, (float val) => val));
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
			}, delegate(BinaryWriter w, float v)
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
			}, delegate(BinaryWriter w, List<float> v)
			{
				PersistedDataSet.WriteList(w, v, (float item) => item);
			});
		}

		public static PlayerV04 Load(BinaryReader reader)
		{
			PlayerV04 playerV = new PlayerV04();
			playerV.Load(reader.BaseStream);
			return playerV;
		}

		public static void AssertAreEqual(PlayerV04 expected, PlayerV04 actual)
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
				TestUtil.AssertAreEqual(item2.Value, actual.delayedProgress[item2.Key], $"Delayed Progress: {item2.Key}");
			}
		}
	}
}
