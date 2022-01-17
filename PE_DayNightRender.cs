using UnityEngine;

public class PE_DayNightRender : MonoBehaviour
{
	public float twentyFourHourTime = 12f;

	public float duskDawnTransitionTime = 1f;

	public GameObject colorGradeObjectDay;

	public GameObject colorGradeObjectDusk;

	public GameObject colorGradeObjectNight;

	public GameObject colorGradeObjectDawn;

	public Color lightColorDay;

	public Color lightColorDusk;

	public Color lightColorNight;

	public Color lightColorDawn;

	public float lightIntensityDay = 1.6f;

	public float lightIntensityDusk = 1f;

	public float lightIntensityNight = 1.2f;

	public float lightIntensityDawn = 1f;

	private PE_ColorGradeLUT colorGradeDay;

	private PE_ColorGradeLUT colorGradeDusk;

	private PE_ColorGradeLUT colorGradeNight;

	private PE_ColorGradeLUT colorGradeDawn;

	private Vector4 m_controlParameters;

	private Light m_directionalLight;

	private Color currentTintColor;

	private Color m_currentLightColor;

	private Vector3 m_currentLightColorHSV;

	private float m_currentLightIntensity = 0.8f;

	public bool IsValid()
	{
		if (null != colorGradeObjectDay && null != colorGradeObjectDusk && null != colorGradeObjectNight && null != colorGradeObjectDawn)
		{
			return true;
		}
		return false;
	}

	public void Start()
	{
		WorldTime.Instance.SetDayNightCycle(this);
		if (null != colorGradeObjectDay)
		{
			colorGradeDay = colorGradeObjectDay.GetComponent<PE_ColorGradeLUT>();
			colorGradeDay.Initialize();
		}
		if (null != colorGradeObjectDusk)
		{
			colorGradeDusk = colorGradeObjectDusk.GetComponent<PE_ColorGradeLUT>();
			colorGradeDusk.Initialize();
		}
		if (null != colorGradeObjectNight)
		{
			colorGradeNight = colorGradeObjectNight.GetComponent<PE_ColorGradeLUT>();
			colorGradeNight.Initialize();
		}
		if (null != colorGradeObjectDawn)
		{
			colorGradeDawn = colorGradeObjectDawn.GetComponent<PE_ColorGradeLUT>();
			colorGradeDawn.Initialize();
		}
		m_controlParameters = default(Vector4);
		m_directionalLight = PE_GameRender.FindSceneDirectionalLight();
		if (null == m_directionalLight)
		{
			Debug.LogError("PE_DayNightRender::Start() - failed to find directional light in scene");
		}
	}

	private void OnDestroy()
	{
		if ((bool)WorldTime.Instance)
		{
			WorldTime.Instance.UnsetDayNightCycle(this);
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void DoUpdate(float timeDelta)
	{
		ApplyShaderSettings();
	}

	public void ApplyShaderSettings()
	{
		twentyFourHourTime = Mathf.Repeat(twentyFourHourTime, 24f);
		float num = 0f;
		Texture3D texture3D = null;
		Texture3D texture3D2 = null;
		Color white = Color.white;
		Color white2 = Color.white;
		float num2 = lightIntensityDay;
		float num3 = lightIntensityDay;
		duskDawnTransitionTime = Mathf.Min(duskDawnTransitionTime, 2f);
		float num4 = duskDawnTransitionTime * 0.5f;
		if (twentyFourHourTime < 6f - num4)
		{
			texture3D = colorGradeNight.converted3DLut;
			texture3D2 = colorGradeNight.converted3DLut;
			white = lightColorNight;
			white2 = lightColorNight;
			num2 = lightIntensityNight;
			num3 = lightIntensityNight;
			num = 0f;
		}
		else if (twentyFourHourTime < 6f)
		{
			texture3D = colorGradeNight.converted3DLut;
			texture3D2 = colorGradeDawn.converted3DLut;
			white = lightColorNight;
			white2 = lightColorDawn;
			num2 = lightIntensityNight;
			num3 = lightIntensityDawn;
			num = Mathf.Clamp01((twentyFourHourTime - (6f - num4)) / num4);
		}
		else if (twentyFourHourTime < 6f + num4)
		{
			texture3D = colorGradeDawn.converted3DLut;
			texture3D2 = colorGradeDay.converted3DLut;
			white = lightColorDawn;
			white2 = lightColorDay;
			num2 = lightIntensityDawn;
			num3 = lightIntensityDay;
			num = Mathf.Clamp01((twentyFourHourTime - 6f) / num4);
		}
		else if (twentyFourHourTime < 18f - num4)
		{
			texture3D = colorGradeDay.converted3DLut;
			texture3D2 = colorGradeDay.converted3DLut;
			white = lightColorDay;
			white2 = lightColorDay;
			num2 = lightIntensityDay;
			num3 = lightIntensityDay;
			num = 0f;
		}
		else if (twentyFourHourTime < 18f)
		{
			texture3D = colorGradeDay.converted3DLut;
			texture3D2 = colorGradeDusk.converted3DLut;
			white = lightColorDay;
			white2 = lightColorDusk;
			num2 = lightIntensityDay;
			num3 = lightIntensityDusk;
			num = Mathf.Clamp01((twentyFourHourTime - (18f - num4)) / num4);
		}
		else if (twentyFourHourTime < 18f + num4)
		{
			texture3D = colorGradeDusk.converted3DLut;
			texture3D2 = colorGradeNight.converted3DLut;
			white = lightColorDusk;
			white2 = lightColorNight;
			num2 = lightIntensityDusk;
			num3 = lightIntensityNight;
			num = Mathf.Clamp01((twentyFourHourTime - 18f) / num4);
		}
		else
		{
			texture3D = colorGradeNight.converted3DLut;
			texture3D2 = colorGradeNight.converted3DLut;
			white = lightColorNight;
			white2 = lightColorNight;
			num2 = lightIntensityNight;
			num3 = lightIntensityNight;
			num = 0f;
		}
		int width = texture3D.width;
		texture3D.wrapMode = TextureWrapMode.Clamp;
		texture3D2.wrapMode = TextureWrapMode.Clamp;
		m_controlParameters.x = (float)(width - 1) / (1f * (float)width);
		m_controlParameters.y = 1f / (2f * (float)width);
		m_controlParameters.z = num;
		m_controlParameters.w = 0f;
		currentTintColor = Color.Lerp(white, white2, num);
		currentTintColor.a = 1f;
		Shader.SetGlobalVector("Trenton_DayNight_Parameters", m_controlParameters);
		Shader.SetGlobalVector("Trenton_DayNight_TintColor", currentTintColor);
		Shader.SetGlobalTexture("Trenton_DayNight_ColorLut0", texture3D);
		Shader.SetGlobalTexture("Trenton_DayNight_ColorLut1", texture3D2);
		m_currentLightIntensity = Mathf.Lerp(num2, num3, num);
		PE_ColorSpace.RGBtoHSV(currentTintColor, out m_currentLightColorHSV);
		m_currentLightColorHSV.z = m_currentLightIntensity;
		PE_ColorSpace.HSVtoRGB(m_currentLightColorHSV, out m_currentLightColor);
		if (m_directionalLight != null)
		{
			m_directionalLight.color = m_currentLightColor;
		}
	}

	public static void ApplyIdentityShaderSettings(Texture3D identityLut)
	{
		int width = identityLut.width;
		identityLut.wrapMode = TextureWrapMode.Clamp;
		Vector4 value = default(Vector4);
		value.x = (float)(width - 1) / (1f * (float)width);
		value.y = 1f / (2f * (float)width);
		value.z = 0f;
		value.w = 0f;
		Shader.SetGlobalVector("Trenton_DayNight_Parameters", value);
		Shader.SetGlobalVector("Trenton_DayNight_TintColor", Color.white);
		Shader.SetGlobalTexture("Trenton_DayNight_ColorLut0", identityLut);
		Shader.SetGlobalTexture("Trenton_DayNight_ColorLut1", identityLut);
	}
}
