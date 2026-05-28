using System;
using System.Collections.Generic;
using UnityEngine;

public class VacDisplayChanger : MonoBehaviour
{
	[Header("Default Game Mode")]
	[Tooltip("GameObject display on the default ammo mode.")]
	public List<GameObject> displayDefault;

	[Tooltip("SFX played when the display transforms to default ammo mode. (optional)")]
	public SECTR_AudioCue onTransformToDefaultCue2D;

	[Header("Nimble Valley Game Mode")]
	[Tooltip("GameObject display on the Nimble Valley ammo mode.")]
	public List<GameObject> displayNimbleValley;

	[Tooltip("Nimble Valley assembler.")]
	public ActorMatAssemble assembler;

	[Tooltip("SFX played when the display transforms to Nimble Valley ammo mode. (optional)")]
	public SECTR_AudioCue onTransformToNimbleValleyCue2D;

	private PlayerState playerState;

	public void Awake()
	{
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		PlayerState obj = playerState;
		obj.onAmmoModeChanged = (PlayerState.OnAmmoModeChanged)Delegate.Combine(obj.onAmmoModeChanged, new PlayerState.OnAmmoModeChanged(SetDisplayMode));
		SetDisplayMode(playerState.GetAmmoMode());
	}

	public void OnDestroy()
	{
		PlayerState obj = playerState;
		obj.onAmmoModeChanged = (PlayerState.OnAmmoModeChanged)Delegate.Remove(obj.onAmmoModeChanged, new PlayerState.OnAmmoModeChanged(SetDisplayMode));
	}

	public void SetDisplayMode(PlayerState.AmmoMode mode)
	{
		bool flag = mode == PlayerState.AmmoMode.DEFAULT;
		bool flag2 = mode == PlayerState.AmmoMode.NIMBLE_VALLEY;
		foreach (GameObject item in displayDefault)
		{
			item.SetActive(flag);
		}
		foreach (GameObject item2 in displayNimbleValley)
		{
			item2.SetActive(flag2);
		}
		if (assembler.Assemble(flag2))
		{
			if (flag)
			{
				SECTR_AudioSystem.Play(onTransformToDefaultCue2D, Vector3.zero, loop: false);
			}
			else if (flag2)
			{
				SECTR_AudioSystem.Play(onTransformToNimbleValleyCue2D, Vector3.zero, loop: false);
			}
		}
	}
}
