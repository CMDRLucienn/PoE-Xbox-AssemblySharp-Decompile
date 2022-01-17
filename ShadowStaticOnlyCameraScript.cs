using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ShadowStaticOnlyCameraScript : MonoBehaviour
{
	private struct LightShadowData
	{
		public Light light;

		public LightShadows light_shadows;
	}

	private GameObject[] m_obj_list;

	public RenderTexture m_ShadowTexture;

	private int m_screen_width;

	private int m_screen_height;

	private List<LightShadowData> m_ShadowLight_list;

	private LayerMask m_dynamics_layer_mask;

	public bool m_enable_static_receive_static_shadows;

	public RenderTexture GetShadowTexture()
	{
		return m_ShadowTexture;
	}

	private void CreateMaskTexture(int width, int height)
	{
		if (m_ShadowTexture == null)
		{
			m_ShadowTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		}
		else
		{
			if (m_ShadowTexture.IsCreated())
			{
				m_ShadowTexture.Release();
			}
			m_ShadowTexture.width = width;
			m_ShadowTexture.height = height;
		}
		m_screen_width = width;
		m_screen_height = height;
		m_ShadowTexture.filterMode = FilterMode.Point;
		m_ShadowTexture.wrapMode = TextureWrapMode.Clamp;
		m_ShadowTexture.useMipMap = false;
		m_ShadowTexture.SetGlobalShaderProperty("_ScreenSpaceShadowTex");
		if (GetComponent<Camera>() != null)
		{
			GetComponent<Camera>().targetTexture = m_ShadowTexture;
		}
	}

	private void Start()
	{
		CreateMaskTexture(Screen.width, Screen.height);
		m_ShadowLight_list = new List<LightShadowData>();
		m_dynamics_layer_mask = LayerMask.NameToLayer("Dynamics");
		CacheShadowLights();
	}

	private void Update()
	{
		int width = Screen.width;
		int height = Screen.height;
		if (width != m_screen_width || height != m_screen_height)
		{
			CreateMaskTexture(width, height);
		}
	}

	private void OnPreRender()
	{
		m_obj_list = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		GameObject[] obj_list = m_obj_list;
		foreach (GameObject gameObject in obj_list)
		{
			ShadowOnlyComponent[] components = gameObject.GetComponents<ShadowOnlyComponent>();
			for (int j = 0; j < components.Length; j++)
			{
				components[j].BeginShadowOnlyRender();
			}
			Renderer[] components2 = gameObject.GetComponents<Renderer>();
			foreach (Renderer renderer in components2)
			{
				if (gameObject.layer == (int)m_dynamics_layer_mask)
				{
					renderer.shadowCastingMode = ShadowCastingMode.Off;
					renderer.receiveShadows = true;
				}
				else
				{
					renderer.shadowCastingMode = ShadowCastingMode.On;
					renderer.receiveShadows = m_enable_static_receive_static_shadows;
				}
			}
		}
		EnableShadowLights();
	}

	private void OnPostRender()
	{
	}

	public void CacheShadowLights()
	{
		Light[] array = Object.FindObjectsOfType(typeof(Light)) as Light[];
		m_ShadowLight_list.Clear();
		if (array.Length > m_ShadowLight_list.Count)
		{
			m_ShadowLight_list.Capacity = array.Length;
		}
		Light[] array2 = array;
		foreach (Light light in array2)
		{
			if (light.enabled && light.shadows != 0)
			{
				LightShadowData item = default(LightShadowData);
				item.light = light;
				item.light_shadows = light.shadows;
				m_ShadowLight_list.Add(item);
			}
		}
	}

	public void EnableShadowLights()
	{
		foreach (LightShadowData item in m_ShadowLight_list)
		{
			if (item.light.enabled)
			{
				item.light.shadows = item.light_shadows;
			}
		}
	}

	public void DisableShadowLights()
	{
		foreach (LightShadowData item in m_ShadowLight_list)
		{
			if (item.light.enabled && item.light_shadows != 0)
			{
				item.light.shadows = LightShadows.None;
			}
		}
	}
}
