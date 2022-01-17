using UnityEngine;

public class PE_DirectionalLight : MonoBehaviour
{
	public enum DirectionLightType
	{
		Main,
		Child,
		None
	}

	public DirectionLightType LightType;

	private Quaternion m_cachedRotation;

	private PE_DirectionalLight m_cachedParent;

	private void Start()
	{
		Light component = GetComponent<Light>();
		if ((bool)component)
		{
			component.intensity /= 2f;
		}
	}

	public void Update()
	{
		if (LightType != DirectionLightType.Child)
		{
			return;
		}
		if (m_cachedParent == null)
		{
			PE_DirectionalLight[] array = (PE_DirectionalLight[])Object.FindObjectsOfType(typeof(PE_DirectionalLight));
			foreach (PE_DirectionalLight pE_DirectionalLight in array)
			{
				if (pE_DirectionalLight.LightType == DirectionLightType.Main)
				{
					m_cachedParent = pE_DirectionalLight;
					break;
				}
			}
			if (null == m_cachedParent)
			{
				Debug.LogError("PE_DirectionalLight::Update() - failed to find main directional light in scene");
				LightType = DirectionLightType.None;
				return;
			}
		}
		if (m_cachedRotation != m_cachedParent.transform.rotation)
		{
			base.transform.rotation = m_cachedParent.transform.rotation;
			Quaternion quaternion = Quaternion.AngleAxis(180f, Vector3.up);
			base.transform.rotation = quaternion * base.transform.rotation;
			m_cachedRotation = m_cachedParent.transform.rotation;
		}
		GetComponent<Light>().intensity = m_cachedParent.GetComponent<Light>().intensity * 0.8f;
	}
}
