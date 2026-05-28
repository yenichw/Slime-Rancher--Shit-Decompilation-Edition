using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class HolidayDirectorV01 : PersistedDataSet
	{
		public List<string> eventGordos = new List<string>();

		public override string Identifier => "SRHD";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			eventGordos = PersistedDataSet.LoadList(reader, (string s) => s);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WriteList(writer, eventGordos, (string s) => s);
		}

		public static HolidayDirectorV01 Load(BinaryReader reader)
		{
			HolidayDirectorV01 holidayDirectorV = new HolidayDirectorV01();
			holidayDirectorV.Load(reader.BaseStream);
			return holidayDirectorV;
		}

		public static void AssertAreEqual(HolidayDirectorV01 expected, HolidayDirectorV01 actual)
		{
			TestUtil.AssertAreEqual(expected.eventGordos, actual.eventGordos, "eventGordos");
		}
	}
}
