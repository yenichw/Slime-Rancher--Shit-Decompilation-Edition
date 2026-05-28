using UnityEngine;

[RequireComponent(typeof(Vacuumable))]
public class ToyTreasureChest : MonoBehaviour
{
	[Tooltip("Rigidbody of the chest lid.")]
	public Rigidbody chestLid;

	private Vacuumable vacuumable;

	public void Awake()
	{
		vacuumable = GetComponent<Vacuumable>();
	}

	public void Update()
	{
		chestLid.isKinematic = vacuumable.isHeld();
	}
}
