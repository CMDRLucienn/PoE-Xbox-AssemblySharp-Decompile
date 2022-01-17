using UnityEngine;

public class PE_DynamicShadowBuffer : MonoBehaviour
{
	private const int NUM_BLUR_SAMPLES = 5;

	public int shadowBufferSize = 1024;

	public float lightFrustumScale = 1.2f;

	public bool enableBlur = true;

	public float blurKernelSizeInPixels = 4f;

	public float blurKernelGaussianStandardDeviation = 1.8f;

	public bool useBoxFilter;

	public float shadowIntensity = 0.2f;

	public Color shadowColor = Color.white;

	public float shadowOverDarkeningFactor = 40f;

	public float lightBias = 0.01f;

	public float worldMaxHeight;

	public float worldMinHeight;

	public RenderTexture m_shadowBuffer;

	public RenderTexture m_tempBuffer;

	private Material m_blurMaterial;

	private Light m_directionalLight;

	private PE_StreamTileManager m_streamTileManager;

	private Vector3 m_groundPos00;

	private Vector3 m_groundPos10;

	private Vector3 m_groundPos01;

	private Vector3 m_groundPos11;

	private Vector3 m_groundVectorX;

	private Vector3 m_groundVectorY;

	private float m_targetGroundWidth;

	private float m_targetGroundHeight;

	private Vector4[] m_blurSampleOffsetsAndWeights;

	private Vector4 m_shadowControls = Vector4.zero;

	public void DoUpdate()
	{
		if (null != m_directionalLight && null != m_streamTileManager)
		{
			GetFrustumGroundCenterAndSpan(worldMinHeight, worldMaxHeight, out var center, out var spanWidth, out var spanHeight);
			Vector3 vector = m_directionalLight.gameObject.transform.TransformDirection(Vector3.forward);
			Vector3 worldPosition = base.transform.position + vector;
			GetComponent<Camera>().transform.LookAt(worldPosition, Vector3.up);
			GetComponent<Camera>().transform.position = center + -vector * 100f;
			UpdateFrustumGroundIntersectionPositions();
			m_groundVectorX = (m_groundPos10 - m_groundPos00).normalized;
			m_groundVectorY = (m_groundPos01 - m_groundPos00).normalized;
			if (Mathf.Abs(Vector3.Dot(spanWidth.normalized, m_groundVectorX.normalized)) > Mathf.Abs(Vector3.Dot(spanHeight.normalized, m_groundVectorX.normalized)))
			{
				m_targetGroundWidth = Mathf.Abs(Vector3.Dot(m_groundVectorX.normalized, spanWidth));
				m_targetGroundHeight = Mathf.Abs(Vector3.Dot(m_groundVectorY.normalized, spanHeight));
			}
			else
			{
				m_targetGroundWidth = Mathf.Abs(Vector3.Dot(m_groundVectorX.normalized, spanHeight));
				m_targetGroundHeight = Mathf.Abs(Vector3.Dot(m_groundVectorY.normalized, spanWidth));
			}
			m_targetGroundWidth *= lightFrustumScale;
			m_targetGroundHeight *= lightFrustumScale;
			float num = Mathf.Abs(Vector3.Dot(vector.normalized, Vector3.up));
			GetComponent<Camera>().orthographicSize = m_targetGroundHeight * 0.5f * num;
			GetComponent<Camera>().aspect = 1f / num * (m_targetGroundWidth / m_targetGroundHeight);
			Matrix4x4 value = GetComponent<Camera>().projectionMatrix * GetComponent<Camera>().worldToCameraMatrix;
			Shader.SetGlobalMatrix("Trenton_ShadowLightVP", value);
			m_shadowControls.x = shadowOverDarkeningFactor;
			m_shadowControls.y = lightBias;
			m_shadowControls.z = 1f / (float)shadowBufferSize;
			m_shadowControls.w = shadowIntensity;
			Shader.SetGlobalVector("Trenton_ShadowControls", m_shadowControls);
			Shader.SetGlobalColor("Trenton_ShadowColor", shadowColor);
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!enableBlur)
		{
			Graphics.Blit(source, destination);
			return;
		}
		float num = blurKernelSizeInPixels / (float)m_shadowBuffer.width;
		float num2 = 0f;
		float mean = 0f;
		if (useBoxFilter)
		{
			for (int i = 0; i < 5; i++)
			{
				float num3 = (float)i / 4f * 2f - 1f;
				m_blurSampleOffsetsAndWeights[i].x = num3 * num;
				m_blurSampleOffsetsAndWeights[i].w = 0.2f;
			}
		}
		else
		{
			for (int j = 0; j < 5; j++)
			{
				float num4 = (float)j / 4f * 2f - 1f;
				m_blurSampleOffsetsAndWeights[j].x = num4 * num;
				float w = PE_Math.GaussianDistribution(num4, mean, blurKernelGaussianStandardDeviation);
				m_blurSampleOffsetsAndWeights[j].w = w;
				num2 += m_blurSampleOffsetsAndWeights[j].w;
			}
			for (int k = 0; k < 5; k++)
			{
				m_blurSampleOffsetsAndWeights[k] *= 1f / num2;
			}
		}
		m_blurMaterial.SetVector("SampleOffsetAndWeight0", m_blurSampleOffsetsAndWeights[0]);
		m_blurMaterial.SetVector("SampleOffsetAndWeight1", m_blurSampleOffsetsAndWeights[1]);
		m_blurMaterial.SetVector("SampleOffsetAndWeight2", m_blurSampleOffsetsAndWeights[2]);
		m_blurMaterial.SetVector("SampleOffsetAndWeight3", m_blurSampleOffsetsAndWeights[3]);
		m_blurMaterial.SetVector("SampleOffsetAndWeight4", m_blurSampleOffsetsAndWeights[4]);
		m_blurMaterial.SetTexture("_MainTex", source);
		m_blurMaterial.SetPass(0);
		Graphics.Blit(source, m_tempBuffer, m_blurMaterial, 0);
		m_blurMaterial.SetTexture("_MainTex", m_tempBuffer);
		m_blurMaterial.SetPass(1);
		Graphics.Blit(m_tempBuffer, destination, m_blurMaterial, 1);
	}

	private void Start()
	{
		RenderTextureFormat format = RenderTextureFormat.RGHalf;
		int width = shadowBufferSize;
		int height = shadowBufferSize;
		m_shadowBuffer = new RenderTexture(width, height, 24, format, RenderTextureReadWrite.Default);
		m_shadowBuffer.filterMode = FilterMode.Bilinear;
		m_shadowBuffer.wrapMode = TextureWrapMode.Clamp;
		m_shadowBuffer.useMipMap = false;
		m_shadowBuffer.SetGlobalShaderProperty("Trenton_ShadowBuffer");
		m_tempBuffer = new RenderTexture(width, height, 0, format, RenderTextureReadWrite.Default);
		m_tempBuffer.filterMode = FilterMode.Bilinear;
		m_tempBuffer.wrapMode = TextureWrapMode.Clamp;
		m_tempBuffer.useMipMap = false;
		m_blurMaterial = new Material(Shader.Find("Trenton/PE_ESMShadowBlur"));
		m_blurSampleOffsetsAndWeights = new Vector4[5];
		for (int i = 0; i < 5; i++)
		{
			m_blurSampleOffsetsAndWeights[i] = new Vector4(0f, 0f, 0f, 0f);
		}
		GetComponent<Camera>().enabled = true;
		GetComponent<Camera>().targetTexture = m_shadowBuffer;
		m_directionalLight = PE_GameRender.FindSceneDirectionalLight();
		if (null == m_directionalLight)
		{
			Debug.LogWarning("PE_DynamicShadowBuffer::Start() - failed to find directional light in scene");
		}
		m_streamTileManager = PE_StreamTileManager.Instance;
		if (null == m_streamTileManager)
		{
			Debug.LogError("PE_DynamicShadowBuffer::Start() - failed to find stream tile manager in scene");
		}
		GetComponent<Camera>().SetReplacementShader(Shader.Find("Trenton/PE_DynamicShadowDepth"), string.Empty);
	}

	private Vector3 GetGroundPlaneRayIntersectionPosition(Plane groundPlane, Ray theRay)
	{
		float enter = 0f;
		if (!groundPlane.Raycast(theRay, out enter) && enter == 0f)
		{
			Debug.LogError("PE_StreamTileManager::GetGroundPlaneRayIntersectionPosition() - failed to find intersection with high plane");
			return Vector3.zero;
		}
		return theRay.origin + theRay.direction * enter;
	}

	private void UpdateFrustumGroundIntersectionPositions()
	{
		Camera component = GetComponent<Camera>();
		Ray theRay = component.ScreenPointToRay(Vector3.zero);
		Ray theRay2 = component.ScreenPointToRay(new Vector3(component.pixelWidth, 0f, 0f));
		Ray theRay3 = component.ScreenPointToRay(new Vector3(0f, component.pixelHeight, 0f));
		Ray theRay4 = component.ScreenPointToRay(new Vector3(component.pixelWidth, component.pixelHeight, 0f));
		Vector3 inNormal = new Vector3(0f, 1f, 0f);
		float d = 0f - worldMaxHeight;
		Plane groundPlane = new Plane(inNormal, d);
		m_groundPos00 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay);
		m_groundPos10 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay2);
		m_groundPos01 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay3);
		m_groundPos11 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay4);
	}

	private void GetFrustumGroundCenterAndSpan(float minWorldHeight, float maxWorldHeight, out Vector3 center, out Vector3 spanWidth, out Vector3 spanHeight)
	{
		Camera main = Camera.main;
		Ray theRay = main.ScreenPointToRay(Vector3.zero);
		Ray theRay2 = main.ScreenPointToRay(new Vector3(main.pixelWidth, 0f, 0f));
		Ray theRay3 = main.ScreenPointToRay(new Vector3(0f, main.pixelHeight, 0f));
		Ray theRay4 = main.ScreenPointToRay(new Vector3(main.pixelWidth, main.pixelHeight, 0f));
		Vector3 inNormal = new Vector3(0f, 1f, 0f);
		Plane groundPlane = new Plane(inNormal, 0f - maxWorldHeight);
		Vector3 groundPlaneRayIntersectionPosition = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay);
		Vector3 groundPlaneRayIntersectionPosition2 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay2);
		groundPlane = new Plane(inNormal, 0f - minWorldHeight);
		Vector3 groundPlaneRayIntersectionPosition3 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay3);
		Vector3 groundPlaneRayIntersectionPosition4 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay4);
		groundPlaneRayIntersectionPosition3.y = maxWorldHeight;
		groundPlaneRayIntersectionPosition4.y = maxWorldHeight;
		Vector3 a = Vector3.Lerp(groundPlaneRayIntersectionPosition, groundPlaneRayIntersectionPosition2, 0.5f);
		Vector3 b = Vector3.Lerp(groundPlaneRayIntersectionPosition3, groundPlaneRayIntersectionPosition4, 0.5f);
		center = Vector3.Lerp(a, b, 0.5f);
		spanWidth = groundPlaneRayIntersectionPosition4 - groundPlaneRayIntersectionPosition;
		spanHeight = groundPlaneRayIntersectionPosition3 - groundPlaneRayIntersectionPosition2;
	}

	private void OnDrawGizmos()
	{
		if (null != m_directionalLight && null != m_streamTileManager)
		{
			Camera main = Camera.main;
			Ray theRay = main.ScreenPointToRay(Vector3.zero);
			Ray theRay2 = main.ScreenPointToRay(new Vector3(main.pixelWidth, 0f, 0f));
			Ray theRay3 = main.ScreenPointToRay(new Vector3(0f, main.pixelHeight, 0f));
			Ray theRay4 = main.ScreenPointToRay(new Vector3(main.pixelWidth, main.pixelHeight, 0f));
			Vector3 inNormal = new Vector3(0f, 1f, 0f);
			float d = 0f - worldMaxHeight;
			Plane groundPlane = new Plane(inNormal, d);
			Vector3 groundPlaneRayIntersectionPosition = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay);
			Vector3 groundPlaneRayIntersectionPosition2 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay2);
			Vector3 groundPlaneRayIntersectionPosition3 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay3);
			Vector3 groundPlaneRayIntersectionPosition4 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay4);
			Vector3 vector = groundPlaneRayIntersectionPosition;
			Vector3 vector2 = groundPlaneRayIntersectionPosition2;
			Vector3 vector3 = groundPlaneRayIntersectionPosition3;
			Vector3 vector4 = groundPlaneRayIntersectionPosition4;
			Color blue = Color.blue;
			Debug.DrawLine(vector, vector2, blue);
			Debug.DrawLine(vector2, vector4, blue);
			Debug.DrawLine(vector4, vector3, blue);
			Debug.DrawLine(vector3, vector, blue);
			d = 0f - worldMinHeight;
			groundPlane = new Plane(inNormal, d);
			groundPlaneRayIntersectionPosition = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay);
			groundPlaneRayIntersectionPosition2 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay2);
			groundPlaneRayIntersectionPosition3 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay3);
			Vector3 groundPlaneRayIntersectionPosition5 = GetGroundPlaneRayIntersectionPosition(groundPlane, theRay4);
			Vector3 vector5 = groundPlaneRayIntersectionPosition;
			Vector3 vector6 = groundPlaneRayIntersectionPosition2;
			Vector3 vector7 = groundPlaneRayIntersectionPosition3;
			Vector3 vector8 = groundPlaneRayIntersectionPosition5;
			blue = Color.cyan;
			Debug.DrawLine(vector5, vector6, blue);
			Debug.DrawLine(vector6, vector8, blue);
			Debug.DrawLine(vector8, vector7, blue);
			Debug.DrawLine(vector7, vector5, blue);
			vector7.y = worldMaxHeight;
			vector8.y = worldMaxHeight;
			Vector3 a = Vector3.Lerp(vector, vector2, 0.5f);
			Vector3 b = Vector3.Lerp(vector7, vector8, 0.5f);
			Vector3 vector9 = Vector3.Lerp(a, b, 0.5f);
			Vector3 vector10 = vector8 - vector;
			Vector3 vector11 = vector7 - vector2;
			Debug.DrawLine(vector9, vector9 + vector10, Color.red);
			Debug.DrawLine(vector9, vector9 + vector11, Color.green);
			blue = Color.yellow;
			Debug.DrawLine(m_groundPos00, m_groundPos10, blue);
			Debug.DrawLine(m_groundPos10, m_groundPos11, blue);
			Debug.DrawLine(m_groundPos11, m_groundPos01, blue);
			Debug.DrawLine(m_groundPos01, m_groundPos00, blue);
			Debug.DrawLine(vector9, vector9 + m_groundVectorX.normalized * m_targetGroundWidth * 0.5f, Color.red);
			Debug.DrawLine(vector9, vector9 + m_groundVectorY.normalized * m_targetGroundHeight * 0.5f, Color.green);
		}
	}

	private void DrawDynamicShadows()
	{
	}
}
