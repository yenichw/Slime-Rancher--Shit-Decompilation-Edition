namespace MonomiPark.SlimeRancher.Persist
{
	public interface FallbackCapable<T> where T : PersistedDataSet, new()
	{
		void UpgradeFrom(T legacyData);
	}
}
