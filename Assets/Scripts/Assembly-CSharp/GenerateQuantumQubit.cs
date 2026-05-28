using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateQuantumQubit : SRBehaviour
{
	public float QubitSearchRadius = 10f;

	private const float POSITION_CHECK_SPHERECAST_START = 1000f;

	private const float SPHERECAST_RADIUS = 0.6f;

	private const float QUBIT_RADIUS = 0.61f;

	private const int SPHERECAST_LAYER_MASK = -539068421;

	private const float MAX_QUBIT_HEIGHT = 20f;

	public float MaxQubitHeight = 10f;

	private const int MAX_GENERATION_ATTEMPTS = 5;

	public int MaxQubits = 5;

	private float nextQubitGenerationTime;

	private List<QubitWander> qubits = new List<QubitWander>();

	private static List<MeshCollider> collidersToReset = new List<MeshCollider>();

	private int generationAttempts;

	private SlimeSubbehaviourPlexer plexer;

	private SlimeEmotions slimeEmotions;

	private CalmedByWaterSpray calmedByWaterSpray;

	public float MinGenerationDelay = 5f;

	public float MaxGenerationDelay = 20f;

	public GameObject QubitPrefab;

	public SlimeAppearanceApplicator AppearanceApplicator;

	public GameObject DissipateFx;

	private const float MIN_PLACEMENT_Y = 0f;

	private static Collider[] Local_OverlapCount = new Collider[6];

	private void Awake()
	{
		plexer = base.gameObject.GetComponent<SlimeSubbehaviourPlexer>();
		slimeEmotions = base.gameObject.GetComponent<SlimeEmotions>();
		calmedByWaterSpray = GetComponent<CalmedByWaterSpray>();
	}

	public QubitWander GetRandomQubit()
	{
		return Randoms.SHARED.Pick(qubits.Where((QubitWander q) => q.HasArrived() && Physics.OverlapSphere(q.transform.position, 0.6f, -5).Length == 0), null);
	}

	public int GetQubitCount()
	{
		return qubits.Count;
	}

	public void ClearQubits()
	{
		DestroyQubits(spawnFX: true);
	}

	public void DissipateQubit(QubitWander qubit)
	{
		qubits.Remove(qubit);
		DestroyQubit(qubit.gameObject, spawnFX: true);
	}

	public bool ReadyForSuperposition()
	{
		if (qubits.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < qubits.Count; i++)
		{
			if (!qubits[i].HasArrived())
			{
				return false;
			}
		}
		return true;
	}

	private float GetGenerationDelay()
	{
		return Mathf.Lerp(MaxGenerationDelay, MinGenerationDelay, slimeEmotions.GetCurr(SlimeEmotions.Emotion.AGITATION));
	}

	private bool ReadyToGenerate()
	{
		if (nextQubitGenerationTime <= Time.time)
		{
			return !plexer.IsGrounded();
		}
		return false;
	}

	private void Start()
	{
		UpdateGenerationTime();
		generationAttempts = 5;
	}

	private void UpdateGenerationTime()
	{
		nextQubitGenerationTime = Time.time + GetGenerationDelay();
	}

	private void Update()
	{
		if (calmedByWaterSpray.IsCalmed())
		{
			DestroyQubits(spawnFX: true);
			UpdateGenerationTime();
		}
		if (ReadyToGenerate() && generationAttempts >= 5)
		{
			generationAttempts = 0;
		}
		if (generationAttempts >= 5 || !plexer.IsGrounded() || plexer.IsCaptive())
		{
			return;
		}
		if (FindValidQubitLocation(out var position))
		{
			GenerateQubit(position);
			if (qubits.Count > MaxQubits)
			{
				DissipateQubit(qubits[0]);
			}
			generationAttempts = 5;
		}
		else
		{
			generationAttempts++;
		}
		if (generationAttempts >= 5)
		{
			UpdateGenerationTime();
		}
	}

	private void GenerateQubit(Vector3 position)
	{
		SlimeAppearance qubitAppearance = AppearanceApplicator.Appearance.QubitAppearance;
		if (qubitAppearance == null)
		{
			Log.Error("No qubit appearance found for slime.", "name", base.gameObject.name);
		}
		GameObject obj = SRBehaviour.InstantiateDynamic(QubitPrefab, base.gameObject.transform.position, Quaternion.identity);
		SlimeAppearanceApplicator componentInChildren = obj.GetComponentInChildren<SlimeAppearanceApplicator>();
		componentInChildren.SlimeDefinition = AppearanceApplicator.SlimeDefinition;
		componentInChildren.Appearance = qubitAppearance;
		componentInChildren.ApplyAppearance();
		float prefabScale = AppearanceApplicator.SlimeDefinition.PrefabScale;
		obj.transform.localScale = new Vector3(prefabScale, prefabScale, prefabScale);
		QubitWander component = obj.GetComponent<QubitWander>();
		component.EndPosition = position;
		component.parentQuantumGenerator = this;
		qubits.Add(component);
	}

	private bool FindValidQubitLocation(out Vector3 position)
	{
		Vector2 vector = Random.insideUnitCircle * QubitSearchRadius;
		Vector3 position2 = base.gameObject.transform.position;
		position2.x += vector.x;
		position2.z += vector.y;
		return GetAdjustedQubitLocation(position2, out position);
	}

	public static bool GetAdjustedQubitLocation(Vector3 castFrom, out Vector3 position)
	{
		position = Vector3.zero;
		castFrom.y += 1000f;
		RaycastHit[] array = Physics.SphereCastAll(castFrom, 0.6f, Vector3.down, float.PositiveInfinity, -539068421);
		collidersToReset.Clear();
		Vector3 vector = Vector3.zero;
		float num = float.MaxValue;
		bool flag = false;
		if (array.Length != 0)
		{
			float num2 = QuantumCeiling.AdjustMinDist(castFrom, 980f);
			for (int i = 0; i < array.Length; i++)
			{
				MeshCollider component = array[i].collider.GetComponent<MeshCollider>();
				if (component != null && !component.convex && array[i].collider.GetComponent<Rigidbody>() == null)
				{
					collidersToReset.Add(component);
					try
					{
						component.convex = true;
					}
					catch
					{
						Log.Error("Exception when changing to convex.", "object name", component.name);
						throw;
					}
				}
				if (array[i].distance > num2 && array[i].distance < num && array[i].point.y >= 0f)
				{
					vector = new Vector3(array[i].point.x, array[i].point.y + 0.61f, array[i].point.z);
					num = array[i].distance;
					flag = true;
				}
			}
			if (flag && Physics.OverlapSphereNonAlloc(vector, 0.6f, Local_OverlapCount, -539068421) == 0 && !CorralRegion.IsWithin(vector))
			{
				flag = true;
				position = vector;
			}
			else
			{
				flag = false;
			}
			foreach (MeshCollider item in collidersToReset)
			{
				item.convex = false;
			}
		}
		return flag;
	}

	private void DestroyQubits(bool spawnFX)
	{
		foreach (QubitWander qubit in qubits)
		{
			if (qubit != null)
			{
				DestroyQubit(qubit.gameObject, spawnFX);
			}
		}
		qubits.Clear();
	}

	private void DestroyQubit(GameObject qubit, bool spawnFX)
	{
		if (spawnFX)
		{
			SRBehaviour.SpawnAndPlayFX(DissipateFx, qubit.transform.position, Quaternion.identity);
		}
		Destroyer.Destroy(qubit.gameObject, "GenerateQuantumQubit.DestroyQubit");
	}

	private void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			DestroyQubits(spawnFX: false);
		}
	}
}
