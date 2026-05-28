using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class PlacedGadgetV07 : VersionedPersistedDataSet<PlacedGadgetV06>
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

		public GadgetPortableSlimeBaitV01 bait;

		public override string Identifier => "SRPG";

		public override uint Version => 7u;

		public PlacedGadgetV07()
		{
		}

		public PlacedGadgetV07(PlacedGadgetV06 legacyData)
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
			bait = PersistedDataSet.LoadPersistable<GadgetPortableSlimeBaitV01>(reader);
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
			PersistedDataSet.WritePersistable(writer, bait);
		}

		public static PlacedGadgetV07 Load(BinaryReader reader)
		{
			PlacedGadgetV07 placedGadgetV = new PlacedGadgetV07();
			placedGadgetV.Load(reader.BaseStream);
			return placedGadgetV;
		}

		protected override void UpgradeFrom(PlacedGadgetV06 legacyData)
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
			drone = legacyData.drone;
			bait = null;
		}

		public static void AssertAreEqual(PlacedGadgetV07 expected, PlacedGadgetV07 actual)
		{
			for (int i = 0; i < expected.ammo.Count; i++)
			{
				AmmoDataV02.AssertAreEqual(expected.ammo[i], actual.ammo[i]);
			}
			TestUtil.AssertAreEqual(expected.fashions, actual.fashions, "fashions");
			DroneGadgetV01.AssertAreEqual(expected.drone, actual.drone);
			GadgetPortableSlimeBaitV01.AssertAreEqual(expected.bait, actual.bait);
		}

		public static void AssertAreEqual(PlacedGadgetV06 expected, PlacedGadgetV07 actual)
		{
			for (int i = 0; i < expected.ammo.Count; i++)
			{
				AmmoDataV02.AssertAreEqual(expected.ammo[i], actual.ammo[i]);
			}
			TestUtil.AssertAreEqual(expected.fashions, actual.fashions, "fashions");
			DroneGadgetV01.AssertAreEqual(expected.drone, actual.drone);
			GadgetPortableSlimeBaitV01.AssertAreEqual(null, actual.bait);
		}
	}
}
