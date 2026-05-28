using System;
using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class PlayerAchievementProgressV01 : PersistedDataSet
	{
		private enum TempEnum
		{

		}

		public Dictionary<int, int> counts = new Dictionary<int, int>();

		public Dictionary<int, bool> events = new Dictionary<int, bool>();

		public Dictionary<int, List<Enum>> lists = new Dictionary<int, List<Enum>>();

		public override string Identifier => "SRPAP";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			ReadCounts(reader);
			ReadSectionSeparator(reader);
			ReadEvents(reader);
			ReadSectionSeparator(reader);
			ReadLists(reader);
		}

		private void ReadCounts(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			int num2 = 0;
			int num3 = 0;
			while (num > 0)
			{
				ReadElementSeparator(reader);
				num2 = reader.ReadInt32();
				num3 = reader.ReadInt32();
				counts.Add(num2, num3);
				num--;
			}
		}

		private void ReadEvents(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			int num2 = 0;
			bool flag = false;
			while (num > 0)
			{
				ReadElementSeparator(reader);
				num2 = reader.ReadInt32();
				flag = reader.ReadBoolean();
				events.Add(num2, flag);
				num--;
			}
		}

		private void ReadLists(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			int num2 = 0;
			List<Enum> list = null;
			int num3 = 0;
			while (num > 0)
			{
				ReadElementSeparator(reader);
				num2 = reader.ReadInt32();
				num3 = reader.ReadInt32();
				list = new List<Enum>();
				while (num3 > 0)
				{
					list.Add((TempEnum)reader.ReadInt32());
					num3--;
				}
				lists.Add(num2, list);
				num--;
			}
		}

		protected override void WriteData(BinaryWriter writer)
		{
			WriteCounts(writer);
			WriteSectionSeparator(writer);
			WriteEvents(writer);
			WriteSectionSeparator(writer);
			WriteLists(writer);
		}

		private void WriteCounts(BinaryWriter writer)
		{
			writer.Write(counts.Count);
			foreach (KeyValuePair<int, int> count in counts)
			{
				WriteElementSeparator(writer);
				writer.Write(count.Key);
				writer.Write(count.Value);
			}
		}

		private void WriteEvents(BinaryWriter writer)
		{
			writer.Write(events.Count);
			foreach (KeyValuePair<int, bool> @event in events)
			{
				WriteElementSeparator(writer);
				writer.Write(@event.Key);
				writer.Write(@event.Value);
			}
		}

		private void WriteLists(BinaryWriter writer)
		{
			writer.Write(lists.Count);
			foreach (KeyValuePair<int, List<Enum>> list in lists)
			{
				WriteElementSeparator(writer);
				writer.Write(list.Key);
				writer.Write(list.Value.Count);
				foreach (Enum item in list.Value)
				{
					writer.Write(Convert.ToInt32(item));
				}
			}
		}
	}
}
