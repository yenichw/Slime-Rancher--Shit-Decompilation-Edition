using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class BindingV01 : PersistedDataSet
	{
		public string action;

		public int primKey;

		public int primMouse;

		public int secKey;

		public int secMouse;

		public int gamepad;

		public override string Identifier => "SRBINDING";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			action = reader.ReadString();
			primKey = reader.ReadInt32();
			primMouse = reader.ReadInt32();
			secKey = reader.ReadInt32();
			secMouse = reader.ReadInt32();
			gamepad = reader.ReadInt32();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(action);
			writer.Write(primKey);
			writer.Write(primMouse);
			writer.Write(secKey);
			writer.Write(secMouse);
			writer.Write(gamepad);
		}

		public static void AssertAreEqual(BindingsV04.Binding expected, BindingV01 actual)
		{
		}

		public static void AssertAreEqual(BindingV01 expected, BindingV01 actual)
		{
		}
	}
}
