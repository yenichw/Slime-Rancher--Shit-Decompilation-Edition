using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Drone))]
[RequireComponent(typeof(DroneSubbehaviourDumpAmmo))]
[RequireComponent(typeof(DroneSubbehaviourIdle))]
[RequireComponent(typeof(DroneSubbehaviourRest))]
public class DroneSubbehaviourPlexer : RegisteredActorBehaviour, RegistryFixedUpdateable
{
	public delegate void OnSubbehaviourSelected(DroneSubbehaviour subbehaviour);

	public class Program
	{
		public enum State
		{
			INACTIVE = 0,
			GATHER = 1,
			DEPOSIT = 2
		}

		private List<DroneProgramSource> sources;

		private List<DroneProgramDestination> destinations;

		public IEnumerable<DroneProgramSource> Sources => sources;

		public IEnumerable<DroneProgramDestination> Destinations => destinations;

		public State state { get; private set; }

		public Program(List<DroneProgramSource> sources, List<DroneProgramDestination> destinations)
		{
			state = State.INACTIVE;
			this.sources = sources;
			this.destinations = destinations;
		}

		public DroneSubbehaviour PickNextBehaviour()
		{
			if (state == State.INACTIVE)
			{
				state = State.GATHER;
			}
			if (state == State.GATHER)
			{
				DroneSubbehaviour droneSubbehaviour = PickNextGatherBehaviour();
				if (droneSubbehaviour != null)
				{
					return droneSubbehaviour;
				}
				state = State.DEPOSIT;
			}
			if (state == State.DEPOSIT)
			{
				DroneSubbehaviour droneSubbehaviour2 = PickNextDepositBehaviour();
				if (droneSubbehaviour2 != null)
				{
					return droneSubbehaviour2;
				}
				state = State.INACTIVE;
			}
			return null;
		}

		public void Destroy()
		{
			foreach (DroneProgramSource source in sources)
			{
				Destroyer.Destroy(source, "DroneSubbehaviourPlexer.Program.Destroy#1");
			}
			foreach (DroneProgramDestination destination in destinations)
			{
				Destroyer.Destroy(destination, "DroneSubbehaviourPlexer.Program.Destroy#2");
			}
		}

		public void ResetProgram()
		{
			state = State.INACTIVE;
		}

		private DroneSubbehaviour PickNextGatherBehaviour()
		{
			return sources.FirstOrDefault((DroneProgramSource b) => b.Relevancy());
		}

		private DroneSubbehaviour PickNextDepositBehaviour()
		{
			return destinations.FirstOrDefault((DroneProgramDestination b) => b.Relevancy(overflow: false));
		}
	}

	private DroneSubbehaviourIdle subbehaviourIdle;

	private List<Program> subbehaviourPrograms;

	private DroneSubbehaviourDumpAmmo subbehaviourDumpAmmo;

	private DroneSubbehaviourRest subbehaviourRest;

	private TimeDirector timeDirector;

	private DroneSubbehaviour currBehaviour;

	private DroneSubbehaviour nextBehaviour;

	private DroneGadget gadget;

	private float activationTime;

	private const float ACTIVATION_DELAY = 3f;

	public List<Program> Programs => subbehaviourPrograms;

	public event OnSubbehaviourSelected onSubbehaviourSelected;

	public void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		subbehaviourPrograms = new List<Program>();
		subbehaviourIdle = GetRequiredComponent<DroneSubbehaviourIdle>();
		subbehaviourDumpAmmo = GetRequiredComponent<DroneSubbehaviourDumpAmmo>();
		subbehaviourRest = GetRequiredComponent<DroneSubbehaviourRest>();
		gadget = GetRequiredComponentInParent<DroneGadget>();
		gadget.onProgramsChanged += OnGadgetProgramsChanged;
	}

	public override void Start()
	{
		base.Start();
		activationTime = Time.fixedTime + 3f;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		ForceRethink();
		if (gadget != null)
		{
			gadget.onProgramsChanged -= OnGadgetProgramsChanged;
			gadget = null;
		}
	}

	private void OnGadgetProgramsChanged(DroneMetadata.Program[] programs)
	{
		if (currBehaviour is DroneProgramSource || currBehaviour is DroneProgramDestination)
		{
			ForceRethink();
		}
		subbehaviourPrograms.ForEach(delegate(Program p)
		{
			p.Destroy();
		});
		subbehaviourPrograms.Clear();
		foreach (DroneMetadata.Program item in programs.Where((DroneMetadata.Program p) => p.IsComplete()))
		{
			List<DroneProgramDestination> list = new List<DroneProgramDestination>();
			for (int i = 0; i < item.destination.types.Length; i++)
			{
				DroneProgramDestination droneProgramDestination = base.gameObject.AddComponent(item.destination.types[i]) as DroneProgramDestination;
				droneProgramDestination.predicate = item.target.predicate;
				list.Add(droneProgramDestination);
			}
			List<DroneProgramSource> list2 = new List<DroneProgramSource>();
			for (int j = 0; j < item.source.types.Length; j++)
			{
				DroneProgramSource droneProgramSource = base.gameObject.AddComponent(item.source.types[j]) as DroneProgramSource;
				droneProgramSource.predicate = item.target.predicate;
				droneProgramSource.destinations = list;
				list2.Add(droneProgramSource);
			}
			subbehaviourPrograms.Add(new Program(list2, list));
		}
		DroneSubbehaviourRest droneSubbehaviourRest = currBehaviour as DroneSubbehaviourRest;
		if (droneSubbehaviourRest != null)
		{
			droneSubbehaviourRest.ForceRethink();
		}
	}

	public void RegistryFixedUpdate()
	{
		if (Time.fixedTime < activationTime || timeDirector.IsFastForwarding() || gadget.drone.region.Hibernated)
		{
			return;
		}
		if (currBehaviour == null)
		{
			currBehaviour = PickNextBehaviour();
			currBehaviour.Selected();
			nextBehaviour = null;
			if (this.onSubbehaviourSelected != null)
			{
				this.onSubbehaviourSelected(currBehaviour);
			}
		}
		currBehaviour.Action();
	}

	public void ForceRethink(float activationDelay = 0f)
	{
		activationTime = Time.fixedTime + activationDelay;
		if (currBehaviour != null)
		{
			currBehaviour.Deselected();
			currBehaviour = null;
		}
	}

	public void ForceResting()
	{
		if (currBehaviour != subbehaviourRest)
		{
			nextBehaviour = subbehaviourRest;
			subbehaviourPrograms.ForEach(delegate(Program p)
			{
				p.ResetProgram();
			});
			ForceRethink();
		}
	}

	public void ForceDumpAmmo(bool destructive)
	{
		subbehaviourDumpAmmo.destructive |= destructive;
		if (currBehaviour != subbehaviourDumpAmmo)
		{
			nextBehaviour = subbehaviourDumpAmmo;
			subbehaviourPrograms.ForEach(delegate(Program p)
			{
				p.ResetProgram();
			});
			ForceRethink();
		}
	}

	public bool IsResting()
	{
		return currBehaviour is DroneSubbehaviourRest;
	}

	public void OnFastForward()
	{
		subbehaviourPrograms.ForEach(delegate(Program p)
		{
			p.ResetProgram();
		});
		ForceRethink();
	}

	public bool PickNextGatherBehaviour()
	{
		if (nextBehaviour == null)
		{
			nextBehaviour = PickNextProgramBehaviour();
			return nextBehaviour != null;
		}
		return false;
	}

	private DroneSubbehaviour PickNextBehaviour()
	{
		if (nextBehaviour != null)
		{
			return nextBehaviour;
		}
		if (subbehaviourIdle.Relevancy())
		{
			return subbehaviourIdle;
		}
		DroneSubbehaviour droneSubbehaviour = PickNextProgramBehaviour();
		if (droneSubbehaviour != null)
		{
			return droneSubbehaviour;
		}
		if (subbehaviourDumpAmmo.Relevancy())
		{
			return subbehaviourDumpAmmo;
		}
		if (subbehaviourRest.Relevancy())
		{
			return subbehaviourRest;
		}
		throw new Exception("Failed to get next drone subbehaviour.");
	}

	private DroneSubbehaviour PickNextProgramBehaviour()
	{
		if (!subbehaviourPrograms.Any())
		{
			return null;
		}
		int num = subbehaviourPrograms.Count;
		if (subbehaviourPrograms[0].state == Program.State.DEPOSIT)
		{
			num++;
		}
		for (int i = 0; i < num; i++)
		{
			Program program = subbehaviourPrograms[0];
			DroneSubbehaviour droneSubbehaviour = program.PickNextBehaviour();
			if (droneSubbehaviour != null)
			{
				return droneSubbehaviour;
			}
			if (subbehaviourPrograms.Count > 1)
			{
				subbehaviourPrograms.RemoveAt(0);
				subbehaviourPrograms.Add(program);
			}
		}
		return null;
	}
}
