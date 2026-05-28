using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class PlacedGadgetV06 : VersionedPersistedDataSet<PlacedGadgetV05>
	{
		public Gadget.Id gadgetId;

		public float yRotation;

		public bool isPrimaryInLink;

		public List<AmmoDataV02> ammo = new List<AmmoDataV02>();

		public int extractorCyclesRemaining;

		public int extractorQueuedToProduce;

		public double extractorCycleEndTime;

		public double extractorNextProduceTime;

		public double waitForChargeupTime;

		public double lastSpawnTime;

		public Identifiable.Id baitTypeId;

		public Identifiable.Id gordoTypeId;

		public int gordoEatenCount;

		public List<Identifiable.Id> fashions = new List<Identifiable.Id>();

		public DroneGadgetV01 drone;

		public override string Identifier => "SRPG";

		public override uint Version => 6u;

		public PlacedGadgetV06()
		{
		}

		public PlacedGadgetV06(PlacedGadgetV05 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			gadgetId = (Gadget.Id)reader.ReadInt32();
			yRotation = reader.ReadSingle();
			isPrimaryInLink = reader.ReadBoolean();
			ammo = PersistedDataSet.LoadList<AmmoDataV02>(reader);
			extractorCyclesRemaining = reader.ReadInt32();
			extractorQueuedToProduce = reader.ReadInt32();
			extractorCycleEndTime = reader.ReadDouble();
			extractorNextProduceTime = reader.ReadDouble();
			waitForChargeupTime = reader.ReadDouble();
			lastSpawnTime = reader.ReadDouble();
			if (gadgetId == Gadget.Id.HYDRO_SHOWER)
			{
				gadgetId = Gadget.Id.HYDRO_TURRET;
			}
			baitTypeId = (Identifiable.Id)reader.ReadInt32();
			gordoTypeId = (Identifiable.Id)reader.ReadInt32();
			gordoEatenCount = reader.ReadInt32();
			fashions = PersistedDataSet.LoadList(reader, (int v) => (Identifiable.Id)v);
			drone = PersistedDataSet.LoadPersistable<DroneGadgetV01>(reader);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)gadgetId);
			writer.Write(yRotation);
			writer.Write(isPrimaryInLink);
			PersistedDataSet.WriteList(writer, ammo);
			writer.Write(extractorCyclesRemaining);
			writer.Write(extractorQueuedToProduce);
			writer.Write(extractorCycleEndTime);
			writer.Write(extractorNextProduceTime);
			writer.Write(waitForChargeupTime);
			writer.Write(lastSpawnTime);
			writer.Write((int)baitTypeId);
			writer.Write((int)gordoTypeId);
			writer.Write(gordoEatenCount);
			PersistedDataSet.WriteList(writer, fashions, (Identifiable.Id v) => (int)v);
			PersistedDataSet.WritePersistable(writer, drone);
		}

		public static PlacedGadgetV06 Load(BinaryReader reader)
		{
			PlacedGadgetV06 placedGadgetV = new PlacedGadgetV06();
			placedGadgetV.Load(reader.BaseStream);
			return placedGadgetV;
		}

		protected override void UpgradeFrom(PlacedGadgetV05 legacyData)
		{
			gadgetId = legacyData.gadgetId;
			yRotation = legacyData.yRotation;
			isPrimaryInLink = legacyData.isPrimaryInLink;
			ammo = legacyData.ammo;
			extractorCycleEndTime = legacyData.extractorCycleEndTime;
			extractorCyclesRemaining = legacyData.extractorCyclesRemaining;
			extractorNextProduceTime = legacyData.extractorNextProduceTime;
			extractorQueuedToProduce = legacyData.extractorQueuedToProduce;
			waitForChargeupTime = legacyData.waitForChargeupTime;
			lastSpawnTime = legacyData.lastSpawnTime;
			gordoTypeId = legacyData.gordoTypeId;
			baitTypeId = legacyData.baitTypeId;
			gordoEatenCount = legacyData.gordoEatenCount;
			fashions = legacyData.fashions;
			drone = ((legacyData.drone != null) ? new DroneGadgetV01(legacyData.drone) : null);
		}

		public static void AssertAreEqual(PlacedGadgetV06 expected, PlacedGadgetV06 actual)
		{
			for (int i = 0; i < expected.ammo.Count; i++)
			{
				AmmoDataV02.AssertAreEqual(expected.ammo[i], actual.ammo[i]);
			}
			TestUtil.AssertAreEqual(expected.fashions, actual.fashions, "fashions");
			DroneGadgetV01.AssertAreEqual(expected.drone, actual.drone);
		}

		public static void AssertAreEqual(PlacedGadgetV05 expected, PlacedGadgetV06 actual)
		{
			for (int i = 0; i < expected.ammo.Count; i++)
			{
				AmmoDataV02.AssertAreEqual(expected.ammo[i], actual.ammo[i]);
			}
			TestUtil.AssertAreEqual(expected.fashions, actual.fashions, "fashions");
		}
	}
}
