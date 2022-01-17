using UnityEngine;

public class AlphaControl : MonoBehaviour
{
	[Tooltip("The object's current alpha.")]
	[Range(0f, 1f)]
	public float Alpha = 1f;

	[Tooltip("The object's maximum alpha.")]
	[Range(0f, 1f)]
	public float MaxAlpha = 1f;

	private float m_AlphaCached = 1f;

	private bool m_fading;

	private float m_desiredAlpha;

	private float m_previousAlpha;

	private float m_alphaTotalTime;

	private float m_alphaTimer;

	private bool m_controlLocked;

	private CharacterStats m_CharacterStats;

	private void Start()
	{
		Ghost component = GetComponent<Ghost>();
		if ((bool)component)
		{
			MaxAlpha = component.GhostColor.a;
		}
		m_CharacterStats = GetComponent<CharacterStats>();
		m_AlphaCached = Alpha;
		SetAlphaValues();
	}

	private void Update()
	{
		UpdateFade();
		if (m_AlphaCached != Alpha)
		{
			SetAlphaValues();
			m_AlphaCached = Alpha;
		}
	}

	private void UpdateFade()
	{
		if (m_fading)
		{
			if (m_alphaTimer > 0f)
			{
				m_alphaTimer -= Time.unscaledDeltaTime;
				float t = Mathf.SmoothStep(0f, 1f, 1f - m_alphaTimer / m_alphaTotalTime);
				Alpha = Mathf.Lerp(m_previousAlpha, m_desiredAlpha, t);
			}
			else
			{
				Alpha = m_desiredAlpha;
				m_fading = false;
			}
		}
	}

	public void FadeOut(float time)
	{
		FadeTo(0f, time);
	}

	public void FadeIn(float time)
	{
		FadeTo(1f, time);
	}

	public void FadeTo(float desiredAlpha, float time)
	{
		if (!m_controlLocked)
		{
			m_fading = true;
			m_alphaTimer = (m_alphaTotalTime = time);
			m_previousAlpha = Alpha;
			m_desiredAlpha = desiredAlpha;
		}
	}

	public void LockAlphaControl()
	{
		m_controlLocked = true;
	}

	public bool IsFadeValid()
	{
		if (!m_fading && Alpha != m_desiredAlpha)
		{
			return false;
		}
		return true;
	}

	public void Refresh()
	{
		SetAlphaValues();
	}

	public void TempEnableRenderers()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		if (componentsInChildren == null)
		{
			return;
		}
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Item componentInParent = renderer.GetComponentInParent<Item>();
			if ((!(componentInParent != null) || componentInParent.Renders) && renderer.gameObject.layer != LayerUtility.InGameUILayer)
			{
				renderer.enabled = true;
			}
		}
	}

	private void SetAlphaValues()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		if (componentsInChildren != null)
		{
			foreach (Renderer renderer in componentsInChildren)
			{
				if (Alpha <= 0f)
				{
					if (renderer.gameObject.layer != LayerUtility.InGameUILayer)
					{
						renderer.enabled = false;
					}
					continue;
				}
				Item componentInParent = renderer.GetComponentInParent<Item>();
				if (componentInParent != null && !componentInParent.Renders)
				{
					continue;
				}
				if (!renderer.enabled)
				{
					SkinnedMeshRenderer componentInParent2 = renderer.GetComponentInParent<SkinnedMeshRenderer>();
					if (componentInParent2 != null)
					{
						Cloth componentInParent3 = renderer.GetComponentInParent<Cloth>();
						ClothMesh componentInParent4 = GetComponentInParent<ClothMesh>();
						if (componentInParent3 != null && !componentInParent3.enabled)
						{
							SkinnedMeshRenderer component = componentInParent3.gameObject.GetComponent<SkinnedMeshRenderer>();
							if (component != null && component == componentInParent2)
							{
								bool flag = false;
								SkinnedMeshRenderer[] skinnedMeshes = componentInParent4.SkinnedMeshes;
								for (int j = 0; j < skinnedMeshes.Length; j++)
								{
									if (skinnedMeshes[j] == componentInParent2)
									{
										flag = true;
									}
								}
								if (flag)
								{
									continue;
								}
							}
						}
					}
					if (renderer.gameObject.layer != LayerUtility.InGameUILayer)
					{
						renderer.enabled = !m_CharacterStats || !m_CharacterStats.IsInvisible;
					}
				}
				Material[] sharedMaterials = renderer.sharedMaterials;
				foreach (Material material in sharedMaterials)
				{
					if ((bool)material && material.HasProperty("_Alpha"))
					{
						material.SetFloat("_Alpha", Alpha * MaxAlpha);
					}
				}
			}
		}
		ParticleSystem[] componentsInChildren2 = GetComponentsInChildren<ParticleSystem>();
		if (componentsInChildren2 == null)
		{
			return;
		}
		foreach (ParticleSystem particleSystem in componentsInChildren2)
		{
			Item componentInParent5 = particleSystem.GetComponent<Renderer>().GetComponentInParent<Item>();
			if (componentInParent5 != null && !componentInParent5.Renders)
			{
				continue;
			}
			particleSystem.GetComponent<Renderer>().enabled = Alpha > 0f;
			if (!(Alpha > 0f))
			{
				continue;
			}
			Material[] sharedMaterials = particleSystem.GetComponent<Renderer>().sharedMaterials;
			foreach (Material material2 in sharedMaterials)
			{
				if ((bool)material2 && material2.HasProperty("_Alpha"))
				{
					material2.SetFloat("_Alpha", Alpha * MaxAlpha);
				}
			}
		}
	}
}
