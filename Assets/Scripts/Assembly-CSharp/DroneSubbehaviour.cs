using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Drone))]
[RequireComponent(typeof(DroneSubbehaviourPlexer))]
public abstract class DroneSubbehaviour : CollidableActorBehaviour
{
	protected class GatherConfig
	{
		public static GatherConfig DEFAULT = new GatherConfig();

		public const int GATHER_ATTEMPTS = 10;

		public Vector3 fallbackOffset;

		public float distanceHorizontal;

		public float distanceVertical;

		public GatherConfig()
		{
			fallbackOffset = Vector3.up;
			distanceHorizontal = 2f;
			distanceVertical = 1f;
		}
	}

	protected class CatcherOrientation : IDisposable
	{
		private static Dictionary<GameObject, HashSet<int>> BLACKLIST = new Dictionary<GameObject, HashSet<int>>();

		private GameObject source;

		private int? index;

		public DroneProgram.Orientation orientation { get; private set; }

		public static bool IsAvailable(GameObject source, int index)
		{
			if (BLACKLIST.ContainsKey(source))
			{
				return !BLACKLIST[source].Contains(index);
			}
			return true;
		}

		public CatcherOrientation(GameObject source, int index, DroneProgram.Orientation orientation)
		{
			this.orientation = orientation;
			this.source = source;
			this.index = index;
			if (!BLACKLIST.ContainsKey(source))
			{
				BLACKLIST[source] = new HashSet<int>();
			}
			BLACKLIST[source].Add(index);
		}

		public CatcherOrientation(DroneProgram.Orientation orientation)
		{
			this.orientation = orientation;
		}

		public void Dispose()
		{
			if (source != null && index.HasValue)
			{
				if (BLACKLIST.ContainsKey(source) && BLACKLIST[source].Remove(index.Value) && BLACKLIST[source].Count == 0)
				{
					BLACKLIST.Remove(source);
				}
				source = null;
				index = null;
			}
		}
	}

	protected Drone drone;

	protected TimeDirector timeDirector;

	private static readonly DroneProgram.Orientation[] CATCHER_ORIENTATIONS = new DroneProgram.Orientation[6]
	{
		new DroneProgram.Orientation(),
		new DroneProgram.Orientation
		{
			rot = Quaternion.Euler(0f, -45f, 0f)
		},
		new DroneProgram.Orientation
		{
			rot = Quaternion.Euler(0f, 45f, 0f)
		},
		new DroneProgram.Orientation
		{
			pos = Vector3.up
		},
		new DroneProgram.Orientation
		{
			rot = Quaternion.Euler(0f, -45f, 0f),
			pos = Vector3.up
		},
		new DroneProgram.Orientation
		{
			rot = Quaternion.Euler(0f, 45f, 0f),
			pos = Vector3.up
		}
	};

	private const float POSITION_CHECK_SPHERECAST_START = 1000f;

	private const float DRONE_RADIUS = 0.5f;

	private List<MeshCollider> collidersToReset = new List<MeshCollider>();

	public DroneSubbehaviourPlexer plexer { get; private set; }

	public override void Awake()
	{
		base.Awake();
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		plexer = GetComponent<DroneSubbehaviourPlexer>();
		drone = GetComponent<Drone>();
	}

	public abstract bool Relevancy();

	public abstract void Action();

	public virtual void Selected()
	{
	}

	public virtual void Deselected()
	{
	}

	protected bool OnAction_DumpAmmo(ref double time)
	{
		if (timeDirector.HasReached(time))
		{
			GameObject gameObject = SRSingleton<SceneContext>.Instance.GameModel.InstantiateActor(SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(drone.ammo.Pop()), drone.region.setId, drone.guideDumpSpawn.position, Quaternion.Euler(Randoms.SHARED.GetInRange(0, 360), Randoms.SHARED.GetInRange(0, 360), Randoms.SHARED.GetInRange(0, 360)));
			PhysicsUtil.RestoreFreezeRotationConstraints(gameObject);
			gameObject.GetComponent<Rigidbody>().velocity = (Quaternion.Euler(0f, Randoms.SHARED.GetInRange(0, 360), 0f) * new Vector3(0f, -0.5f, 0.5f) + Vector3.down).normalized * 5f;
			float fromValue = gameObject.transform.localScale.x * 0.2f;
			gameObject.transform.DOScale(gameObject.transform.localScale, 0.1f).From(fromValue).SetEase(Ease.Linear);
			time = timeDirector.HoursFromNow(0.0050000004f);
			return true;
		}
		return false;
	}

	protected IEnumerable<DroneProgram.Orientation> GetTargetOrientations_Gather(GameObject source)
	{
		return GetTargetOrientations_Gather(source, GatherConfig.DEFAULT);
	}

	protected IEnumerable<DroneProgram.Orientation> GetTargetOrientations_Gather(GameObject source, GatherConfig config)
	{
		for (float distanceMultiplier = 1f; distanceMultiplier <= 3f; distanceMultiplier += 1f)
		{
			float angle = Randoms.SHARED.GetFloat((float)Math.PI * 2f);
			float delta = (float)Math.PI / 5f;
			int ii = 0;
			while (ii < 10)
			{
				Vector3 vector = new Vector3(Mathf.Cos(angle) * config.distanceHorizontal, distanceMultiplier * config.distanceVertical, Mathf.Sin(angle) * config.distanceHorizontal);
				Vector3 vector2 = source.transform.position + vector;
				if (SpaceIsClearForDrone(vector2))
				{
					yield return new DroneProgram.Orientation(vector2, Quaternion.LookRotation(-vector));
				}
				int num = ii + 1;
				ii = num;
				angle += delta;
			}
		}
		yield return new DroneProgram.Orientation(source.transform.position + config.fallbackOffset, Quaternion.LookRotation(config.fallbackOffset));
	}

	protected CatcherOrientation GetTargetOrientation_Catcher(GameObject source)
	{
		for (int i = 0; i < CATCHER_ORIENTATIONS.Length; i++)
		{
			if (CatcherOrientation.IsAvailable(source, i))
			{
				DroneProgram.Orientation orientation = CATCHER_ORIENTATIONS[i];
				Vector3 vector = orientation.rot * (source.transform.forward * 2f + orientation.pos);
				Vector3 vector2 = source.transform.position + vector;
				if (SpaceIsClearForDrone(vector2))
				{
					return new CatcherOrientation(source, i, new DroneProgram.Orientation
					{
						pos = vector2,
						rot = Quaternion.LookRotation(-vector)
					});
				}
			}
		}
		return new CatcherOrientation(new DroneProgram.Orientation
		{
			pos = source.transform.position + Vector3.up,
			rot = Quaternion.LookRotation(-Vector3.up)
		});
	}

	private bool SpaceIsClearForDrone(Vector3 position)
	{
		if (drone.noClip.enabled)
		{
			return true;
		}
		RaycastHit[] array = Physics.SphereCastAll(position + Vector3.up * 1000f, 0.5f, Vector3.down, 1000f, -537968901);
		collidersToReset.Clear();
		bool flag = false;
		if (array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				MeshCollider component = array[i].collider.GetComponent<MeshCollider>();
				if (component != null && !component.convex && array[i].collider.GetComponent<Rigidbody>() == null)
				{
					collidersToReset.Add(component);
					try
					{
						component.convex = true;
					}
					catch
					{
						Log.Error("Exception when changing to convex.", "object name", component.name);
						throw;
					}
				}
			}
			flag = !Physics.CheckSphere(position, 0.5f, -537968901);
			foreach (MeshCollider item in collidersToReset)
			{
				item.convex = false;
			}
		}
		else
		{
			flag = true;
		}
		return flag;
	}
}
