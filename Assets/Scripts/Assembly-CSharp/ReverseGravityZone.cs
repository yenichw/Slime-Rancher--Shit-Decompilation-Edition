using System.Collections.Generic;
using UnityEngine;

public class ReverseGravityZone : MonoBehaviour
{
	private bool operating;

	private List<Rigidbody> bodies = new List<Rigidbody>();

	public void SetOperating(bool operating)
	{
		if (operating != this.operating)
		{
			this.operating = operating;
		}
	}

	public bool GetOperating()
	{
		return operating;
	}

	public void OnTriggerEnter(Collider col)
	{
		Rigidbody component = col.GetComponent<Rigidbody>();
		if (component != null)
		{
			bodies.Add(component);
		}
	}

	public void OnTriggerExit(Collider col)
	{
		Rigidbody component = col.GetComponent<Rigidbody>();
		if (component != null)
		{
			bodies.Remove(component);
		}
	}

	public void Update()
	{
		if (!operating)
		{
			return;
		}
		List<Rigidbody> list = new List<Rigidbody>();
		foreach (Rigidbody body in bodies)
		{
			if (body == null)
			{
				list.Add(body);
			}
			else
			{
				AntiGrav(body);
			}
		}
		foreach (Rigidbody item in list)
		{
			bodies.Remove(item);
		}
	}

	private void AntiGrav(Rigidbody body)
	{
		float num = 2f;
		Vector3 force = -Physics.gravity * num;
		body.AddForce(force, ForceMode.Acceleration);
	}
}
