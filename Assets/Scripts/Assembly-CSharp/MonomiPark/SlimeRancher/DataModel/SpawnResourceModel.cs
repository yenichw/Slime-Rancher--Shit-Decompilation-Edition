using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class SpawnResourceModel : PositionalModel
	{
		public interface Participant
		{
			void InitModel(SpawnResourceModel model);

			void SetModel(SpawnResourceModel model);

			Joint NearestJoint(Vector3 pos, float maxDist);
		}

		public float storedWater;

		public double nextSpawnTime;

		public bool nextSpawnRipens;

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

		public Joint NearestJoint(Vector3 position, float maxDist)
		{
			return part.NearestJoint(position, maxDist);
		}

		public void Push(float storedWater, double nextSpawnTime)
		{
			this.storedWater = storedWater;
			this.nextSpawnTime = nextSpawnTime;
		}

		public void Pull(out float storedWater, out double nextSpawnTime)
		{
			storedWater = this.storedWater;
			nextSpawnTime = this.nextSpawnTime;
		}
	}
}
