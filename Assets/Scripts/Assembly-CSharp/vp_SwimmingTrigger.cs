using System.Collections.Generic;
using UnityEngine;

public class vp_SwimmingTrigger : MonoBehaviour
{
	public LayerMask LayerMask = 256;

	public string StateName = "Swimming";

	protected static string m_IsPlayerKey = "Is Player";

	protected static string m_PlayerKey = "Player";

	protected virtual void Start()
	{
		GetComponent<Collider>().isTrigger = true;
	}

	protected virtual void OnTriggerEnter(Collider col)
	{
		Dictionary<string, object> dataForCollider = GetDataForCollider(col);
		if ((bool)dataForCollider[m_IsPlayerKey])
		{
			vp_FPPlayerEventHandler vp_FPPlayerEventHandler2 = (vp_FPPlayerEventHandler)dataForCollider[m_PlayerKey];
			vp_FPPlayerEventHandler2.SetState(StateName);
			vp_FPPlayerEventHandler2.MotorThrottle.Set(Vector3.zero);
			vp_FPPlayerEventHandler2.Jump.TryStop();
			Vector3 force = new Vector3(0f, vp_FPPlayerEventHandler2.Velocity.Get().normalized.y * 0.25f, 0f);
			vp_FPPlayerEventHandler2.Stop.Send();
			vp_FPPlayerEventHandler2.GetComponent<vp_FPController>().AddSoftForce(force, 10f);
		}
	}

	protected virtual void OnTriggerExit(Collider col)
	{
		Dictionary<string, object> dataForCollider = GetDataForCollider(col);
		if ((bool)dataForCollider[m_IsPlayerKey])
		{
			((vp_FPPlayerEventHandler)dataForCollider[m_PlayerKey]).SetState(StateName, setActive: false);
		}
	}

	protected virtual Dictionary<string, object> GetDataForCollider(Collider col)
	{
		if ((LayerMask.value & (1 << col.gameObject.layer)) == 0)
		{
			return new Dictionary<string, object> { { m_IsPlayerKey, false } };
		}
		vp_FPPlayerEventHandler component = col.gameObject.GetComponent<vp_FPPlayerEventHandler>();
		if (component == null)
		{
			return new Dictionary<string, object> { { m_IsPlayerKey, false } };
		}
		return new Dictionary<string, object>
		{
			{ m_IsPlayerKey, true },
			{ m_PlayerKey, component }
		};
	}
}
