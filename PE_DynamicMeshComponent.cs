using System;
using UnityEngine;

[Obsolete("No longer used according to Roby and Adam.")]
public class PE_DynamicMeshComponent : MonoBehaviour
{
	private Renderer m_renderer;

	private Material[] m_materials;

	private Material[] m_albedoSpecMaterials;

	private Material[] m_normalsMaterials;

	private Material[] m_depthMaterials;

	private void Start()
	{
		UpdateMaterials();
	}

	private void OnDestroy()
	{
		if (m_albedoSpecMaterials != null)
		{
			Material[] albedoSpecMaterials = m_albedoSpecMaterials;
			for (int i = 0; i < albedoSpecMaterials.Length; i++)
			{
				GameUtilities.Destroy(albedoSpecMaterials[i]);
			}
		}
		if (m_normalsMaterials != null)
		{
			Material[] albedoSpecMaterials = m_normalsMaterials;
			for (int i = 0; i < albedoSpecMaterials.Length; i++)
			{
				GameUtilities.Destroy(albedoSpecMaterials[i]);
			}
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void UpdateMaterials()
	{
		float mipMapBias = -0.5f;
		float mipMapBias2 = -0.5f;
		PE_GameRender instance = PE_GameRender.Instance;
		if ((bool)instance)
		{
			mipMapBias = instance.mipMapBias_characterDiffuse;
			mipMapBias2 = instance.mipMapBias_characterNormal;
		}
		m_renderer = GetComponent<Renderer>();
		if (!(m_renderer != null))
		{
			return;
		}
		int num = 0;
		num = (Application.isEditor ? GetComponent<Renderer>().sharedMaterials.Length : GetComponent<Renderer>().materials.Length);
		m_materials = new Material[num];
		m_normalsMaterials = new Material[num];
		m_albedoSpecMaterials = new Material[num];
		m_depthMaterials = new Material[num];
		Material[] array = null;
		array = (Application.isEditor ? GetComponent<Renderer>().sharedMaterials : GetComponent<Renderer>().materials);
		for (int i = 0; i < num; i++)
		{
			m_materials[i] = array[i];
			m_normalsMaterials[i] = new Material(Shader.Find("Trenton/PE_DynamicMeshNormals"));
			m_albedoSpecMaterials[i] = new Material(Shader.Find("Trenton/PE_DynamicMeshAlbedoSpec"));
			m_depthMaterials[i] = instance.m_depthMaterial;
			Texture texture = instance.m_defaultNormalTexture;
			Texture texture2 = instance.m_defaultWhiteTexture;
			Material material = array[i];
			if (material != null)
			{
				if (material.HasProperty("_MainTex"))
				{
					texture2 = m_materials[i].GetTexture("_MainTex");
				}
				if ((bool)texture2)
				{
					texture2.mipMapBias = mipMapBias;
				}
				if (material.HasProperty("_BumpMap"))
				{
					texture = m_materials[i].GetTexture("_BumpMap");
				}
				if ((bool)texture)
				{
					texture.mipMapBias = mipMapBias2;
				}
			}
			m_normalsMaterials[i].mainTexture = texture;
			m_albedoSpecMaterials[i].mainTexture = texture2;
		}
	}

	public void SetRenderMode(PE_GameRender.ScreenTextureType textureType)
	{
		if (m_materials != null && m_materials.Length != 0)
		{
			switch (textureType)
			{
			case PE_GameRender.ScreenTextureType.None:
				m_renderer.materials = m_materials;
				break;
			case PE_GameRender.ScreenTextureType.Depth:
				m_renderer.materials = m_depthMaterials;
				break;
			case PE_GameRender.ScreenTextureType.Normals:
				m_renderer.materials = m_normalsMaterials;
				break;
			case PE_GameRender.ScreenTextureType.AlbedoSpec:
				m_renderer.materials = m_albedoSpecMaterials;
				break;
			default:
				Debug.LogError("PE_DynamicMeshComponent::SetRenderMode() - need to handle this texture type");
				break;
			case PE_GameRender.ScreenTextureType.DynamicShadow:
				break;
			}
		}
	}
}
