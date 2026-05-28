using System.Collections.Generic;
using MonomiPark.SlimeRancher.Persist;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class DroneModel : GadgetModel
	{
		public struct ProgramData
		{
			public string target;

			public string source;

			public string destination;
		}

		public AmmoModel ammo = new AmmoModel();

		public List<Identifiable.Id> fashions = new List<Identifiable.Id>();

		public bool noClip;

		public ProgramData[] programs;

		public double batteryDepleteTime;

		public Vector3 position;

		public Quaternion rotation;

		public RegionRegistry.RegionSetId currRegionSetId;

		public DroneModel(Gadget.Id gadgetId, string siteId, Transform transform)
			: base(gadgetId, siteId, transform)
		{
		}

		public void Push(Vector3 dronePosition, Vector3 droneRotation, Ammo.Slot[] ammoSlots, List<Identifiable.Id> fashions, bool noClip, double batteryDepleteTime, List<DroneProgramV01> programs)
		{
			position = dronePosition;
			rotation = Quaternion.Euler(droneRotation);
			ammo.Push(ammoSlots);
			this.fashions = new List<Identifiable.Id>(fashions);
			this.noClip = noClip;
			this.batteryDepleteTime = batteryDepleteTime;
			this.programs = new ProgramData[programs.Count];
			int num = 0;
			foreach (DroneProgramV01 program in programs)
			{
				this.programs[num] = new ProgramData
				{
					target = program.target,
					source = program.source,
					destination = program.destination
				};
				num++;
			}
		}

		public void Pull(out Vector3 dronePosition, out Vector3 droneRotation, out Ammo.Slot[] ammoSlots, out List<Identifiable.Id> fashions, out bool noClip, out double batteryDepleteTime, out List<DroneProgramV01> programs)
		{
			dronePosition = position;
			droneRotation = rotation.eulerAngles;
			ammo.Pull(out ammoSlots);
			fashions = new List<Identifiable.Id>(this.fashions);
			noClip = this.noClip;
			batteryDepleteTime = this.batteryDepleteTime;
			programs = new List<DroneProgramV01>();
			for (int i = 0; i < this.programs.Length; i++)
			{
				ProgramData programData = this.programs[i];
				programs.Add(new DroneProgramV01
				{
					target = programData.target,
					source = programData.source,
					destination = programData.destination
				});
			}
		}
	}
}
