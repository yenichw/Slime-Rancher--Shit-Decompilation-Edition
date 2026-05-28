using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class DroneV03 : VersionedPersistedDataSet<DroneV02>
	{
		public Vector3V02 position;

		public Vector3V02 rotation;

		public AmmoDataV02 ammo;

		public List<DroneProgramV01> programs;

		public List<Identifiable.Id> fashions;

		public DroneStationV01 station;

		public override string Identifier => "SRDRONE";

		public override uint Version => 3u;

		protected override void LoadData(BinaryReader reader)
		{
			position = PersistedDataSet.LoadPersistable<Vector3V02>(reader);
			rotation = PersistedDataSet.LoadPersistable<Vector3V02>(reader);
			ammo = PersistedDataSet.LoadPersistable<AmmoDataV02>(reader);
			programs = PersistedDataSet.LoadList<DroneProgramV01>(reader);
			fashions = PersistedDataSet.LoadList(reader, (int v) => (Identifiable.Id)v);
			station = PersistedDataSet.LoadPersistable<DroneStationV01>(reader);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WritePersistable(writer, position);
			PersistedDataSet.WritePersistable(writer, rotation);
			PersistedDataSet.WritePersistable(writer, ammo);
			PersistedDataSet.WriteList(writer, programs);
			PersistedDataSet.WriteList(writer, fashions, (Identifiable.Id v) => (int)v);
			PersistedDataSet.WritePersistable(writer, station);
		}

		protected override void UpgradeFrom(DroneV02 legacy)
		{
			position = legacy.position;
			rotation = legacy.rotation;
			ammo = legacy.ammo;
			programs = legacy.programs;
			fashions = legacy.fashions;
			station = new DroneStationV01();
		}

		public static void AssertAreEqual(DroneV03 expected, DroneV03 actual)
		{
			if (TestUtil.AssertNullness(expected, actual))
			{
				AmmoDataV02.AssertAreEqual(expected.ammo, actual.ammo);
				TestUtil.AssertAreEqual(expected.programs, actual.programs, delegate(DroneProgramV01 e, DroneProgramV01 a, string m)
				{
					DroneProgramV01.AssertAreEqual(e, a);
				}, "programs");
				TestUtil.AssertAreEqual(expected.fashions, actual.fashions, "fashions");
			}
		}

		public static void AssertAreEqual(DroneV02 expected, DroneV03 actual)
		{
			if (TestUtil.AssertNullness(expected, actual))
			{
				AmmoDataV02.AssertAreEqual(expected.ammo, actual.ammo);
				TestUtil.AssertAreEqual(expected.programs, actual.programs, delegate(DroneProgramV01 e, DroneProgramV01 a, string m)
				{
					DroneProgramV01.AssertAreEqual(e, a);
				}, "programs");
				TestUtil.AssertAreEqual(expected.fashions, actual.fashions, "fashions");
				DroneStationV01.AssertAreEqual(new DroneStationV01(), actual.station);
			}
		}
	}
}
