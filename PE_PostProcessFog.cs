using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PE_PostProcessFog : MonoBehaviour
{
	public enum FogType
	{
		None,
		HeightAndDepth,
		Height,
		Depth
	}

	public FogType fogType;

	public Color depthFogColor = Color.grey;

	public float depthFogDensityFalloff = 1f;

	public float depthFogDensityScale = 1f;

	public float startDepth = 100f;

	public float depthRange = 20f;

	public Color verticalFogColor = Color.grey;

	public float verticalFogDensityFalloff = 1f;

	public float verticalFogDensityScale = 1f;

	public float heightScale = 100f;

	public float height;

	public float noiseVelocity = 0.1f;

	public float noiseScale = 1f;

	public float noise0_relativeScale = 1f;

	public float noise0_relativeVelocity = 1f;

	public float noise0_direction_degrees;

	public float noise1_relativeScale = 1f;

	public float noise1_relativeVelocity = 0.9f;

	public float noise1_direction_degrees = 10f;

	public float noise2_relativeScale = 1f;

	public float noise2_relativeVelocity = 0.8f;

	public float noise2_direction_degrees = 30f;

	public Texture2D noiseTexture;

	public float noiseContrast = 1f;

	public float noiseBrightness;

	public float litFogIntensity = 1f;

	private Material fogMaterial;

	private Vector4 m_uvOffset0 = Vector4.zero;

	private Vector4 m_uvOffset1 = Vector4.zero;

	private void Start()
	{
		fogMaterial = new Material(Shader.Find("Trenton/PE_GlobalFog"));
		Camera component = GetComponent<Camera>();
		if ((bool)component)
		{
			component.depth = 79f;
		}
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		Vector2 vectorFromAngle = GetVectorFromAngle(noise0_direction_degrees);
		Vector2 vectorFromAngle2 = GetVectorFromAngle(noise0_direction_degrees);
		Vector2 vectorFromAngle3 = GetVectorFromAngle(noise0_direction_degrees);
		float num = noise0_relativeVelocity * noiseVelocity * noiseScale;
		float num2 = noise1_relativeVelocity * noiseVelocity * noiseScale;
		float num3 = noise2_relativeVelocity * noiseVelocity * noiseScale;
		m_uvOffset0.x += deltaTime * num * vectorFromAngle.x;
		m_uvOffset0.y += deltaTime * num * vectorFromAngle.y;
		m_uvOffset0.z += deltaTime * num2 * vectorFromAngle2.x;
		m_uvOffset0.w += deltaTime * num2 * vectorFromAngle2.y;
		m_uvOffset1.x += deltaTime * num3 * vectorFromAngle3.x;
		m_uvOffset1.y += deltaTime * num3 * vectorFromAngle3.y;
		m_uvOffset0.x = Mathf.Repeat(m_uvOffset0.x, 1f);
		m_uvOffset0.y = Mathf.Repeat(m_uvOffset0.y, 1f);
		m_uvOffset0.z = Mathf.Repeat(m_uvOffset0.z, 1f);
		m_uvOffset0.w = Mathf.Repeat(m_uvOffset0.w, 1f);
		m_uvOffset1.x = Mathf.Repeat(m_uvOffset1.x, 1f);
		m_uvOffset1.y = Mathf.Repeat(m_uvOffset1.y, 1f);
		Shader.SetGlobalTexture("Trenton_NoiseTexture", noiseTexture);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (fogType == FogType.None)
		{
			Graphics.Blit(source, destination);
			return;
		}
		Vector4 value = new Vector4(verticalFogDensityFalloff * 0.01f, depthFogDensityFalloff * 0.01f, verticalFogDensityScale, depthFogDensityScale);
		fogMaterial.SetVector("_DensityControls", value);
		fogMaterial.SetVector("_Y", new Vector4(height, 1f / heightScale));
		fogMaterial.SetColor("_DepthFogColor", depthFogColor);
		Vector4 vector = new Vector4(verticalFogColor.r, verticalFogColor.g, verticalFogColor.b, litFogIntensity);
		fogMaterial.SetColor("_VerticalFogColor", vector);
		fogMaterial.SetTexture("_NoiseTex", noiseTexture);
		fogMaterial.SetVector("_NoiseUVScale", new Vector4(noiseScale * noise0_relativeScale, noiseScale * noise1_relativeScale, noiseScale * noise2_relativeScale, 0f));
		fogMaterial.SetVector("_NoiseUVOffset0", m_uvOffset0);
		fogMaterial.SetVector("_NoiseUVOffset1", m_uvOffset1);
		Vector4 value2 = new Vector4(noiseBrightness, noiseBrightness, noiseBrightness, noiseBrightness);
		fogMaterial.SetVector("_NoiseBrightness", value2);
		Vector4 value3 = new Vector4(noiseContrast, noiseContrast, noiseContrast, noiseContrast);
		fogMaterial.SetVector("_NoiseContrast", value3);
		PE_GameRender instance = PE_GameRender.Instance;
		PE_DeferredLightPass deferredLightPass = instance.GetDeferredLightPass();
		fogMaterial.SetTexture("_FogLightTex", deferredLightPass.m_fogLightRenderTexture);
		Vector3 cameraDirection = deferredLightPass.GetCameraDirection();
		PE_StreamTileManager streamTileManager = instance.GetStreamTileManager();
		Vector3 frustumGroundPos = streamTileManager.GetFrustumGroundPos00();
		Vector3 frustumGroundPos2 = streamTileManager.GetFrustumGroundPos10();
		Vector3 frustumGroundPos3 = streamTileManager.GetFrustumGroundPos01();
		Vector3 frustumGroundPos4 = streamTileManager.GetFrustumGroundPos11();
		Vector3 vector2 = Vector3.Lerp(frustumGroundPos, frustumGroundPos2, 0.5f);
		float magnitude = (Vector3.Lerp(frustumGroundPos3, frustumGroundPos4, 0.5f) - vector2).magnitude;
		float zoomLevel = instance.GetSyncCameraOrthoSettings().GetZoomLevel();
		magnitude /= zoomLevel;
		Vector4 value4 = new Vector4(0f, 1f / magnitude, 0f, 0f);
		fogMaterial.SetVector("_DistanceControls", value4);
		Vector2 vector3 = new Vector2(cameraDirection.x, cameraDirection.z);
		vector3.Normalize();
		Vector4 value5 = new Vector4(vector2.x, vector2.z, vector3.x, vector3.y);
		fogMaterial.SetVector("_CamPosAndDirection2D", value5);
		RenderTexture.active = destination;
		int pass = 0;
		switch (fogType)
		{
		case FogType.HeightAndDepth:
			pass = 0;
			break;
		case FogType.Height:
			pass = 1;
			break;
		case FogType.Depth:
			pass = 2;
			break;
		}
		Graphics.Blit(source, destination, fogMaterial, pass);
	}

	private Vector3 ComputeFrustumPosition(Vector4 frustumEdge, float cameraDepth)
	{
		return new Vector3(frustumEdge.x, frustumEdge.y, frustumEdge.z) + cameraDepth * PE_GameRender.s_cameraDirection;
	}

	private Vector2 GetVectorFromAngle(float degrees)
	{
		Vector2 result = default(Vector2);
		result.x = Mathf.Cos(degrees * ((float)Math.PI / 180f));
		result.y = Mathf.Sin(degrees * ((float)Math.PI / 180f));
		return result;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = depthFogColor;
		Vector3 vector = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, Camera.main.nearClipPlane));
		Vector3 vector2 = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0f, Camera.main.nearClipPlane));
		Vector3 vector3 = Camera.main.ScreenToWorldPoint(new Vector3(0f, Camera.main.pixelHeight, Camera.main.nearClipPlane));
		Vector3 vector4 = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, Camera.main.nearClipPlane));
		Vector3 forward = Camera.main.transform.forward;
		Vector3 vector5 = vector + forward * startDepth;
		Vector3 vector6 = vector2 + forward * startDepth;
		Vector3 vector7 = vector3 + forward * startDepth;
		Vector3 vector8 = vector4 + forward * startDepth;
		Gizmos.DrawLine(vector5, vector6);
		Gizmos.DrawLine(vector6, vector8);
		Gizmos.DrawLine(vector8, vector7);
		Gizmos.DrawLine(vector7, vector5);
		float num = startDepth + depthRange;
		Vector3 vector9 = vector + forward * num;
		Vector3 vector10 = vector2 + forward * num;
		Vector3 vector11 = vector3 + forward * num;
		Vector3 vector12 = vector4 + forward * num;
		Gizmos.DrawLine(vector9, vector10);
		Gizmos.DrawLine(vector10, vector12);
		Gizmos.DrawLine(vector12, vector11);
		Gizmos.DrawLine(vector11, vector9);
	}
}
