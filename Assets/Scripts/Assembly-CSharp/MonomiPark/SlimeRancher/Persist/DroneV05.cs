using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class DroneV05 : VersionedPersistedDataSet<DroneV04>
	{
		public Vector3V02 position = new Vector3V02();

		public Vector3V02 rotation = new Vector3V02();

		public AmmoDataV02 ammo;

		public List<Identifiable.Id> fashions;

		public bool noClip;

		public override string Identifier => "SRDRONE";

		public override uint Version => 5u;

		public DroneV05()
		{
		}

		public DroneV05(DroneV04 legacy)
		{
			UpgradeFrom(legacy);
		}

		protected override void LoadData(BinaryReader reader)
		{
			position = PersistedDataSet.LoadPersistable<Vector3V02>(reader);
			rotation = PersistedDataSet.LoadPersistable<Vector3V02>(reader);
			ammo = PersistedDataSet.LoadPersistable<AmmoDataV02>(reader);
			fashions = PersistedDataSet.LoadList(reader, (int v) => (Identifiable.Id)v);
			noClip = reader.ReadBoolean();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WritePersistable(writer, position);
			PersistedDataSet.WritePersistable(writer, rotation);
			PersistedDataSet.WritePersistable(writer, ammo);
			PersistedDataSet.WriteList(writer, fashions, (Identifiable.Id v) => (int)v);
			writer.Write(noClip);
		}

		protected override void UpgradeFrom(DroneV04 legacy)
		{
			position = legacy.position;
			rotation = legacy.rotation;
			ammo = legacy.ammo;
			fashions = legacy.fashions;
			noClip = legacy.noClip;
		}

		public static void AssertAreEqual(DroneV05 expected, DroneV05 actual)
		{
			if (TestUtil.AssertNullness(expected, actual))
			{
				AmmoDataV02.AssertAreEqual(expected.ammo, actual.ammo);
				TestUtil.AssertAreEqual(expected.fashions, actual.fashions, "fashions");
			}
		}

		public static void AssertAreEqual(DroneV04 expected, DroneV05 actual)
		{
			if (TestUtil.AssertNullness(expected, actual))
			{
				AmmoDataV02.AssertAreEqual(expected.ammo, actual.ammo);
				TestUtil.AssertAreEqual(expected.fashions, actual.fashions, "fashions");
			}
		}
	}
}
