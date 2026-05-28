using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class AppearancesV01 : PersistedDataSet
	{
		public Dictionary<Identifiable.Id, List<SlimeAppearance.AppearanceSaveSet>> unlocks = new Dictionary<Identifiable.Id, List<SlimeAppearance.AppearanceSaveSet>>();

		public Dictionary<Identifiable.Id, SlimeAppearance.AppearanceSaveSet> selections = new Dictionary<Identifiable.Id, SlimeAppearance.AppearanceSaveSet>();

		public override string Identifier => "SRAPP";

		public override uint Version => 1u;

		public static AppearancesV01 Load(BinaryReader reader)
		{
			AppearancesV01 appearancesV = new AppearancesV01();
			appearancesV.Load(reader.BaseStream);
			return appearancesV;
		}

		protected override void LoadData(BinaryReader reader)
		{
			unlocks = LoadDictionary(reader, (BinaryReader r) => (Identifiable.Id)r.ReadInt32(), (BinaryReader r) => PersistedDataSet.LoadList(r, (int val) => (SlimeAppearance.AppearanceSaveSet)val));
			selections = LoadDictionary(reader, (BinaryReader r) => (Identifiable.Id)r.ReadInt32(), (BinaryReader r) => (SlimeAppearance.AppearanceSaveSet)r.ReadInt32());
		}

		protected override void WriteData(BinaryWriter writer)
		{
			WriteDictionary(writer, unlocks, delegate(BinaryWriter w, Identifiable.Id v)
			{
				w.Write((int)v);
			}, delegate(BinaryWriter w, List<SlimeAppearance.AppearanceSaveSet> v)
			{
				PersistedDataSet.WriteList(w, v, (SlimeAppearance.AppearanceSaveSet val) => (int)val);
			});
			WriteDictionary(writer, selections, delegate(BinaryWriter w, Identifiable.Id v)
			{
				w.Write((int)v);
			}, delegate(BinaryWriter w, SlimeAppearance.AppearanceSaveSet v)
			{
				w.Write((int)v);
			});
		}

		public static void AssertAreEqual(AppearancesV01 expected, AppearancesV01 actual)
		{
			TestUtil.AssertAreEqual(expected.unlocks, actual.unlocks, delegate(List<SlimeAppearance.AppearanceSaveSet> expectedUnlockList, List<SlimeAppearance.AppearanceSaveSet> actualUnlockList)
			{
				TestUtil.AssertAreEqual(expectedUnlockList, actualUnlockList, "unlock.Value");
			}, "unlocks");
			TestUtil.AssertAreEqual(expected.selections, actual.selections, delegate
			{
			}, "selections");
		}
	}
}
