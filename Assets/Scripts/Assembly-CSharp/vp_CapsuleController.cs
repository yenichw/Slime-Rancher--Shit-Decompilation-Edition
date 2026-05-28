using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class vp_CapsuleController : vp_Controller
{
	protected CapsuleCollider m_CapsuleCollider;

	protected CapsuleCollider CapsuleCollider
	{
		get
		{
			if (m_CapsuleCollider == null)
			{
				m_CapsuleCollider = base.Collider as CapsuleCollider;
				if (m_CapsuleCollider != null && m_CapsuleCollider.isTrigger)
				{
					m_CapsuleCollider = null;
				}
			}
			return m_CapsuleCollider;
		}
	}

	protected override float OnValue_Radius => CapsuleCollider.radius;

	protected override float OnValue_Height => CapsuleCollider.height;

	protected override void InitCollider()
	{
		m_NormalHeight = CapsuleCollider.height;
		CapsuleCollider.center = (m_NormalCenter = m_NormalHeight * (Vector3.up * 0.5f));
		CapsuleCollider.radius = m_NormalHeight * 0.25f;
		m_CrouchHeight = m_NormalHeight * PhysicsCrouchHeightModifier;
		m_CrouchCenter = m_NormalCenter * PhysicsCrouchHeightModifier;
		base.Collider.transform.localPosition = Vector3.zero;
	}

	protected override void RefreshCollider()
	{
		if (base.Player.Crouch.Active && (!MotorFreeFly || base.Grounded))
		{
			CapsuleCollider.height = m_NormalHeight * PhysicsCrouchHeightModifier;
			CapsuleCollider.center = m_NormalCenter * PhysicsCrouchHeightModifier;
		}
		else
		{
			CapsuleCollider.height = m_NormalHeight;
			CapsuleCollider.center = m_NormalCenter;
		}
	}

	public override void EnableCollider(bool isEnabled = true)
	{
		if (CapsuleCollider != null)
		{
			CapsuleCollider.enabled = isEnabled;
		}
	}

	protected override float Get_Radius()
	{
		return CapsuleCollider.radius;
	}

	protected override float Get_Height()
	{
		return CapsuleCollider.height;
	}
}
