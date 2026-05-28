using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class ProfileV05 : VersionedPersistedDataSet<ProfileV04>
	{
		public PlayerAchievementsV03 achievements = new PlayerAchievementsV03();

		public string continueGameName = "";

		public override string Identifier => "SRPF";

		public override uint Version => 5u;

		protected override void LoadData(BinaryReader reader)
		{
			achievements = new PlayerAchievementsV03();
			achievements.Load(reader.BaseStream);
			continueGameName = reader.ReadString();
		}

		protected override void UpgradeFrom(ProfileV04 legacyData)
		{
			achievements = legacyData.achievements;
			continueGameName = legacyData.continueGameName;
		}

		protected override void WriteData(BinaryWriter writer)
		{
			achievements.Write(writer.BaseStream);
			writer.Write(continueGameName);
		}
	}
}
