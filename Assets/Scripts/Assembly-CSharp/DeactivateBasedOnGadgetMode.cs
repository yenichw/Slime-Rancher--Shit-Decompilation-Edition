using UnityEngine;

public class DeactivateBasedOnGadgetMode : MonoBehaviour
{
	public GameObject toDeactivate;

	public bool activateOnModeOff;

	private PlayerState playerState;

	public void Awake()
	{
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
	}

	public void Update()
	{
		toDeactivate.SetActive(playerState.InGadgetMode ^ activateOnModeOff);
	}
}
