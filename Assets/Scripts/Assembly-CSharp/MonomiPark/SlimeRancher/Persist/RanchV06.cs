using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class RanchV06 : VersionedPersistedDataSet<RanchV05>
	{
		public List<LandPlotV07> plots;

		public Dictionary<string, AccessDoor.State> accessDoorStates;

		public Dictionary<RanchDirector.PaletteType, RanchDirector.Palette> palettes;

		public override string Identifier => "SRRANCH";

		public override uint Version => 6u;

		public RanchV06()
		{
		}

		public RanchV06(RanchV05 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			plots = PersistedDataSet.LoadList<LandPlotV07>(reader);
			accessDoorStates = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => (AccessDoor.State)r.ReadInt32());
			palettes = LoadDictionary(reader, (BinaryReader r) => (RanchDirector.PaletteType)r.ReadInt32(), (BinaryReader r) => (RanchDirector.Palette)r.ReadInt32());
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WriteList(writer, plots);
			WriteDictionary(writer, accessDoorStates, delegate(BinaryWriter w, string k)
			{
				w.Write(k);
			}, delegate(BinaryWriter w, AccessDoor.State v)
			{
				w.Write((int)v);
			});
			WriteDictionary(writer, palettes, delegate(BinaryWriter w, RanchDirector.PaletteType k)
			{
				w.Write((int)k);
			}, delegate(BinaryWriter w, RanchDirector.Palette v)
			{
				w.Write((int)v);
			});
		}

		public static RanchV06 Load(BinaryReader reader)
		{
			RanchV06 ranchV = new RanchV06();
			ranchV.Load(reader.BaseStream);
			return ranchV;
		}

		protected override void UpgradeFrom(RanchV05 legacyData)
		{
			plots = new List<LandPlotV07>();
			foreach (LandPlotV04 plot in legacyData.plots)
			{
				plots.Add(new LandPlotV07(new LandPlotV06(new LandPlotV05(plot))));
			}
			accessDoorStates = legacyData.accessDoorStates;
			palettes = new Dictionary<RanchDirector.PaletteType, RanchDirector.Palette>();
		}

		public static void AssertAreEqual(RanchV06 expected, RanchV06 actual)
		{
			for (int i = 0; i < expected.plots.Count; i++)
			{
				LandPlotV07.AssertAreEqual(expected.plots[i], actual.plots[i]);
			}
			TestUtil.AssertAreEqual(expected.accessDoorStates, actual.accessDoorStates, delegate
			{
			}, "accessDoorStates");
			TestUtil.AssertAreEqual(expected.palettes, actual.palettes, delegate
			{
			}, "palettes");
		}

		public static void AssertAreEqual(RanchV05 expected, RanchV06 actual)
		{
			for (int i = 0; i < expected.plots.Count; i++)
			{
				LandPlotV07.AssertAreEqual(new LandPlotV06(new LandPlotV05(expected.plots[i])), actual.plots[i]);
			}
			TestUtil.AssertAreEqual(expected.accessDoorStates, actual.accessDoorStates, delegate
			{
			}, "accessDoorStates");
		}
	}
}
