using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class DroneV04 : VersionedPersistedDataSet<DroneV03>
	{
		public Vector3V02 position;

		public Vector3V02 rotation;

		public AmmoDataV02 ammo;

		public List<DroneProgramV01> programs;

		public List<Identifiable.Id> fashions;

		public DroneStationV01 station;

		public bool noClip;

		public override string Identifier => "SRDRONE";

		public override uint Version => 4u;

		protected override void LoadData(BinaryReader reader)
		{
			position = PersistedDataSet.LoadPersistable<Vector3V02>(reader);
			rotation = PersistedDataSet.LoadPersistable<Vector3V02>(reader);
			ammo = PersistedDataSet.LoadPersistable<AmmoDataV02>(reader);
			programs = PersistedDataSet.LoadList<DroneProgramV01>(reader);
			fashions = PersistedDataSet.LoadList(reader, (int v) => (Identifiable.Id)v);
			station = PersistedDataSet.LoadPersistable<DroneStationV01>(reader);
			noClip = reader.ReadBoolean();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WritePersistable(writer, position);
			PersistedDataSet.WritePersistable(writer, rotation);
			PersistedDataSet.WritePersistable(writer, ammo);
			PersistedDataSet.WriteList(writer, programs);
			PersistedDataSet.WriteList(writer, fashions, (Identifiable.Id v) => (int)v);
			PersistedDataSet.WritePersistable(writer, station);
			writer.Write(noClip);
		}

		protected override void UpgradeFrom(DroneV03 legacy)
		{
			position = legacy.position;
			rotation = legacy.rotation;
			ammo = legacy.ammo;
			programs = legacy.programs;
			fashions = legacy.fashions;
			station = legacy.station;
			noClip = false;
		}

		public static void AssertAreEqual(DroneV04 expected, DroneV04 actual)
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

		public static void AssertAreEqual(DroneV03 expected, DroneV04 actual)
		{
			if (TestUtil.AssertNullness(expected, actual))
			{
				AmmoDataV02.AssertAreEqual(expected.ammo, actual.ammo);
				TestUtil.AssertAreEqual(expected.programs, actual.programs, delegate(DroneProgramV01 e, DroneProgramV01 a, string m)
				{
					DroneProgramV01.AssertAreEqual(e, a);
				}, "programs");
				TestUtil.AssertAreEqual(expected.fashions, actual.fashions, "fashions");
				DroneStationV01.AssertAreEqual(expected.station, actual.station);
			}
		}
	}
}
