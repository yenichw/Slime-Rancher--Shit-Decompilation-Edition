using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class RanchV04 : PersistedDataSet
	{
		public List<LandPlotV03> plots;

		public Dictionary<Vector3V02, AccessDoor.State> accessDoorStates;

		public override string Identifier => "SRRANCH";

		public override uint Version => 4u;

		protected override void LoadData(BinaryReader reader)
		{
			plots = PersistedDataSet.LoadList<LandPlotV03>(reader);
			accessDoorStates = LoadDictionary(reader, (BinaryReader r) => Vector3V02.Load(r), (BinaryReader r) => (AccessDoor.State)r.ReadInt32());
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WriteList(writer, plots);
			WriteDictionary(writer, accessDoorStates, delegate(BinaryWriter w, Vector3V02 k)
			{
				k.Write(w.BaseStream);
			}, delegate(BinaryWriter w, AccessDoor.State v)
			{
				w.Write((int)v);
			});
		}

		public static RanchV04 Load(BinaryReader reader)
		{
			RanchV04 ranchV = new RanchV04();
			ranchV.Load(reader.BaseStream);
			return ranchV;
		}

		public static void AssertAreEqual(RanchV04 expected, RanchV04 actual)
		{
			for (int i = 0; i < expected.plots.Count; i++)
			{
				LandPlotV03.AssertAreEqual(expected.plots[i], actual.plots[i]);
			}
			TestUtil.AssertAreEqual(expected.accessDoorStates, actual.accessDoorStates, delegate
			{
			}, "accessDoorStates");
		}
	}
}
