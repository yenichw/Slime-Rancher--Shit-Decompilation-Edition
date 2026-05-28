using MonomiPark.SlimeRancher.Persist;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class GlitchStorageModel : IdHandlerModel
	{
		public Identifiable.Id id;

		public int count;

		public void Push(GlitchStorageV01 persistence)
		{
			id = persistence.id;
			count = persistence.count;
		}

		public GlitchStorageV01 Pull()
		{
			return new GlitchStorageV01
			{
				id = id,
				count = count
			};
		}
	}
}
