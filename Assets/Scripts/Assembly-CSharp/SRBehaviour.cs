using Assets.Script.Util.Extensions;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;
using UnityEngine.UI;

public class SRBehaviour : MonoBehaviour
{
	public enum NavigationDirection
	{
		UP = 0,
		DOWN = 1,
		RIGHT = 2,
		LEFT = 3,
		RIGHT_LEFT = 4,
		DOWN_UP = 5
	}

	private DestroyRequestHandler destroyRequestHandler;

	public I GetInterfaceComponent<I>() where I : class
	{
		return GetComponent(typeof(I)) as I;
	}

	public static GameObject InstantiateDynamic(GameObject original)
	{
		return Object.Instantiate(original);
	}

	public static GameObject InstantiateDynamic(GameObject original, Vector3 position, Quaternion rotation, bool asActor = false)
	{
		return Object.Instantiate(original, position, rotation);
	}

	public static GameObject InstantiateActor(GameObject original, RegionRegistry.RegionSetId regionSetId, bool nonActorOk = false)
	{
		return InstantiateActor(original, regionSetId, Vector3.zero, Quaternion.identity, nonActorOk);
	}

	public static GameObject InstantiateActor(GameObject original, RegionRegistry.RegionSetId regionSetId, Vector3 position, Quaternion rotation, bool nonActorOk = false)
	{
		return SRSingleton<SceneContext>.Instance.GameModel.InstantiateActor(original, regionSetId, position, rotation, nonActorOk);
	}

	public static GameObject InstantiatePooledDynamic(GameObject original, Vector3 position, Quaternion rotation)
	{
		return SRSingleton<SceneContext>.Instance.fxPool.Spawn(original, null, position, rotation);
	}

	public static GameObject SpawnAndPlayFX(GameObject prefab)
	{
		return SpawnAndPlayFX(prefab, Vector3.zero, Quaternion.identity);
	}

	public static GameObject SpawnAndPlayFX(GameObject prefab, GameObject parentObject)
	{
		return SpawnAndPlayFX(prefab, parentObject, Vector3.zero, Quaternion.identity);
	}

	public static GameObject SpawnAndPlayFX(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		return SpawnAndPlayFX(prefab, null, position, rotation);
	}

	public static GameObject SpawnAndPlayFX(GameObject prefab, GameObject parentObject, Vector3 position, Quaternion rotation)
	{
		GameObject gameObject = null;
		if (SRSingleton<SceneContext>.Instance != null)
		{
			gameObject = SRSingleton<SceneContext>.Instance.fxPool.Spawn(prefab, (parentObject != null) ? parentObject.transform : null, position, rotation);
			PlayFX(gameObject);
		}
		return gameObject;
	}

	public static void RecycleAndStopFX(GameObject obj)
	{
		StopFX(obj);
		SRSingleton<SceneContext>.Instance.fxPool.Recycle(obj);
	}

	public static void PlayFX(GameObject fxObject)
	{
		if (!(fxObject != null))
		{
			return;
		}
		ParticleSystem particleSystem = fxObject.GetComponent<ParticleSystem>();
		if (particleSystem == null)
		{
			particleSystem = fxObject.GetComponentInChildren<ParticleSystem>();
		}
		if (!(particleSystem != null))
		{
			return;
		}
		particleSystem.Play();
		if (!(particleSystem.gameObject != null))
		{
			return;
		}
		SECTR_PointSource[] components = particleSystem.gameObject.GetComponents<SECTR_PointSource>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i] != null)
			{
				components[i].Play();
			}
		}
	}

	public static void StopFX(GameObject fxObject)
	{
		if (!(fxObject != null))
		{
			return;
		}
		ParticleSystem particleSystem = fxObject.GetComponent<ParticleSystem>();
		if (particleSystem == null)
		{
			particleSystem = fxObject.GetComponentInChildren<ParticleSystem>();
		}
		if (!(particleSystem != null))
		{
			return;
		}
		particleSystem.Stop();
		if (!(particleSystem.gameObject != null))
		{
			return;
		}
		SECTR_PointSource[] components = particleSystem.gameObject.GetComponents<SECTR_PointSource>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i] != null)
			{
				components[i].Stop(stopImmediately: true);
			}
		}
	}

	private DestroyRequestHandler GetDestroyRequestHandler()
	{
		if (destroyRequestHandler == null)
		{
			destroyRequestHandler = GetComponent<DestroyRequestHandler>();
		}
		return destroyRequestHandler;
	}

	public void RequestDestroy(string source)
	{
		DestroyRequestHandler destroyRequestHandler = GetDestroyRequestHandler();
		if (destroyRequestHandler != null)
		{
			destroyRequestHandler.OnDestroyRequest(base.gameObject);
		}
		else
		{
			Destroyer.Destroy(base.gameObject, source);
		}
	}

	public static void RequestDestroy(GameObject obj, string source)
	{
		obj.GetComponent<SRBehaviour>().RequestDestroy(source);
	}

	public static void LinkNavigation(Selectable source, Selectable target, NavigationDirection direction)
	{
		switch (direction)
		{
		case NavigationDirection.UP:
			if (source != null)
			{
				Navigation navigation3 = source.navigation;
				navigation3.mode = Navigation.Mode.Explicit;
				navigation3.selectOnUp = ((target != null && target.interactable) ? target : null);
				source.navigation = navigation3;
			}
			break;
		case NavigationDirection.DOWN:
			if (source != null)
			{
				Navigation navigation4 = source.navigation;
				navigation4.mode = Navigation.Mode.Explicit;
				navigation4.selectOnDown = ((target != null && target.interactable) ? target : null);
				source.navigation = navigation4;
			}
			break;
		case NavigationDirection.LEFT:
			if (source != null)
			{
				Navigation navigation2 = source.navigation;
				navigation2.mode = Navigation.Mode.Explicit;
				navigation2.selectOnLeft = ((target != null && target.interactable) ? target : null);
				source.navigation = navigation2;
			}
			break;
		case NavigationDirection.RIGHT:
			if (source != null)
			{
				Navigation navigation = source.navigation;
				navigation.mode = Navigation.Mode.Explicit;
				navigation.selectOnRight = ((target != null && target.interactable) ? target : null);
				source.navigation = navigation;
			}
			break;
		case NavigationDirection.DOWN_UP:
			LinkNavigation(source, target, NavigationDirection.DOWN);
			LinkNavigation(target, source, NavigationDirection.UP);
			break;
		case NavigationDirection.RIGHT_LEFT:
			LinkNavigation(source, target, NavigationDirection.RIGHT);
			LinkNavigation(target, source, NavigationDirection.LEFT);
			break;
		}
	}

	public T GetComponentInParent<T>(bool includeInactive) where T : Component
	{
		return ComponentExtensions.GetComponentInParent<T>(this, includeInactive);
	}

	public T GetRequiredComponent<T>() where T : Component
	{
		return ComponentExtensions.GetRequiredComponent<T>(this);
	}

	public T GetRequiredComponentInParent<T>(bool includeInactive = false) where T : Component
	{
		return ComponentExtensions.GetRequiredComponentInParent<T>(this, includeInactive);
	}

	public T GetRequiredComponentInChildren<T>(bool includeInactive = false) where T : Component
	{
		return ComponentExtensions.GetRequiredComponentInChildren<T>(this, includeInactive);
	}
}
