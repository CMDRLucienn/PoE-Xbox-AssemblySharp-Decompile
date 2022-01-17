using System;
using System.Collections.Generic;
using UnityEngine;

public class GUICastingWall : GUICastingElement
{
	private float m_Distance;

	private float m_Radius;

	private static List<GUICastingWall> m_Pool = new List<GUICastingWall>();

	private const int CIRCLE_RESOLUTION = 40;

	private static int s_Index = 0;

	public static void Begin()
	{
		s_Index = 0;
	}

	public static void End()
	{
		for (int i = s_Index; i < m_Pool.Count; i++)
		{
			GUICastingWall gUICastingWall = m_Pool[i];
			if ((bool)gUICastingWall && (bool)gUICastingWall.m_Renderer)
			{
				gUICastingWall.m_Renderer.enabled = false;
			}
		}
	}

	public static void ClearCastingPool()
	{
		m_Pool.Clear();
	}

	public static GUICastingWall Create(Vector3 from, Vector3 to, float radius, GUICastingManager.ColorScheme colorScheme)
	{
		GUICastingWall gUICastingWall;
		if (s_Index < m_Pool.Count)
		{
			gUICastingWall = m_Pool[s_Index];
		}
		else
		{
			GameObject obj = NGUITools.AddChild(GUICastingManager.Instance.gameObject);
			obj.name = "CastWall" + s_Index.ToString("0000");
			gUICastingWall = obj.AddComponent<GUICastingWall>();
			m_Pool.Add(gUICastingWall);
		}
		s_Index++;
		float magnitude = (to - from).magnitude;
		gUICastingWall.transform.rotation = Quaternion.FromToRotation(Vector3.forward, to - from);
		gUICastingWall.transform.position = from;
		if (gUICastingWall.m_Distance != magnitude || gUICastingWall.m_Radius != radius)
		{
			gUICastingWall.m_Distance = magnitude;
			gUICastingWall.m_Radius = radius;
			gUICastingWall.Initialize();
		}
		switch (colorScheme)
		{
		case GUICastingManager.ColorScheme.Friendly:
			gUICastingWall.m_Renderer.sharedMaterial = GUICastingManager.Instance.BeamMaterial;
			break;
		case GUICastingManager.ColorScheme.Hostile:
			gUICastingWall.m_Renderer.sharedMaterial = GUICastingManager.Instance.HostileBeamMaterial;
			break;
		case GUICastingManager.ColorScheme.HostileFoeOnly:
			gUICastingWall.m_Renderer.sharedMaterial = GUICastingManager.Instance.HostileFoeOnlyBeamMaterial;
			break;
		}
		gUICastingWall.m_Renderer.enabled = true;
		return gUICastingWall;
	}

	protected override void FillBuffers()
	{
		float num = m_Radius - GUICastingManager.Instance.FadeWidth;
		Color color = new Color(1f, 1f, 1f, GUICastingManager.Instance.MinAlpha);
		if (num < 0f)
		{
			float t = m_Radius / GUICastingManager.Instance.FadeWidth;
			color.a = Mathf.Lerp(GUICastingManager.Instance.MaxAlpha, color.a, t);
			num = 0f;
		}
		Color color2 = new Color(1f, 1f, 1f, GUICastingManager.Instance.MaxAlpha);
		float num2 = (float)Math.PI / 20f;
		int num3 = 19;
		int num4 = num3 * 2 * 2 + 10;
		if (m_Vertices.Length != num4)
		{
			m_Vertices = new Vector3[num4];
		}
		if (m_Colors.Length != num4)
		{
			m_Colors = new Color[num4];
		}
		int num5 = (num3 * 2 * 3 + 8) * 3 + 18;
		if (m_Triangles.Length != num5)
		{
			m_Triangles = new int[num5];
		}
		m_Vertices[0] = new Vector3(0f - m_Radius, 0f, 0f);
		m_Vertices[1] = new Vector3(0f - m_Radius, 0f, m_Distance);
		m_Vertices[2] = new Vector3(0f - num, 0f, 0f);
		m_Vertices[3] = new Vector3(0f - num, 0f, m_Distance);
		m_Colors[0] = color2;
		m_Colors[1] = color2;
		m_Colors[2] = color;
		m_Colors[3] = color;
		m_Vertices[4] = new Vector3(m_Radius, 0f, 0f);
		m_Vertices[5] = new Vector3(m_Radius, 0f, m_Distance);
		m_Vertices[6] = new Vector3(num, 0f, 0f);
		m_Vertices[7] = new Vector3(num, 0f, m_Distance);
		m_Colors[4] = color2;
		m_Colors[5] = color2;
		m_Colors[6] = color;
		m_Colors[7] = color;
		m_Triangles[0] = 0;
		m_Triangles[1] = 1;
		m_Triangles[2] = 3;
		m_Triangles[3] = 0;
		m_Triangles[4] = 2;
		m_Triangles[5] = 3;
		m_Triangles[6] = 4;
		m_Triangles[7] = 5;
		m_Triangles[8] = 7;
		m_Triangles[9] = 4;
		m_Triangles[10] = 6;
		m_Triangles[11] = 7;
		m_Triangles[12] = 2;
		m_Triangles[13] = 3;
		m_Triangles[14] = 9;
		m_Triangles[15] = 2;
		m_Triangles[16] = 8;
		m_Triangles[17] = 9;
		m_Triangles[18] = 6;
		m_Triangles[19] = 7;
		m_Triangles[20] = 9;
		m_Triangles[21] = 6;
		m_Triangles[22] = 8;
		m_Triangles[23] = 9;
		int num6 = 8;
		int num7 = 9;
		m_Vertices[num6] = Vector3.zero;
		m_Vertices[num7] = Vector3.forward * m_Distance;
		m_Colors[8] = color;
		m_Colors[9] = color;
		float num8 = 0f;
		for (int i = 0; i < num3; i++)
		{
			for (int j = 0; j <= 1; j++)
			{
				int num9 = 10;
				int num10 = 42;
				float num11 = 0f;
				Vector3 vector = Vector3.forward * m_Distance;
				int num12 = 3;
				int num13 = 1;
				if (j == 1)
				{
					num9 += num3 * 2;
					num10 += num3 * 3 * 3;
					num11 = (float)Math.PI;
					vector = Vector3.zero;
					num12 = 6;
					num13 = 4;
				}
				int num14 = i + num9;
				int num15 = ((i == num3 - 1) ? num12 : (num14 + 1));
				m_Vertices[num14] = new Vector3(Mathf.Cos(num8 + num11), 0f, Mathf.Sin(num8 + num11)) * num + vector;
				m_Triangles[i * 3 + num10] = num14;
				m_Triangles[i * 3 + 1 + num10] = num15;
				m_Triangles[i * 3 + 2 + num10] = num7;
				m_Colors[num14] = color;
				int num16 = i + num3 + num9;
				int num17 = ((i == num3 - 1) ? num13 : (num16 + 1));
				m_Vertices[num16] = new Vector3(Mathf.Cos(num8 + num11), 0f, Mathf.Sin(num8 + num11)) * m_Radius + vector;
				int num18 = num10 + num3 * 3;
				m_Triangles[i * 6 + num18] = num16;
				m_Triangles[i * 6 + 1 + num18] = num17;
				m_Triangles[i * 6 + 2 + num18] = num14;
				m_Triangles[i * 6 + 3 + num18] = num17;
				m_Triangles[i * 6 + 4 + num18] = num15;
				m_Triangles[i * 6 + 5 + num18] = num14;
				m_Colors[num16] = color2;
			}
			num8 += num2;
		}
	}
}
