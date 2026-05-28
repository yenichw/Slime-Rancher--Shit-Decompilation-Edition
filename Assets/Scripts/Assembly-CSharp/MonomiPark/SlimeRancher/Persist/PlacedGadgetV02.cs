using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class PlacedGadgetV02 : VersionedPersistedDataSet<PlacedGadgetV01>
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

		public override string Identifier => "SRPG";

		public override uint Version => 2u;

		public PlacedGadgetV02()
		{
		}

		public PlacedGadgetV02(PlacedGadgetV01 legacyData)
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
			if (gadgetId == Gadget.Id.HYDRO_SHOWER)
			{
				gadgetId = Gadget.Id.HYDRO_TURRET;
			}
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
		}

		public static PlacedGadgetV02 Load(BinaryReader reader)
		{
			PlacedGadgetV02 placedGadgetV = new PlacedGadgetV02();
			placedGadgetV.Load(reader.BaseStream);
			return placedGadgetV;
		}

		protected override void UpgradeFrom(PlacedGadgetV01 legacyData)
		{
			gadgetId = legacyData.gadgetId;
			yRotation = legacyData.yRotation;
			isPrimaryInLink = legacyData.isPrimaryInLink;
			ammo = legacyData.ammo;
			extractorCycleEndTime = legacyData.extractorCycleEndTime;
			extractorCyclesRemaining = legacyData.extractorCyclesRemaining;
			extractorNextProduceTime = legacyData.extractorNextProduceTime;
			extractorQueuedToProduce = legacyData.extractorQueuedToProduce;
			waitForChargeupTime = 0.0;
		}

		public static void AssertAreEqual(PlacedGadgetV02 expected, PlacedGadgetV02 actual)
		{
			for (int i = 0; i < expected.ammo.Count; i++)
			{
				AmmoDataV02.AssertAreEqual(expected.ammo[i], actual.ammo[i]);
			}
		}
	}
}
