using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class vp_Interactable : MonoBehaviour
{
	public enum vp_InteractType
	{
		Normal = 0,
		Trigger = 1,
		CollisionTrigger = 2
	}

	public vp_InteractType InteractType;

	public List<string> RecipientTags = new List<string>();

	public float InteractDistance;

	public Texture m_InteractCrosshair;

	public string InteractText = "";

	public float DelayShowingText = 2f;

	protected Transform m_Transform;

	protected vp_FPController m_Controller;

	protected vp_FPCamera m_Camera;

	protected vp_WeaponHandler m_WeaponHandler;

	protected vp_PlayerEventHandler m_Player;

	protected virtual void Start()
	{
		m_Transform = base.transform;
		if (RecipientTags.Count == 0)
		{
			RecipientTags.Add("Player");
		}
		if (InteractType == vp_InteractType.Trigger && GetComponent<Collider>() != null)
		{
			GetComponent<Collider>().isTrigger = true;
		}
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	public virtual bool TryInteract(vp_PlayerEventHandler player)
	{
		return false;
	}

	protected virtual void OnTriggerEnter(Collider col)
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
		m_Player = col.gameObject.GetComponent<vp_PlayerEventHandler>();
		if (!(m_Player == null))
		{
			TryInteract(m_Player);
		}
	}

	public virtual void FinishInteraction()
	{
	}
}
