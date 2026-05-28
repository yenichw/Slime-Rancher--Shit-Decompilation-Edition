using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

[RequireComponent(typeof(DroneStation))]
public class DroneStationBattery : SRBehaviour, LiquidConsumer, GadgetModel.Participant
{
	public delegate void OnReset();

	public delegate void OnHasAnyChanged();

	[Tooltip("Battery meter transform.")]
	public Transform meter;

	private const float DURATION_HOURS = 28f;

	private const float DURATION_SECONDS = 100800f;

	private TimeDirector timeDirector;

	private bool? previousHasAny;

	private double fxCooldownTime;

	private DroneModel droneModel;

	public DroneStation station { get; private set; }

	public double Time => droneModel.batteryDepleteTime;

	private float percentage
	{
		get
		{
			return meter.localScale.y;
		}
		set
		{
			meter.localScale = new Vector3(1f, value, 1f);
		}
	}

	public event OnReset onReset;

	public event OnHasAnyChanged onHasAnyChanged;

	public void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		station = GetComponent<DroneStation>();
	}

	public void InitModel(GadgetModel droneModel)
	{
		Reset((DroneModel)droneModel);
	}

	public void SetModel(GadgetModel droneModel)
	{
		this.droneModel = (DroneModel)droneModel;
	}

	public void AddLiquid(Identifiable.Id id, float units)
	{
		if (percentage < 1f && timeDirector.HasReached(fxCooldownTime))
		{
			SECTR_AudioSystem.Play(station.gadget.metadata.onBatteryFilledCue, base.transform.position, loop: false);
			fxCooldownTime = timeDirector.HoursFromNow(0.050000004f);
			if (station.gadget.metadata.onBatteryFilledFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(station.gadget.metadata.onBatteryFilledFX, base.gameObject);
			}
		}
		Reset(droneModel);
	}

	public void Update()
	{
		percentage = Mathf.Clamp01((float)((Time - timeDirector.WorldTime()) / 100800.0));
		if (previousHasAny != HasAny())
		{
			previousHasAny = HasAny();
			if (this.onHasAnyChanged != null)
			{
				this.onHasAnyChanged();
			}
		}
	}

	public bool HasAny()
	{
		return percentage > 0f;
	}

	private void Reset(DroneModel droneModel)
	{
		percentage = 1f;
		droneModel.batteryDepleteTime = timeDirector.HoursFromNow(28f);
		if (this.onReset != null)
		{
			this.onReset();
		}
	}
}
