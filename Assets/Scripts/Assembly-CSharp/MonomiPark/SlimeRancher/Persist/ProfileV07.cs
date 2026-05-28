using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class ProfileV07 : VersionedPersistedDataSet_Profile<ProfileV06>
	{
		public PlayerAchievementsV03 achievements = new PlayerAchievementsV03();

		public string continueGameName = string.Empty;

		public DLCV02 DLC = new DLCV02();

		public override uint Version => 7u;

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

		protected override void UpgradeFrom(ProfileV06 previous)
		{
			base.UpgradeFrom(previous);
			achievements = previous.achievements;
			continueGameName = previous.continueGameName;
			DLC = previous.DLC;
		}
	}
}
