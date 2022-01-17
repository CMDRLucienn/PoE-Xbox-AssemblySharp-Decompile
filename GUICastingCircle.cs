using System;
using System.Collections.Generic;
using UnityEngine;

public class GUICastingCircle : GUICastingElement
{
	private float m_Radius;

	private float m_Arc;

	private static List<GUICastingCircle> m_Pool = new List<GUICastingCircle>();

	private const int CIRCLE_RESOLUTION = 60;

	private static int s_Index = 0;

	public static void Begin()
	{
		s_Index = 0;
	}

	public static void End()
	{
		for (int i = s_Index; i < m_Pool.Count; i++)
		{
			GUICastingCircle gUICastingCircle = m_Pool[i];
			if ((bool)gUICastingCircle && (bool)gUICastingCircle.m_Renderer)
			{
				gUICastingCircle.m_Renderer.enabled = false;
			}
		}
	}

	public static void ClearCastingPool()
	{
		m_Pool.Clear();
	}

	public static GUICastingCircle Create(Vector3 origin, Quaternion forward, float radius, bool extended, float arcDeg, GUICastingManager.ColorScheme colorscheme)
	{
		GUICastingCircle gUICastingCircle;
		if (s_Index < m_Pool.Count)
		{
			gUICastingCircle = m_Pool[s_Index];
		}
		else
		{
			GameObject obj = NGUITools.AddChild(GUICastingManager.Instance.gameObject);
			obj.name = "CastCircle" + s_Index.ToString("0000");
			gUICastingCircle = obj.AddComponent<GUICastingCircle>();
			m_Pool.Add(gUICastingCircle);
		}
		s_Index++;
		gUICastingCircle.transform.rotation = forward;
		gUICastingCircle.transform.position = origin;
		float num = arcDeg * ((float)Math.PI / 180f);
		if (gUICastingCircle.m_Radius != radius || gUICastingCircle.m_Arc != num)
		{
			gUICastingCircle.m_Radius = radius;
			gUICastingCircle.m_Arc = num;
			gUICastingCircle.Initialize();
		}
		if (extended)
		{
			switch (colorscheme)
			{
			case GUICastingManager.ColorScheme.Friendly:
				gUICastingCircle.m_Renderer.sharedMaterial = GUICastingManager.Instance.ExtraRadiusMaterial;
				break;
			case GUICastingManager.ColorScheme.Hostile:
				gUICastingCircle.m_Renderer.sharedMaterial = GUICastingManager.Instance.HostileExtraRadiusMaterial;
				break;
			case GUICastingManager.ColorScheme.WeaponRange:
				gUICastingCircle.m_Renderer.sharedMaterial = GUICastingManager.Instance.WeaponRangeMaterial;
				break;
			case GUICastingManager.ColorScheme.HostileFoeOnly:
				gUICastingCircle.m_Renderer.sharedMaterial = GUICastingManager.Instance.HostileExtraRadiusMaterial;
				break;
			}
		}
		else
		{
			switch (colorscheme)
			{
			case GUICastingManager.ColorScheme.Friendly:
				gUICastingCircle.m_Renderer.sharedMaterial = GUICastingManager.Instance.Material;
				break;
			case GUICastingManager.ColorScheme.Hostile:
				gUICastingCircle.m_Renderer.sharedMaterial = GUICastingManager.Instance.HostileMaterial;
				break;
			case GUICastingManager.ColorScheme.WeaponRange:
				gUICastingCircle.m_Renderer.sharedMaterial = GUICastingManager.Instance.WeaponRangeMaterial;
				break;
			case GUICastingManager.ColorScheme.HostileFoeOnly:
				gUICastingCircle.m_Renderer.sharedMaterial = GUICastingManager.Instance.HostileFoeOnlyMaterial;
				break;
			}
		}
		gUICastingCircle.m_Renderer.enabled = true;
		if (gUICastingCircle.m_ColorScheme != colorscheme)
		{
			gUICastingCircle.m_ColorScheme = colorscheme;
			gUICastingCircle.FillBuffers();
			gUICastingCircle.LoadBuffers();
		}
		return gUICastingCircle;
	}

	protected override void FillBuffers()
	{
		GUICastingManager.ColorScheme colorScheme = m_ColorScheme;
		float num;
		float a;
		float a2;
		if (colorScheme == GUICastingManager.ColorScheme.WeaponRange)
		{
			num = GUICastingManager.Instance.WeaponRangeFadeWidth;
			a = GUICastingManager.Instance.WeaponRangeMinAlpha;
			a2 = GUICastingManager.Instance.WeaponRangeMaxAlpha;
		}
		else
		{
			num = GUICastingManager.Instance.FadeWidth;
			a = GUICastingManager.Instance.MinAlpha;
			a2 = GUICastingManager.Instance.MaxAlpha;
		}
		float num2 = m_Radius - num;
		Color color = new Color(1f, 1f, 1f, a);
		if (num2 < 0f)
		{
			float t = m_Radius / num;
			color.a = Mathf.Lerp(a2, color.a, t);
			num2 = 0f;
		}
		Color color2 = new Color(1f, 1f, 1f, a2);
		float num3 = (float)Math.PI / 30f;
		int num4 = Mathf.CeilToInt(m_Arc / num3);
		int num5 = num4 + 1;
		int num6 = num5 * 2 + 1;
		if (m_Vertices.Length != num6)
		{
			m_Vertices = new Vector3[num6];
		}
		if (m_UVs.Length != num6)
		{
			m_UVs = new Vector2[num6];
		}
		if (m_Colors.Length != num6)
		{
			m_Colors = new Color[num6];
		}
		int num7 = num4 * 3 * 3;
		if (m_Triangles.Length != num7)
		{
			m_Triangles = new int[num7];
		}
		m_Vertices[0] = Vector3.zero;
		m_UVs[0] = Vector2.zero;
		float num8 = (0f - m_Arc) / 2f;
		for (int i = 0; i < num5; i++)
		{
			if (i == num5 - 1)
			{
				num8 = m_Arc / 2f;
			}
			int num9 = i + 1;
			int num10 = num9 + 1;
			m_Vertices[num9] = new Vector3(Mathf.Cos(num8), 0f, Mathf.Sin(num8)) * num2;
			m_UVs[num9] = new Vector2((float)i / (float)num5, 0f);
			if (i < num5 - 1)
			{
				m_Triangles[i * 3] = num9;
				m_Triangles[i * 3 + 1] = num10;
				m_Triangles[i * 3 + 2] = 0;
			}
			m_Colors[num9] = color;
			int num11 = i + num5 + 1;
			int num12 = num11 + 1;
			m_Vertices[num11] = new Vector3(Mathf.Cos(num8), 0f, Mathf.Sin(num8)) * m_Radius;
			m_UVs[num11] = new Vector2((float)i / (float)num5, 1f);
			if (i < num5 - 1)
			{
				m_Triangles[(i + num4) * 3] = num11;
				m_Triangles[(i + num4) * 3 + 1] = num12;
				m_Triangles[(i + num4) * 3 + 2] = num9;
				m_Triangles[(i + num4 * 2) * 3] = num12;
				m_Triangles[(i + num4 * 2) * 3 + 1] = num10;
				m_Triangles[(i + num4 * 2) * 3 + 2] = num9;
			}
			m_Colors[num11] = color2;
			num8 += num3;
		}
		m_Colors[0] = color;
	}
}
