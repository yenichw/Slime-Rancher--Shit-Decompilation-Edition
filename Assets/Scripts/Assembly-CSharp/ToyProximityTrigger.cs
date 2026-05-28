using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ToyProximityTrigger : SRBehaviour
{
	private struct Metadata
	{
		public bool isFavorite;
	}

	[Tooltip("Fashion pairing. Slimes wearing this fashion will consider us a 'favorite' toy.")]
	public Identifiable.Id fashion;

	private const float CLEANUP_TIME_DELAY = 30f;

	private float nextCleanupTime;

	private Dictionary<GameObject, Metadata> registered = new Dictionary<GameObject, Metadata>();

	private Identifiable.Id id;

	public void Awake()
	{
		id = GetComponentInParent<Identifiable>().id;
	}

	public void OnDestroy()
	{
		List<GameObject> list = registered.Keys.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Deregister(list[i]);
		}
	}

	public void Update()
	{
		if (Time.time >= nextCleanupTime)
		{
			List<GameObject> list = registered.Keys.Where((GameObject go) => go == null).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				registered.Remove(list[i]);
			}
			nextCleanupTime = Time.time + 30f;
		}
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (collider.isTrigger)
		{
			return;
		}
		SlimeEmotions component = collider.gameObject.GetComponent<SlimeEmotions>();
		if (!(component == null))
		{
			ReactToToyNearby component2 = collider.gameObject.GetComponent<ReactToToyNearby>();
			if (!(component2 == null) && !registered.ContainsKey(collider.gameObject))
			{
				Register(collider.gameObject, component, component2);
			}
		}
	}

	public void OnTriggerExit(Collider collider)
	{
		if (!collider.isTrigger)
		{
			Deregister(collider.gameObject);
		}
	}

	private void Register(GameObject other, SlimeEmotions emotions, ReactToToyNearby reaction)
	{
		Metadata value = default(Metadata);
		value.isFavorite = IsFavorite(other, reaction.slimeDefinition);
		registered[other] = value;
		emotions.AddNearbyToy(value.isFavorite);
		if (id == Identifiable.Id.RUBBER_DUCKY_TOY)
		{
			SlimeEatWater component = other.GetComponent<SlimeEatWater>();
			if (component != null)
			{
				component.EnterToyProximity();
			}
		}
	}

	private void Deregister(GameObject other)
	{
		if (!registered.TryGetValue(other, out var value))
		{
			return;
		}
		registered.Remove(other);
		if (!(other != null))
		{
			return;
		}
		other.GetComponent<SlimeEmotions>().RemoveNearbyToy(value.isFavorite);
		if (id == Identifiable.Id.RUBBER_DUCKY_TOY)
		{
			SlimeEatWater component = other.GetComponent<SlimeEatWater>();
			if (component != null)
			{
				component.ExitToyProximity();
			}
		}
	}

	private bool IsFavorite(GameObject other, SlimeDefinition slimeDefinition)
	{
		if (slimeDefinition.FavoriteToys.Contains(id))
		{
			return true;
		}
		if (fashion != 0)
		{
			AttachFashions component = other.GetComponent<AttachFashions>();
			if (component != null && component.HasFashion(fashion))
			{
				return true;
			}
		}
		return false;
	}
}
