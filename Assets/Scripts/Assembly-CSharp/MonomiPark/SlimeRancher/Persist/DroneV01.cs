using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class DroneV01 : PersistedDataSet
	{
		public Vector3V02 position;

		public Vector3V02 rotation;

		public AmmoDataV02 ammo;

		public List<DroneProgramV01> programs;

		public override string Identifier => "SRDRONE";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			position = PersistedDataSet.LoadPersistable<Vector3V02>(reader);
			rotation = PersistedDataSet.LoadPersistable<Vector3V02>(reader);
			ammo = PersistedDataSet.LoadPersistable<AmmoDataV02>(reader);
			programs = PersistedDataSet.LoadList<DroneProgramV01>(reader);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WritePersistable(writer, position);
			PersistedDataSet.WritePersistable(writer, rotation);
			PersistedDataSet.WritePersistable(writer, ammo);
			PersistedDataSet.WriteList(writer, programs);
		}

		public static void AssertAreEqual(DroneV01 expected, DroneV01 actual)
		{
			if (TestUtil.AssertNullness(expected, actual))
			{
				AmmoDataV02.AssertAreEqual(expected.ammo, actual.ammo);
				TestUtil.AssertAreEqual(expected.programs, actual.programs, delegate(DroneProgramV01 e, DroneProgramV01 a, string m)
				{
					DroneProgramV01.AssertAreEqual(e, a);
				}, "programs");
			}
		}
	}
}
