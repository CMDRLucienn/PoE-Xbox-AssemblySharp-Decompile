using UnityEngine;

public class SoftParticleController : MonoBehaviour
{
	[Range(0f, 100f)]
	public float Factor = 1f;

	private void Start()
	{
		if (base.gameObject.GetComponent<ParticleSystem>() == null)
		{
			return;
		}
		ParticleSystemRenderer particleSystemRenderer = base.gameObject.GetComponent<ParticleSystem>().GetComponent<Renderer>() as ParticleSystemRenderer;
		if (!(particleSystemRenderer != null))
		{
			return;
		}
		Material[] materials = particleSystemRenderer.materials;
		foreach (Material material in materials)
		{
			if (material != null && material.HasProperty("_InvFade"))
			{
				material.SetFloat("_InvFade", Factor);
			}
		}
	}
}
