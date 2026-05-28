using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class LiquidSource : IdHandler<LiquidSourceModel>
{
	public Identifiable.Id liquidId;

	[Tooltip("A position marking the top of the water at which objects should float.")]
	public Transform waterTop;

	public float bounceDamp = 0.8f;

	public float floatForcePerDepth = 10f;

	private ReferenceCount<Rigidbody> floating = new ReferenceCount<Rigidbody>();

	protected LiquidSourceModel model;

	protected override string IdPrefix()
	{
		return "LiquidSource";
	}

	protected override GameModel.Unregistrant Register(GameModel game)
	{
		return game.LiquidSources.Register(this);
	}

	protected override void InitModel(LiquidSourceModel model)
	{
		model.pos = base.transform.position;
		model.isScaling = false;
		model.unitsFilled = 0f;
	}

	protected override void SetModel(LiquidSourceModel model)
	{
		this.model = model;
	}

	public void FixedUpdate()
	{
		if (!CountsAsUnderwater())
		{
			return;
		}
		List<Rigidbody> list = new List<Rigidbody>();
		foreach (Rigidbody key in floating.Keys)
		{
			if (ShouldRemoveBody(key))
			{
				list.Add(key);
				if (key != null)
				{
					UpdateFloatingReactors(key, isFloating: false);
				}
			}
			else
			{
				Buoyancy(key);
			}
		}
		foreach (Rigidbody item in list)
		{
			floating.Remove(item);
		}
	}

	private bool ShouldRemoveBody(Rigidbody body)
	{
		if (body == null || !body.gameObject.activeInHierarchy)
		{
			return true;
		}
		Collider[] components = body.GetComponents<Collider>();
		foreach (Collider collider in components)
		{
			if (!collider.isTrigger && collider.enabled)
			{
				return false;
			}
		}
		return true;
	}

	public void OnTriggerEnter(Collider collider)
	{
		Rigidbody floatingRigidBody = GetFloatingRigidBody(collider);
		if (floatingRigidBody != null && floating.Increment(floatingRigidBody) == 1)
		{
			UpdateFloatingReactors(floatingRigidBody, isFloating: true);
		}
	}

	public void OnTriggerExit(Collider collider)
	{
		Rigidbody floatingRigidBody = GetFloatingRigidBody(collider);
		if (floatingRigidBody != null && floating.Decrement(floatingRigidBody) == 0)
		{
			UpdateFloatingReactors(floatingRigidBody, isFloating: false);
		}
	}

	private Rigidbody GetFloatingRigidBody(Collider collider)
	{
		if (collider.isTrigger || !CountsAsUnderwater())
		{
			return null;
		}
		Identifiable componentInParent = collider.GetComponentInParent<Identifiable>();
		if (componentInParent == null || Identifiable.IsWater(componentInParent.id) || Identifiable.IsEcho(componentInParent.id) || Identifiable.IsEchoNote(componentInParent.id))
		{
			return null;
		}
		return collider.GetComponentInParent<Rigidbody>();
	}

	public bool CountsAsUnderwater()
	{
		return waterTop != null;
	}

	private void Buoyancy(Rigidbody body)
	{
		if (!CountsAsUnderwater())
		{
			return;
		}
		BuoyancyOffset component = body.GetComponent<BuoyancyOffset>();
		List<Vector3> list = new List<Vector3>();
		float num = 1f;
		if (component != null && component.centersOfBuoyancy.Count > 0)
		{
			foreach (Vector3 item in component.centersOfBuoyancy)
			{
				list.Add(body.transform.TransformPoint(item));
			}
			num = component.buoyancyFactor;
		}
		else
		{
			list.Add(body.transform.position);
		}
		foreach (Vector3 item2 in list)
		{
			float num2 = waterTop.position.y - item2.y;
			if (num2 > 0f)
			{
				float num3 = num2 * floatForcePerDepth;
				float y = body.velocity.y;
				Vector3 force = -Physics.gravity * (num * (num3 - y * bounceDamp) / (float)list.Count);
				body.AddForceAtPosition(force, item2);
			}
		}
	}

	public void OnDisable()
	{
		floating.Clear();
	}

	private void UpdateFloatingReactors(Rigidbody body, bool isFloating)
	{
		FloatingReactor[] componentsInParent = body.GetComponentsInParent<FloatingReactor>();
		for (int i = 0; i < componentsInParent.Length; i++)
		{
			componentsInParent[i]?.SetIsFloating(isFloating);
		}
	}

	public virtual bool Available()
	{
		return true;
	}

	public virtual void ConsumeLiquid()
	{
	}

	public virtual bool ReplacesExistingLiquidAmmo()
	{
		return false;
	}
}
