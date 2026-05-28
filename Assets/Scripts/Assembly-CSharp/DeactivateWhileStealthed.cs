using UnityEngine;

public class DeactivateWhileStealthed : MonoBehaviour
{
	private ParticleSystem particleSys;

	private SlimeStealth stealth;

	public void Start()
	{
		particleSys = GetComponent<ParticleSystem>();
		stealth = GetComponentInParent<SlimeStealth>();
	}

	public void Update()
	{
		if (stealth != null)
		{
			ParticleSystem.EmissionModule emission = particleSys.emission;
			emission.enabled = !stealth.IsStealthed;
		}
	}
}
