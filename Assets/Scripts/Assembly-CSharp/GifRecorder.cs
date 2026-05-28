using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Gif.Components;
using UnityEngine;
using UnityEngine.Events;

public class GifRecorder : MonoBehaviour
{
	private class Frame
	{
		public RenderTexture tex;

		public float time;

		public float delay;

		public Frame(RenderTexture tex, float time, float delay)
		{
			this.tex = tex;
			this.time = time;
			this.delay = delay;
		}
	}

	private class CameraHook : SRBehaviour
	{
		public GifRecorder recorder;

		public void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			recorder.DoRenderImage(source);
			Graphics.Blit(source, destination);
		}

		public void OnDestroy()
		{
			recorder.hooks.Remove(this);
		}
	}

	public GameObject storeGifUI;

	public GameObject cannotStoreGifUI;

	private Queue<Frame> frames = new Queue<Frame>();

	private float nextFrameTime;

	private float lastFrameTime;

	private List<CameraHook> hooks = new List<CameraHook>();

	private static float MIN_FRAME_DELAY = 0.075f;

	private static float GIF_LENGTH = 3.5f;

	private RenderTexture renderTex;

	private static int GIF_WIDTH = 560;

	private static int GIF_HEIGHT = 315;

	private static float MILLIS_PER_SEC = 1000f;

	public void Update()
	{
		if (!SRSingleton<GameContext>.Instance.OptionsDirector.bufferForGif)
		{
			foreach (Frame frame in frames)
			{
				Destroyer.Destroy(frame.tex, "GifRecorder.Update#1");
			}
			frames.Clear();
			if (hooks.Count <= 0)
			{
				return;
			}
			foreach (CameraHook hook in hooks)
			{
				Destroyer.Destroy(hook, "GifRecorder.Update#2");
			}
			hooks.Clear();
			return;
		}
		if (hooks.Count == 0)
		{
			frames.Clear();
			Camera[] allCameras = Camera.allCameras;
			for (int i = 0; i < allCameras.Length; i++)
			{
				CameraHook cameraHook = allCameras[i].gameObject.AddComponent<CameraHook>();
				cameraHook.recorder = this;
				hooks.Add(cameraHook);
			}
		}
		float time = Time.time;
		if (time >= nextFrameTime)
		{
			renderTex = new RenderTexture(GIF_WIDTH, GIF_HEIGHT, 24);
			float delay = ((frames.Count == 0) ? MIN_FRAME_DELAY : (time - lastFrameTime));
			frames.Enqueue(new Frame(renderTex, time, delay));
			if (frames.Peek().time < time - GIF_LENGTH)
			{
				Destroyer.Destroy(frames.Dequeue().tex, "GifRecorder.Update#3");
			}
			lastFrameTime = time;
			nextFrameTime = time + MIN_FRAME_DELAY;
		}
		else
		{
			renderTex = null;
		}
	}

	public void DoRenderImage(RenderTexture source)
	{
		if (renderTex != null)
		{
			Graphics.Blit(source, renderTex);
		}
	}

	public void MaybeSaveGif()
	{
		if (SRSingleton<GameContext>.Instance.OptionsDirector.bufferForGif)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(storeGifUI);
			StoreGifUI ui = gameObject.GetComponent<StoreGifUI>();
			ui.onConfirm = delegate
			{
				StartCoroutine(SaveGif(delegate
				{
					ui.Close();
				}));
			};
		}
		else
		{
			UnityEngine.Object.Instantiate(cannotStoreGifUI);
		}
	}

	private IEnumerator SaveGif(UnityAction onComplete)
	{
		Debug.Log("Starting saving Gif...");
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SlimeRancher-" + $"{DateTime.Now:yyyy-MM-dd-hh-mm-ss-ff}" + ".gif");
		using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate))
		{
			AnimatedGifEncoder gifEncoder = new AnimatedGifEncoder();
			try
			{
				gifEncoder.SetRepeat(0);
				gifEncoder.Start(fileStream);
				int count = 0;
				foreach (Frame frame in frames)
				{
					yield return new WaitForEndOfFrame();
					Texture2D texture2D = new Texture2D(GIF_WIDTH, GIF_HEIGHT, TextureFormat.RGB24, mipChain: false);
					try
					{
						RenderTexture.active = frame.tex;
						texture2D.ReadPixels(new Rect(0f, 0f, GIF_WIDTH, GIF_HEIGHT), 0, 0);
					}
					finally
					{
						RenderTexture.active = null;
					}
					gifEncoder.AddFrame(texture2D.GetPixels32(), GIF_WIDTH, GIF_HEIGHT);
					gifEncoder.SetDelay(Mathf.RoundToInt(frame.delay * MILLIS_PER_SEC));
					int num = count + 1;
					count = num;
					if (num > 1000)
					{
						Debug.Log("Too many frames in gif, ejecting...");
						break;
					}
				}
			}
			finally
			{
				gifEncoder.Finish();
			}
		}
		Debug.Log("...finished Gif");
		onComplete?.Invoke();
	}
}
