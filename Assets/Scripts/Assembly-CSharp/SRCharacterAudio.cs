public class SRCharacterAudio : SECTR_CharacterAudio, EventHandlerRegistrable
{
	private vp_FPPlayerEventHandler playerEvents;

	private vp_FPController playerController;

	public void Awake()
	{
		playerController = GetComponent<vp_FPController>();
		playerEvents = GetComponentInChildren<vp_FPPlayerEventHandler>();
		GetComponentInChildren<vp_FPCamera>().BobStepCallback = delegate
		{
			if (playerController.Grounded)
			{
				OnFootstep(null);
			}
		};
	}

	protected virtual void OnEnable()
	{
		if (playerEvents != null)
		{
			Register(playerEvents);
		}
	}

	protected virtual void OnDisable()
	{
		if (playerEvents != null)
		{
			Unregister(playerEvents);
		}
	}

	public virtual void OnStart_Jump()
	{
		OnJump(null);
	}

	public virtual void OnMessage_FallImpact(float val)
	{
		if (val > 0.05f)
		{
			OnLand(null);
		}
	}

	public void Register(vp_EventHandler eventHandler)
	{
		eventHandler.RegisterMessage<float>("FallImpact", OnMessage_FallImpact);
		eventHandler.RegisterActivity("Jump", OnStart_Jump, null, null, null, null, null);
	}

	public void Unregister(vp_EventHandler eventHandler)
	{
		eventHandler.UnregisterMessage<float>("FallImpact", OnMessage_FallImpact);
		eventHandler.UnregisterActivity("Jump", OnStart_Jump, null, null, null, null, null);
	}
}
