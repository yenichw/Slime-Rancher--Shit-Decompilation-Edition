using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public abstract class GamePersistedDataSet<T> : VersionedPersistedDataSet<T>, GamePersistedDataSet where T : Persistable, new()
	{
		private bool isLoadSummary;

		public void LoadSummary(Stream stream)
		{
			isLoadSummary = true;
			Load(stream, skipPayloadEnd: true);
			isLoadSummary = false;
		}

		protected override T LoadLegacy(T instance, Stream stream, bool skipPayloadEnd)
		{
			GamePersistedDataSet gamePersistedDataSet = instance as GamePersistedDataSet;
			if (isLoadSummary && gamePersistedDataSet != null)
			{
				gamePersistedDataSet.LoadSummary(stream);
				return instance;
			}
			return base.LoadLegacy(instance, stream, skipPayloadEnd);
		}

		protected sealed override void LoadData(BinaryReader reader)
		{
			LoadSummaryData(reader);
			if (!isLoadSummary)
			{
				LoadGameData(reader);
			}
		}

		protected sealed override void WriteData(BinaryWriter writer)
		{
			WriteSummaryData(writer);
			WriteGameData(writer);
		}

		protected abstract void LoadSummaryData(BinaryReader reader);

		protected abstract void LoadGameData(BinaryReader reader);

		protected abstract void WriteSummaryData(BinaryWriter writer);

		protected abstract void WriteGameData(BinaryWriter writer);
	}
	public interface GamePersistedDataSet
	{
		void LoadSummary(Stream stream);
	}
}
