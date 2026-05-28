using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioListener))]
[ExecuteInEditMode]
[AddComponentMenu("SECTR/Audio/SECTR Audio System")]
public class SECTR_AudioSystem : MonoBehaviour
{
	private class Instance : SECTR_IAudioInstance
	{
		[Flags]
		private enum Flags
		{
			Loops = 1,
			FadingIn = 2,
			FadingOut = 4,
			Muted = 8,
			Local = 0x10,
			ThreeD = 0x20,
			Paused = 0x40,
			HDR = 0x80,
			Occludable = 0x100,
			Occluded = 0x200,
			ForcedInfinite = 0x400,
			Delayed = 0x800
		}

		private int? clipIndex;

		private bool hasPriority;

		private int pauseCount;

		private int generation;

		private AudioSource source;

		private AudioLowPassFilter lowpass;

		private SECTR_AudioCue audioCue;

		private Transform parent;

		private Vector3 localPosition = Vector3.zero;

		private Flags flags;

		private float nextTestTime;

		private float fadeStarTime;

		private float basePitch = 1f;

		private float baseVolumeLoudness = 1f;

		private float userVolume = 1f;

		private float userPitch = 1f;

		private float occlusionAlpha = 1f;

		private AnimationCurve hdrCurve;

		public int Generation => generation;

		public bool Active
		{
			get
			{
				if ((Loops || Delayed || ((bool)source && (source.isPlaying || Paused || AudioListener.pause))) && !FadingOut)
				{
					if (!(source == null))
					{
						return source.enabled;
					}
					return true;
				}
				return false;
			}
		}

		public Vector3 Position
		{
			get
			{
				Vector3 vector = localPosition;
				if ((bool)parent)
				{
					if (ThreeD && Local)
					{
						vector += parent.transform.position;
					}
					else
					{
						vector = parent.localToWorldMatrix.MultiplyPoint3x4(vector);
					}
				}
				return vector;
			}
			set
			{
				if ((bool)parent)
				{
					if (ThreeD && Local)
					{
						localPosition = value - parent.transform.position;
					}
					else
					{
						localPosition = parent.worldToLocalMatrix.MultiplyPoint3x4(value);
					}
				}
				else
				{
					localPosition = value;
				}
				if ((bool)source)
				{
					source.transform.position = value;
				}
			}
		}

		public Vector3 LocalPosition
		{
			get
			{
				return localPosition;
			}
			set
			{
				localPosition = value;
				if ((bool)source)
				{
					source.transform.position = Position;
				}
			}
		}

		public float Volume
		{
			get
			{
				return userVolume;
			}
			set
			{
				if (userVolume != value)
				{
					userVolume = Mathf.Clamp01(value);
					Update(0f, volumeOnly: true);
				}
			}
		}

		public float Pitch
		{
			get
			{
				return userPitch;
			}
			set
			{
				if (userPitch != value)
				{
					userPitch = Mathf.Clamp(value, 0f, 2f);
					Update(0f, volumeOnly: true);
				}
			}
		}

		public bool Mute
		{
			get
			{
				return Mute;
			}
			set
			{
				if (Muted != value)
				{
					_SetFlag(Flags.Muted, value);
					if ((bool)source)
					{
						source.mute = value;
					}
				}
			}
		}

		public float TimeSeconds
		{
			get
			{
				if (!(source != null))
				{
					return 0f;
				}
				return source.time;
			}
			set
			{
				if ((bool)source)
				{
					source.time = value;
				}
			}
		}

		public int TimeSamples
		{
			get
			{
				if (!(source != null))
				{
					return 0;
				}
				return source.timeSamples;
			}
			set
			{
				if ((bool)source)
				{
					source.timeSamples = value;
				}
			}
		}

		public bool HasPriority => hasPriority;

		public bool Loops => (flags & Flags.Loops) != 0;

		public bool Local => (flags & Flags.Local) != 0;

		public bool ThreeD => (flags & Flags.ThreeD) != 0;

		public bool FadingIn => (flags & Flags.FadingIn) != 0;

		public bool FadingOut => (flags & Flags.FadingOut) != 0;

		public bool Muted => (flags & Flags.Muted) != 0;

		public bool Paused => (flags & Flags.Paused) != 0;

		public bool HDR => (flags & Flags.HDR) != 0;

		public bool Occludable => (flags & Flags.Occludable) != 0;

		public bool Occluded => (flags & Flags.Occluded) != 0;

		public bool ForcedInfinite => (flags & Flags.ForcedInfinite) != 0;

		public bool Delayed => (flags & Flags.Delayed) != 0;

		public SECTR_AudioBus Bus
		{
			get
			{
				if (!(audioCue != null))
				{
					return null;
				}
				return audioCue.Bus;
			}
		}

		public SECTR_AudioCue Cue => audioCue;

		public void ForceInfinite()
		{
			_SetFlag(Flags.ForcedInfinite, on: true);
			_SetFlag(Flags.Local, on: true);
			_SetFlag(Flags.ThreeD, on: true);
			occlusionAlpha = 1f;
			if ((bool)source)
			{
				source.rolloffMode = AudioRolloffMode.Linear;
				source.maxDistance = 1000000f;
				source.minDistance = source.maxDistance - 0.001f;
				source.dopplerLevel = 0f;
			}
			Update(0f, volumeOnly: true);
		}

		public void ForceOcclusion(bool occluded)
		{
			if ((bool)audioCue && audioCue.SourceCue.Spatialization == SECTR_AudioCue.Spatializations.Occludable3D)
			{
				_SetFlag(Flags.Occluded, occluded);
			}
		}

		public void Init(SECTR_AudioCue audioCue, Transform parent, Vector3 localPosition, bool loops, int? clipIndex, bool hasPriority)
		{
			if (this.audioCue == null)
			{
				generation++;
				this.audioCue = audioCue;
				this.clipIndex = clipIndex;
				this.hasPriority = hasPriority;
				SECTR_AudioCue sourceCue = audioCue.SourceCue;
				flags = (Flags)0;
				_SetFlag(Flags.Loops, loops);
				_SetFlag(Flags.Local, sourceCue.IsLocal);
				_SetFlag(Flags.ThreeD, sourceCue.Is3D);
				_SetFlag(Flags.HDR, sourceCue.HDR);
				_SetFlag(Flags.Occludable, system.OcclusionFlags != 0 && sourceCue.Spatialization == SECTR_AudioCue.Spatializations.Occludable3D);
				userVolume = 1f;
				userPitch = 1f;
				if (Local)
				{
					this.parent = Listener;
				}
				else
				{
					this.parent = parent;
				}
				this.localPosition = localPosition;
				_AddProximityInstance(sourceCue);
				_ScheduleNextTest();
			}
		}

		public void Clone(Instance instance, Vector3 newPosition)
		{
			if (!instance.Active)
			{
				return;
			}
			generation++;
			audioCue = instance.audioCue;
			flags = instance.flags;
			fadeStarTime = instance.fadeStarTime;
			basePitch = instance.basePitch;
			baseVolumeLoudness = instance.baseVolumeLoudness;
			userVolume = instance.userVolume;
			userPitch = instance.userPitch;
			occlusionAlpha = instance.occlusionAlpha;
			hdrCurve = instance.hdrCurve;
			parent = instance.parent;
			Position = newPosition;
			_AddProximityInstance(audioCue.SourceCue);
			_ScheduleNextTest();
			if (_AcquireSource())
			{
				Update(0f, volumeOnly: true);
				if ((bool)source)
				{
					_SetFlag(Flags.Paused, on: false);
					source.clip = instance.source.clip;
					source.timeSamples = instance.source.timeSamples;
					PlaySource();
				}
			}
		}

		public void Uninit()
		{
			if (audioCue != null)
			{
				if (audioCue.SourceCue.ProximityLimit > 0 && proximityTable.TryGetValue(audioCue, out var value))
				{
					value.Remove(this);
				}
				_ReleaseSource();
				audioCue = null;
				parent = null;
				flags = (Flags)0;
			}
		}

		public void Play()
		{
			SECTR_AudioCue.ClipData clipData = (clipIndex.HasValue ? audioCue.AudioClips[clipIndex.Value] : audioCue.GetNextClip());
			if (clipData == null || !(clipData.Clip != null) || !_AcquireSource())
			{
				return;
			}
			if (clipData.Clip.loadState == AudioDataLoadState.Unloaded)
			{
				clipData.Clip.LoadAudioData();
			}
			SECTR_AudioCue sourceCue = audioCue.SourceCue;
			if (sourceCue.FadeInTime > 0f)
			{
				fadeStarTime = currentTime;
				_SetFlag(Flags.FadingIn, on: true);
				_SetFlag(Flags.FadingOut, on: false);
			}
			if (Occludable && !ForcedInfinite)
			{
				_SetFlag(Flags.Occluded, IsOccluded(Position, system.OcclusionFlags));
				occlusionAlpha = (Occluded ? 1f : 0f);
			}
			if (HDR)
			{
				baseVolumeLoudness = UnityEngine.Random.Range(sourceCue.Loudness.x, sourceCue.Loudness.y);
			}
			else
			{
				baseVolumeLoudness = UnityEngine.Random.Range(sourceCue.Volume.x, sourceCue.Volume.y);
			}
			baseVolumeLoudness *= clipData.Volume;
			if (HDR)
			{
				if (clipData.HDRCurve != null && clipData.HDRCurve.length > 0)
				{
					hdrCurve = clipData.HDRCurve;
				}
				else
				{
					Debug.LogWarning("Playing " + audioCue.name + " without HDR keys. Bake HDR keys for higher quality audio.");
				}
			}
			Update(0f, volumeOnly: true);
			if ((bool)source)
			{
				_SetFlag(Flags.Paused, on: false);
				source.clip = clipData.Clip;
				if (sourceCue.Delay.y > 0f)
				{
					_SetFlag(Flags.Delayed, on: true);
					nextTestTime = currentTime + UnityEngine.Random.Range(sourceCue.Delay.x, sourceCue.Delay.y);
				}
				else
				{
					PlaySource();
				}
			}
		}

		public void Pause(bool paused)
		{
			if (paused)
			{
				pauseCount++;
			}
			else
			{
				pauseCount = Math.Max(0, pauseCount - 1);
			}
			paused = pauseCount > 0;
			_SetFlag(Flags.Paused, paused);
			if ((bool)source)
			{
				if (paused)
				{
					source.Pause();
				}
				else if (!source.isPlaying)
				{
					PlaySource();
				}
			}
		}

		public void PlaySource()
		{
			if (source != null && Bus != null)
			{
				if (!Bus.Paused)
				{
					source.Play();
				}
				else if (Loops || source.loop)
				{
					source.Play();
					source.Pause();
				}
			}
			else if (source != null)
			{
				source.Play();
			}
		}

		public void Stop(bool stopImmediately)
		{
			_SetFlag(Flags.Loops, on: false);
			_Stop(stopImmediately);
		}

		public void SkipFadeIn()
		{
			_SetFlag(Flags.FadingIn, on: false);
		}

		public void Update(float deltaTime, bool volumeOnly)
		{
			if (Delayed)
			{
				if (!(currentTime >= nextTestTime))
				{
					return;
				}
				if (source != null)
				{
					PlaySource();
				}
				_SetFlag(Flags.Delayed, on: false);
				_ScheduleNextTest();
			}
			Vector3 position;
			if (ThreeD)
			{
				position = Position;
				if ((bool)source)
				{
					source.transform.position = position;
				}
			}
			else
			{
				position = Listener.position;
			}
			float num = 1f;
			if (FadingIn)
			{
				num = Mathf.Clamp01((currentTime - fadeStarTime) / audioCue.SourceCue.FadeInTime);
				if (num >= 1f)
				{
					_SetFlag(Flags.FadingIn, on: false);
				}
			}
			else if (FadingOut)
			{
				float num2 = currentTime - fadeStarTime;
				num = Mathf.Clamp01(1f - num2 / audioCue.SourceCue.FadeOutTime);
				if (num <= 0f)
				{
					_SetFlag(Flags.FadingOut, on: false);
					_Stop(stopImmediately: true);
				}
			}
			if ((bool)source && (source.isPlaying || Paused || volumeOnly) && !Muted)
			{
				float num3 = (audioCue.Bus ? audioCue.Bus.EffectiveVolume : system.MasterBus.EffectiveVolume);
				float num4 = (audioCue.Bus ? audioCue.Bus.EffectivePitch : system.MasterBus.Pitch);
				float num5 = 1f;
				if (HDR)
				{
					SECTR_AudioCue sourceCue = audioCue.SourceCue;
					float num6 = 1f;
					if (!Local)
					{
						float maxDistance = sourceCue.MaxDistance;
						float minDistance = sourceCue.MinDistance;
						Vector3 position2 = Listener.transform.position;
						float num7 = Vector3.SqrMagnitude(position - position2);
						if (num7 > maxDistance * maxDistance)
						{
							num6 = 0f;
						}
						else if (num7 > minDistance * minDistance)
						{
							float num8 = Mathf.Sqrt(num7);
							switch (audioCue.SourceCue.Falloff)
							{
							case SECTR_AudioCue.FalloffTypes.Linear:
								num6 = 1f - Mathf.Clamp01((num8 - minDistance) / (maxDistance - minDistance));
								break;
							case SECTR_AudioCue.FalloffTypes.Logrithmic:
								num6 = Mathf.Clamp01(1f / Mathf.Max(num8 - minDistance - 1f, 0.001f));
								break;
							}
						}
					}
					float num9 = baseVolumeLoudness;
					if (hdrCurve != null)
					{
						float num10 = hdrCurve.Evaluate(source.time);
						num9 += num10;
					}
					num9 += 20f * Mathf.Log10(Mathf.Max(userVolume * num * num6, 0.001f));
					if (num9 < windowHDRMin && (volumeOnly || (baseVolumeLoudness - windowHDRMin) / system.HDRDecay > source.time - source.clip.length))
					{
						_Stop(stopImmediately: false);
						return;
					}
					currentLoudness += Mathf.Pow(10f, num9 * 0.1f);
					num5 = Mathf.Clamp01(Mathf.Pow(10f, (num9 - windowHDRMax) * 0.05f));
				}
				else
				{
					num5 = baseVolumeLoudness * num * userVolume;
				}
				if (Occludable)
				{
					float num11 = 1f;
					occlusionAlpha += deltaTime * (Occluded ? num11 : (0f - num11));
					occlusionAlpha = Mathf.Clamp01(occlusionAlpha);
					float t = occlusionAlpha * audioCue.SourceCue.OcclusionScale;
					num5 *= Mathf.Lerp(1f, system.OcclusionVolume, t);
					if ((bool)lowpass)
					{
						lowpass.enabled = occlusionAlpha > 0f;
						if (lowpass.enabled)
						{
							lowpass.cutoffFrequency = Mathf.Lerp(22000f, system.OcclusionCutoff, t);
							lowpass.lowpassResonanceQ = Mathf.Lerp(1f, system.OcclusionResonanceQ, t);
						}
					}
				}
				source.volume = Mathf.Clamp01(num5 * num3);
				source.pitch = Mathf.Clamp(userPitch * basePitch * num4, 0f, 2f);
			}
			if (volumeOnly)
			{
				return;
			}
			if ((bool)source && (source.isPlaying || Paused) && !Local && system.BlendNearbySounds)
			{
				float num12 = Vector3.SqrMagnitude(Listener.position - position);
				float num13 = 0f;
				num13 = ((num12 <= system.NearBlendRange.x * system.NearBlendRange.x) ? 0f : ((!(num12 <= system.NearBlendRange.y * system.NearBlendRange.y)) ? 1f : Mathf.Clamp01((Mathf.Sqrt(num12) - system.NearBlendRange.x) / (system.NearBlendRange.y - system.NearBlendRange.x))));
				source.spatialBlend = num13;
			}
			if (!Loops || Paused)
			{
				return;
			}
			bool flag = source != null && source.isPlaying;
			bool flag2 = !flag && (!HDR || baseVolumeLoudness >= windowHDRMin);
			if (Local)
			{
				if (!flag && flag2 && _CheckInstances(audioCue, flag))
				{
					Play();
				}
			}
			else if (currentTime >= nextTestTime)
			{
				bool flag3 = _CheckProximity(audioCue, parent, localPosition, this);
				if (flag3 && !flag && flag2 && _CheckInstances(audioCue, flag))
				{
					Play();
				}
				else if (!flag3 && flag)
				{
					_Stop(stopImmediately: true);
				}
				else if (Occludable && !ForcedInfinite)
				{
					_SetFlag(Flags.Occluded, IsOccluded(position, system.OcclusionFlags));
				}
				_ScheduleNextTest();
			}
		}

		private void _SetFlag(Flags flag, bool on)
		{
			if (on)
			{
				flags |= flag;
			}
			else
			{
				flags &= ~flag;
			}
		}

		private bool _AcquireSource()
		{
			if (!source)
			{
				SECTR_AudioCue sourceCue = audioCue.SourceCue;
				bool flag = Occludable && !sourceCue.BypassEffects && SECTR_Modules.HasPro() && lowpassSourcePool.Count > 0;
				source = (flag ? lowpassSourcePool.Pop() : simpleSourcePool.Pop());
				if ((bool)source)
				{
					if (flag)
					{
						lowpass = source.GetComponent<AudioLowPassFilter>();
						lowpass.enabled = false;
					}
					source.time = 0f;
					source.timeSamples = 0;
					source.priority = sourceCue.Priority;
					source.bypassEffects = sourceCue.BypassEffects;
					source.loop = sourceCue.Loops;
					source.spread = sourceCue.Spread;
					source.mute = Muted;
					basePitch = UnityEngine.Random.Range(sourceCue.Pitch.x, sourceCue.Pitch.y);
					if (sourceCue.MaxInstances > 0)
					{
						if (!maxInstancesTable.ContainsKey(audioCue))
						{
							maxInstancesTable[audioCue] = new List<Instance>();
						}
						maxInstancesTable[audioCue].Add(this);
					}
					source.panStereo = 0f;
					source.spatialBlend = 1f;
					source.bypassReverbZones = Local;
					if (Local)
					{
						if (ThreeD)
						{
							source.rolloffMode = AudioRolloffMode.Linear;
							source.maxDistance = 1000000f;
							source.minDistance = source.maxDistance - 0.001f;
						}
						else
						{
							source.panStereo = sourceCue.Pan2D;
							source.spatialBlend = 0f;
						}
						source.dopplerLevel = 0f;
						if ((currentAmbience != null && currentAmbience.BackgroundLoop == audioCue) || (currentMusic != null && currentMusic == audioCue))
						{
							source.priority = 0;
						}
					}
					else
					{
						if (HDR)
						{
							source.rolloffMode = AudioRolloffMode.Linear;
							source.minDistance = 1000000f;
							source.maxDistance = source.minDistance + 0.001f;
						}
						else
						{
							SECTR_AudioCue.FalloffTypes falloff = sourceCue.Falloff;
							if (falloff != 0 && falloff == SECTR_AudioCue.FalloffTypes.Logrithmic)
							{
								source.rolloffMode = AudioRolloffMode.Logarithmic;
							}
							else
							{
								source.rolloffMode = AudioRolloffMode.Linear;
							}
							source.minDistance = sourceCue.MinDistance;
							source.maxDistance = Mathf.Max(sourceCue.MaxDistance, sourceCue.MinDistance + 0.001f);
						}
						source.dopplerLevel = sourceCue.DopplerLevel;
						source.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
					}
					source.transform.position = Position;
					source.gameObject.SetActive(value: true);
				}
			}
			return source != null;
		}

		private void _ReleaseSource()
		{
			if (source != null)
			{
				if (audioCue.MaxInstances > 0 && maxInstancesTable.ContainsKey(audioCue) && maxInstancesTable[audioCue].Remove(this) && maxInstancesTable[audioCue].Count == 0)
				{
					maxInstancesTable.Remove(audioCue);
				}
				source.Stop();
				source.gameObject.SetActive(value: false);
				if ((bool)lowpass)
				{
					lowpass.enabled = false;
					lowpassSourcePool.Push(source);
				}
				else
				{
					simpleSourcePool.Push(source);
				}
				source = null;
				lowpass = null;
				hdrCurve = null;
			}
		}

		private void _AddProximityInstance(SECTR_AudioCue srcCue)
		{
			int proximityLimit = srcCue.ProximityLimit;
			if (proximityLimit > 0)
			{
				if (!proximityTable.TryGetValue(audioCue, out var value))
				{
					value = new List<Instance>(proximityLimit * 2);
					proximityTable[audioCue] = value;
				}
				value.Add(this);
			}
		}

		private void _ScheduleNextTest()
		{
			nextTestTime = currentTime + UnityEngine.Random.Range(system.RetestInterval.x, system.RetestInterval.y);
		}

		private void _Stop(bool stopImmediately)
		{
			if (!stopImmediately && (bool)source && source.isPlaying && (bool)audioCue && audioCue.SourceCue.FadeOutTime > 0f)
			{
				if (FadingIn)
				{
					float num = 1f - Mathf.Clamp01((currentTime - fadeStarTime) / audioCue.SourceCue.FadeInTime);
					fadeStarTime = currentTime - num * audioCue.SourceCue.FadeOutTime;
				}
				else
				{
					fadeStarTime = currentTime;
				}
				_SetFlag(Flags.FadingOut, on: true);
				_SetFlag(Flags.FadingIn, on: false);
			}
			else
			{
				_ReleaseSource();
			}
		}
	}

	[Flags]
	public enum OcclusionModes
	{
		Graph = 1,
		Raycast = 2,
		Distance = 4
	}

	private static SECTR_AudioSystem system = null;

	private static Stack<Instance> instancePool = null;

	private static Stack<AudioSource> simpleSourcePool = null;

	private static Stack<AudioSource> lowpassSourcePool = null;

	private static Transform sourcePoolParent = null;

	private static List<Instance> activeInstances = null;

	private static Dictionary<SECTR_AudioCue, List<Instance>> maxInstancesTable = null;

	private static Dictionary<SECTR_AudioCue, List<Instance>> proximityTable = null;

	private static float currentTime = 0f;

	private static List<SECTR_AudioAmbience> ambienceStack;

	private static SECTR_AudioAmbience currentAmbience = null;

	private static SECTR_AudioCueInstance ambienceLoop;

	private static SECTR_AudioCueInstance ambienceOneShot;

	private static float nextAmbienceOneShotTime = 0f;

	private static SECTR_AudioCue currentMusic = null;

	private static SECTR_AudioCueInstance musicLoop;

	private static float windowHDRMax = 0f;

	private static float windowHDRMin = 0f;

	private static float currentLoudness = 0f;

	private static List<SECTR_Graph.Node> occlusionPath;

	private const float EPSILON = 0.001f;

	[SECTR_ToolTip("The maximum number of instances that can be active at once. Inaudible sounds do not count against this limit.")]
	public int MaxInstances = 128;

	[SECTR_ToolTip("The number of instances to allocate with lowpass effects (for occlusion and the like).")]
	public int LowpassInstances = 32;

	[SECTR_ToolTip("The Bus at the top of the mixing heirarchy. Required to play sounds.", null, false)]
	public SECTR_AudioBus MasterBus;

	[SECTR_ToolTip("The baseline settings for any environmental audio. Will be audible when no other ambiences are active.")]
	public SECTR_AudioAmbience DefaultAmbience = new SECTR_AudioAmbience();

	[SECTR_ToolTip("Minimum Loudness for the HDR mixer. Current Loudness will never drop below this.", 0f, 200f)]
	public float HDRBaseLoudness = 50f;

	[SECTR_ToolTip("The maximum difference between the loudest sound and the softest sound before sounds are simply culled out.", 0f, 200f)]
	public float HDRWindowSize = 50f;

	[SECTR_ToolTip("Speed at which HDR window decays after a loud sound is played.", 0f, 100f)]
	public float HDRDecay = 1f;

	[SECTR_ToolTip("Should sounds close to the listener be blended into 2D (to avoid harsh stereo switching).")]
	public bool BlendNearbySounds = true;

	[SECTR_ToolTip("Objects close to the listener will be blended into 2D, as a kind of fake HRTF. This determines the start and end of that blend.", "BlendNearbySounds")]
	public Vector2 NearBlendRange = new Vector2(0.25f, 0.75f);

	[SECTR_ToolTip("Determines what kind of logic to use for computing sound occlusion.", null, typeof(OcclusionModes))]
	public OcclusionModes OcclusionFlags;

	[SECTR_ToolTip("The distance beyond which sounds will be considered occluded, if Distance occlusion is enabled.", "OcclusionFlags")]
	public float OcclusionDistance = 100f;

	[SECTR_ToolTip("The layers to test against when raycasting for occlusion.", "OcclusionFlags")]
	public LayerMask RaycastLayers = -5;

	[SECTR_ToolTip("The amount by which to decrease the volume of occluded sounds.", "OcclusionFlags", 0f, 1f)]
	public float OcclusionVolume = 0.5f;

	[SECTR_ToolTip("The frequency cutoff of the lowpass filter for occluded sounds.", "OcclusionFlags", 10f, 22000f)]
	public float OcclusionCutoff = 2200f;

	[SECTR_ToolTip("The resonance Q of the lowpass filter for occluded sounds.", "OcclusionFlags", 1f, 10f)]
	public float OcclusionResonanceQ = 1f;

	[SECTR_ToolTip("The amount of time between tests to see if looping sounds should start or stop running.")]
	public Vector2 RetestInterval = new Vector2(0.5f, 1f);

	[SECTR_ToolTip("The amount of buffer to give before culling distant sounds.")]
	public float CullingBuffer = 10f;

	[SECTR_ToolTip("Enable or disable of the in-game audio HUD.", true)]
	public bool ShowAudioHUD;

	[SECTR_ToolTip("In the editor only, puts the listener at the AudioSystem, not at the Scene Camera.", true)]
	public bool Debugging;

	private static bool firstMaxInstanceWarning = true;

	public static bool Initialized => system != null;

	public static SECTR_AudioSystem System => system;

	public static Transform Listener => system.transform;

	private static int GetInstancesCount(SECTR_AudioCue cue)
	{
		maxInstancesTable.TryGetValue(cue, out var value);
		return value?.Count ?? 0;
	}

	public static SECTR_AudioCueInstance Play(SECTR_AudioCue audioCue, Vector3 position, bool loop)
	{
		return Play(audioCue, null, position, loop);
	}

	public static SECTR_AudioCueInstance Play(SECTR_AudioCue audioCue, Transform parent, Vector3 localPosition, bool loop, int? clipIndex = null, bool hasPriority = false)
	{
		if (!Initialized)
		{
			return default(SECTR_AudioCueInstance);
		}
		if (system.MasterBus == null)
		{
			Debug.LogWarning("SECTR_AudioSystem needs a Master Bus before you can play sounds.");
			return default(SECTR_AudioCueInstance);
		}
		if (activeInstances.Count >= system.MaxInstances)
		{
			Debug.LogWarning("Global max audio instances exceeded.");
			if (firstMaxInstanceWarning)
			{
				firstMaxInstanceWarning = false;
				foreach (Instance activeInstance in activeInstances)
				{
					Debug.LogWarning("Instance: " + activeInstance.Cue.name);
				}
			}
			return default(SECTR_AudioCueInstance);
		}
		if (audioCue == null)
		{
			return default(SECTR_AudioCueInstance);
		}
		if (!_CheckInstances(audioCue, isPlaying: false))
		{
			if (!hasPriority || !maxInstancesTable.ContainsKey(audioCue))
			{
				return default(SECTR_AudioCueInstance);
			}
			Instance instance = maxInstancesTable[audioCue].FirstOrDefault((Instance i) => !i.HasPriority);
			if (instance == null)
			{
				return default(SECTR_AudioCueInstance);
			}
			instance.Stop(stopImmediately: true);
		}
		else if (audioCue.AudioClips.Count == 0)
		{
			Debug.LogWarning("Cannot play a clipless Audio Cues.");
			return default(SECTR_AudioCueInstance);
		}
		SECTR_AudioCue sourceCue = audioCue.SourceCue;
		if (UnityEngine.Random.value <= sourceCue.PlayProbability)
		{
			bool flag = sourceCue.IsLocal || _CheckProximity(audioCue, parent, localPosition, null);
			loop |= sourceCue.Loops;
			if (flag || loop)
			{
				Instance instance2 = instancePool.Pop();
				if (instance2 != null)
				{
					instance2.Init(audioCue, parent, localPosition, loop, clipIndex, hasPriority);
					if (flag)
					{
						instance2.Play();
					}
					activeInstances.Add(instance2);
					return new SECTR_AudioCueInstance(instance2, instance2.Generation, loop);
				}
			}
		}
		return default(SECTR_AudioCueInstance);
	}

	public static SECTR_AudioCueInstance Clone(SECTR_AudioCueInstance instance, Vector3 newPosition)
	{
		if ((bool)instance)
		{
			Instance instance2 = instancePool.Pop();
			instance2.Clone((Instance)instance.GetInternalInstance(), newPosition);
			return new SECTR_AudioCueInstance(instance2, instance2.Generation, instance2.Loops);
		}
		return default(SECTR_AudioCueInstance);
	}

	public static void PlayMusic(SECTR_AudioCue musicCue)
	{
		if (!Initialized)
		{
			Debug.LogWarning("Cannot play music before Audio System is initialized.");
		}
		else if (musicCue != null)
		{
			if (musicCue.Is3D)
			{
				Debug.LogWarning("Music Cue " + musicCue.name + "is 3Dm but music should be Simple 2D.");
			}
			musicLoop.Stop(stopImmediately: false);
			currentMusic = musicCue;
			musicLoop = Play(currentMusic, Listener, Vector3.zero, loop: true);
		}
	}

	public static void StopMusic(bool stopImmediate)
	{
		if (Initialized)
		{
			musicLoop.Stop(stopImmediate);
			currentMusic = null;
		}
	}

	public static void PushAmbience(SECTR_AudioAmbience ambience)
	{
		if (!Initialized)
		{
			Debug.LogWarning("Cannot activate an ambience before audio system is initialzied.");
		}
		else if (ambience != null)
		{
			ambienceStack.Add(ambience);
		}
	}

	public static void RemoveAmbience(SECTR_AudioAmbience ambience)
	{
		if (Initialized && ambience != null)
		{
			ambienceStack.Remove(ambience);
		}
	}

	public static float GetBusVolume(string busName)
	{
		if (!Initialized)
		{
			Debug.LogWarning("Cannot get bus volume before Audio System is initialzied.");
		}
		else if (!string.IsNullOrEmpty(busName))
		{
			return GetBusVolume(_FindBus(system.MasterBus, busName));
		}
		return 0f;
	}

	public static float GetBusVolume(SECTR_AudioBus bus)
	{
		if (!Initialized)
		{
			Debug.LogWarning("Cannot get bus volume before Audio System is initialzied.");
		}
		else if ((bool)bus)
		{
			return bus.UserVolume;
		}
		return 0f;
	}

	public static void SetBusVolume(string busName, float volume)
	{
		if (!Initialized)
		{
			Debug.LogWarning("Cannot activate an ambience before audio system is initialzied.");
		}
		else if (!string.IsNullOrEmpty(busName))
		{
			SetBusVolume(_FindBus(system.MasterBus, busName), volume);
		}
	}

	public static void SetBusVolume(SECTR_AudioBus bus, float volume)
	{
		if (!Initialized)
		{
			Debug.LogWarning("Cannot set bus volume before Audio System is initialzied.");
		}
		else if ((bool)bus)
		{
			bus.UserVolume = volume;
		}
	}

	public static void MuteBus(string busName, bool mute)
	{
		if (!Initialized)
		{
			Debug.LogWarning("Cannot mute bus before Audio System is initialzied.");
		}
		else if (!string.IsNullOrEmpty(busName))
		{
			MuteBus(_FindBus(system.MasterBus, busName), mute);
		}
	}

	public static void MuteBus(SECTR_AudioBus bus, bool mute)
	{
		if (!Initialized)
		{
			Debug.LogWarning("Cannot mute bus before Audio System is initialzied.");
		}
		else if ((bool)bus)
		{
			bus.Muted = mute;
		}
	}

	public static void PauseBus(string busName, bool paused)
	{
		if (!Initialized)
		{
			Debug.LogWarning("Cannot pause bus before Audio System is initialzied.");
		}
		else if (!string.IsNullOrEmpty(busName))
		{
			PauseBus(_FindBus(system.MasterBus, busName), paused);
		}
	}

	public static void PauseBus(SECTR_AudioBus bus, bool paused)
	{
		if (!Initialized)
		{
			Debug.LogWarning("Cannot pause bus before Audio System is initialzied.");
		}
		else
		{
			if (!bus)
			{
				return;
			}
			bool paused2 = bus.Paused;
			bus.Pause(paused);
			if (paused2 == bus.Paused)
			{
				return;
			}
			int count = activeInstances.Count;
			for (int i = 0; i < count; i++)
			{
				Instance instance = activeInstances[i];
				if (bus.IsAncestorOf(instance.Bus))
				{
					instance.Pause(paused);
				}
			}
		}
	}

	public static bool IsOccluded(Vector3 worldSpacePosition, OcclusionModes occlusionFlags)
	{
		bool flag = false;
		Vector3 position = Listener.position;
		Vector3 direction = position - worldSpacePosition;
		float sqrMagnitude = direction.sqrMagnitude;
		if (!flag && (occlusionFlags & OcclusionModes.Distance) != 0)
		{
			flag = sqrMagnitude >= system.OcclusionDistance * system.OcclusionDistance;
		}
		if (!flag && (occlusionFlags & OcclusionModes.Raycast) != 0)
		{
			float maxDistance = Mathf.Sqrt(sqrMagnitude);
			flag = Physics.Raycast(worldSpacePosition, direction, out var hitInfo, maxDistance, system.RaycastLayers) && hitInfo.transform != Listener;
		}
		if (!flag && (occlusionFlags & OcclusionModes.Graph) != 0)
		{
			SECTR_Graph.FindShortestPath(ref occlusionPath, worldSpacePosition, position, (SECTR_Portal.PortalFlags)0);
			int count = occlusionPath.Count;
			for (int i = 0; i < count; i++)
			{
				if (flag)
				{
					break;
				}
				SECTR_Graph.Node node = occlusionPath[i];
				if ((bool)node.Portal && (node.Portal.Flags & SECTR_Portal.PortalFlags.Closed) != 0)
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	private static IEnumerable<SECTR_AudioBus> FindNonUISFX()
	{
		if (!(System != null) || !(System.MasterBus != null))
		{
			yield break;
		}
		SECTR_AudioBus sECTR_AudioBus = _FindBus(System.MasterBus, "SFX");
		if (sECTR_AudioBus == null)
		{
			yield break;
		}
		foreach (SECTR_AudioBus child in sECTR_AudioBus.Children)
		{
			if (child.name != "UI" && child.name != "Pause Transition")
			{
				yield return child;
			}
		}
	}

	public static void PauseNonUISFX(bool pause)
	{
		foreach (SECTR_AudioBus item in FindNonUISFX())
		{
			PauseBus(item, pause);
		}
	}

	public static void MuteNonUISFX(bool mute)
	{
		foreach (SECTR_AudioBus item in FindNonUISFX())
		{
			MuteBus(item, mute);
		}
	}

	private void OnEnable()
	{
		if ((bool)system && system != this)
		{
			Log.Error("Found duplicate SECTR_AudioSystem singleton instance.");
			Destroyer.Destroy(this, "SECTR_AudioSystem.OnEnable");
		}
		else if (system == null)
		{
			system = this;
			instancePool = new Stack<Instance>(MaxInstances);
			for (int i = 0; i < MaxInstances; i++)
			{
				instancePool.Push(new Instance());
			}
			int num = (SECTR_Modules.HasPro() ? Mathf.Max(0, MaxInstances - LowpassInstances) : MaxInstances);
			int num2 = MaxInstances - num;
			simpleSourcePool = new Stack<AudioSource>(num);
			lowpassSourcePool = (SECTR_Modules.HasPro() ? new Stack<AudioSource>(num2) : null);
			HideFlags hideFlags = HideFlags.HideAndDontSave;
			sourcePoolParent = new GameObject("SourcePool")
			{
				hideFlags = hideFlags
			}.transform;
			sourcePoolParent.transform.parent = sourcePoolParent;
			for (int j = 0; j < num; j++)
			{
				GameObject obj = new GameObject("SimpleInstance" + j);
				obj.hideFlags = hideFlags;
				obj.transform.parent = sourcePoolParent.transform;
				AudioSource audioSource = obj.AddComponent<AudioSource>();
				audioSource.playOnAwake = false;
				obj.SetActive(value: false);
				simpleSourcePool.Push(audioSource);
			}
			for (int k = 0; k < num2; k++)
			{
				GameObject obj2 = new GameObject("LowpassInstance" + k);
				obj2.hideFlags = hideFlags;
				obj2.transform.parent = sourcePoolParent.transform;
				AudioSource audioSource2 = obj2.AddComponent<AudioSource>();
				audioSource2.playOnAwake = false;
				obj2.AddComponent<AudioLowPassFilter>().enabled = false;
				obj2.SetActive(value: false);
				lowpassSourcePool.Push(audioSource2);
			}
			ambienceStack = new List<SECTR_AudioAmbience>(32);
			activeInstances = new List<Instance>(MaxInstances);
			maxInstancesTable = new Dictionary<SECTR_AudioCue, List<Instance>>(MaxInstances / 8);
			proximityTable = new Dictionary<SECTR_AudioCue, List<Instance>>(MaxInstances / 8);
			_UpdateTime();
			windowHDRMax = HDRBaseLoudness;
			windowHDRMin = windowHDRMax - HDRWindowSize;
			occlusionPath = new List<SECTR_Graph.Node>(32);
			if (MasterBus != null)
			{
				MasterBus.ResetUserVolume();
				_UpdateBusPitchVolume(MasterBus, 1f, 1f);
			}
			else
			{
				Debug.LogWarning("SECTR AudioSystem has no MasterBus. Game sounds will not play.");
			}
			MasterBus.ResetPauseState();
		}
	}

	private void OnDisable()
	{
		if (system == this)
		{
			int count = activeInstances.Count;
			for (int i = 0; i < count; i++)
			{
				activeInstances[i].Stop(stopImmediately: true);
			}
			MasterBus.ResetPauseState();
			if ((bool)sourcePoolParent)
			{
				Destroyer.Destroy(sourcePoolParent.gameObject, "SECTR_AudioSystem.OnDisable#1");
				sourcePoolParent = null;
			}
			system = null;
			activeInstances = null;
			maxInstancesTable = null;
			proximityTable = null;
			instancePool = null;
			simpleSourcePool = null;
			lowpassSourcePool = null;
			currentTime = 0f;
			ambienceStack = null;
			currentAmbience = null;
			nextAmbienceOneShotTime = 0f;
			currentMusic = null;
			occlusionPath = null;
		}
	}

	private void LateUpdate()
	{
		if (!(system == this) || AudioListener.pause || !MasterBus)
		{
			return;
		}
		float num = _UpdateTime();
		_UpdateBusPitchVolume(MasterBus, 1f, 1f);
		_UpdateAmbience();
		windowHDRMax = Mathf.Max(HDRBaseLoudness, windowHDRMax - HDRDecay * num);
		windowHDRMin = windowHDRMax - HDRWindowSize;
		currentLoudness = 0f;
		int num2 = activeInstances.Count;
		int num3 = 0;
		while (num3 < num2)
		{
			Instance instance = activeInstances[num3];
			instance.Update(num, volumeOnly: false);
			if (!instance.Active && !instance.FadingOut)
			{
				instance.Uninit();
				activeInstances.RemoveAt(num3);
				instancePool.Push(instance);
				num2--;
			}
			else
			{
				num3++;
			}
		}
		currentLoudness = 10f * Mathf.Log10(currentLoudness);
		windowHDRMax = Mathf.Max(currentLoudness, windowHDRMax);
	}

	private static bool _CheckInstances(SECTR_AudioCue audioCue, bool isPlaying)
	{
		int num = audioCue.SourceCue.MaxInstances;
		if (isPlaying)
		{
			num++;
		}
		if (num > 0 && GetInstancesCount(audioCue) >= num)
		{
			return false;
		}
		return true;
	}

	private static bool _CheckProximity(SECTR_AudioCue audioCue, Transform parent, Vector3 position, Instance testInstance)
	{
		if ((bool)parent)
		{
			position = parent.localToWorldMatrix.MultiplyPoint3x4(position);
		}
		SECTR_AudioCue sourceCue = audioCue.SourceCue;
		float num = sourceCue.MaxDistance + system.CullingBuffer;
		if (Vector3.SqrMagnitude(position - Listener.position) <= num * num)
		{
			int proximityLimit = sourceCue.ProximityLimit;
			if (proximityLimit > 0 && proximityTable.TryGetValue(audioCue, out var value))
			{
				int count = value.Count;
				if (count > proximityLimit)
				{
					float num2 = sourceCue.MaxDistance + sourceCue.MaxDistance;
					int num3 = 0;
					for (int i = 0; i < count; i++)
					{
						Instance instance = value[i];
						if (instance != testInstance && Vector3.SqrMagnitude(position - instance.Position) < num2 && ++num3 >= proximityLimit)
						{
							return false;
						}
					}
				}
			}
			return true;
		}
		return false;
	}

	private static float _UpdateTime()
	{
		float num = (float)AudioSettings.dspTime;
		float result = num - currentTime;
		currentTime = num;
		return result;
	}

	private static void _UpdateBusPitchVolume(SECTR_AudioBus bus, float effectiveVolume, float effectivePitch)
	{
		if ((bool)bus)
		{
			bus.EffectiveVolume = effectiveVolume;
			bus.EffectivePitch = effectivePitch;
			int count = bus.Children.Count;
			for (int i = 0; i < count; i++)
			{
				_UpdateBusPitchVolume(bus.Children[i], bus.EffectiveVolume, bus.EffectivePitch);
			}
		}
	}

	private static void _UpdateAmbience()
	{
		SECTR_AudioAmbience sECTR_AudioAmbience = ((ambienceStack.Count > 0) ? ambienceStack[ambienceStack.Count - 1] : system.DefaultAmbience);
		if (sECTR_AudioAmbience != currentAmbience)
		{
			ambienceLoop.Stop(stopImmediately: false);
			ambienceOneShot.Stop(stopImmediately: false);
			currentAmbience = sECTR_AudioAmbience;
			if (currentAmbience != null)
			{
				if (currentAmbience.OneShots.Count > 0)
				{
					nextAmbienceOneShotTime = currentTime + UnityEngine.Random.Range(currentAmbience.OneShotInterval.x, currentAmbience.OneShotInterval.y);
				}
				if ((bool)currentAmbience.BackgroundLoop)
				{
					if (currentAmbience.BackgroundLoop.Spatialization == SECTR_AudioCue.Spatializations.Infinite3D)
					{
						ambienceLoop = Play(currentAmbience.BackgroundLoop, Listener, UnityEngine.Random.onUnitSphere, loop: true);
					}
					else
					{
						ambienceLoop = Play(currentAmbience.BackgroundLoop, Listener, Vector3.zero, loop: true);
					}
				}
			}
		}
		if (currentAmbience == null)
		{
			return;
		}
		if (currentAmbience.OneShots.Count > 0 && currentTime >= nextAmbienceOneShotTime)
		{
			SECTR_AudioCue sECTR_AudioCue = currentAmbience.OneShots[UnityEngine.Random.Range(0, currentAmbience.OneShots.Count)];
			if (sECTR_AudioCue != null)
			{
				if (sECTR_AudioCue.SourceCue.Loops)
				{
					Debug.LogWarning("Cannot play ambient one shot " + sECTR_AudioCue.name + ". It is set to loop.");
				}
				else
				{
					if (!sECTR_AudioCue.IsLocal)
					{
						Debug.LogWarning("Ambient one shot " + sECTR_AudioCue.name + "should be 2D or Infinite 3D.");
					}
					ambienceOneShot = Play(sECTR_AudioCue, Listener, UnityEngine.Random.onUnitSphere, loop: false);
				}
			}
			nextAmbienceOneShotTime = currentTime + UnityEngine.Random.Range(currentAmbience.OneShotInterval.x, currentAmbience.OneShotInterval.y);
		}
		if ((bool)ambienceLoop)
		{
			ambienceLoop.Volume = currentAmbience.Volume;
		}
		if ((bool)ambienceOneShot)
		{
			ambienceOneShot.Volume = currentAmbience.Volume;
		}
	}

	private static SECTR_AudioBus _FindBus(SECTR_AudioBus bus, string busName)
	{
		if ((bool)bus)
		{
			if (bus.name == busName)
			{
				return bus;
			}
			int count = bus.Children.Count;
			for (int i = 0; i < count; i++)
			{
				SECTR_AudioBus sECTR_AudioBus = _FindBus(bus.Children[i], busName);
				if ((bool)sECTR_AudioBus)
				{
					return sECTR_AudioBus;
				}
			}
		}
		return null;
	}
}
