using System.Collections.Generic;
using UnityEngine;

public class RubberEffect : MonoBehaviour
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

	internal class VertexRubber
	{
		public int indexId;

		private RubberEffect effect;

		public Vector3 pos;

		public Vector3 vel;

		public Vector3 force;

		public Vector3 lastPos;

		public Vector3 lastVel;

		public Vector3 lastForce;

		public bool vertSleeping;

		public float colorIntensity;

		public VertexRubber(Vector3 target, RubberEffect effect)
		{
			this.effect = effect;
			pos = target;
		}

		public void Reset()
		{
			Vector3 vector = effect.transform.TransformPoint(effect.originalMesh.vertices[indexId]);
			lastVel = Vector3.zero;
			lastForce = Vector3.zero;
			lastPos = vector;
			pos = vector;
			force = Vector3.zero;
			vel = Vector3.zero;
			vertSleeping = false;
		}

		public void Update(Vector3 target, float timeFactor, float timeAdjDamping)
		{
			if (!vertSleeping)
			{
				force = (target - pos) * effect.stiffness;
				force.y -= effect.gravity * 0.1f;
				vel = timeAdjDamping * (vel + force * (timeFactor * effect.invMass));
				pos += vel * timeFactor;
				if (pos == lastPos && vel == lastVel && force == lastForce)
				{
					vertSleeping = true;
					return;
				}
				lastPos = pos;
				lastVel = vel;
				lastForce = force;
			}
		}
	}

	public RubberType Presets;

	public float effectIntensity = 1f;

	public float gravity;

	public float damping = 0.7f;

	public float mass = 1f;

	public float stiffness = 0.2f;

	private float invMass;

	private float vacHeldFactor = 0.33f;

	private Mesh workingMesh;

	private Mesh originalMesh;

	private VertexRubber[] verts;

	private Vector3[] workingMeshVectors;

	private bool sleeping = true;

	private Vector3 lastWorldPosition;

	private Quaternion lastWorldRotation;

	private bool wasVisible;

	private Vacuumable vacuumable;

	private void Start()
	{
		CheckPreset();
		invMass = 1f / mass;
		vacuumable = GetComponentInParent<Vacuumable>();
		MeshFilter meshFilter = (MeshFilter)GetComponent(typeof(MeshFilter));
		originalMesh = meshFilter.sharedMesh;
		workingMesh = Object.Instantiate(meshFilter.sharedMesh);
		meshFilter.sharedMesh = workingMesh;
		List<int> list = new List<int>();
		Color[] colors = originalMesh.colors;
		Vector3[] vertices = originalMesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			if (colors[i].grayscale != 1f)
			{
				list.Add(i);
			}
		}
		verts = new VertexRubber[list.Count];
		for (int j = 0; j < list.Count; j++)
		{
			int num = list[j];
			verts[j] = new VertexRubber(base.transform.TransformPoint(vertices[num]), this);
			verts[j].colorIntensity = (1f - colors[num].grayscale) * effectIntensity;
			verts[j].indexId = num;
		}
		workingMeshVectors = originalMesh.vertices;
	}

	public void OnEnable()
	{
		wasVisible = false;
	}

	private void LateUpdate()
	{
		if (!GetComponent<Renderer>().isVisible)
		{
			if (!sleeping)
			{
				workingMesh.vertices = originalMesh.vertices;
				workingMesh.RecalculateBounds();
				sleeping = true;
				VertexRubber[] array = verts;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].vertSleeping = true;
				}
			}
		}
		else
		{
			if (!wasVisible || base.transform.position != lastWorldPosition || base.transform.rotation != lastWorldRotation)
			{
				VertexRubber[] array = verts;
				foreach (VertexRubber vertexRubber in array)
				{
					if (vertexRubber.vertSleeping || !wasVisible)
					{
						vertexRubber.Reset();
					}
				}
				sleeping = false;
			}
			if (!sleeping)
			{
				float num = (vacuumable.isHeld() ? vacHeldFactor : 1f);
				workingMeshVectors = originalMesh.vertices;
				int num2 = 0;
				float num3 = Time.deltaTime / 0.016667f;
				float timeAdjDamping = Mathf.Pow(damping, num3);
				VertexRubber[] array = verts;
				foreach (VertexRubber vertexRubber2 in array)
				{
					if (vertexRubber2.vertSleeping)
					{
						num2++;
					}
					else
					{
						Vector3 target = base.transform.TransformPoint(workingMeshVectors[vertexRubber2.indexId]);
						vertexRubber2.Update(target, num3, timeAdjDamping);
					}
					Vector3 b = base.transform.InverseTransformPoint(vertexRubber2.pos);
					workingMeshVectors[vertexRubber2.indexId] = Vector3.Lerp(workingMeshVectors[vertexRubber2.indexId], b, vertexRubber2.colorIntensity * num);
					if (vertexRubber2.vertSleeping)
					{
						num2++;
					}
				}
				workingMesh.vertices = workingMeshVectors;
				workingMesh.RecalculateBounds();
				if (base.transform.position == lastWorldPosition && base.transform.rotation == lastWorldRotation)
				{
					if (num2 == verts.Length)
					{
						sleeping = true;
					}
				}
				else
				{
					lastWorldPosition = base.transform.position;
					lastWorldRotation = base.transform.rotation;
				}
			}
		}
		wasVisible = GetComponent<Renderer>().isVisible;
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
			gravity = 0.9f;
			mass = 6f;
			stiffness = 0.333f;
			damping = 0.85f;
			effectIntensity = 1f;
			break;
		case RubberType.SlimeTarr:
			gravity = 0.9f;
			mass = 8f;
			stiffness = 0.333f;
			damping = 0.85f;
			effectIntensity = 1f;
			break;
		}
	}
}
