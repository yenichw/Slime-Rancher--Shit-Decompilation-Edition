using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageCycler : MonoBehaviour
{
	public Sprite[] sprites;

	[Tooltip("Seconds between frames.")]
	public float timePerFrame = 1f;

	private Image img;

	private float flipTime;

	private int idx;

	public void Awake()
	{
		img = GetComponent<Image>();
		if (sprites != null)
		{
			SetSprites(sprites);
		}
	}

	public void OnEnable()
	{
		flipTime = Time.time;
	}

	public void SetSprites(Sprite[] sprites)
	{
		this.sprites = sprites;
		if (sprites.Length != 0)
		{
			idx = 0;
			img.sprite = sprites[idx];
		}
		flipTime = Time.time + timePerFrame;
	}

	public void Update()
	{
		if (sprites.Length >= 2 && Time.time > flipTime)
		{
			idx = (idx + 1) % sprites.Length;
			img.sprite = sprites[idx];
			flipTime += timePerFrame;
		}
	}
}
