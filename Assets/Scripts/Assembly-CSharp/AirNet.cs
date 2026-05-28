using UnityEngine;

public class AirNet : MonoBehaviour
{
	public int hitForceToDestroy = 300;

	public float hoursToStartRecovery = 0.1f;

	public float hoursToRecover = 0.1f;

	public Color fullColor = Color.white;

	public Color brokenColor = Color.red;

	private TimeDirector timeDir;

	private Collider netCollider;

	private Material netMaterial;

	private float netStrength = 1f;

	private double recoverStartTime;

	private float dmgPerImpulse;

	private float recoverFactor;

	private const float NEW_NET_STRENGTH = 0.33f;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		netCollider = GetComponent<Collider>();
		netMaterial = GetComponent<Renderer>().material;
		dmgPerImpulse = 1f / (float)hitForceToDestroy;
		recoverFactor = 1f / (hoursToRecover * 3600f);
	}

	public void OnDestroy()
	{
		Destroyer.Destroy(netMaterial, "AirNet.OnDestroy");
	}

	public void OnCollisionEnter(Collision col)
	{
		if (Identifiable.IsSlime(Identifiable.GetId(col.gameObject)))
		{
			float magnitude = col.impulse.magnitude;
			netStrength = Mathf.Max(0f, netStrength - magnitude * dmgPerImpulse);
			recoverStartTime = timeDir.HoursFromNow(hoursToRecover);
		}
	}

	public void Update()
	{
		if (netStrength < 1f && timeDir.HasReached(recoverStartTime))
		{
			netStrength = Mathf.Clamp((float)((double)netStrength + timeDir.DeltaWorldTime() * (double)recoverFactor), 0.33f, 1f);
		}
		netCollider.enabled = netStrength > 0f;
		netMaterial.color = CurrColor();
	}

	private Color CurrColor()
	{
		if (netStrength <= 0f)
		{
			return Color.clear;
		}
		return Color.Lerp(brokenColor, fullColor, netStrength);
	}

	public bool IsNetActive()
	{
		if (base.gameObject.activeInHierarchy)
		{
			return netStrength > 0f;
		}
		return false;
	}
}
