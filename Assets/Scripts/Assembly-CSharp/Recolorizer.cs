using UnityEngine;

public class Recolorizer : MonoBehaviour
{
	public void Start()
	{
		RanchDirector ranchDirector = SRSingleton<SceneContext>.Instance.RanchDirector;
		ZoneDirector componentInParent = GetComponentInParent<ZoneDirector>();
		ZoneDirector.Zone zone = ((!(componentInParent == null)) ? componentInParent.zone : ZoneDirector.Zone.RANCH);
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			Material[] array = new Material[renderer.sharedMaterials.Length];
			for (int j = 0; j < renderer.sharedMaterials.Length; j++)
			{
				Material material = renderer.sharedMaterials[j];
				if (material == null)
				{
					array[j] = material;
					continue;
				}
				Material recolorMaterial = ranchDirector.GetRecolorMaterial(material, zone);
				if (recolorMaterial != null)
				{
					array[j] = recolorMaterial;
				}
				else
				{
					array[j] = material;
				}
			}
			renderer.sharedMaterials = array;
		}
	}
}
