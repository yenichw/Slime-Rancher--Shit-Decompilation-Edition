using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class DecorizerV01 : PersistedDataSet
	{
		public Dictionary<Identifiable.Id, int> contents;

		public Dictionary<string, DecorizerSettingsV01> settings;

		public override string Identifier => "SRDZR";

		public override uint Version => 1u;

		public static DecorizerV01 FromLegacy()
		{
			return new DecorizerV01
			{
				contents = new Dictionary<Identifiable.Id, int>(),
				settings = new Dictionary<string, DecorizerSettingsV01>()
			};
		}

		protected override void LoadData(BinaryReader reader)
		{
			contents = LoadDictionary(reader, (BinaryReader r) => (Identifiable.Id)r.ReadInt32(), (BinaryReader r) => r.ReadInt32());
			settings = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => PersistedDataSet.LoadPersistable<DecorizerSettingsV01>(r));
		}

		protected override void WriteData(BinaryWriter writer)
		{
			WriteDictionary(writer, contents, delegate(BinaryWriter w, Identifiable.Id v)
			{
				w.Write((int)v);
			}, delegate(BinaryWriter w, int v)
			{
				w.Write(v);
			});
			WriteDictionary(writer, settings, delegate(BinaryWriter w, string v)
			{
				w.Write(v);
			}, PersistedDataSet.WritePersistable);
		}

		public static void AssertAreEqual(DecorizerV01 expected, DecorizerV01 actual)
		{
			TestUtil.AssertAreEqual(expected.contents, actual.contents, delegate
			{
			}, "contents");
			TestUtil.AssertAreEqual(expected.settings, actual.settings, DecorizerSettingsV01.AssertAreEqual, "settings");
		}
	}
}
