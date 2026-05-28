using System.Collections.Generic;
using UnityEngine;

public class BoundsQuadtreeNode<T>
{
	private class QuadtreeObject
	{
		public T Obj;

		public Bounds Bounds;
	}

	private float looseness;

	private float minSize;

	private float adjLength;

	private Bounds bounds;

	private readonly List<QuadtreeObject> objects = new List<QuadtreeObject>();

	private BoundsQuadtreeNode<T>[] children;

	private Bounds[] childBounds;

	private const int numObjectsAllowed = 4;

	private const float DRAW_AS_HEIGHT = 1000f;

	public Vector3 Center { get; private set; }

	public float BaseLength { get; private set; }

	public BoundsQuadtreeNode(float baseLengthVal, float minSizeVal, float loosenessVal, Vector3 centerVal)
	{
		SetValues(baseLengthVal, minSizeVal, loosenessVal, centerVal);
	}

	public bool Add(T obj, Bounds objBounds)
	{
		if (!Encapsulates(bounds, objBounds))
		{
			return false;
		}
		SubAdd(obj, objBounds);
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
			for (int j = 0; j < children.Length; j++)
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

	public bool IsColliding(Bounds checkBounds)
	{
		if (objects.Count == 0 && (children == null || children.Length == 0))
		{
			return false;
		}
		if (!bounds.Intersects(checkBounds))
		{
			return false;
		}
		for (int i = 0; i < objects.Count; i++)
		{
			if (objects[i].Bounds.Intersects(checkBounds))
			{
				return true;
			}
		}
		if (children != null)
		{
			for (int j = 0; j < children.Length; j++)
			{
				if (children[j].IsColliding(checkBounds))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IntersectsBounds(Bounds checkBounds)
	{
		return bounds.Intersects(checkBounds);
	}

	public bool ContainsPoint(Vector3 checkPoint)
	{
		return bounds.Contains(checkPoint);
	}

	public void GetColliding(Bounds checkBounds, ref List<T> result)
	{
		if (objects.Count == 0 && (children == null || children.Length == 0))
		{
			return;
		}
		for (int i = 0; i < objects.Count; i++)
		{
			if (objects[i].Bounds.Intersects(checkBounds))
			{
				result.Add(objects[i].Obj);
			}
		}
		if (children == null)
		{
			return;
		}
		bool flag = checkBounds.min.y <= children[0].bounds.max.y;
		bool flag2 = checkBounds.max.y >= children[2].bounds.min.y;
		bool num = checkBounds.min.x <= children[0].bounds.max.x;
		bool flag3 = checkBounds.max.x >= children[1].bounds.min.x;
		if (num)
		{
			if (flag)
			{
				children[0].GetColliding(checkBounds, ref result);
			}
			if (flag2)
			{
				children[2].GetColliding(checkBounds, ref result);
			}
		}
		if (flag3)
		{
			if (flag)
			{
				children[1].GetColliding(checkBounds, ref result);
			}
			if (flag2)
			{
				children[3].GetColliding(checkBounds, ref result);
			}
		}
	}

	public void GetColliding(Vector3 checkPoint, ref List<T> result)
	{
		if (objects.Count == 0 && (children == null || children.Length == 0))
		{
			return;
		}
		for (int i = 0; i < objects.Count; i++)
		{
			if (objects[i].Bounds.Contains(checkPoint))
			{
				result.Add(objects[i].Obj);
			}
		}
		if (children == null)
		{
			return;
		}
		bool flag = checkPoint.y <= children[0].bounds.max.y;
		bool flag2 = checkPoint.y >= children[2].bounds.min.y;
		bool num = checkPoint.x <= children[0].bounds.max.x;
		bool flag3 = checkPoint.x >= children[1].bounds.min.x;
		if (num)
		{
			if (flag)
			{
				children[0].GetColliding(checkPoint, ref result);
			}
			if (flag2)
			{
				children[2].GetColliding(checkPoint, ref result);
			}
		}
		if (flag3)
		{
			if (flag)
			{
				children[1].GetColliding(checkPoint, ref result);
			}
			if (flag2)
			{
				children[3].GetColliding(checkPoint, ref result);
			}
		}
	}

	public void SetChildren(BoundsQuadtreeNode<T>[] childQuadtrees)
	{
		if (childQuadtrees.Length != 4)
		{
			Log.Error("Child quadtree array must be length 4. Was length: " + childQuadtrees.Length);
		}
		else
		{
			children = childQuadtrees;
		}
	}

	public void DrawAllBounds(float depth = 0f)
	{
		float num = depth / 7f;
		Gizmos.color = new Color(num, 0f, 1f - num);
		Bounds bounds = new Bounds(Center, new Vector3(adjLength, 1000f, adjLength));
		Gizmos.DrawWireCube(bounds.center, bounds.size);
		if (children != null)
		{
			depth += 1f;
			for (int i = 0; i < children.Length; i++)
			{
				children[i].DrawAllBounds(depth);
			}
		}
		Gizmos.color = Color.white;
	}

	public void DrawAllObjects()
	{
		float num = BaseLength / 20f;
		Gizmos.color = new Color(0f, 1f - num, num, 0.25f);
		foreach (QuadtreeObject @object in objects)
		{
			Gizmos.DrawCube(@object.Bounds.center, @object.Bounds.size);
			Gizmos.color = Color.magenta;
			Gizmos.DrawLine(@object.Bounds.center, bounds.center);
		}
		if (children != null)
		{
			for (int i = 0; i < children.Length; i++)
			{
				children[i].DrawAllObjects();
			}
		}
		Gizmos.color = Color.white;
	}

	public BoundsQuadtreeNode<T> ShrinkIfPossible(float minLength)
	{
		if (BaseLength < 2f * minLength)
		{
			return this;
		}
		if (objects.Count == 0 && (children == null || children.Length == 0))
		{
			return this;
		}
		int num = -1;
		for (int i = 0; i < objects.Count; i++)
		{
			QuadtreeObject quadtreeObject = objects[i];
			int num2 = BestFitChild(quadtreeObject.Bounds);
			if (i == 0 || num2 == num)
			{
				if (Encapsulates(childBounds[num2], quadtreeObject.Bounds))
				{
					if (num < 0)
					{
						num = num2;
					}
					continue;
				}
				return this;
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
			SetValues(BaseLength / 2f, minSize, looseness, childBounds[num].center);
			return this;
		}
		return children[num];
	}

	private void SetValues(float baseLengthVal, float minSizeVal, float loosenessVal, Vector3 centerVal)
	{
		BaseLength = baseLengthVal;
		minSize = minSizeVal;
		looseness = loosenessVal;
		Center = centerVal;
		adjLength = looseness * baseLengthVal;
		bounds = new Bounds(size: new Vector3(adjLength, float.PositiveInfinity, adjLength), center: Center);
		float num = BaseLength / 4f;
		float num2 = BaseLength / 2f * looseness;
		Vector3 size2 = new Vector3(num2, float.PositiveInfinity, num2);
		childBounds = new Bounds[4];
		childBounds[0] = new Bounds(Center + new Vector3(0f - num, 0f, 0f - num), size2);
		childBounds[1] = new Bounds(Center + new Vector3(num, 0f, 0f - num), size2);
		childBounds[2] = new Bounds(Center + new Vector3(0f - num, 0f, num), size2);
		childBounds[3] = new Bounds(Center + new Vector3(num, 0f, num), size2);
	}

	private void SubAdd(T obj, Bounds objBounds)
	{
		if (children == null && (objects.Count < 4 || BaseLength / 2f < minSize))
		{
			QuadtreeObject item = new QuadtreeObject
			{
				Obj = obj,
				Bounds = objBounds
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
				QuadtreeObject quadtreeObject = objects[num];
				num2 = BestFitChild(quadtreeObject.Bounds);
				if (Encapsulates(children[num2].bounds, quadtreeObject.Bounds))
				{
					children[num2].SubAdd(quadtreeObject.Obj, quadtreeObject.Bounds);
					objects.Remove(quadtreeObject);
				}
			}
		}
		num2 = BestFitChild(objBounds);
		if (Encapsulates(children[num2].bounds, objBounds))
		{
			children[num2].SubAdd(obj, objBounds);
			return;
		}
		QuadtreeObject item2 = new QuadtreeObject
		{
			Obj = obj,
			Bounds = objBounds
		};
		objects.Add(item2);
	}

	private void Split()
	{
		float num = BaseLength / 4f;
		float baseLengthVal = BaseLength / 2f;
		children = new BoundsQuadtreeNode<T>[4];
		children[0] = new BoundsQuadtreeNode<T>(baseLengthVal, minSize, looseness, Center + new Vector3(0f - num, 0f, 0f - num));
		children[1] = new BoundsQuadtreeNode<T>(baseLengthVal, minSize, looseness, Center + new Vector3(num, 0f, 0f - num));
		children[2] = new BoundsQuadtreeNode<T>(baseLengthVal, minSize, looseness, Center + new Vector3(0f - num, 0f, num));
		children[3] = new BoundsQuadtreeNode<T>(baseLengthVal, minSize, looseness, Center + new Vector3(num, 0f, num));
	}

	private void Merge()
	{
		for (int i = 0; i < children.Length; i++)
		{
			BoundsQuadtreeNode<T> boundsQuadtreeNode = children[i];
			for (int num = boundsQuadtreeNode.objects.Count - 1; num >= 0; num--)
			{
				QuadtreeObject item = boundsQuadtreeNode.objects[num];
				objects.Add(item);
			}
		}
		children = null;
	}

	private static bool Encapsulates(Bounds outerBounds, Bounds innerBounds)
	{
		if (outerBounds.Contains(innerBounds.min))
		{
			return outerBounds.Contains(innerBounds.max);
		}
		return false;
	}

	private int BestFitChild(Bounds objBounds)
	{
		return ((!(objBounds.center.x <= Center.x)) ? 1 : 0) + ((!(objBounds.center.z <= Center.z)) ? 2 : 0);
	}

	private bool ShouldMerge()
	{
		int num = objects.Count;
		if (children != null)
		{
			BoundsQuadtreeNode<T>[] array = children;
			foreach (BoundsQuadtreeNode<T> boundsQuadtreeNode in array)
			{
				if (boundsQuadtreeNode.children != null)
				{
					return false;
				}
				num += boundsQuadtreeNode.objects.Count;
			}
		}
		return num <= 4;
	}

	private bool HasAnyObjects()
	{
		if (objects.Count > 0)
		{
			return true;
		}
		if (children != null)
		{
			for (int i = 0; i < children.Length; i++)
			{
				if (children[i].HasAnyObjects())
				{
					return true;
				}
			}
		}
		return false;
	}
}
