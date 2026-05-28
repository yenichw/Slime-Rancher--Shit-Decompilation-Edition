using System.Collections.Generic;
using System.IO;
using MonomiPark.SlimeRancher.DataModel;

namespace MonomiPark.SlimeRancher.Persist
{
	public class InstrumentV01 : PersistedDataSet
	{
		public List<InstrumentModel.Instrument> unlocks = new List<InstrumentModel.Instrument>();

		public InstrumentModel.Instrument selection = InstrumentModel.Instrument.NONE;

		public override string Identifier => "SRINSTR";

		public override uint Version => 1u;

		public static InstrumentV01 Load(BinaryReader reader)
		{
			InstrumentV01 instrumentV = new InstrumentV01();
			instrumentV.Load(reader.BaseStream);
			return instrumentV;
		}

		protected override void LoadData(BinaryReader reader)
		{
			unlocks = PersistedDataSet.LoadList(reader, (int val) => (InstrumentModel.Instrument)val);
			selection = (InstrumentModel.Instrument)reader.ReadInt32();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WriteList(writer, unlocks, (InstrumentModel.Instrument instrument) => (int)instrument);
			writer.Write((int)selection);
		}

		public static void AssertAreEqual(InstrumentV01 expected, InstrumentV01 actual)
		{
			TestUtil.AssertAreEqual(expected.unlocks, actual.unlocks);
		}
	}
}
