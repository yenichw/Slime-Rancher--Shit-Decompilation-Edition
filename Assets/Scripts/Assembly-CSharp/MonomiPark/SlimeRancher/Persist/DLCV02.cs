using System.Collections.Generic;
using System.IO;
using DLCPackage;

namespace MonomiPark.SlimeRancher.Persist
{
	public class DLCV02 : PersistedDataSet
	{
		public List<Id> installed;

		public override string Identifier => "SRDLC";

		public override uint Version => 2u;

		public DLCV02()
		{
			installed = new List<Id>();
		}

		protected override void LoadData(BinaryReader reader)
		{
			installed = PersistedDataSet.LoadList(reader, (int ii) => (Id)ii);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WriteList(writer, installed, (Id package) => (int)package);
		}
	}
}
