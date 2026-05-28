using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class RanchV07 : VersionedPersistedDataSet<RanchV06>
	{
		public List<LandPlotV08> plots;

		public Dictionary<string, AccessDoor.State> accessDoorStates;

		public Dictionary<RanchDirector.PaletteType, RanchDirector.Palette> palettes;

		public Dictionary<string, double> ranchFastForward;

		public override string Identifier => "SRRANCH";

		public override uint Version => 7u;

		public RanchV07()
		{
		}

		public RanchV07(RanchV06 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			plots = PersistedDataSet.LoadList<LandPlotV08>(reader);
			accessDoorStates = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => (AccessDoor.State)r.ReadInt32());
			palettes = LoadDictionary(reader, (BinaryReader r) => (RanchDirector.PaletteType)r.ReadInt32(), (BinaryReader r) => (RanchDirector.Palette)r.ReadInt32());
			ranchFastForward = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => r.ReadDouble());
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
			WriteDictionary(writer, ranchFastForward, delegate(BinaryWriter w, string k)
			{
				w.Write(k);
			}, delegate(BinaryWriter w, double v)
			{
				w.Write(v);
			});
		}

		public static RanchV07 Load(BinaryReader reader)
		{
			RanchV07 ranchV = new RanchV07();
			ranchV.Load(reader.BaseStream);
			return ranchV;
		}

		protected override void UpgradeFrom(RanchV06 legacyData)
		{
			plots = UpgradeFrom(legacyData.plots);
			accessDoorStates = legacyData.accessDoorStates;
			palettes = legacyData.palettes;
			ranchFastForward = new Dictionary<string, double>();
		}

		private List<LandPlotV08> UpgradeFrom(List<LandPlotV07> legacyData)
		{
			if (legacyData == null)
			{
				return null;
			}
			List<LandPlotV08> list = new List<LandPlotV08>();
			foreach (LandPlotV07 legacyDatum in legacyData)
			{
				list.Add(new LandPlotV08(legacyDatum));
			}
			return list;
		}

		public static void AssertAreEqual(RanchV07 expected, RanchV07 actual)
		{
			TestUtil.AssertAreEqual(expected.plots, actual.plots, delegate(LandPlotV08 e, LandPlotV08 a, string m)
			{
				LandPlotV08.AssertAreEqual(e, a);
			}, "plots");
			TestUtil.AssertAreEqual(expected.accessDoorStates, actual.accessDoorStates, delegate
			{
			}, "accessDoorStates");
			TestUtil.AssertAreEqual(expected.palettes, actual.palettes, delegate
			{
			}, "palettes");
			TestUtil.AssertAreEqual(expected.ranchFastForward, actual.ranchFastForward, delegate
			{
			}, "ranchFastForward");
		}

		public static void AssertAreEqual(RanchV06 expected, RanchV07 actual)
		{
			TestUtil.AssertAreEqual(expected.plots, actual.plots, delegate(LandPlotV07 e, LandPlotV08 a, string m)
			{
				LandPlotV08.AssertAreEqual(e, a);
			}, "plots");
			TestUtil.AssertAreEqual(expected.accessDoorStates, actual.accessDoorStates, delegate
			{
			}, "accessDoorStates");
			TestUtil.AssertAreEqual(expected.palettes, actual.palettes, delegate
			{
			}, "palettes");
		}
	}
}
