using System;
using UnityEngine;

public static class GUIHelper
{
	private static Material m_lineMaterial;

	private static void InitMaterial()
	{
		if (!m_lineMaterial)
		{
			m_lineMaterial = new Material(Shader.Find("Lines/ColoredBlended"));
			m_lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			m_lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	public static void DrawLine(Vector3 pointA, Vector3 pointB, Color color)
	{
		if (InGameHUD.Instance.ShowHUD)
		{
			InitMaterial();
			m_lineMaterial.SetPass(0);
			GL.Begin(1);
			GL.Color(color);
			GL.Vertex3(pointA.x, pointA.y, pointA.z);
			GL.Vertex3(pointB.x, pointB.y, pointB.z);
			GL.End();
		}
	}

	public static void DrawRect(Rect rect, Color color)
	{
		if (InGameHUD.Instance.ShowHUD)
		{
			InitMaterial();
			m_lineMaterial.SetPass(0);
			Vector2 vector = new Vector2(rect.xMin, rect.yMin);
			Vector2 vector2 = new Vector2(rect.xMax, rect.yMax);
			Vector2 vector3 = new Vector2(vector2.x, vector.y);
			Vector2 vector4 = new Vector2(vector.x, vector2.y);
			GL.Begin(7);
			GL.Color(color);
			GL.Vertex3(vector.x, vector.y, 0f);
			GL.Vertex3(vector4.x, vector4.y, 0f);
			GL.Vertex3(vector2.x, vector2.y, 0f);
			GL.Vertex3(vector3.x, vector3.y, 0f);
			GL.End();
		}
	}

	public static void DrawRectHollow(Rect rect, Color color)
	{
		Vector2 vector = new Vector2(rect.xMin, rect.yMin);
		Vector2 vector2 = new Vector2(rect.xMax, rect.yMax);
		Vector2 vector3 = new Vector2(vector2.x, vector.y);
		Vector2 vector4 = new Vector2(vector.x, vector2.y);
		DrawLine(vector, vector3, color);
		DrawLine(vector3, vector2, color);
		DrawLine(vector2, vector4, color);
		DrawLine(vector4, vector, color);
	}

	public static void DrawCircle(Vector3 center, float radius, Color color)
	{
		DrawCircle(center, radius, color, null, null);
	}

	public static void DrawCircle(Vector3 center, float radius, Color color, Vector3[] exclusion_center, float[] exclusion_radius)
	{
		if (radius <= 0f)
		{
			return;
		}
		int num = 60;
		float num2 = 360f / (float)num;
		Vector3 vector = center;
		vector.z += radius;
		Vector3[] array = new Vector3[num];
		bool[] array2 = new bool[num];
		GL.PushMatrix();
		GL.MultMatrix(Matrix4x4.TRS(new Vector3(center.x, center.y + 0.1f, center.z), Quaternion.identity, Vector3.one));
		for (int i = 0; i < num; i++)
		{
			array2[i] = PointInExcludedCircles((array[i] = Quaternion.AngleAxis(num2 * (float)i, Vector3.up) * new Vector3(radius, 0f, 0f)) + center, exclusion_center, exclusion_radius);
		}
		for (int j = 0; j < num - 1; j++)
		{
			if (!array2[j] && !array2[j + 1])
			{
				DrawLine(array[j], array[j + 1], color);
			}
		}
		if (!array2[num - 1] && !array2[0])
		{
			DrawLine(array[num - 1], array[0], color);
		}
		GL.PopMatrix();
	}

	private static bool PointInExcludedCircles(Vector3 point, Vector3[] exclusion_center, float[] exclusion_radius)
	{
		if (exclusion_center == null || exclusion_radius == null)
		{
			return false;
		}
		uint num = 0u;
		foreach (float num2 in exclusion_radius)
		{
			if (num2 > 0f && Vector3.Distance(point, exclusion_center[num]) < num2 * 0.98f)
			{
				return true;
			}
			num++;
		}
		return false;
	}

	public static void DrawDestination(Vector3 pos, float radius, Color color)
	{
		DrawCircle(pos, radius, color);
		Vector3 pointA = new Vector3(radius, 0f, radius);
		Vector3 pointB = new Vector3(0f - radius, 0f, 0f - radius);
		Vector3 pointA2 = new Vector3(0f - radius, 0f, radius);
		Vector3 pointB2 = new Vector3(radius, 0f, 0f - radius);
		GL.PushMatrix();
		GL.MultMatrix(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));
		DrawLine(pointA, pointB, color);
		DrawLine(pointA2, pointB2, color);
		GL.PopMatrix();
	}

	public static void DrawTargetingCone(Vector3 center, Vector3 forward, float radius, float angleDeg, Color color)
	{
		if (angleDeg > 359f)
		{
			DrawCircle(center, radius, color);
			return;
		}
		int num = ((angleDeg > 180f) ? 30 : 15);
		float num2 = angleDeg / (float)num;
		Vector3 vector = center;
		vector.z += radius;
		Vector3[] array = new Vector3[num + 2];
		GL.PushMatrix();
		GL.MultMatrix(Matrix4x4.TRS(new Vector3(center.x, center.y + 0.1f, center.z), Quaternion.identity, Vector3.one));
		int num3 = 0;
		array[num3++] = Vector3.zero;
		for (int num4 = num / 2 - 1; num4 >= 0; num4--)
		{
			Vector3 vector2 = Quaternion.AngleAxis((0f - num2) * (float)num4, Vector3.up) * forward * radius;
			array[num3++] = vector2;
		}
		for (int i = 0; i < num / 2; i++)
		{
			Vector3 vector3 = Quaternion.AngleAxis(num2 * (float)i, Vector3.up) * forward * radius;
			array[num3++] = vector3;
		}
		array[num3++] = Vector3.zero;
		for (int j = 0; j < num - 1; j++)
		{
			DrawLine(array[j], array[j + 1], color);
		}
		DrawLine(array[num - 1], array[0], color);
		GL.PopMatrix();
	}

	public static void DrawArrowToMover(Mover source, Mover target, bool drawTip, Color color)
	{
		Vector3 vector = target.transform.position - source.transform.position;
		float magnitude = vector.magnitude;
		vector.Normalize();
		Vector3 vector2 = source.transform.position + vector * source.Radius * 0.9f + source.transform.right * 0.15f;
		float radius = target.Radius;
		magnitude -= radius;
		if (!(magnitude <= 0f))
		{
			Vector3 vector3 = vector * magnitude + vector2 + source.transform.right * 0.15f;
			DrawLine(vector2, vector3, color);
			if (drawTip)
			{
				Quaternion quaternion = Quaternion.AngleAxis(135f, Vector3.up);
				Quaternion quaternion2 = Quaternion.AngleAxis(-135f, Vector3.up);
				Vector3 vector4 = quaternion * vector;
				vector4 = vector3 + vector4;
				Vector3 vector5 = quaternion2 * vector;
				vector5 = vector3 + vector5;
				DrawLine(vector4, vector3, color);
				DrawLine(vector5, vector3, color);
			}
		}
	}

	public static void DrawArrow(Vector3 source, Vector3 target, Color color)
	{
		Vector3 vector = target - source;
		vector.Normalize();
		DrawLine(source, target, color);
		Quaternion quaternion = Quaternion.AngleAxis(135f, Vector3.up);
		Quaternion quaternion2 = Quaternion.AngleAxis(-135f, Vector3.up);
		Vector3 vector2 = quaternion * vector * 0.25f;
		vector2 = target + vector2;
		Vector3 vector3 = quaternion2 * vector * 0.25f;
		vector3 = target + vector3;
		DrawLine(vector2, target, color);
		DrawLine(vector3, target, color);
	}

	public static void GizmoDrawCircle(Vector3 center, float radius)
	{
		int num = 10;
		float num2 = 360f / (float)num;
		float f = 0f;
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		for (int i = 1; i <= num; i++)
		{
			float num3 = (float)Math.PI / 180f * num2 * (float)i;
			vector.x = Mathf.Sin(f) * radius;
			vector.y = 0f;
			vector.z = Mathf.Cos(f) * radius;
			vector2.x = Mathf.Sin(num3) * radius;
			vector2.y = 0f;
			vector2.z = Mathf.Cos(num3) * radius;
			Gizmos.DrawLine(center + vector, center + vector2);
			f = num3;
		}
	}
}
