using UnityEngine;

public class SlimeUtil
{
	private static Color[] DEFAULTS = new Color[3]
	{
		Color.grey,
		Color.grey,
		Color.grey
	};

	private static int TopColorPropertyId = Shader.PropertyToID("_TopColor");

	private static int MiddleColorPropertyId = Shader.PropertyToID("_MiddleColor");

	private static int BottomColorPropertyId = Shader.PropertyToID("_BottomColor");

	private static int ColorRampPropertyId = Shader.PropertyToID("_ColorRamp");

	public static Color[] GetColors(GameObject slimeObj, Identifiable.Id identId, bool isGordo = false)
	{
		if (Identifiable.IsTarr(identId))
		{
			return GetColors(slimeObj, "prefab_slimeBase/slime_tarr");
		}
		if (!isGordo && identId == Identifiable.Id.GOLD_SLIME)
		{
			return GetColors(slimeObj, "prefab_slimeBase/slime_gold");
		}
		string transformRootPath = (isGordo ? "Vibrating/slime_gordo" : "prefab_slimeBase/slime_default");
		return GetColors(slimeObj, transformRootPath);
	}

	private static Color[] GetColors(GameObject slimeObj, string transformRootPath)
	{
		Transform transform = slimeObj.transform.Find(transformRootPath);
		if (transform == null)
		{
			Log.Warning("Could not find renderer transform, returning default colors: " + slimeObj.name);
			return DEFAULTS;
		}
		Renderer component = transform.GetComponent<Renderer>();
		if (component == null)
		{
			Log.Warning("Could not get renderer, returning default colors: " + slimeObj.name);
			return DEFAULTS;
		}
		Material material = component.sharedMaterials[0];
		return new Color[3]
		{
			material.GetColor(TopColorPropertyId),
			material.GetColor(MiddleColorPropertyId),
			material.GetColor(BottomColorPropertyId)
		};
	}

	public static Material SetTarrColors(GameObject slimeObj, Color[] colors)
	{
		Transform transform = slimeObj.transform.Find("prefab_slimeBase/slime_tarr");
		if (transform == null)
		{
			Log.Warning("Could not find renderer transform, returning default colors: " + slimeObj.name);
			return null;
		}
		Renderer component = transform.GetComponent<Renderer>();
		if (component == null)
		{
			Log.Warning("Could not get renderer, returning default colors: " + slimeObj.name);
			return null;
		}
		Material material = component.material;
		material.SetColor(TopColorPropertyId, colors[0]);
		material.SetColor(MiddleColorPropertyId, colors[0]);
		material.SetColor(BottomColorPropertyId, colors[0]);
		return material;
	}

	public static Material SetTarrSterile(GameObject slimeObj, Texture rampTex)
	{
		Transform transform = slimeObj.transform.Find("prefab_slimeBase/slime_tarr");
		Transform transform2 = slimeObj.transform.Find("prefab_slimeBase/slime_tarr_bite");
		Transform transform3 = slimeObj.transform.Find("prefab_slimeBase/bone_root/bone_slime/slime_default_LOD1");
		Transform transform4 = slimeObj.transform.Find("prefab_slimeBase/bone_root/bone_slime/slime_default_LOD2");
		Transform transform5 = slimeObj.transform.Find("prefab_slimeBase/bone_root/bone_slime/slime_default_LOD3");
		if (transform == null)
		{
			Log.Warning("Could not find renderer transform, returning default colors: " + slimeObj.name);
			return null;
		}
		Renderer component = transform.GetComponent<Renderer>();
		if (component == null)
		{
			Log.Warning("Could not get renderer, returning default colors: " + slimeObj.name);
			return null;
		}
		Material material = component.material;
		material.SetTexture(ColorRampPropertyId, rampTex);
		Transform[] array = new Transform[4] { transform2, transform3, transform4, transform5 };
		foreach (Transform transform6 in array)
		{
			if (transform6 != null)
			{
				Renderer component2 = transform6.GetComponent<Renderer>();
				if (component2 != null)
				{
					component2.material = material;
				}
			}
		}
		return material;
	}

	public static FixedJoint AttachToMouth(GameObject slimeObj, GameObject target)
	{
		Vector3 vector = Vector3.forward * (PhysicsUtil.RadiusOfObject(slimeObj) + PhysicsUtil.RadiusOfObject(target) * 0.25f) / slimeObj.transform.localScale.z;
		Rigidbody component = target.GetComponent<Rigidbody>();
		target.transform.position = slimeObj.transform.position + slimeObj.transform.localToWorldMatrix.MultiplyVector(vector);
		Vector3 velocity = (component.angularVelocity = Vector3.zero);
		component.velocity = velocity;
		FixedJoint fixedJoint = slimeObj.AddComponent<FixedJoint>();
		SafeJointReference.AttachSafely(target, fixedJoint);
		fixedJoint.anchor = vector;
		fixedJoint.breakForce = 1000f;
		fixedJoint.breakTorque = 1000f;
		return fixedJoint;
	}
}
