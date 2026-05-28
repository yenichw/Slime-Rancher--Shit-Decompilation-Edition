using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class DroneProgramV01 : PersistedDataSet
	{
		public string target;

		public string source;

		public string destination;

		public override string Identifier => "SRDRONEPROG";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			target = reader.ReadString();
			source = reader.ReadString();
			destination = reader.ReadString();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(target);
			writer.Write(source);
			writer.Write(destination);
		}

		public static void AssertAreEqual(DroneProgramV01 expected, DroneProgramV01 actual)
		{
		}

		public override string ToString()
		{
			return $"{GetType()} [target={target}, source={source}, destination={destination}]";
		}
	}
}
