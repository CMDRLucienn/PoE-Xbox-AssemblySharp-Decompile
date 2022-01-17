using System.Collections.Generic;
using UnityEngine;

public class MaterialCache : MonoBehaviour
{
	private Dictionary<Material, Material> m_CachedMaterials = new Dictionary<Material, Material>();

	private MaterialReplacement m_ActiveReplacement;

	public int Size;

	private void OnDestroy()
	{
		Clear();
		m_CachedMaterials = null;
	}

	public void Clear()
	{
		foreach (Material value in m_CachedMaterials.Values)
		{
			Object.Destroy(value);
		}
		m_CachedMaterials.Clear();
	}

	public void Reapply()
	{
		Clear();
		if (m_ActiveReplacement != null)
		{
			m_ActiveReplacement.Replace(base.gameObject);
		}
	}

	public static MaterialCache Get(GameObject obj)
	{
		if (!obj)
		{
			return null;
		}
		MaterialCache materialCache = obj.GetComponent<MaterialCache>();
		if (!materialCache)
		{
			materialCache = obj.AddComponent<MaterialCache>();
		}
		return materialCache;
	}

	public static void Clear(GameObject obj)
	{
		if ((bool)obj)
		{
			MaterialCache component = obj.GetComponent<MaterialCache>();
			if ((bool)component)
			{
				GameUtilities.DestroyComponent(component);
			}
		}
	}

	public void Replace(MaterialReplacement rep)
	{
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = componentsInChildren[i].materials;
			foreach (Material m in materials)
			{
				Replace(rep, m);
			}
		}
		m_ActiveReplacement = rep;
	}

	public void Restore(MaterialReplacement rep)
	{
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = componentsInChildren[i].materials;
			foreach (Material m in materials)
			{
				Restore(rep, m);
			}
		}
		m_ActiveReplacement = null;
	}

	public Material Replace(MaterialReplacement rep, Material m)
	{
		if (rep.Empty)
		{
			return m;
		}
		if (!m_CachedMaterials.ContainsKey(m))
		{
			m_CachedMaterials[m] = new Material(m);
		}
		int nameID = Shader.PropertyToID("_MainTex");
		int nameID2 = Shader.PropertyToID("_BumpMap");
		int nameID3 = Shader.PropertyToID("_EmissiveMap");
		int nameID4 = Shader.PropertyToID("_TintMap");
		Texture value = (m.HasProperty(nameID) ? m.GetTexture(nameID) : null);
		Texture value2 = (m.HasProperty(nameID2) ? m.GetTexture(nameID2) : null);
		Texture value3 = (m.HasProperty(nameID3) ? m.GetTexture(nameID3) : null);
		Texture value4 = (m.HasProperty(nameID4) ? m.GetTexture(nameID4) : null);
		m.shader = rep.Material.shader;
		m.CopyPropertiesFromMaterial(rep.Material);
		if (!rep.ReplaceColor && m.HasProperty(nameID))
		{
			m.SetTexture(nameID, value);
		}
		if (!rep.ReplaceNormal && m.HasProperty(nameID2))
		{
			m.SetTexture(nameID2, value2);
		}
		if (!rep.ReplaceEmissive && m.HasProperty(nameID3))
		{
			m.SetTexture(nameID3, value3);
		}
		if (!rep.ReplaceTint && m.HasProperty(nameID4))
		{
			m.SetTexture(nameID4, value4);
		}
		Size = m_CachedMaterials.Count;
		return m;
	}

	public Material Restore(MaterialReplacement rep, Material m)
	{
		if (m_CachedMaterials.ContainsKey(m))
		{
			Material material = m_CachedMaterials[m];
			m_CachedMaterials.Remove(m);
			m.shader = material.shader;
			m.CopyPropertiesFromMaterial(material);
			Object.Destroy(material);
		}
		Size = m_CachedMaterials.Count;
		return m;
	}
}
