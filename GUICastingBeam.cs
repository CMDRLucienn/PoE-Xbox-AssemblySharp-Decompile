using System;
using System.Collections.Generic;
using UnityEngine;

public class GUICastingBeam : GUICastingElement
{
	private float m_Distance;

	private float m_Radius;

	private static List<GUICastingBeam> m_Pool = new List<GUICastingBeam>();

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
			GUICastingBeam gUICastingBeam = m_Pool[i];
			if ((bool)gUICastingBeam && (bool)gUICastingBeam.m_Renderer)
			{
				gUICastingBeam.m_Renderer.enabled = false;
			}
		}
	}

	public static void ClearCastingPool()
	{
		m_Pool.Clear();
	}

	public static GUICastingBeam Create(Vector3 origin, Quaternion forward, float radius, float distance, GUICastingManager.ColorScheme colorScheme)
	{
		GUICastingBeam gUICastingBeam;
		if (s_Index < m_Pool.Count)
		{
			gUICastingBeam = m_Pool[s_Index];
		}
		else
		{
			GameObject obj = NGUITools.AddChild(GUICastingManager.Instance.gameObject);
			obj.name = "CastBeam" + s_Index.ToString("0000");
			gUICastingBeam = obj.AddComponent<GUICastingBeam>();
			m_Pool.Add(gUICastingBeam);
		}
		s_Index++;
		gUICastingBeam.transform.rotation = forward;
		gUICastingBeam.transform.position = origin;
		if (gUICastingBeam.m_Distance != distance || gUICastingBeam.m_Radius != radius)
		{
			gUICastingBeam.m_Distance = distance;
			gUICastingBeam.m_Radius = radius;
			gUICastingBeam.Initialize();
		}
		switch (colorScheme)
		{
		case GUICastingManager.ColorScheme.Friendly:
			gUICastingBeam.m_Renderer.sharedMaterial = GUICastingManager.Instance.BeamMaterial;
			break;
		case GUICastingManager.ColorScheme.Hostile:
			gUICastingBeam.m_Renderer.sharedMaterial = GUICastingManager.Instance.HostileBeamMaterial;
			break;
		case GUICastingManager.ColorScheme.HostileFoeOnly:
			gUICastingBeam.m_Renderer.sharedMaterial = GUICastingManager.Instance.HostileFoeOnlyBeamMaterial;
			break;
		}
		gUICastingBeam.m_Renderer.enabled = true;
		return gUICastingBeam;
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
		int num4 = num3 * 2 + 10;
		if (m_Vertices.Length != num4)
		{
			m_Vertices = new Vector3[num4];
		}
		if (m_Colors.Length != num4)
		{
			m_Colors = new Color[num4];
		}
		int num5 = (num3 * 3 + 8) * 3 + 18;
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
		int num6 = 9;
		m_Vertices[8] = Vector3.zero;
		m_Vertices[num6] = Vector3.forward * m_Distance;
		m_Colors[8] = color;
		m_Colors[9] = color;
		float num7 = 0f;
		for (int i = 0; i < num3; i++)
		{
			int num8 = i + 10;
			int num9 = ((i == num3 - 1) ? 3 : (i + 11));
			m_Vertices[num8] = new Vector3(Mathf.Cos(num7), 0f, Mathf.Sin(num7)) * num + Vector3.forward * m_Distance;
			m_Triangles[i * 3 + 42] = num8;
			m_Triangles[i * 3 + 1 + 42] = num9;
			m_Triangles[i * 3 + 2 + 42] = num6;
			m_Colors[num8] = color;
			int num10 = i + num3 + 10;
			int num11 = ((i == num3 - 1) ? 1 : (num10 + 1));
			m_Vertices[num10] = new Vector3(Mathf.Cos(num7), 0f, Mathf.Sin(num7)) * m_Radius + Vector3.forward * m_Distance;
			int num12 = 42 + num3 * 3;
			m_Triangles[i * 6 + num12] = num10;
			m_Triangles[i * 6 + 1 + num12] = num11;
			m_Triangles[i * 6 + 2 + num12] = num8;
			m_Triangles[i * 6 + 3 + num12] = num11;
			m_Triangles[i * 6 + 4 + num12] = num9;
			m_Triangles[i * 6 + 5 + num12] = num8;
			m_Colors[num10] = color2;
			num7 += num2;
		}
		int num13 = 24;
		m_Triangles[num13] = 3;
		m_Triangles[num13 + 1] = num6;
		m_Triangles[num13 + 2] = 10;
	}
}
