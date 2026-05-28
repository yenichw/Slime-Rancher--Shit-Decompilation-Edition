using System.Collections.Generic;
using UnityEngine;

public class PointOctreeNode<T> where T : class
{
	private class OctreeObject
	{
		public T Obj;

		public Vector3 Pos;
	}

	private float minSize;

	private Bounds bounds;

	private readonly List<OctreeObject> objects = new List<OctreeObject>();

	private PointOctreeNode<T>[] children;

	private Bounds[] childBounds;

	private const int NUM_OBJECTS_ALLOWED = 8;

	private Vector3 actualBoundsSize;

	public Vector3 Center { get; private set; }

	public float SideLength { get; private set; }

	public PointOctreeNode(float baseLengthVal, float minSizeVal, Vector3 centerVal)
	{
		SetValues(baseLengthVal, minSizeVal, centerVal);
	}

	public bool Add(T obj, Vector3 objPos)
	{
		if (!Encapsulates(bounds, objPos))
		{
			return false;
		}
		SubAdd(obj, objPos);
		return true;
	}

	public bool Remove(T obj)
	{
		bool flag = false;
		for (int i = 0; i < objects.Count; i++)
		{
			if (objects[i].Obj.Equals(obj))
			{
				flag = objects.Remove(objects[i]);
				break;
			}
		}
		if (!flag && children != null)
		{
			for (int j = 0; j < 8; j++)
			{
				flag = children[j].Remove(obj);
				if (flag)
				{
					break;
				}
			}
		}
		if (flag && children != null && ShouldMerge())
		{
			Merge();
		}
		return flag;
	}

	public T[] GetNearby(Ray ray, float maxDistance)
	{
		bounds.Expand(new Vector3(maxDistance, maxDistance, maxDistance));
		bool num = bounds.IntersectRay(ray);
		bounds.size = actualBoundsSize;
		if (!num)
		{
			return new T[0];
		}
		List<T> list = new List<T>();
		for (int i = 0; i < objects.Count; i++)
		{
			if (DistanceToRay(ray, objects[i].Pos) <= maxDistance)
			{
				list.Add(objects[i].Obj);
			}
		}
		if (children != null)
		{
			for (int j = 0; j < 8; j++)
			{
				T[] nearby = children[j].GetNearby(ray, maxDistance);
				if (nearby != null)
				{
					list.AddRange(nearby);
				}
			}
		}
		return list.ToArray();
	}

	public void SetChildren(PointOctreeNode<T>[] childOctrees)
	{
		if (childOctrees.Length != 8)
		{
			Log.Error("Child octree array must be length 8. Was length: " + childOctrees.Length);
		}
		else
		{
			children = childOctrees;
		}
	}

	public void DrawAllBounds(float depth = 0f)
	{
		float num = depth / 7f;
		Gizmos.color = new Color(num, 0f, 1f - num);
		Bounds bounds = new Bounds(Center, new Vector3(SideLength, SideLength, SideLength));
		Gizmos.DrawWireCube(bounds.center, bounds.size);
		if (children != null)
		{
			depth += 1f;
			for (int i = 0; i < 8; i++)
			{
				children[i].DrawAllBounds(depth);
			}
		}
		Gizmos.color = Color.white;
	}

	public void DrawAllObjects()
	{
		float num = SideLength / 20f;
		Gizmos.color = new Color(0f, 1f - num, num, 0.25f);
		foreach (OctreeObject @object in objects)
		{
			Gizmos.DrawIcon(@object.Pos, "marker.tif", allowScaling: true);
		}
		if (children != null)
		{
			for (int i = 0; i < 8; i++)
			{
				children[i].DrawAllObjects();
			}
		}
		Gizmos.color = Color.white;
	}

	public PointOctreeNode<T> ShrinkIfPossible(float minLength)
	{
		if (SideLength < 2f * minLength)
		{
			return this;
		}
		if (objects.Count == 0 && children.Length == 0)
		{
			return this;
		}
		int num = -1;
		for (int i = 0; i < objects.Count; i++)
		{
			OctreeObject octreeObject = objects[i];
			int num2 = BestFitChild(octreeObject.Pos);
			if (i == 0 || num2 == num)
			{
				if (num < 0)
				{
					num = num2;
				}
				continue;
			}
			return this;
		}
		if (children != null)
		{
			bool flag = false;
			for (int j = 0; j < children.Length; j++)
			{
				if (children[j].HasAnyObjects())
				{
					if (flag)
					{
						return this;
					}
					if (num >= 0 && num != j)
					{
						return this;
					}
					flag = true;
					num = j;
				}
			}
		}
		if (children == null)
		{
			SetValues(SideLength / 2f, minSize, childBounds[num].center);
			return this;
		}
		return children[num];
	}

	private void SetValues(float baseLengthVal, float minSizeVal, Vector3 centerVal)
	{
		SideLength = baseLengthVal;
		minSize = minSizeVal;
		Center = centerVal;
		actualBoundsSize = new Vector3(SideLength, SideLength, SideLength);
		bounds = new Bounds(Center, actualBoundsSize);
		float num = SideLength / 4f;
		float num2 = SideLength / 2f;
		Vector3 size = new Vector3(num2, num2, num2);
		childBounds = new Bounds[8];
		childBounds[0] = new Bounds(Center + new Vector3(0f - num, num, 0f - num), size);
		childBounds[1] = new Bounds(Center + new Vector3(num, num, 0f - num), size);
		childBounds[2] = new Bounds(Center + new Vector3(0f - num, num, num), size);
		childBounds[3] = new Bounds(Center + new Vector3(num, num, num), size);
		childBounds[4] = new Bounds(Center + new Vector3(0f - num, 0f - num, 0f - num), size);
		childBounds[5] = new Bounds(Center + new Vector3(num, 0f - num, 0f - num), size);
		childBounds[6] = new Bounds(Center + new Vector3(0f - num, 0f - num, num), size);
		childBounds[7] = new Bounds(Center + new Vector3(num, 0f - num, num), size);
	}

	private void SubAdd(T obj, Vector3 objPos)
	{
		if (objects.Count < 8 || SideLength / 2f < minSize)
		{
			OctreeObject item = new OctreeObject
			{
				Obj = obj,
				Pos = objPos
			};
			objects.Add(item);
			return;
		}
		int num2;
		if (children == null)
		{
			Split();
			if (children == null)
			{
				Debug.Log("Child creation failed for an unknown reason. Early exit.");
				return;
			}
			for (int num = objects.Count - 1; num >= 0; num--)
			{
				OctreeObject octreeObject = objects[num];
				num2 = BestFitChild(octreeObject.Pos);
				children[num2].SubAdd(octreeObject.Obj, octreeObject.Pos);
				objects.Remove(octreeObject);
			}
		}
		num2 = BestFitChild(objPos);
		children[num2].SubAdd(obj, objPos);
	}

	private void Split()
	{
		float num = SideLength / 4f;
		float baseLengthVal = SideLength / 2f;
		children = new PointOctreeNode<T>[8];
		children[0] = new PointOctreeNode<T>(baseLengthVal, minSize, Center + new Vector3(0f - num, num, 0f - num));
		children[1] = new PointOctreeNode<T>(baseLengthVal, minSize, Center + new Vector3(num, num, 0f - num));
		children[2] = new PointOctreeNode<T>(baseLengthVal, minSize, Center + new Vector3(0f - num, num, num));
		children[3] = new PointOctreeNode<T>(baseLengthVal, minSize, Center + new Vector3(num, num, num));
		children[4] = new PointOctreeNode<T>(baseLengthVal, minSize, Center + new Vector3(0f - num, 0f - num, 0f - num));
		children[5] = new PointOctreeNode<T>(baseLengthVal, minSize, Center + new Vector3(num, 0f - num, 0f - num));
		children[6] = new PointOctreeNode<T>(baseLengthVal, minSize, Center + new Vector3(0f - num, 0f - num, num));
		children[7] = new PointOctreeNode<T>(baseLengthVal, minSize, Center + new Vector3(num, 0f - num, num));
	}

	private void Merge()
	{
		for (int i = 0; i < 8; i++)
		{
			PointOctreeNode<T> pointOctreeNode = children[i];
			for (int num = pointOctreeNode.objects.Count - 1; num >= 0; num--)
			{
				OctreeObject item = pointOctreeNode.objects[num];
				objects.Add(item);
			}
		}
		children = null;
	}

	private static bool Encapsulates(Bounds outerBounds, Vector3 point)
	{
		return outerBounds.Contains(point);
	}

	private int BestFitChild(Vector3 objPos)
	{
		return ((!(objPos.x <= Center.x)) ? 1 : 0) + ((!(objPos.y >= Center.y)) ? 4 : 0) + ((!(objPos.z <= Center.z)) ? 2 : 0);
	}

	private bool ShouldMerge()
	{
		int num = objects.Count;
		if (children != null)
		{
			PointOctreeNode<T>[] array = children;
			foreach (PointOctreeNode<T> pointOctreeNode in array)
			{
				if (pointOctreeNode.children != null)
				{
					return false;
				}
				num += pointOctreeNode.objects.Count;
			}
		}
		return num <= 8;
	}

	private bool HasAnyObjects()
	{
		if (objects.Count > 0)
		{
			return true;
		}
		if (children != null)
		{
			for (int i = 0; i < 8; i++)
			{
				if (children[i].HasAnyObjects())
				{
					return true;
				}
			}
		}
		return false;
	}

	public static float DistanceToRay(Ray ray, Vector3 point)
	{
		return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
	}
}
