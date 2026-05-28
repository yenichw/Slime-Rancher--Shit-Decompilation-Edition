using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class MailV02 : PersistedDataSet
	{
		public MailDirector.Type mailType;

		public string messageKey;

		public bool isRead;

		public override string Identifier => "SRMAIL";

		public override uint Version => 2u;

		protected override void LoadData(BinaryReader reader)
		{
			mailType = (MailDirector.Type)reader.ReadInt32();
			messageKey = reader.ReadString();
			isRead = reader.ReadBoolean();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)mailType);
			writer.Write(messageKey);
			writer.Write(isRead);
		}

		public static void AssertAreEqual(MailV02 expected, MailV02 actual)
		{
		}
	}
}
