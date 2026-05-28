using System.Collections.Generic;
using UnityEngine;

public class vp_PlatformSwitch : vp_Interactable
{
	public float SwitchTimeout;

	public vp_MovingPlatform Platform;

	public AudioSource AudioSource;

	public Vector2 SwitchPitchRange = new Vector2(1f, 1.5f);

	public List<AudioClip> SwitchSounds = new List<AudioClip>();

	protected bool m_IsSwitched;

	protected float m_Timeout;

	protected override void Start()
	{
		base.Start();
		if (AudioSource == null)
		{
			AudioSource = ((GetComponent<AudioSource>() == null) ? base.gameObject.AddComponent<AudioSource>() : GetComponent<AudioSource>());
		}
	}

	public override bool TryInteract(vp_PlayerEventHandler player)
	{
		if (Platform == null)
		{
			return false;
		}
		if (m_Player == null)
		{
			m_Player = player;
		}
		if (Time.time < m_Timeout)
		{
			return false;
		}
		PlaySound();
		if (vp_Gameplay.isMaster)
		{
			Platform.SendMessage("GoTo", (Platform.TargetedWaypoint == 0) ? 1 : 0, SendMessageOptions.DontRequireReceiver);
		}
		else if (InteractType == vp_InteractType.Normal)
		{
			SendMessage("ClientTryInteract");
		}
		m_Timeout = Time.time + SwitchTimeout;
		m_IsSwitched = !m_IsSwitched;
		return true;
	}

	public virtual void PlaySound()
	{
		if (AudioSource == null)
		{
			Debug.LogWarning("Audio Source is not set");
		}
		else if (SwitchSounds.Count != 0)
		{
			AudioClip audioClip = SwitchSounds[Random.Range(0, SwitchSounds.Count)];
			if (!(audioClip == null))
			{
				AudioSource.pitch = Random.Range(SwitchPitchRange.x, SwitchPitchRange.y);
				AudioSource.PlayOneShot(audioClip);
			}
		}
	}

	protected override void OnTriggerEnter(Collider col)
	{
		if (InteractType != vp_InteractType.Trigger)
		{
			return;
		}
		using (List<string>.Enumerator enumerator = RecipientTags.GetEnumerator())
		{
			string current;
			do
			{
				if (enumerator.MoveNext())
				{
					current = enumerator.Current;
					continue;
				}
				return;
			}
			while (!(col.gameObject.tag == current));
		}
		m_Player = col.transform.root.GetComponent<vp_PlayerEventHandler>();
		if (!(m_Player == null) && (!(m_Player.Interactable.Get() != null) || !(m_Player.Interactable.Get().GetComponent<Collider>() == col)))
		{
			TryInteract(m_Player);
		}
	}
}
