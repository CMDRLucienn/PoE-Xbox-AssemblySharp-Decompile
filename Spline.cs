using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{
	[HideInInspector]
	public List<SplineControlPoint> points = new List<SplineControlPoint>();

	public static int NumberOfSubdivisions = 50;

	public void AddPoint(Vector3 p)
	{
		SplineControlPoint splineControlPoint = new SplineControlPoint();
		splineControlPoint.p = p;
		points.Add(splineControlPoint);
	}

	public void InsertPoint(Vector3 p, float t)
	{
		int num = (int)(t * (float)(points.Count - 1));
		SplineControlPoint splineControlPoint = new SplineControlPoint();
		splineControlPoint.p = p;
		int index = Mathf.Min(points.Count - 1, num + 1);
		points.Insert(index, splineControlPoint);
	}

	public Vector3 Evaluate(float t)
	{
		float num = t * (float)(points.Count - 1);
		int num2 = (int)num;
		num -= (float)num2;
		int index = Mathf.Max(0, num2 - 1);
		int index2 = Mathf.Min(points.Count - 1, num2 + 1);
		int index3 = Mathf.Min(points.Count - 1, num2 + 2);
		return CatmullRom(num, points[index].p, points[num2].p, points[index2].p, points[index3].p);
	}

	public float GetClosestPoint(Vector3 p, out float d)
	{
		d = float.MaxValue;
		float result = 0f;
		for (float num = 0f; num <= 1f; num += 0.005f)
		{
			Vector3 b = Evaluate(num);
			b.y = 0f;
			float num2 = Vector3.Distance(p, b);
			if (num2 < d)
			{
				d = num2;
				result = num;
			}
		}
		return result;
	}

	public Vector3 GetVelocity(float t, float d)
	{
		if (d > 20f)
		{
			return Vector3.zero;
		}
		Vector3 vector = Evaluate(t);
		vector.y = 0f;
		Vector3 vector2 = Evaluate(t + 0.005f);
		vector2.y = 0f;
		float num = Mathf.Lerp(1f, 0f, d / 20f);
		return Vector3.Normalize(vector2 - vector) * num;
	}

	private Vector3 CatmullRom(float t, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
	{
		float num = ((0f - t + 2f) * t - 1f) * t * 0.5f;
		float num2 = ((3f * t - 5f) * t * t + 2f) * 0.5f;
		float num3 = ((-3f * t + 4f) * t + 1f) * t * 0.5f;
		float num4 = (t - 1f) * t * t * 0.5f;
		return p1 * num + p2 * num2 + p3 * num3 + p4 * num4;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = Color.cyan;
		if (points.Count < 4)
		{
			return;
		}
		NumberOfSubdivisions = Mathf.Max(1, NumberOfSubdivisions);
		Vector3 zero = Vector3.zero;
		Vector3 to = Vector3.zero;
		for (int i = 0; i <= NumberOfSubdivisions; i++)
		{
			float t = (float)i / (float)NumberOfSubdivisions;
			zero = Evaluate(t);
			if (i > 0)
			{
				Gizmos.DrawLine(zero, to);
			}
			to = zero;
		}
	}
}
