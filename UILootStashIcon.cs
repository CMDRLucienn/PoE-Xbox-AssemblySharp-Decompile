using UnityEngine;

public class UILootStashIcon : MonoBehaviour
{
	public UITexture Texture;

	public UIWidget Background;

	private UITweener[] m_Tween;

	private void Update()
	{
		if (Texture.alpha == 0f)
		{
			End();
		}
	}

	public void End()
	{
		base.gameObject.SetActive(value: false);
		UILootManager.Instance.StashIcons.RecycleIcon(this);
	}

	public void Begin(Item item)
	{
		if (m_Tween == null)
		{
			m_Tween = GetComponentsInChildren<UITweener>(includeInactive: true);
		}
		base.gameObject.SetActive(value: true);
		Texture.mainTexture = item.IconTexture;
		for (int i = 0; i < m_Tween.Length; i++)
		{
			m_Tween[i].Reset();
			m_Tween[i].Play(forward: true);
		}
	}
}
