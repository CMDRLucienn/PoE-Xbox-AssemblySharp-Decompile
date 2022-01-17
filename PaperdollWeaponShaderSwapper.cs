using UnityEngine;

public static class PaperdollWeaponShaderSwapper
{
	private const string SHADER_PARTICLE_ADD_NAME = "Trenton/Particles/PE_ParticleAdd";

	private const string SHADER_PARTICLE_ADD_ALWAYS_VISIBLE_NAME = "Trenton/Particles/PE_ParticleAdd_AlwaysVisible";

	public static void SwapShaders(GameObject gameObject)
	{
		ParticleSystemRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystemRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = componentsInChildren[i].materials;
			foreach (Material material in materials)
			{
				if (string.Equals(material.shader.name, "Trenton/Particles/PE_ParticleAdd"))
				{
					material.shader = Shader.Find("Trenton/Particles/PE_ParticleAdd_AlwaysVisible");
				}
			}
		}
	}
}
