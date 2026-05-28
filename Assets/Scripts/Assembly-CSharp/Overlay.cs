using UnityEngine;

public class Overlay : SRSingleton<Overlay>
{
	public GameObject teleportFX;

	public GameObject damageFX;

	public GameObject chompFX;

	public GameObject radFX;

	public GameObject firestormFX;

	public GameObject gadgetFX;

	[Tooltip("FX played while the dash pad is active.")]
	public GameObject dashPadFX;

	private GameObject activeRadFX;

	private Material activeRadMat;

	private float tgtRadAlpha;

	private GameObject activeFirestormFX;

	private Material activeFirestormMat;

	private float tgtFirestormAlpha;

	private GameObject activeGadgetFX;

	private Material activeGadgetMat;

	private float tgtGadgetAlpha;

	private const float RAD_ALPHA_DELTA = 2f;

	private const float FIRESTORM_ALPHA_DELTA = 2f;

	private const float GADGET_ALPHA_DELTA = 2f;

	public void Update()
	{
		if (activeRadFX != null)
		{
			Color color = activeRadMat.color;
			float num = color.a;
			if (tgtRadAlpha > num)
			{
				num = Mathf.Min(tgtRadAlpha, num + 2f * Time.deltaTime);
			}
			else if (tgtRadAlpha < num)
			{
				num = Mathf.Max(tgtRadAlpha, num - 2f * Time.deltaTime);
			}
			color.a = num;
			activeRadMat.color = color;
			if (num <= 0f)
			{
				Destroyer.Destroy(activeRadFX, "Overlay.Update#1");
				Destroyer.Destroy(activeRadMat, "Overlay.Update#2");
				activeRadFX = null;
			}
		}
		if (activeFirestormFX != null)
		{
			Color color2 = activeFirestormMat.color;
			float num2 = color2.a;
			if (tgtFirestormAlpha > num2)
			{
				num2 = Mathf.Min(tgtFirestormAlpha, num2 + 2f * Time.deltaTime);
			}
			else if (tgtFirestormAlpha < num2)
			{
				num2 = Mathf.Max(tgtFirestormAlpha, num2 - 2f * Time.deltaTime);
			}
			color2.a = num2;
			activeFirestormMat.color = color2;
			if (num2 <= 0f)
			{
				Destroyer.Destroy(activeFirestormFX, "Overlay.Update#3");
				Destroyer.Destroy(activeFirestormMat, "Overlay.Update#4");
				activeFirestormFX = null;
			}
		}
		if (activeGadgetFX != null)
		{
			Color color3 = activeGadgetMat.color;
			float num3 = color3.a;
			if (tgtGadgetAlpha > num3)
			{
				num3 = Mathf.Min(tgtGadgetAlpha, num3 + 2f * Time.deltaTime);
			}
			else if (tgtGadgetAlpha < num3)
			{
				num3 = Mathf.Max(tgtGadgetAlpha, num3 - 2f * Time.deltaTime);
			}
			color3.a = num3;
			activeGadgetMat.color = color3;
			if (num3 <= 0f)
			{
				Destroyer.Destroy(activeGadgetFX, "Overlay.Update#5");
				Destroyer.Destroy(activeGadgetMat, "Overlay.Update#6");
				activeGadgetFX = null;
			}
		}
	}

	public void PlayTeleport()
	{
		Play(teleportFX);
	}

	public void PlayDamage()
	{
		Play(damageFX);
	}

	public void PlayChomp()
	{
		Play(chompFX);
	}

	public void SetEnableRad(bool enabled)
	{
		tgtRadAlpha = (enabled ? 1f : 0f);
		if (enabled && activeRadFX == null)
		{
			activeRadFX = Play(radFX);
			activeRadMat = activeRadFX.GetComponent<Renderer>().material;
			Color color = activeRadMat.color;
			color.a = 0f;
			activeRadMat.color = color;
		}
	}

	public void SetEnableFirestorm(bool enabled)
	{
		tgtFirestormAlpha = (enabled ? 1f : 0f);
		if (enabled && activeFirestormFX == null)
		{
			activeFirestormFX = Play(firestormFX);
			activeFirestormMat = activeFirestormFX.GetComponent<Renderer>().material;
			Color color = activeFirestormMat.color;
			color.a = 0f;
			activeFirestormMat.color = color;
		}
	}

	public void SetEnableGadgetMode(bool enabled)
	{
		tgtGadgetAlpha = (enabled ? 1f : 0f);
		if (enabled && activeGadgetFX == null)
		{
			activeGadgetFX = Play(gadgetFX);
			activeGadgetMat = activeGadgetFX.GetComponent<Renderer>().material;
			Color color = activeGadgetMat.color;
			color.a = 0f;
			activeGadgetMat.color = color;
		}
	}

	public GameObject Play(GameObject fxOrig)
	{
		GameObject obj = Object.Instantiate(fxOrig, base.transform.position, base.transform.rotation);
		obj.transform.parent = base.transform;
		SRBehaviour.PlayFX(obj);
		return obj;
	}
}
