using Assets.Script.Util.Extensions;
using UnityEngine;

public class TarrSpawnFX : MonoBehaviour
{
	public GameObject SpawnFX;

	private BiteEventAggregator aggregator;

	public void Awake()
	{
		aggregator = base.gameObject.GetRequiredComponentInChildren<BiteEventAggregator>();
	}

	public void Start()
	{
		aggregator.OnSpawnBubbles += OnSpawnBubbles;
	}

	public void OnSpawnBubbles()
	{
		SRBehaviour.SpawnAndPlayFX(SpawnFX, base.gameObject);
	}

	public void Destroy()
	{
		if (aggregator != null)
		{
			aggregator.OnSpawnBubbles -= OnSpawnBubbles;
		}
	}
}
