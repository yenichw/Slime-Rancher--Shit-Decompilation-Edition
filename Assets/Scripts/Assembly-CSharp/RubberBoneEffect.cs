using System;
using UnityEngine;

public class RubberBoneEffect : MonoBehaviour
{
	public enum RubberType
	{
		Custom = 0,
		RubberDuck = 1,
		HardRubber = 2,
		Jelly = 3,
		SoftLatex = 4,
		Slime = 5,
		SlimeTarr = 6
	}

	[Serializable]
	public class BoneEntry
	{
		public Transform trans;

		public float intensity;
	}

	internal class VertexRubber
	{
		private RubberBoneEffect effect;

		public Vector3 pos;

		public Vector3 vel;

		public Vector3 force;

		public Vector3 rootPos;

		public float intensity;

		public Vector3 lastLocalPos;

		private BoneEntry bone;

		public VertexRubber(BoneEntry bone, RubberBoneEffect effect)
		{
			this.bone = bone;
			this.effect = effect;
			pos = bone.trans.position;
			rootPos = bone.trans.localPosition;
			lastLocalPos = rootPos;
			intensity = bone.intensity * effect.effectIntensity;
			Reset();
		}

		public void Reset()
		{
			Vector3 vector = rootPos + Vector3.down * ((effect.gravity * 5f - effect.damping) * 0.75f);
			Vector3 vector2 = effect.transform.TransformPoint(vector);
			lastLocalPos = vector;
			pos = vector2;
			force = Vector3.zero;
			vel = Vector3.zero;
			bone.trans.localPosition = vector;
		}

		public void LateUpdate(float timeFactor, float timeAdjDamping, float heldFactor)
		{
			Vector3 vector = effect.transform.TransformPoint(rootPos) - pos;
			force = vector * (effect.stiffness * Mathf.Min(1f, vector.magnitude));
			force.y -= effect.gravity * 0.1f;
			vel = timeAdjDamping * (vel + force * (timeFactor * effect.invMass));
			pos += vel * timeFactor;
			Vector3 b = effect.transform.InverseTransformPoint(pos);
			bone.trans.localPosition = Vector3.Lerp(rootPos, b, intensity * heldFactor);
			lastLocalPos = bone.trans.localPosition;
		}
	}

	public RubberType Presets;

	public float effectIntensity = 1f;

	public float gravity;

	public float damping = 0.7f;

	public float mass = 1f;

	public float stiffness = 0.2f;

	[Tooltip("All the bones we want to manipulate and how much to manipulate them.")]
	public BoneEntry[] bones;

	public SkinnedMeshRenderer skinRenderer;

	[Tooltip("If the effect should ignore time scaling.")]
	public bool unscaledTime;

	private float invMass;

	private float vacHeldFactor = 0.1f;

	private VertexRubber[] verts;

	private bool sleeping = true;

	private Vector3 lastWorldPosition;

	private Quaternion lastWorldRotation;

	private bool wasSleeping;

	private Vacuumable vacuumable;

	private Rigidbody ownBody;

	private Renderer ownRenderer;

	public void Reset()
	{
		for (int i = 0; i < verts.Length; i++)
		{
			verts[i].Reset();
		}
	}

	private void Start()
	{
		ownBody = GetComponentInParent<Rigidbody>();
		ownRenderer = ((skinRenderer != null) ? skinRenderer : GetComponentInParent<Renderer>());
		CheckPreset();
		invMass = 1f / mass;
		vacuumable = GetComponentInParent<Vacuumable>();
		verts = new VertexRubber[bones.Length];
		for (int i = 0; i < bones.Length; i++)
		{
			verts[i] = new VertexRubber(bones[i], this);
		}
	}

	public void OnEnable()
	{
		wasSleeping = true;
	}

	private void LateUpdate()
	{
		bool flag = (ownBody != null && ownBody.IsSleeping()) || (ownRenderer != null && !ownRenderer.isVisible);
		if (flag)
		{
			if (!sleeping)
			{
				VertexRubber[] array = verts;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Reset();
				}
			}
			sleeping = true;
		}
		else
		{
			if (wasSleeping || base.transform.position != lastWorldPosition || base.transform.rotation != lastWorldRotation)
			{
				VertexRubber[] array = verts;
				foreach (VertexRubber vertexRubber in array)
				{
					if (wasSleeping)
					{
						vertexRubber.Reset();
					}
				}
				sleeping = false;
			}
			if (!sleeping)
			{
				float heldFactor = ((vacuumable != null && vacuumable.isHeld()) ? vacHeldFactor : 1f);
				float num = (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * 60f;
				float timeAdjDamping = Mathf.Pow(damping, num);
				VertexRubber[] array = verts;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].LateUpdate(num, timeAdjDamping, heldFactor);
				}
				lastWorldPosition = base.transform.position;
				lastWorldRotation = base.transform.rotation;
			}
		}
		wasSleeping = flag;
	}

	private void CheckPreset()
	{
		switch (Presets)
		{
		case RubberType.HardRubber:
			gravity = 0f;
			mass = 8f;
			stiffness = 0.5f;
			damping = 0.9f;
			effectIntensity = 0.5f;
			break;
		case RubberType.Jelly:
			gravity = 0f;
			mass = 1f;
			stiffness = 0.95f;
			damping = 0.95f;
			effectIntensity = 1f;
			break;
		case RubberType.RubberDuck:
			gravity = 0f;
			mass = 2f;
			stiffness = 0.5f;
			damping = 0.85f;
			effectIntensity = 1f;
			break;
		case RubberType.SoftLatex:
			gravity = 1f;
			mass = 0.9f;
			stiffness = 0.3f;
			damping = 0.25f;
			effectIntensity = 1f;
			break;
		case RubberType.Slime:
			gravity = 0.2f;
			mass = 6f;
			stiffness = 1f;
			damping = 0.75f;
			effectIntensity = 1f;
			break;
		case RubberType.SlimeTarr:
			gravity = 0.2f;
			mass = 8f;
			stiffness = 1f;
			damping = 0.85f;
			effectIntensity = 1f;
			break;
		}
	}
}
