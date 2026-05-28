using System.IO;
using MonomiPark.SlimeRancher.DataModel;

namespace MonomiPark.SlimeRancher.Persist
{
	public class EchoNoteGordoV01 : PersistedDataSet
	{
		public EchoNoteGordoModel.State state;

		public override string Identifier => "SRENG";

		public override uint Version => 1u;

		public static EchoNoteGordoV01 Load(BinaryReader reader)
		{
			EchoNoteGordoV01 echoNoteGordoV = new EchoNoteGordoV01();
			echoNoteGordoV.Load(reader.BaseStream);
			return echoNoteGordoV;
		}

		protected override void LoadData(BinaryReader reader)
		{
			state = (EchoNoteGordoModel.State)reader.ReadInt32();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)state);
		}

		public static void AssertAreEqual(EchoNoteGordoV01 expected, EchoNoteGordoV01 actual)
		{
		}
	}
}
