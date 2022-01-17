using System.Collections.Generic;
using UnityEngine;

public class PE_DeferredLightPass : MonoBehaviour
{
	public RenderTexture m_fogLightRenderTexture;

	public static PE_DeferredLightPass Instance;

	public bool debugShowFrustumRays;

	public bool debugShowHighPlaneRays = true;

	public bool debugShowInterpolatedHighPlaneRay = true;

	public Vector2 debugInterpolateScreenCoords = Vector2.zero;

	private Vector3 m_cameraDirection;

	private Vector3 m_cameraPosition;

	private Vector4 m_highPlaneRay00;

	private Vector4 m_highPlaneRay10;

	private Vector4 m_highPlaneRay01;

	private Vector4 m_highPlaneRay11;

	private Mesh m_mesh;

	private Mesh m_fogLightMesh;

	private Vector3[] m_meshPositions;

	private Vector3[] m_meshLightColors;

	private Vector4[] m_meshLightCenterAndInverseRadius;

	private int[] m_meshTriangles;

	private List<PE_DeferredPointLight> m_pointLights = new List<PE_DeferredPointLight>();

	private PE_DeferredPointLight[] m_pointLightsArray;

	private Material m_deferredLightMaterial;

	private int m_screenWidth;

	private int m_screenHeight;

	public Vector3 GetCameraDirection()
	{
		return m_cameraDirection;
	}

	public Vector3 GetCameraPosition()
	{
		return m_cameraPosition;
	}

	public void AddPointLight(PE_DeferredPointLight pointLight)
	{
		if (!m_pointLights.Contains(pointLight) && pointLight.enabled)
		{
			m_pointLights.Add(pointLight);
			m_pointLightsArray = m_pointLights.ToArray();
		}
	}

	public void RemovePointLight(PE_DeferredPointLight pointLight)
	{
		m_pointLights.Remove(pointLight);
		m_pointLightsArray = m_pointLights.ToArray();
	}

	public Vector4 GetFrustumRayForScreenCoords(float x, float y)
	{
		Vector4 a = Vector4.Lerp(m_highPlaneRay00, m_highPlaneRay10, x);
		Vector4 b = Vector4.Lerp(m_highPlaneRay01, m_highPlaneRay11, x);
		return Vector4.Lerp(a, b, y);
	}

	public void DoUpdate(float timeDelta, LevelInfo levelInfo)
	{
		if (!(null == m_mesh))
		{
			if (Screen.width != m_screenWidth || Screen.height != m_screenHeight)
			{
				CreateRenderTargets(Screen.width, Screen.height);
			}
			ComputeHighPlaneRays(levelInfo);
			float w = 1f / m_cameraDirection.y;
			Vector4 value = new Vector4(m_cameraDirection.x, m_cameraDirection.y, m_cameraDirection.z, w);
			Shader.SetGlobalVector("Trenton_CameraDirection", value);
			Shader.SetGlobalVector("Trenton_HighPlaneRay00", m_highPlaneRay00);
			Shader.SetGlobalVector("Trenton_HighPlaneRay10", m_highPlaneRay10);
			Shader.SetGlobalVector("Trenton_HighPlaneRay01", m_highPlaneRay01);
			Shader.SetGlobalVector("Trenton_HighPlaneRay11", m_highPlaneRay11);
			Camera component = Camera.main.GetComponent<Camera>();
			Vector4 value2 = new Vector4(1f / (float)component.pixelWidth, 1f / (float)component.pixelHeight, 0f, 0f);
			Shader.SetGlobalVector("Trenton_ScreenPixelSize", value2);
			PE_DeferredPointLight[] pointLightsArray = m_pointLightsArray;
			UpdateDeferredLightMesh(m_mesh, pointLightsArray, isFogLightMesh: false);
			UpdateDeferredLightMesh(m_fogLightMesh, pointLightsArray, isFogLightMesh: true);
			DebugDrawFrustumRays();
			DebugDrawHighPlaneRays();
			DebugDrawInterpolatdHighPlaneRay();
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		CreateRenderTargets(Screen.width, Screen.height);
		m_mesh = new Mesh();
		m_fogLightMesh = new Mesh();
		m_deferredLightMaterial = new Material(Shader.Find("Trenton/PE_DeferredLightMesh"));
		PE_DeferredPointLight[] array = Object.FindObjectsOfType(typeof(PE_DeferredPointLight)) as PE_DeferredPointLight[];
		foreach (PE_DeferredPointLight pointLight in array)
		{
			AddPointLight(pointLight);
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void CreateRenderTargets(int screenWidth, int screenHeight)
	{
		if (null == m_fogLightRenderTexture)
		{
			GameUtilities.Destroy(m_fogLightRenderTexture);
			m_fogLightRenderTexture = null;
		}
		int width = screenWidth / 4;
		int height = screenHeight / 4;
		m_fogLightRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		m_fogLightRenderTexture.filterMode = FilterMode.Bilinear;
		m_fogLightRenderTexture.wrapMode = TextureWrapMode.Clamp;
		m_fogLightRenderTexture.useMipMap = false;
		m_screenWidth = screenWidth;
		m_screenHeight = screenHeight;
	}

	public void UpdateDeferredLightMesh(Mesh mesh, PE_DeferredPointLight[] pointLights, bool isFogLightMesh)
	{
		mesh.Clear();
		if (pointLights == null || pointLights.Length == 0)
		{
			return;
		}
		int num = pointLights.Length;
		int num2 = 4 * num;
		int num3 = 6 * num;
		if (m_meshPositions == null || m_meshPositions.Length != num2)
		{
			m_meshPositions = new Vector3[num2];
		}
		if (m_meshLightColors == null || m_meshLightColors.Length != num2)
		{
			m_meshLightColors = new Vector3[num2];
		}
		if (m_meshLightCenterAndInverseRadius == null || m_meshLightCenterAndInverseRadius.Length != num2)
		{
			m_meshLightCenterAndInverseRadius = new Vector4[num2];
		}
		if (m_meshTriangles == null || m_meshTriangles.Length != num3)
		{
			m_meshTriangles = new int[num3];
		}
		int num4 = 0;
		int num5 = 0;
		Vector3 vector2 = default(Vector3);
		foreach (PE_DeferredPointLight pE_DeferredPointLight in pointLights)
		{
			if (!(pE_DeferredPointLight == null))
			{
				bool num6 = (!isFogLightMesh && pE_DeferredPointLight.affectScene) || (isFogLightMesh && pE_DeferredPointLight.affectFog);
				GameObject obj = pE_DeferredPointLight.gameObject;
				m_meshTriangles[num5++] = num4;
				m_meshTriangles[num5++] = num4 + 2;
				m_meshTriangles[num5++] = num4 + 1;
				m_meshTriangles[num5++] = num4;
				m_meshTriangles[num5++] = num4 + 3;
				m_meshTriangles[num5++] = num4 + 2;
				Vector3 vector = obj.transform.localToWorldMatrix.MultiplyPoint(Vector3.zero);
				float num7 = (num6 ? pE_DeferredPointLight.GetLightRadius() : 0f);
				float num8 = (num6 ? (pE_DeferredPointLight.lightIntensity * pE_DeferredPointLight.lightAlpha) : 0f);
				vector2.x = pE_DeferredPointLight.lightColor.r * num8;
				vector2.y = pE_DeferredPointLight.lightColor.g * num8;
				vector2.z = pE_DeferredPointLight.lightColor.b * num8;
				Vector4 vector3 = new Vector4(vector.x, vector.y, vector.z, 1f / num7);
				Vector3 vector4 = num7 * GetComponent<Camera>().transform.up;
				Vector3 vector5 = num7 * GetComponent<Camera>().transform.right;
				for (int j = 0; j < 4; j++)
				{
					m_meshLightColors[num4 + j] = vector2;
					m_meshLightCenterAndInverseRadius[num4 + j] = vector3;
				}
				m_meshPositions[num4] = vector + 1f * vector4 + 1f * vector5;
				m_meshPositions[num4 + 1] = vector + 1f * vector4 + -1f * vector5;
				m_meshPositions[num4 + 2] = vector + -1f * vector4 + -1f * vector5;
				m_meshPositions[num4 + 3] = vector + -1f * vector4 + 1f * vector5;
				num4 += 4;
			}
		}
		mesh.vertices = m_meshPositions;
		mesh.normals = m_meshLightColors;
		mesh.tangents = m_meshLightCenterAndInverseRadius;
		mesh.triangles = m_meshTriangles;
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (null == m_mesh)
		{
			Graphics.Blit(source, destination);
			return;
		}
		RenderTexture active = RenderTexture.active;
		Graphics.SetRenderTarget(m_fogLightRenderTexture.colorBuffer, m_fogLightRenderTexture.depthBuffer);
		GL.Clear(clearDepth: false, clearColor: true, Color.black);
		m_deferredLightMaterial.SetPass(1);
		Graphics.DrawMeshNow(m_fogLightMesh, Vector3.zero, Quaternion.identity);
		Graphics.SetRenderTarget(source);
		m_deferredLightMaterial.SetPass(0);
		Graphics.DrawMeshNow(m_mesh, Vector3.zero, Quaternion.identity);
		RenderTexture.active = active;
		Graphics.Blit(source, destination);
	}

	private Vector4 ComputeHighPlaneRay(float x, float y, Plane highPlane)
	{
		Vector3 origin = GetComponent<Camera>().ScreenToWorldPoint(new Vector3(x, y, GetComponent<Camera>().nearClipPlane));
		Ray ray = new Ray(origin, m_cameraDirection);
		float enter = 0f;
		if (!highPlane.Raycast(ray, out enter) && enter == 0f)
		{
			Debug.LogError("PE_DeferredLightPass::ComputeHighPlaneRay() - failed to find intersection with high plane");
		}
		return new Vector4(origin.x, origin.y, origin.z, enter);
	}

	private void ComputeHighPlaneRays(LevelInfo levelInfo)
	{
		Vector3 vector = GetComponent<Camera>().ScreenToWorldPoint(new Vector3(0f, 0f, 0f));
		Vector3 vector2 = GetComponent<Camera>().ScreenToWorldPoint(new Vector3(0f, 0f, 100f));
		m_cameraDirection = vector2 - vector;
		m_cameraDirection.Normalize();
		m_cameraPosition = vector;
		Vector3 inNormal = new Vector3(0f, 1f, 0f);
		Plane highPlane = new Plane(inNormal, 0f - levelInfo.m_HeightRange_Max);
		float width = GetComponent<Camera>().pixelRect.width;
		float height = GetComponent<Camera>().pixelRect.height;
		m_highPlaneRay00 = ComputeHighPlaneRay(0f, 0f, highPlane);
		m_highPlaneRay10 = ComputeHighPlaneRay(width, 0f, highPlane);
		m_highPlaneRay01 = ComputeHighPlaneRay(0f, height, highPlane);
		m_highPlaneRay11 = ComputeHighPlaneRay(width, height, highPlane);
		PE_GameRender.s_cameraFrustumRay00 = m_highPlaneRay00;
		PE_GameRender.s_cameraFrustumRay10 = m_highPlaneRay10;
		PE_GameRender.s_cameraFrustumRay01 = m_highPlaneRay01;
		PE_GameRender.s_cameraFrustumRay11 = m_highPlaneRay11;
		PE_GameRender.s_cameraDirection = m_cameraDirection;
	}

	private void DebugDrawHighPlaneRay(Vector4 highPlaneRay, Color drawColor)
	{
		Vector3 vector = new Vector3(highPlaneRay.x, highPlaneRay.y, highPlaneRay.z);
		Vector3 end = vector + highPlaneRay.w * m_cameraDirection;
		Debug.DrawLine(vector, end, drawColor);
	}

	private void DebugDrawHighPlaneRays()
	{
		if (debugShowHighPlaneRays)
		{
			DebugDrawHighPlaneRay(m_highPlaneRay00, Color.blue);
			DebugDrawHighPlaneRay(m_highPlaneRay10, Color.blue);
			DebugDrawHighPlaneRay(m_highPlaneRay01, Color.blue);
			DebugDrawHighPlaneRay(m_highPlaneRay11, Color.blue);
		}
	}

	private void DebugDrawInterpolatdHighPlaneRay()
	{
		if (debugShowInterpolatedHighPlaneRay)
		{
			Vector4 frustumRayForScreenCoords = GetFrustumRayForScreenCoords(debugInterpolateScreenCoords.x, debugInterpolateScreenCoords.y);
			DebugDrawHighPlaneRay(frustumRayForScreenCoords, Color.magenta);
		}
	}

	private void DebugDrawFrustumRays(float x, float y)
	{
		Vector3 position = new Vector3(x, y, GetComponent<Camera>().nearClipPlane);
		Vector3 position2 = new Vector3(x, y, GetComponent<Camera>().farClipPlane);
		Vector3 start = GetComponent<Camera>().ScreenToWorldPoint(position);
		Vector3 end = GetComponent<Camera>().ScreenToWorldPoint(position2);
		Debug.DrawLine(start, end, Color.green);
	}

	private void DebugDrawFrustumRays()
	{
		if (debugShowFrustumRays)
		{
			float width = GetComponent<Camera>().pixelRect.width;
			float height = GetComponent<Camera>().pixelRect.height;
			DebugDrawFrustumRays(0f, 0f);
			DebugDrawFrustumRays(width, 0f);
			DebugDrawFrustumRays(width, height);
			DebugDrawFrustumRays(0f, height);
		}
	}
}
