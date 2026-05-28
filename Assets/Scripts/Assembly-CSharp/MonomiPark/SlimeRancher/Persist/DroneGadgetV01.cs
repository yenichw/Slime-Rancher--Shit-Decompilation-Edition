using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class DroneGadgetV01 : PersistedDataSet
	{
		public DroneV05 drone = new DroneV05();

		public DroneStationV01 station = new DroneStationV01();

		public List<DroneProgramV01> programs = new List<DroneProgramV01>();

		public override string Identifier => "SRDRGD";

		public override uint Version => 1u;

		public DroneGadgetV01()
		{
		}

		public DroneGadgetV01(DroneV04 drone)
		{
			this.drone = new DroneV05(drone);
			station = drone.station;
			programs = drone.programs;
		}

		protected override void LoadData(BinaryReader reader)
		{
			drone = PersistedDataSet.LoadPersistable<DroneV05>(reader);
			station = PersistedDataSet.LoadPersistable<DroneStationV01>(reader);
			programs = PersistedDataSet.LoadList<DroneProgramV01>(reader);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WritePersistable(writer, drone);
			PersistedDataSet.WritePersistable(writer, station);
			PersistedDataSet.WriteList(writer, programs);
		}

		public static void AssertAreEqual(DroneGadgetV01 expected, DroneGadgetV01 actual)
		{
			if (TestUtil.AssertNullness(expected, actual))
			{
				DroneV05.AssertAreEqual(expected.drone, actual.drone);
				DroneStationV01.AssertAreEqual(expected.station, actual.station);
				TestUtil.AssertAreEqual(expected.programs, actual.programs, delegate(DroneProgramV01 e, DroneProgramV01 a, string m)
				{
					DroneProgramV01.AssertAreEqual(e, a);
				}, "programs");
			}
		}
	}
}
