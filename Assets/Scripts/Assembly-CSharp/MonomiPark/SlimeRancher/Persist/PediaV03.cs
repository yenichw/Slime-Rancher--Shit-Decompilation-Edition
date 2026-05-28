using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class PediaV03 : VersionedPersistedDataSet<PediaV02>
	{
		public List<string> unlockedIds = new List<string>();

		public List<string> completedTuts = new List<string>();

		public List<string> popupQueue = new List<string>();

		public int progressGivenForPediaCount;

		public override string Identifier => "SRPED";

		public override uint Version => 3u;

		public PediaV03()
		{
		}

		public PediaV03(PediaV02 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			progressGivenForPediaCount = reader.ReadInt32();
			unlockedIds = PersistedDataSet.LoadList(reader, (string val) => val);
			completedTuts = PersistedDataSet.LoadList(reader, (string val) => val);
			popupQueue = PersistedDataSet.LoadList(reader, (string val) => val);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(progressGivenForPediaCount);
			PersistedDataSet.WriteList(writer, unlockedIds, (string val) => val);
			PersistedDataSet.WriteList(writer, completedTuts, (string val) => val);
			PersistedDataSet.WriteList(writer, popupQueue, (string val) => val);
		}

		public static PediaV03 Load(BinaryReader reader)
		{
			PediaV03 pediaV = new PediaV03();
			pediaV.Load(reader.BaseStream);
			return pediaV;
		}

		public static void AssertAreEqual(PediaV03 expected, PediaV03 actual)
		{
			TestUtil.AssertAreEqual(expected.unlockedIds, actual.unlockedIds, "unlockedIds");
			TestUtil.AssertAreEqual(expected.completedTuts, actual.completedTuts, "completedTuts");
			TestUtil.AssertAreEqual(expected.popupQueue, actual.popupQueue, "popupQueue");
		}

		public static void AssertAreEqual(PediaV02 expected, PediaV03 actual)
		{
			TestUtil.AssertAreEqual(expected.unlockedIds, actual.unlockedIds, "unlockedIds");
			TestUtil.AssertAreEqual(expected.completedTuts, actual.completedTuts, "completedTuts");
		}

		protected override void UpgradeFrom(PediaV02 legacyData)
		{
			unlockedIds = new List<string>();
			foreach (string unlockedId in legacyData.unlockedIds)
			{
				unlockedIds.Add(UpgradePediaId(unlockedId));
			}
			completedTuts = legacyData.completedTuts;
			progressGivenForPediaCount = legacyData.progressGivenForPediaCount;
			popupQueue = new List<string>();
		}

		private string UpgradePediaId(string pediaId)
		{
			if (!(pediaId == "VACPACK"))
			{
				if (pediaId == "SHOOTING")
				{
					return "CORRALLING";
				}
				return pediaId;
			}
			return "TUTORIALS";
		}
	}
}
