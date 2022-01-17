using System;
using UnityEngine;

public static class PE_Math
{
	public const float TwoPi = (float)Math.PI * 2f;

	public static float GaussianDistribution(float x, float mean, float standardDeviation)
	{
		return 1f / Mathf.Sqrt((float)Math.PI * 2f * standardDeviation * standardDeviation) * Mathf.Exp((0f - (x - mean) * (x - mean)) / (2f * standardDeviation * standardDeviation));
	}

	public static Vector2 ElementOp(Vector2 vec, Func<float, float> op)
	{
		vec.x = op(vec.x);
		vec.y = op(vec.y);
		return vec;
	}

	public static Vector3 ElementOp(Vector3 vec, Func<float, float> op)
	{
		vec.x = op(vec.x);
		vec.y = op(vec.y);
		vec.z = op(vec.z);
		return vec;
	}

	public static Vector4 ElementOp(Vector4 vec, Func<float, float> op)
	{
		vec.x = op(vec.x);
		vec.y = op(vec.y);
		vec.z = op(vec.z);
		vec.w = op(vec.w);
		return vec;
	}

	public static Vector2 GetVectorFromAngle(float degrees)
	{
		Vector2 result = default(Vector2);
		result.x = Mathf.Cos(degrees * ((float)Math.PI / 180f));
		result.y = Mathf.Sin(degrees * ((float)Math.PI / 180f));
		return result;
	}
}
