using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class PlayerAchievementsV03 : PersistedDataSet
	{
		public List<int> earnedAchievements = new List<int>();

		public PlayerAchievementProgressV01 progress = new PlayerAchievementProgressV01();

		public override string Identifier => "SRPA";

		public override uint Version => 3u;

		protected override void LoadData(BinaryReader reader)
		{
			ReadAchievements(reader);
			ReadSectionSeparator(reader);
			progress = new PlayerAchievementProgressV01();
			progress.Load(reader.BaseStream);
		}

		private void ReadAchievements(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			earnedAchievements.Clear();
			while (num > 0)
			{
				earnedAchievements.Add(reader.ReadInt32());
				num--;
			}
		}

		protected override void WriteData(BinaryWriter writer)
		{
			WriteAchievements(writer);
			WriteSectionSeparator(writer);
			progress.Write(writer.BaseStream);
		}

		private void WriteAchievements(BinaryWriter writer)
		{
			writer.Write(earnedAchievements.Count);
			foreach (int earnedAchievement in earnedAchievements)
			{
				writer.Write(earnedAchievement);
			}
		}
	}
}
