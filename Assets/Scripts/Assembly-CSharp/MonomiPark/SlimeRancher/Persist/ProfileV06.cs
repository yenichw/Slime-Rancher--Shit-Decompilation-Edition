using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class ProfileV06 : VersionedPersistedDataSet<ProfileV05>
	{
		public PlayerAchievementsV03 achievements = new PlayerAchievementsV03();

		public string continueGameName = string.Empty;

		public DLCV02 DLC = new DLCV02();

		public override string Identifier => "SRPF";

		public override uint Version => 6u;

		protected override void LoadData(BinaryReader reader)
		{
			achievements = new PlayerAchievementsV03();
			achievements.Load(reader.BaseStream);
			continueGameName = reader.ReadString();
			DLC = PersistedDataSet.LoadPersistable<DLCV02>(reader);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			achievements.Write(writer.BaseStream);
			writer.Write(continueGameName);
			PersistedDataSet.WritePersistable(writer, DLC);
		}

		protected override void UpgradeFrom(ProfileV05 legacyData)
		{
			achievements = legacyData.achievements;
			continueGameName = legacyData.continueGameName;
			DLC = new DLCV02();
		}
	}
}
