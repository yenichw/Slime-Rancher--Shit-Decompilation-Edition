namespace MonomiPark.SlimeRancher.DataModel
{
	public class DirectedAnimalSpawnerModel : PositionalModel
	{
		public interface Participant
		{
			void InitModel(DirectedAnimalSpawnerModel model);

			void SetModel(DirectedAnimalSpawnerModel model);
		}

		public double nextSpawnTime;

		private Participant part;

		public void SetParticipant(Participant part)
		{
			this.part = part;
		}

		public void Init()
		{
			part.InitModel(this);
		}

		public void NotifyParticipants()
		{
			part.SetModel(this);
		}

		public void Push(double nextSpawnTime)
		{
			this.nextSpawnTime = nextSpawnTime;
		}

		public void Pull(out double nextSpawnTime)
		{
			nextSpawnTime = this.nextSpawnTime;
		}
	}
}
