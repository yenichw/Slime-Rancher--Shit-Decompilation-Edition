using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class DroneStationBatteryV01 : PersistedDataSet
	{
		public double time;

		public override string Identifier => "SRDRSTB";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			time = reader.ReadDouble();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(time);
		}

		public static void AssertAreEqual(DroneStationBatteryV01 expected, DroneStationBatteryV01 actual)
		{
			TestUtil.AssertNullness(expected, actual);
		}

		public static DroneStationBatteryV01 GetSample()
		{
			return new DroneStationBatteryV01
			{
				time = 3200.0
			};
		}
	}
}
