using UnityEngine;
using UnityEngine.Rendering;

public class ShadowPass : MonoBehaviour
{
	private Vector3[] m_ScreenQuad;

	private Vector2[] m_ScreenQuadUV = new Vector2[4]
	{
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(0f, 0f),
		new Vector2(1f, 0f)
	};

	private int[] m_ScreenQuadTri = new int[6] { 0, 1, 2, 2, 1, 3 };

	private int m_screen_width;

	private int m_screen_height;

	public float m_offset_x = 1f;

	public float m_offset_y = 1f;

	private MeshRenderer m_ScreenQuadRenderer;

	private Mesh m_ScreenQuadMesh;

	private Material m_ScreenQuadMat_Blt;

	private Material m_ScreenQuadMat_Manual;

	private bool m_UseBlt;

	private ShadowStaticOnlyCameraScript m_ShadowStaticOnlyComponent;

	private void Start()
	{
		base.gameObject.AddComponent(typeof(MeshFilter));
		m_ScreenQuadRenderer = base.gameObject.AddComponent<MeshRenderer>();
		m_ScreenQuadMesh = new Mesh();
		m_ScreenQuadMat_Blt = new Material(Shader.Find("Custom/ShadowPass"));
		m_ScreenQuadMat_Manual = new Material(Shader.Find("Custom/ShadowPass_Manual"));
		m_screen_width = 0;
		m_screen_height = 0;
		Transform parent = base.transform;
		while (parent.parent != null)
		{
			parent = parent.parent;
		}
		m_ShadowStaticOnlyComponent = parent.gameObject.GetComponentInChildren<ShadowStaticOnlyCameraScript>();
	}

	private void Update()
	{
		if (!m_UseBlt)
		{
			if (m_screen_width != Screen.width || m_screen_height != Screen.height)
			{
				CreateScreenQuad(Screen.width, Screen.height);
			}
			m_ScreenQuadRenderer.enabled = true;
		}
		else
		{
			m_ScreenQuadRenderer.enabled = false;
		}
	}

	private void OnGUI()
	{
		if (m_UseBlt)
		{
			Graphics.DrawTexture(new Rect(0f - m_offset_x, 0f - m_offset_y, Screen.width, Screen.height), m_ShadowStaticOnlyComponent.GetShadowTexture(), m_ScreenQuadMat_Blt);
		}
	}

	private void CreateScreenQuad(int width, int height)
	{
		m_screen_width = width;
		m_screen_height = height;
		float num = m_offset_x / (float)width;
		float num2 = m_offset_y / (float)height;
		m_ScreenQuad = new Vector3[4];
		m_ScreenQuad[0] = new Vector3(-1f - num, 1f + num2, GetComponent<Camera>().nearClipPlane + 0.1f);
		m_ScreenQuad[1] = new Vector3(1f + num, 1f + num2, GetComponent<Camera>().nearClipPlane + 0.1f);
		m_ScreenQuad[2] = new Vector3(-1f - num, -1f - num2, GetComponent<Camera>().nearClipPlane + 0.1f);
		m_ScreenQuad[3] = new Vector3(1f + num, -1f - num2, GetComponent<Camera>().nearClipPlane + 0.1f);
		m_ScreenQuadMesh.vertices = m_ScreenQuad;
		float x = 0f;
		float y = 0f;
		m_ScreenQuadUV[0] = new Vector2(x, 1f + num2);
		m_ScreenQuadUV[1] = new Vector2(1f + num, 1f + num2);
		m_ScreenQuadUV[2] = new Vector2(x, y);
		m_ScreenQuadUV[3] = new Vector2(1f + num, y);
		m_ScreenQuadMesh.uv = m_ScreenQuadUV;
		m_ScreenQuadMesh.triangles = m_ScreenQuadTri;
		MeshFilter obj = (MeshFilter)GetComponent(typeof(MeshFilter));
		obj.mesh = m_ScreenQuadMesh;
		obj.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
		obj.GetComponent<Renderer>().receiveShadows = false;
		obj.GetComponent<Renderer>().material = m_ScreenQuadMat_Manual;
	}
}
