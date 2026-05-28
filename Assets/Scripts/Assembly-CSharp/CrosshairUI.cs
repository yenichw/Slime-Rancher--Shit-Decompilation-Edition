using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
	public float normScale = 1f;

	public float highScale = 1.5f;

	public float gadgetScale = 2f;

	public float vibrateAmount = 0.4f;

	public Color hasTarget = Color.green;

	public Color noTarget = Color.white;

	public Color gadgetTarget = Color.white;

	public Sprite normalSprite;

	public Sprite gadgetSprite;

	private PlayerState player;

	private float hudCrosshairScale;

	private float hudCrosshairScaleGoal;

	private Image img;

	private WeaponVacuum vacuum;

	private void Start()
	{
		player = SRSingleton<SceneContext>.Instance.PlayerState;
		vacuum = SRSingleton<SceneContext>.Instance.Player.GetComponentInChildren<WeaponVacuum>();
		hudCrosshairScale = normScale;
		hudCrosshairScaleGoal = normScale;
		img = GetComponent<Image>();
	}

	public void Update()
	{
		if (!(Time.timeScale <= 0f))
		{
			bool flag = vacuum.InGadgetMode();
			if (flag && img.sprite != gadgetSprite)
			{
				img.sprite = gadgetSprite;
			}
			else if (!flag && img.sprite != normalSprite)
			{
				img.sprite = normalSprite;
			}
			if (flag)
			{
				img.color = gadgetTarget;
			}
			else if (player.PointedAtVaccable)
			{
				img.color = hasTarget;
			}
			else
			{
				img.color = noTarget;
			}
			if (flag)
			{
				hudCrosshairScaleGoal = gadgetScale;
			}
			else if (vacuum.InVacMode())
			{
				hudCrosshairScaleGoal = highScale;
			}
			else
			{
				hudCrosshairScaleGoal = normScale;
			}
			hudCrosshairScale += (hudCrosshairScaleGoal - hudCrosshairScale) * 0.95f * Time.deltaTime * 4f;
			if (vacuum.InVacMode() && hudCrosshairScaleGoal >= highScale)
			{
				hudCrosshairScale = highScale + Randoms.SHARED.GetInRange(0f, vibrateAmount);
			}
			img.transform.localScale = new Vector3(hudCrosshairScale, hudCrosshairScale, hudCrosshairScale);
		}
	}
}
