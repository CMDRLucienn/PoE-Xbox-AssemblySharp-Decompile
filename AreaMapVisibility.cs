using UnityEngine;

public class AreaMapVisibility : MonoBehaviour
{
	[Tooltip("If set, this object will be disabled during the Area Map render.")]
	public bool Disable = true;

	private bool m_CachedState = true;

	private static AreaMapVisibility[] s_CachedComponents;

	public static void StartRender()
	{
		s_CachedComponents = Object.FindObjectsOfType<AreaMapVisibility>();
		if (s_CachedComponents == null)
		{
			return;
		}
		for (int i = 0; i < s_CachedComponents.Length; i++)
		{
			if ((bool)s_CachedComponents[i])
			{
				s_CachedComponents[i].StartAreaMapRender();
			}
		}
	}

	public static void EndRender()
	{
		if (s_CachedComponents == null)
		{
			return;
		}
		for (int i = 0; i < s_CachedComponents.Length; i++)
		{
			if ((bool)s_CachedComponents[i])
			{
				s_CachedComponents[i].EndAreaMapRender();
			}
		}
		s_CachedComponents = null;
	}

	private void StartAreaMapRender()
	{
		m_CachedState = base.gameObject.activeSelf;
		if (Disable)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void EndAreaMapRender()
	{
		base.gameObject.SetActive(m_CachedState);
	}
}
