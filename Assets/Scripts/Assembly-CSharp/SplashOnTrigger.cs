using UnityEngine;

public class SplashOnTrigger : SRBehaviour
{
	public GameObject splashFX;

	public GameObject playerSplashFX;

	private Collider[] splashColliders;

	private const float SPLASH_THRESHOLD = 4f;

	public void Awake()
	{
		splashColliders = GetComponents<Collider>();
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (PhysicsUtil.IsPlayerMainCollider(collider))
		{
			SpawnAndPlayFX(playerSplashFX, collider);
		}
		else
		{
			if (collider.isTrigger)
			{
				return;
			}
			Identifiable component = collider.gameObject.GetComponent<Identifiable>();
			if (component == null || component.isPhysicsInitialized)
			{
				Rigidbody component2 = collider.GetComponent<Rigidbody>();
				if (component2 != null && !component2.isKinematic && Mathf.Abs(component2.velocity.y) >= 4f)
				{
					SpawnAndPlayFX(splashFX, collider);
				}
			}
		}
	}

	private void SpawnAndPlayFX(GameObject prefab, Collider collider)
	{
		Ray ray = new Ray(collider.gameObject.transform.position, Vector3.down);
		float num = float.PositiveInfinity;
		Vector3 position = collider.gameObject.transform.position;
		Collider[] array = splashColliders;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Raycast(ray, out var hitInfo, 2f) && hitInfo.distance < num)
			{
				num = hitInfo.distance;
				position = hitInfo.point;
			}
		}
		SRBehaviour.SpawnAndPlayFX(prefab, position, Quaternion.identity);
	}
}
