using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class HolidayDirectorV02 : VersionedPersistedDataSet<HolidayDirectorV01>
	{
		public List<string> eventGordos;

		public List<string> eventEchoNoteGordos;

		public override string Identifier => "SRHD";

		public override uint Version => 2u;

		public static HolidayDirectorV02 Load(BinaryReader reader)
		{
			HolidayDirectorV02 holidayDirectorV = new HolidayDirectorV02();
			holidayDirectorV.Load(reader.BaseStream);
			return holidayDirectorV;
		}

		public HolidayDirectorV02()
		{
			eventGordos = new List<string>();
			eventEchoNoteGordos = new List<string>();
		}

		public HolidayDirectorV02(HolidayDirectorV01 legacy)
		{
			UpgradeFrom(legacy);
		}

		protected override void LoadData(BinaryReader reader)
		{
			eventGordos = PersistedDataSet.LoadList(reader, (string s) => s);
			eventEchoNoteGordos = PersistedDataSet.LoadList(reader, (string s) => s);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WriteList(writer, eventGordos, (string s) => s);
			PersistedDataSet.WriteList(writer, eventEchoNoteGordos, (string s) => s);
		}

		protected override void UpgradeFrom(HolidayDirectorV01 legacy)
		{
			eventGordos = legacy.eventGordos;
			eventEchoNoteGordos = new List<string>();
		}

		public static void AssertAreEqual(HolidayDirectorV02 expected, HolidayDirectorV02 actual)
		{
			TestUtil.AssertAreEqual(expected.eventGordos, actual.eventGordos, "eventGordos");
			TestUtil.AssertAreEqual(expected.eventEchoNoteGordos, actual.eventEchoNoteGordos, "eventEchoNoteGordos");
		}

		public static void AssertAreEqual(HolidayDirectorV01 expected, HolidayDirectorV02 actual)
		{
			TestUtil.AssertAreEqual(expected.eventGordos, actual.eventGordos, "eventGordos");
		}
	}
}
