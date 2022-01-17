using UnityEngine;

public class TintController : MonoBehaviour
{
	public Color TintColor = new Color(1f, 1f, 1f, 1f);

	private void Start()
	{
		Material[] materials = (base.gameObject.GetComponent<ParticleSystem>().GetComponent<Renderer>() as ParticleSystemRenderer).materials;
		foreach (Material material in materials)
		{
			if (material.HasProperty("_TintColor"))
			{
				material.SetColor("_TintColor", TintColor);
			}
		}
	}
}
