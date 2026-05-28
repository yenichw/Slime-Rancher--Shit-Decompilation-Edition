using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class DroneStationV01 : PersistedDataSet
	{
		public DroneStationBatteryV01 battery = new DroneStationBatteryV01();

		public override string Identifier => "SRDRST";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			battery = PersistedDataSet.LoadPersistable<DroneStationBatteryV01>(reader);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WritePersistable(writer, battery);
		}

		public static void AssertAreEqual(DroneStationV01 expected, DroneStationV01 actual)
		{
			if (TestUtil.AssertNullness(expected, actual))
			{
				DroneStationBatteryV01.AssertAreEqual(expected.battery, actual.battery);
			}
		}
	}
}
