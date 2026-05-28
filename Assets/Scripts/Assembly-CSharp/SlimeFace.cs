using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Slimes/Slime Face")]
public class SlimeFace : ScriptableObject
{
	public enum SlimeExpression
	{
		None = 0,
		Alarm = 1,
		Angry = 2,
		AttackTelegraph = 3,
		Awe = 4,
		Blink = 5,
		Blush = 6,
		BlushBlink = 7,
		ChompClosed = 8,
		ChompOpen = 9,
		Elated = 10,
		Feral = 11,
		Fried = 12,
		Glitch = 13,
		Grimace = 14,
		Happy = 15,
		Hungry = 16,
		Invoke = 17,
		Scared = 18,
		Starving = 19,
		Wince = 20
	}

	public class SlimeExpressionComparer : IEqualityComparer<SlimeExpression>
	{
		public bool Equals(SlimeExpression slimeExpr1, SlimeExpression slimeExpr2)
		{
			return slimeExpr1 == slimeExpr2;
		}

		public int GetHashCode(SlimeExpression slimeExpr)
		{
			return (int)slimeExpr;
		}
	}

	public SlimeExpressionFace[] ExpressionFaces;

	private Dictionary<SlimeExpression, SlimeExpressionFace> _expressionToFaceLookup = new Dictionary<SlimeExpression, SlimeExpressionFace>(DefaultSlimeExpressionComparer);

	public static SlimeExpressionComparer DefaultSlimeExpressionComparer = new SlimeExpressionComparer();

	public SlimeExpressionFace GetExpressionFace(SlimeExpression expression)
	{
		return _expressionToFaceLookup[expression];
	}

	public void OnEnable()
	{
		_expressionToFaceLookup.Clear();
		for (int i = 0; i < ExpressionFaces.Length; i++)
		{
			SlimeExpressionFace value = ExpressionFaces[i];
			_expressionToFaceLookup.Add(value.SlimeExpression, value);
		}
	}
}
