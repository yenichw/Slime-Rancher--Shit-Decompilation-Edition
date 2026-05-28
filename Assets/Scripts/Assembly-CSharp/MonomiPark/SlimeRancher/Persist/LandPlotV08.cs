using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class LandPlotV08 : VersionedPersistedDataSet<LandPlotV07>
	{
		public double feederNextTime;

		public int feederPendingCount;

		public SlimeFeeder.FeedSpeed feederCycleSpeed;

		public double collectorNextTime;

		public double attachedDeathTime;

		public LandPlot.Id typeId;

		public SpawnResource.Id attachedId;

		public string id;

		public List<LandPlot.Upgrade> upgrades;

		public Dictionary<SiloStorage.StorageType, List<AmmoDataV02>> siloAmmo = new Dictionary<SiloStorage.StorageType, List<AmmoDataV02>>();

		public List<int> siloActivatorIndices = new List<int>();

		public float ashUnits;

		public override string Identifier => "SRLP";

		public override uint Version => 8u;

		public LandPlotV08()
		{
		}

		public LandPlotV08(LandPlotV07 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			feederNextTime = reader.ReadDouble();
			feederPendingCount = reader.ReadInt32();
			feederCycleSpeed = (SlimeFeeder.FeedSpeed)reader.ReadInt32();
			collectorNextTime = reader.ReadDouble();
			attachedDeathTime = reader.ReadDouble();
			typeId = (LandPlot.Id)reader.ReadInt32();
			attachedId = (SpawnResource.Id)reader.ReadInt32();
			id = reader.ReadString();
			upgrades = PersistedDataSet.LoadList(reader, (int v) => (LandPlot.Upgrade)v);
			siloAmmo = LoadDictionary(reader, (BinaryReader r) => (SiloStorage.StorageType)r.ReadInt32(), (BinaryReader r) => PersistedDataSet.LoadList<AmmoDataV02>(reader));
			siloActivatorIndices = PersistedDataSet.LoadList(reader, (int i) => i);
			ashUnits = reader.ReadSingle();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(feederNextTime);
			writer.Write(feederPendingCount);
			writer.Write((int)feederCycleSpeed);
			writer.Write(collectorNextTime);
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
			writer.Write(ashUnits);
		}

		public static void AssertAreEqual(LandPlotV08 expected, LandPlotV08 actual)
		{
			TestUtil.AssertAreEqual(expected.upgrades, actual.upgrades, "upgrades");
			TestUtil.AssertAreEqual(expected.siloAmmo, actual.siloAmmo, delegate(List<AmmoDataV02> e, List<AmmoDataV02> a)
			{
				AmmoDataV02.AssertAreEqual(e, a);
			}, "siloAmmo typeId: " + actual.typeId);
			TestUtil.AssertAreEqual(expected.siloActivatorIndices, actual.siloActivatorIndices, "siloActivatorIndices");
		}

		public static void AssertAreEqual(LandPlotV07 expected, LandPlotV08 actual)
		{
			TestUtil.AssertAreEqual(expected.upgrades, actual.upgrades, "upgrades");
			TestUtil.AssertAreEqual(expected.siloAmmo, actual.siloAmmo, delegate(List<AmmoDataV02> e, List<AmmoDataV02> a)
			{
				AmmoDataV02.AssertAreEqual(e, a);
			}, "siloAmmo");
			TestUtil.AssertAreEqual(expected.siloActivatorIndices, actual.siloActivatorIndices, "siloActivatorIndices");
		}

		protected override void UpgradeFrom(LandPlotV07 legacyData)
		{
			feederNextTime = legacyData.feederNextTime;
			feederPendingCount = legacyData.feederPendingCount;
			feederCycleSpeed = legacyData.feederCycleSpeed;
			collectorNextTime = legacyData.collectorNextTime;
			attachedDeathTime = legacyData.attachedDeathTime;
			typeId = legacyData.typeId;
			attachedId = legacyData.attachedId;
			id = legacyData.id;
			upgrades = legacyData.upgrades;
			siloAmmo = legacyData.siloAmmo;
			siloActivatorIndices = legacyData.siloActivatorIndices;
			feederCycleSpeed = legacyData.feederCycleSpeed;
			ashUnits = UpgradeAshUnitsForType(typeId);
		}

		private static float UpgradeAshUnitsForType(LandPlot.Id typeId)
		{
			if (typeId == LandPlot.Id.INCINERATOR)
			{
				return 20f;
			}
			return 0f;
		}
	}
}
