using UnityEngine;
using UnityEngine.Rendering;

public class ShadowDynamicOnlyCameraScript : MonoBehaviour
{
	private GameObject[] m_obj_list;

	private LayerMask m_dynamics_layer_mask;

	private void Start()
	{
		m_dynamics_layer_mask = LayerMask.NameToLayer("Dynamics");
		Transform parent = base.transform;
		while (parent.parent != null)
		{
			parent = parent.parent;
		}
	}

	private void OnPreRender()
	{
		GetComponent<Camera>().targetTexture = PE_GameRender.Instance.GetScreenTextureType(PE_GameRender.ScreenTextureType.Depth);
		m_obj_list = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		GameObject[] obj_list = m_obj_list;
		foreach (GameObject gameObject in obj_list)
		{
			Renderer[] components = gameObject.GetComponents<Renderer>();
			foreach (Renderer renderer in components)
			{
				if (gameObject.layer == (int)m_dynamics_layer_mask)
				{
					renderer.shadowCastingMode = ShadowCastingMode.On;
					renderer.receiveShadows = true;
				}
				else
				{
					renderer.shadowCastingMode = ShadowCastingMode.Off;
					renderer.receiveShadows = true;
				}
			}
		}
	}

	private void OnPostRender()
	{
		GameObject[] obj_list = m_obj_list;
		for (int i = 0; i < obj_list.Length; i++)
		{
			ShadowOnlyComponent[] components = obj_list[i].GetComponents<ShadowOnlyComponent>();
			for (int j = 0; j < components.Length; j++)
			{
				components[j].EndShadowOnlyRender();
			}
		}
	}
}
