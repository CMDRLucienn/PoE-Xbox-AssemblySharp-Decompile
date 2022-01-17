using UnityEngine;

public class ShadowOnlyComponent : MonoBehaviour
{
	private Renderer m_renderer;

	public Shader[] m_Shadow_Only_Shader;

	private int m_material_count;

	private Shader[] m_old_shader;

	private Shader m_shadow_only_diffuse;

	private Shader m_shadow_only_optimized_bark;

	private Shader m_shadow_only_optimized_leaf;

	private void Start()
	{
		m_renderer = base.gameObject.GetComponent<Renderer>();
		if (!m_renderer)
		{
			return;
		}
		m_material_count = m_renderer.materials.Length;
		if (m_material_count > 0)
		{
			if (m_Shadow_Only_Shader.Length < m_material_count)
			{
				Debug.LogError("GameObj: " + base.gameObject.name + " Renderer: " + m_renderer.name + " ShadowOnlyComponent shaders not initialized!");
			}
			m_old_shader = new Shader[m_material_count];
			for (int i = 0; i < m_material_count; i++)
			{
				m_old_shader[i] = m_renderer.materials[i].shader;
			}
		}
	}

	public void BeginShadowOnlyRender()
	{
		if ((bool)m_renderer)
		{
			for (int i = 0; i < m_material_count; i++)
			{
				m_renderer.materials[i].shader = m_Shadow_Only_Shader[i];
			}
		}
	}

	public void EndShadowOnlyRender()
	{
		if ((bool)m_renderer)
		{
			for (int i = 0; i < m_material_count; i++)
			{
				m_renderer.materials[i].shader = m_old_shader[i];
			}
		}
	}

	private void SetupShaders()
	{
		string text = "Custom/ShadowOnly_Diffuse";
		m_shadow_only_diffuse = Shader.Find(text);
		if (m_shadow_only_diffuse == null)
		{
			Debug.LogError("ShadowOnlyComponent: Cannot find \"" + text + "\"");
		}
		string text2 = "Custom/Shadow Only Tree Creator Bark Optimized";
		m_shadow_only_optimized_bark = Shader.Find(text2);
		if (m_shadow_only_optimized_bark == null)
		{
			Debug.LogError("ShadowOnlyComponent: Cannot find \"" + text2 + "\"");
		}
		string text3 = "Custom/ShadowOnly_TreeLeaf";
		m_shadow_only_optimized_leaf = Shader.Find(text3);
		if (m_shadow_only_optimized_leaf == null)
		{
			Debug.LogError("ShadowOnlyComponent: Cannot find \"" + text3 + "\"");
		}
	}

	private Shader GetShadowOnlyShadow(Shader original_shader)
	{
		if (original_shader.name.Contains("Bark"))
		{
			return m_shadow_only_optimized_bark;
		}
		if (original_shader.name.Contains("Leaves"))
		{
			return m_shadow_only_optimized_leaf;
		}
		return m_shadow_only_diffuse;
	}

	public void InitShadowOnlyShaders()
	{
		SetupShaders();
		m_renderer = base.gameObject.GetComponent<Renderer>();
		if (!m_renderer)
		{
			return;
		}
		m_material_count = m_renderer.sharedMaterials.Length;
		if (m_material_count <= 0)
		{
			return;
		}
		if (m_Shadow_Only_Shader == null)
		{
			m_Shadow_Only_Shader = new Shader[m_material_count];
			for (int i = 0; i < m_material_count; i++)
			{
				m_Shadow_Only_Shader[i] = GetShadowOnlyShadow(m_renderer.sharedMaterials[i].shader);
			}
		}
		else if (m_material_count != m_Shadow_Only_Shader.Length)
		{
			Shader[] array = new Shader[m_Shadow_Only_Shader.Length];
			int j;
			for (j = 0; j < m_Shadow_Only_Shader.Length; j++)
			{
				array[j] = m_Shadow_Only_Shader[j];
			}
			m_Shadow_Only_Shader = new Shader[m_material_count];
			for (j = 0; j < array.Length; j++)
			{
				m_Shadow_Only_Shader[j] = array[j];
			}
			for (; j < m_material_count; j++)
			{
				m_Shadow_Only_Shader[j] = GetShadowOnlyShadow(m_renderer.sharedMaterials[j].shader);
			}
		}
	}
}
