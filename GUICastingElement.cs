using UnityEngine;
using UnityEngine.Rendering;

public abstract class GUICastingElement : MonoBehaviour
{
	protected Mesh m_Mesh;

	protected MeshRenderer m_Renderer;

	protected MeshFilter m_Filter;

	protected Vector3[] m_Vertices;

	protected Vector2[] m_UVs;

	protected Color[] m_Colors;

	protected int[] m_Triangles;

	protected GUICastingManager.ColorScheme m_ColorScheme;

	protected void Initialize()
	{
		if (!m_Mesh)
		{
			m_Mesh = new Mesh();
			base.gameObject.layer = LayerUtility.FindLayerValue("InGameUI");
			m_Filter = base.gameObject.AddComponent<MeshFilter>();
			m_Renderer = base.gameObject.AddComponent<MeshRenderer>();
			m_Filter.mesh = m_Mesh;
			m_Renderer.shadowCastingMode = ShadowCastingMode.Off;
			m_Renderer.receiveShadows = false;
			m_Renderer.sharedMaterial = GUICastingManager.Instance.Material;
			m_Vertices = new Vector3[0];
			m_UVs = new Vector2[0];
			m_Colors = new Color[0];
			m_Triangles = new int[0];
		}
		FillBuffers();
		LoadBuffers();
	}

	protected void LoadBuffers()
	{
		if (m_Vertices.Length >= m_Mesh.vertexCount || m_Mesh.vertexCount == 0)
		{
			m_Mesh.vertices = m_Vertices;
			m_Mesh.colors = m_Colors;
			m_Mesh.uv = m_UVs;
			m_Mesh.triangles = m_Triangles;
		}
		else
		{
			m_Mesh.triangles = m_Triangles;
			m_Mesh.vertices = m_Vertices;
			m_Mesh.colors = m_Colors;
			m_Mesh.uv = m_UVs;
		}
	}

	protected abstract void FillBuffers();
}
