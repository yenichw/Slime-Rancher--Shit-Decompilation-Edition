using UnityEngine;
using UnityEngine.UI;

public class UpdateUIMaterialUnscaledTime : MonoBehaviour
{
	public Material[] mats;

	private int unscaledTimeVarId;

	private Graphic[] graphics;

	public void Awake()
	{
		unscaledTimeVarId = Shader.PropertyToID("_UnscaledTime");
		graphics = GetComponents<Graphic>();
	}

	public void Update()
	{
		Graphic[] array = graphics;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].materialForRendering.SetFloat(unscaledTimeVarId, Time.unscaledTime);
		}
	}
}
