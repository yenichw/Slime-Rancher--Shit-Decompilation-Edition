using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class WorldStateMasterSwitch : IdHandler, TechActivator, MasterSwitchModel.Participant
{
	public SwitchHandler.Switchable[] switchables;

	public SwitchHandler.State initState;

	public WorldStateSlaveSwitch[] slaves;

	private bool firstUpdate = true;

	private SwitchHandler switchHandler;

	private float blockSwitchActivationUntil;

	private MasterSwitchModel model;

	private const float ACTIVATION_THROTTLE = 2f;

	private bool IsActivationBlocked()
	{
		return Time.time < blockSwitchActivationUntil;
	}

	public void Awake()
	{
		switchHandler = new SwitchHandler(GetComponent<Animator>(), base.gameObject);
		SRSingleton<SceneContext>.Instance.GameModel.RegisterSwitch(base.id, base.gameObject);
		WorldStateSlaveSwitch[] array = slaves;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RegisterMaster(this);
		}
	}

	public void OnEnable()
	{
		if (model != null)
		{
			switchHandler.SetState(model.state, immediate: true);
		}
	}

	public void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.GameModel.UnregisterSwitch(base.id);
		}
	}

	public void InitModel(MasterSwitchModel model)
	{
		model.state = initState;
	}

	public void SetModel(MasterSwitchModel model)
	{
		this.model = model;
		switchHandler.SetState(model.state, immediate: true);
	}

	protected override string IdPrefix()
	{
		return "switch";
	}

	public void Update()
	{
		if (firstUpdate)
		{
			SetStateForAll(model.state, immediate: true);
			firstUpdate = false;
		}
	}

	public void Activate()
	{
		if (!IsActivationBlocked())
		{
			blockSwitchActivationUntil = Time.time + 2f;
			SwitchHandler.State state = ((model.state == SwitchHandler.State.UP) ? SwitchHandler.State.DOWN : SwitchHandler.State.UP);
			SetStateForAll(state, immediate: false);
		}
	}

	private void SetStateForAll(SwitchHandler.State state, bool immediate)
	{
		model.state = state;
		switchHandler.SetState(state, immediate);
		WorldStateSlaveSwitch[] array = slaves;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetState(state, immediate);
		}
		SwitchHandler.Switchable[] array2 = switchables;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].SetState(state, immediate);
		}
	}

	public GameObject GetCustomGuiPrefab()
	{
		return null;
	}
}
