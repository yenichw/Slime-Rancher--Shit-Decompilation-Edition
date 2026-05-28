using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class PlacedGadgetV03 : VersionedPersistedDataSet<PlacedGadgetV02>
	{
		public Gadget.Id gadgetId;

		public float yRotation;

		public bool isPrimaryInLink;

		public List<AmmoDataV02> ammo;

		public int extractorCyclesRemaining;

		public int extractorQueuedToProduce;

		public double extractorCycleEndTime;

		public double extractorNextProduceTime;

		public double waitForChargeupTime;

		public double lastSpawnTime;

		public Identifiable.Id baitTypeId;

		public Identifiable.Id gordoTypeId;

		public int gordoEatenCount;

		public override string Identifier => "SRPG";

		public override uint Version => 3u;

		public PlacedGadgetV03()
		{
		}

		public PlacedGadgetV03(PlacedGadgetV02 legacyData)
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
		}

		public static PlacedGadgetV03 Load(BinaryReader reader)
		{
			PlacedGadgetV03 placedGadgetV = new PlacedGadgetV03();
			placedGadgetV.Load(reader.BaseStream);
			return placedGadgetV;
		}

		protected override void UpgradeFrom(PlacedGadgetV02 legacyData)
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
			gordoTypeId = Identifiable.Id.NONE;
			baitTypeId = Identifiable.Id.NONE;
			gordoEatenCount = 0;
		}

		public static void AssertAreEqual(PlacedGadgetV03 expected, PlacedGadgetV03 actual)
		{
			for (int i = 0; i < expected.ammo.Count; i++)
			{
				AmmoDataV02.AssertAreEqual(expected.ammo[i], actual.ammo[i]);
			}
		}

		public static void AssertAreEqual(PlacedGadgetV02 expected, PlacedGadgetV03 actual)
		{
			for (int i = 0; i < expected.ammo.Count; i++)
			{
				AmmoDataV02.AssertAreEqual(expected.ammo[i], actual.ammo[i]);
			}
		}
	}
}
