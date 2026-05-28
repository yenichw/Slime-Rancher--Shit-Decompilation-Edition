using UnityEngine;

public class WorldStateSlaveSwitch : MonoBehaviour, TechActivator
{
	private WorldStateMasterSwitch master;

	private SwitchHandler switchHandler;

	private SwitchHandler.State currState;

	public void Awake()
	{
		switchHandler = new SwitchHandler(GetComponent<Animator>(), base.gameObject);
	}

	public void Start()
	{
	}

	public void OnEnable()
	{
		SetState(currState, immediate: true);
	}

	public void Activate()
	{
		master.Activate();
	}

	public void RegisterMaster(WorldStateMasterSwitch master)
	{
		this.master = master;
	}

	internal void SetState(SwitchHandler.State state, bool immediate)
	{
		currState = state;
		if (base.isActiveAndEnabled)
		{
			switchHandler.SetState(state, immediate);
		}
	}

	public GameObject GetCustomGuiPrefab()
	{
		return null;
	}
}
