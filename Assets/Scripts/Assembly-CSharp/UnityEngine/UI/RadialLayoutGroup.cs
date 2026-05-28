using System;

namespace UnityEngine.UI
{
	public class RadialLayoutGroup : LayoutGroup
	{
		[Tooltip("Width/height of the layout child.")]
		public Vector2 childSize;

		[Tooltip("Radius to spread the layout children from the center.")]
		public float radius;

		[Tooltip("Angular offset, in degrees.")]
		public float offset;

		public Transform GetChild(float radians)
		{
			Transform result = null;
			if (base.rectChildren.Count > 0)
			{
				Vector2 vector = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
				float num = float.MaxValue;
				foreach (RectTransform rectChild in base.rectChildren)
				{
					float sqrMagnitude = ((Vector2)rectChild.localPosition - vector).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						result = rectChild;
					}
				}
			}
			return result;
		}

		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();
			SetLayoutInputForAxis(childSize.x, childSize.x, childSize.x, 0);
		}

		public override void CalculateLayoutInputVertical()
		{
			SetLayoutInputForAxis(childSize.y, childSize.y, childSize.y, 1);
		}

		public override void SetLayoutHorizontal()
		{
			SetLayoutAlongAxis(0);
		}

		public override void SetLayoutVertical()
		{
			SetLayoutAlongAxis(1);
		}

		private void SetLayoutAlongAxis(int axis)
		{
			if (base.rectChildren.Count <= 0)
			{
				return;
			}
			float num = (float)Math.PI * 2f / (float)base.rectChildren.Count;
			float num2 = offset * ((float)Math.PI / 180f);
			foreach (RectTransform rectChild in base.rectChildren)
			{
				SetChildAlongAxis(rectChild, axis, ((axis == 0) ? Mathf.Cos(num2) : Mathf.Sin(num2)) * radius, base.rectTransform.rect.size[axis]);
				num2 += num;
			}
		}
	}
}
