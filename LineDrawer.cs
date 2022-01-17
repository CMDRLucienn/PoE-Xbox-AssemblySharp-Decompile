using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
	public struct Line
	{
		public Vector3[] p;

		public Color c;

		public float width;
	}

	private List<Line> m_lineList = new List<Line>();

	private Material m_lineMaterial;

	public static LineDrawer Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'LineDrawer' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		m_lineMaterial = new Material(Shader.Find("Lines/ColoredBlended"));
		m_lineMaterial.hideFlags = HideFlags.HideAndDontSave;
		m_lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void AddLine(Line line)
	{
		m_lineList.Add(line);
	}

	private void OnPostRender()
	{
		if (!InGameHUD.Instance.ShowHUD)
		{
			m_lineList.Clear();
			return;
		}
		for (int i = 0; i < m_lineList.Count; i++)
		{
			Vector3[] p = m_lineList[i].p;
			if (p == null || p.Length < 2)
			{
				continue;
			}
			m_lineMaterial.SetPass(0);
			GL.Color(m_lineList[i].c);
			float num = m_lineList[i].width / 2f;
			Vector3 vector = (new Vector3(p[1].z, p[0].y, p[0].x) - new Vector3(p[0].z, p[0].y, p[1].x)).normalized * num;
			GL.Color(m_lineList[i].c);
			GL.Begin(7);
			for (int j = 0; j < p.Length - 1; j++)
			{
				Vector3 vector2 = vector;
				GL.Vertex(p[j] - vector2);
				GL.Vertex(p[j] + vector2);
				GL.Vertex(p[j + 1] + vector2);
				GL.Vertex(p[j + 1] - vector2);
				if (j + 2 < p.Length)
				{
					vector = (new Vector3(p[j + 2].z, p[j + 1].y, p[j + 1].x) - new Vector3(p[j + 1].z, p[j + 1].y, p[j + 2].x)).normalized * num;
					if (Vector3.Dot(vector2, p[j + 2] - p[j + 1]) < 0f)
					{
						GL.Vertex(p[j + 1]);
						GL.Vertex(p[j + 1] + vector2);
						GL.Vertex(p[j + 1] + (vector2 + vector) * 0.5f);
						GL.Vertex(p[j + 1] + vector);
					}
					else
					{
						GL.Vertex(p[j + 1]);
						GL.Vertex(p[j + 1] - vector2);
						GL.Vertex(p[j + 1] - (vector2 + vector) * 0.5f);
						GL.Vertex(p[j + 1] - vector);
					}
				}
			}
			GL.End();
		}
		m_lineList.Clear();
	}

	private void OnApplicationQuit()
	{
		GameUtilities.DestroyImmediate(m_lineMaterial);
	}
}
