using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class LandPlotV06 : VersionedPersistedDataSet<LandPlotV05>
	{
		public double feederNextTime;

		public int feederPendingCount;

		public SlimeFeeder.FeedSpeed feederCycleSpeed;

		public double collectorNextTime;

		public double fastforwarderDisableTime;

		public double attachedDeathTime;

		public LandPlot.Id typeId;

		public SpawnResource.Id attachedId;

		public string id;

		public List<LandPlot.Upgrade> upgrades;

		public Dictionary<SiloStorage.StorageType, List<AmmoDataV02>> siloAmmo = new Dictionary<SiloStorage.StorageType, List<AmmoDataV02>>();

		public List<int> siloActivatorIndices = new List<int>();

		public override string Identifier => "SRLP";

		public override uint Version => 6u;

		public LandPlotV06()
		{
		}

		public LandPlotV06(LandPlotV05 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			feederNextTime = reader.ReadDouble();
			feederPendingCount = reader.ReadInt32();
			feederCycleSpeed = (SlimeFeeder.FeedSpeed)reader.ReadInt32();
			collectorNextTime = reader.ReadDouble();
			fastforwarderDisableTime = reader.ReadDouble();
			attachedDeathTime = reader.ReadDouble();
			typeId = (LandPlot.Id)reader.ReadInt32();
			attachedId = (SpawnResource.Id)reader.ReadInt32();
			id = reader.ReadString();
			upgrades = PersistedDataSet.LoadList(reader, (int v) => (LandPlot.Upgrade)v);
			siloAmmo = LoadDictionary(reader, (BinaryReader r) => (SiloStorage.StorageType)r.ReadInt32(), (BinaryReader r) => PersistedDataSet.LoadList<AmmoDataV02>(reader));
			siloActivatorIndices = PersistedDataSet.LoadList(reader, (int i) => i);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(feederNextTime);
			writer.Write(feederPendingCount);
			writer.Write((int)feederCycleSpeed);
			writer.Write(collectorNextTime);
			writer.Write(fastforwarderDisableTime);
			writer.Write(attachedDeathTime);
			writer.Write((int)typeId);
			writer.Write((int)attachedId);
			writer.Write(id);
			PersistedDataSet.WriteList(writer, upgrades, (LandPlot.Upgrade v) => (int)v);
			WriteDictionary(writer, siloAmmo, delegate(BinaryWriter w, SiloStorage.StorageType key)
			{
				w.Write((int)key);
			}, delegate(BinaryWriter w, List<AmmoDataV02> val)
			{
				PersistedDataSet.WriteList(w, val);
			});
			PersistedDataSet.WriteList(writer, siloActivatorIndices, (int i) => i);
		}

		public static void AssertAreEqual(LandPlotV06 expected, LandPlotV06 actual)
		{
			TestUtil.AssertAreEqual(expected.upgrades, actual.upgrades, "upgrades");
			TestUtil.AssertAreEqual(expected.siloAmmo, actual.siloAmmo, delegate(List<AmmoDataV02> e, List<AmmoDataV02> a)
			{
				AmmoDataV02.AssertAreEqual(e, a);
			}, "siloAmmo");
			TestUtil.AssertAreEqual(expected.siloActivatorIndices, actual.siloActivatorIndices, "siloActivatorIndices");
		}

		public static void AssertAreEqual(LandPlotV05 expected, LandPlotV06 actual)
		{
			TestUtil.AssertAreEqual(expected.upgrades, actual.upgrades, "upgrades");
			TestUtil.AssertAreEqual(expected.siloAmmo, actual.siloAmmo, delegate(List<AmmoDataV02> e, List<AmmoDataV02> a)
			{
				AmmoDataV02.AssertAreEqual(e, a);
			}, "siloAmmo");
		}

		protected override void UpgradeFrom(LandPlotV05 legacyData)
		{
			feederNextTime = legacyData.feederNextTime;
			feederPendingCount = legacyData.feederPendingCount;
			feederCycleSpeed = legacyData.feederCycleSpeed;
			collectorNextTime = legacyData.collectorNextTime;
			fastforwarderDisableTime = legacyData.fastforwarderDisableTime;
			attachedDeathTime = legacyData.attachedDeathTime;
			typeId = legacyData.typeId;
			attachedId = legacyData.attachedId;
			id = legacyData.id;
			upgrades = legacyData.upgrades;
			siloAmmo = legacyData.siloAmmo;
			siloActivatorIndices = new List<int>(new int[4]);
		}
	}
}
