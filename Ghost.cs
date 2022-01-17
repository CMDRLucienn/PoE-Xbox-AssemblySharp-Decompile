using UnityEngine;

public class Ghost : MonoBehaviour
{
	public Color GhostColor = new Color(0.8980392f, 0.7647059f, 1f);

	public static bool Debug;

	private static int m_AlphaProp;

	private SkinnedMeshRenderer[] m_Renderers;

	public MaterialReplacement Replacement;

	private void OnDestroy()
	{
		if (Replacement != null)
		{
			Replacement.Restore(base.gameObject);
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		m_AlphaProp = Shader.PropertyToID("_Alpha");
		m_Renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
		if (Replacement.Material == null)
		{
			Replacement = new MaterialReplacement
			{
				Material = InGameHUD.Instance.GhostCharacterMat,
				Layer = LayerUtility.FindLayerValue("Dynamics No Shadow No Occlusion"),
				ReplaceNormal = true,
				ReplaceTint = true
			};
		}
		ApplyReplacement();
	}

	private void ApplyReplacement()
	{
		Replacement.Replace(base.gameObject);
		SkinnedMeshRenderer[] renderers = m_Renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			Material[] materials = renderers[i].materials;
			for (int j = 0; j < materials.Length; j++)
			{
				materials[j].SetFloat(m_AlphaProp, GhostColor.a);
			}
		}
	}
}
