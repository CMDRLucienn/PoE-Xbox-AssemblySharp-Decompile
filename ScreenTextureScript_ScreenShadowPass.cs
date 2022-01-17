using UnityEngine;

public class ScreenTextureScript_ScreenShadowPass : ScreenTextureScript
{
	private const int NUM_BLUR_SAMPLES = 5;

	public bool enableBlur = true;

	public float blurKernelSizeInPixels = 4f;

	public float blurKernelGaussianStandardDeviation = 1.8f;

	public RenderTexture m_tempBuffer;

	private Material m_blurMaterial;

	private Vector4[] m_blurSampleOffset;

	private Vector4[] m_blurSampleWeight;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!enableBlur)
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (null == m_tempBuffer || m_tempBuffer.width != source.width || m_tempBuffer.height != source.height)
		{
			if (null != m_tempBuffer)
			{
				GameUtilities.Destroy(m_tempBuffer);
			}
			m_tempBuffer = new RenderTexture(source.width, source.height, 0, source.format, RenderTextureReadWrite.Default);
			m_tempBuffer.filterMode = FilterMode.Bilinear;
			m_tempBuffer.wrapMode = TextureWrapMode.Clamp;
			m_tempBuffer.useMipMap = false;
		}
		float num = blurKernelSizeInPixels / (float)source.width;
		float num2 = 0f;
		float mean = 0f;
		for (int i = 0; i < 5; i++)
		{
			float num3 = (float)i / 4f * 2f - 1f;
			float num4 = num3 * num;
			float num5 = PE_Math.GaussianDistribution(num3, mean, blurKernelGaussianStandardDeviation);
			int num6 = i / 4;
			switch (i % 4)
			{
			case 0:
				m_blurSampleOffset[num6].x = num4;
				m_blurSampleWeight[num6].x = num5;
				break;
			case 1:
				m_blurSampleOffset[num6].y = num4;
				m_blurSampleWeight[num6].y = num5;
				break;
			case 2:
				m_blurSampleOffset[num6].z = num4;
				m_blurSampleWeight[num6].z = num5;
				break;
			case 3:
				m_blurSampleOffset[num6].w = num4;
				m_blurSampleWeight[num6].w = num5;
				break;
			}
			num2 += num5;
		}
		for (int j = 0; j < 1; j++)
		{
			m_blurSampleWeight[j].x *= 1f / num2;
			m_blurSampleWeight[j].y *= 1f / num2;
			m_blurSampleWeight[j].z *= 1f / num2;
			m_blurSampleWeight[j].w *= 1f / num2;
		}
		m_blurMaterial.SetVector("SampleOffset0", m_blurSampleOffset[0]);
		m_blurMaterial.SetVector("SampleOffset1", m_blurSampleOffset[1]);
		m_blurMaterial.SetVector("SampleWeight0", m_blurSampleWeight[0]);
		m_blurMaterial.SetVector("SampleWeight1", m_blurSampleWeight[1]);
		m_blurMaterial.SetTexture("_MainTex", source);
		m_blurMaterial.SetPass(0);
		Graphics.Blit(source, m_tempBuffer, m_blurMaterial, 0);
		m_blurMaterial.SetTexture("_MainTex", m_tempBuffer);
		m_blurMaterial.SetPass(1);
		Graphics.Blit(m_tempBuffer, destination, m_blurMaterial, 1);
	}

	private void Start()
	{
		m_blurMaterial = new Material(Shader.Find("Trenton/PE_ScreenShadowBlur"));
		m_blurSampleOffset = new Vector4[2];
		m_blurSampleWeight = new Vector4[2];
		for (int i = 0; i < 2; i++)
		{
			m_blurSampleOffset[i] = new Vector4(0f, 0f, 0f, 0f);
			m_blurSampleWeight[i] = new Vector4(0f, 0f, 0f, 0f);
		}
		GetComponent<Camera>().SetReplacementShader(Shader.Find("Trenton/PE_DynamicShadowScreenPass"), string.Empty);
	}
}
