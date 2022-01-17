using UnityEngine;

public static class RectExtender
{
	public static bool Intersects(this Rect a, Rect b, out Rect i)
	{
		float num = a.xMax - b.xMin;
		float num2 = b.xMax - a.xMin;
		float num3 = a.yMax - b.yMin;
		float num4 = b.yMax - a.yMin;
		if (num > 0f && num2 > 0f && num3 > 0f && num4 > 0f)
		{
			i = new Rect(Mathf.Max(a.xMin, b.xMin), Mathf.Max(a.yMin, b.yMin), Mathf.Min(num, num2), Mathf.Min(num3, num4));
			return true;
		}
		i = new Rect(0f, 0f, 0f, 0f);
		return false;
	}

	public static Rect Pad(this Rect a, float amount)
	{
		Rect result = new Rect(a);
		result.xMin -= amount;
		result.xMax += amount;
		result.yMin -= amount;
		result.yMax += amount;
		return result;
	}

	public static Rect Move(this Rect a, Vector2 offset)
	{
		return new Rect(a.xMin + offset.x, a.yMin + offset.y, a.width, a.height);
	}
}
