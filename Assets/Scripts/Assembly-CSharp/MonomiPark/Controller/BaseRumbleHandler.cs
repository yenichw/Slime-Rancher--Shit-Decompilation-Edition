using System.Collections.Generic;
using UnityEngine;

namespace MonomiPark.Controller
{
	public abstract class BaseRumbleHandler : MonoBehaviour, RumbleHandler
	{
		protected bool rumbleEnabled = true;

		protected Dictionary<Rumble.Motor, List<Rumble>> rumbles = new Dictionary<Rumble.Motor, List<Rumble>>(Rumble.motorComparer);

		protected Dictionary<Rumble.Motor, int> aggregateRumblePower = new Dictionary<Rumble.Motor, int>(Rumble.motorComparer);

		protected List<Rumble> toRemove = new List<Rumble>();

		public void AddRumble(Rumble rumble)
		{
			rumbles[rumble.GetMotor()].Add(rumble);
		}

		private void Awake()
		{
			rumbles[Rumble.Motor.LARGE] = new List<Rumble>();
			rumbles[Rumble.Motor.SMALL] = new List<Rumble>();
			rumbles[Rumble.Motor.LEFT] = new List<Rumble>();
			rumbles[Rumble.Motor.RIGHT] = new List<Rumble>();
		}

		private void Update()
		{
			AggregateRumbles();
			ApplyRumblePower();
			CleanupRumbles();
		}

		private void AggregateRumbles()
		{
			aggregateRumblePower[Rumble.Motor.RIGHT] = 0;
			aggregateRumblePower[Rumble.Motor.LEFT] = 0;
			aggregateRumblePower[Rumble.Motor.SMALL] = 0;
			aggregateRumblePower[Rumble.Motor.LARGE] = 0;
			foreach (KeyValuePair<Rumble.Motor, List<Rumble>> rumble in rumbles)
			{
				foreach (Rumble item in rumble.Value)
				{
					ApplyRumble(item);
				}
			}
		}

		private void ApplyRumble(Rumble rumble)
		{
			if (rumble.IsFinished())
			{
				toRemove.Add(rumble);
			}
			else
			{
				aggregateRumblePower[rumble.GetMotor()] += rumble.CurrentPower();
			}
		}

		private void CleanupRumbles()
		{
			foreach (Rumble item in toRemove)
			{
				rumbles[item.GetMotor()].Remove(item);
			}
			toRemove.Clear();
		}

		protected abstract void ApplyRumblePower();

		public void EnableRumble()
		{
			rumbleEnabled = true;
		}

		public void DisableRumble()
		{
			rumbleEnabled = false;
		}

		public bool IsRumbleEnabled()
		{
			return rumbleEnabled;
		}
	}
}
