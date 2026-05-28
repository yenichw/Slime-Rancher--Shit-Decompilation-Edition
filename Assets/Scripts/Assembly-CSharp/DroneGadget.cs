using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class DroneGadget : Gadget, GadgetModel.Participant
{
	public delegate void OnProgramsChanged(DroneMetadata.Program[] programs);

	[Tooltip("Drone metadata.")]
	public DroneMetadata metadata;

	[Tooltip("Drone prefab.")]
	public GameObject prefab;

	[Tooltip("Number of programs accessible to the drone.")]
	public int programCount;

	private DroneModel droneModel;

	public Drone drone { get; private set; }

	public DroneStation station { get; private set; }

	public Region region { get; private set; }

	public DroneMetadata.Program[] programs { get; private set; }

	public event OnProgramsChanged onProgramsChanged;

	public override void Awake()
	{
		base.Awake();
		station = GetComponentInChildren<DroneStation>();
		region = GetComponentInParent<Region>();
		rotationTransform = station.transform;
		InstantiateDrone();
	}

	public void OnDestroy()
	{
		drone.onDestroyed -= InstantiateDrone;
	}

	public void InitModel(GadgetModel model)
	{
		DroneModel droneModel = (DroneModel)model;
		droneModel.programs = new DroneModel.ProgramData[programCount];
		for (int i = 0; i < droneModel.programs.Length; i++)
		{
			droneModel.programs[i] = new DroneModel.ProgramData
			{
				target = "drone.target.none",
				source = "drone.behaviour.none",
				destination = "drone.behaviour.none"
			};
		}
	}

	public void SetModel(GadgetModel model)
	{
		droneModel = (DroneModel)model;
		SetPrograms(ProgramsFromData(droneModel.programs));
	}

	public DroneMetadata.Program[] ProgramsFromData(DroneModel.ProgramData[] programData)
	{
		DroneMetadata.Program[] array = new DroneMetadata.Program[programData.Length];
		for (int i = 0; i < programData.Length; i++)
		{
			DroneModel.ProgramData programDataItem = programData[i];
			array[i] = new DroneMetadata.Program
			{
				target = (metadata.targets.FirstOrDefault((DroneMetadata.Program.Target c) => c.id == programDataItem.target) ?? metadata.GetDefaultTarget()),
				source = (metadata.sources.FirstOrDefault((DroneMetadata.Program.Behaviour c) => c.id == programDataItem.source) ?? metadata.GetDefaultBehaviour()),
				destination = (metadata.destinations.FirstOrDefault((DroneMetadata.Program.Behaviour c) => c.id == programDataItem.destination) ?? metadata.GetDefaultBehaviour())
			};
		}
		return array;
	}

	public DroneModel.ProgramData[] DataFromPrograms(DroneMetadata.Program[] programs)
	{
		DroneModel.ProgramData[] array = new DroneModel.ProgramData[programs.Length];
		for (int i = 0; i < programs.Length; i++)
		{
			DroneMetadata.Program program = programs[i];
			array[i] = new DroneModel.ProgramData
			{
				target = program.target.id,
				source = program.source.id,
				destination = program.destination.id
			};
		}
		return array;
	}

	public void SetPrograms(DroneMetadata.Program[] programs)
	{
		this.programs = programs;
		droneModel.programs = DataFromPrograms(programs);
		if (this.onProgramsChanged != null)
		{
			this.onProgramsChanged(programs);
		}
	}

	public override void OnUserDestroyed()
	{
		base.OnUserDestroyed();
		drone.OnGadgetDestroyed();
	}

	private void InstantiateDrone()
	{
		GameObject gameObject = Object.Instantiate(prefab, base.transform);
		drone = gameObject.GetComponent<Drone>();
		drone.onDestroyed += InstantiateDrone;
		drone.TeleportToStation(includeFX: false);
	}
}
