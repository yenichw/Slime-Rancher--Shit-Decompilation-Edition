using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public abstract class ActorModel
	{
		public interface Participant
		{
			void InitModel(ActorModel model);

			void SetModel(ActorModel model);
		}

		public static class Id
		{
			public const long PLAYER = 1L;

			public const long BEGIN_DYNAMIC = 100L;
		}

		public readonly Identifiable.Id ident;

		public readonly long actorId;

		private readonly Transform transform;

		public RegionRegistry.RegionSetId currRegionSetId { get; protected set; }

		public ActorModel(long actorId, Identifiable.Id ident, RegionRegistry.RegionSetId regionSetId, Transform transform)
		{
			this.actorId = actorId;
			this.ident = ident;
			this.transform = transform;
			currRegionSetId = regionSetId;
		}

		public virtual bool IsEdible()
		{
			return true;
		}

		public virtual Vector3 GetPos()
		{
			return transform.position;
		}

		public virtual Quaternion GetRot()
		{
			return transform.rotation;
		}

		public void Init(GameObject gameObj)
		{
			Participant[] componentsInChildren = gameObj.GetComponentsInChildren<Participant>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].InitModel(this);
			}
		}

		public void NotifyParticipants(GameObject gameObj)
		{
			Participant[] componentsInChildren = gameObj.GetComponentsInChildren<Participant>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetModel(this);
			}
		}
	}
}
