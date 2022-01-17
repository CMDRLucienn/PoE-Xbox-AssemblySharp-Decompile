using System;
using UnityEngine;

public class PE_PostProcessColorLevels : MonoBehaviour
{
	public Shader curShader;

	[Range(0f, 10f)]
	public float brightnessAmount = 1f;

	[Range(0f, 10f)]
	public float saturationAmount = 1f;

	[Range(0f, 10f)]
	public float contrastAmount = 1f;

	[Range(0f, 255f)]
	public float inRedBlack;

	[Range(0.1f, 10f)]
	public float inRedGamma = 1f;

	[Range(0f, 255f)]
	public float inRedWhite = 255f;

	[Range(0f, 255f)]
	public float inGreenBlack;

	[Range(0.1f, 10f)]
	public float inGreenGamma = 1f;

	[Range(0f, 255f)]
	public float inGreenWhite = 255f;

	[Range(0f, 255f)]
	public float inBlueBlack;

	[Range(0.1f, 10f)]
	public float inBlueGamma = 1f;

	[Range(0f, 255f)]
	public float inBlueWhite = 255f;

	public float LerpTime;

	private float m_lerpAmount;

	private bool m_lerpOn = true;

	[HideInInspector]
	public float MaxLerpValue = 1f;

	private Material curMaterial;

	private Material material
	{
		get
		{
			if (curMaterial == null)
			{
				curMaterial = new Material(curShader);
				curMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return curMaterial;
		}
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
		else
		{
			curShader = Shader.Find("Trenton/PE_ColorLevels");
		}
	}

	private void OnEnable()
	{
		m_lerpAmount = 0f;
		m_lerpOn = true;
	}

	public void FadeOut()
	{
		m_lerpOn = false;
	}

	public void FadeIn()
	{
		m_lerpOn = true;
	}

	public bool IsFadeComplete()
	{
		if (m_lerpOn)
		{
			return m_lerpAmount == MaxLerpValue;
		}
		return m_lerpAmount == 0f;
	}

	private void Update()
	{
		if (LerpTime == 0f)
		{
			m_lerpAmount = (m_lerpOn ? MaxLerpValue : 0f);
		}
		else
		{
			m_lerpAmount = (m_lerpOn ? Math.Min(MaxLerpValue, m_lerpAmount + Time.unscaledDeltaTime / LerpTime) : Math.Max(0f, m_lerpAmount - Time.unscaledDeltaTime / LerpTime));
		}
	}

	private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if (curShader != null)
		{
			material.SetFloat("_BrightnessAmount", Mathf.Lerp(1f, brightnessAmount, m_lerpAmount));
			material.SetFloat("_SaturationAmount", Mathf.Lerp(1f, saturationAmount, m_lerpAmount));
			material.SetFloat("_ContrastAmount", Mathf.Lerp(1f, contrastAmount, m_lerpAmount));
			material.SetFloat("_InRedBlack", Mathf.Lerp(0f, inRedBlack, m_lerpAmount));
			material.SetFloat("_InRedGamma", Mathf.Lerp(1f, inRedGamma, m_lerpAmount));
			material.SetFloat("_InRedWhite", Mathf.Lerp(255f, inRedWhite, m_lerpAmount));
			material.SetFloat("_InGreenBlack", Mathf.Lerp(0f, inGreenBlack, m_lerpAmount));
			material.SetFloat("_InGreenGamma", Mathf.Lerp(1f, inGreenGamma, m_lerpAmount));
			material.SetFloat("_InGreenWhite", Mathf.Lerp(255f, inGreenWhite, m_lerpAmount));
			material.SetFloat("_InBlueBlack", Mathf.Lerp(0f, inBlueBlack, m_lerpAmount));
			material.SetFloat("_InBlueGamma", Mathf.Lerp(1f, inBlueGamma, m_lerpAmount));
			material.SetFloat("_InBlueWhite", Mathf.Lerp(255f, inBlueWhite, m_lerpAmount));
			Graphics.Blit(sourceTexture, destTexture, material);
		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);
		}
	}

	private void OnDisable()
	{
		if ((bool)curMaterial)
		{
			GameUtilities.DestroyImmediate(curMaterial);
		}
	}
}
