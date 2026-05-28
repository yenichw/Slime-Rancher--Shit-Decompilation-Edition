using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public abstract class GadgetModel
	{
		public interface Participant
		{
			void InitModel(GadgetModel model);

			void SetModel(GadgetModel model);
		}

		public Gadget.Id ident;

		public string siteId;

		public Transform transform;

		public double waitForChargeupTime;

		public GadgetModel(Gadget.Id ident, string siteId, Transform transform)
		{
			this.ident = ident;
			this.siteId = siteId;
			this.transform = transform;
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

		public void PushBase(double waitForChargeupTime, float yRotation)
		{
			this.waitForChargeupTime = waitForChargeupTime;
			transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
		}

		public void PullBase(out double waitForChargeupTime, out float yRotation)
		{
			waitForChargeupTime = this.waitForChargeupTime;
			yRotation = transform.localRotation.eulerAngles.y;
		}
	}
}
