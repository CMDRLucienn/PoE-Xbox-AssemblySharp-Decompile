using System;
using UnityEngine;

[RequireComponent(typeof(UITexture))]
public class UIJournalBestiaryImage : MonoBehaviour
{
	private UIDynamicLoadTexture m_Texture;

	private void Start()
	{
		UIJournalManager instance = UIJournalManager.Instance;
		instance.OnBestiarySelectionChanged = (UIJournalManager.BestiarySelectionChanged)Delegate.Combine(instance.OnBestiarySelectionChanged, new UIJournalManager.BestiarySelectionChanged(RefreshImage));
		RefreshImage(UIJournalManager.Instance.CyclopediaCurrentBestiary);
	}

	private void OnDestroy()
	{
		if ((bool)UIJournalManager.Instance)
		{
			UIJournalManager instance = UIJournalManager.Instance;
			instance.OnBestiarySelectionChanged = (UIJournalManager.BestiarySelectionChanged)Delegate.Remove(instance.OnBestiarySelectionChanged, new UIJournalManager.BestiarySelectionChanged(RefreshImage));
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void RefreshImage(BestiaryReference reference)
	{
		if (m_Texture == null)
		{
			m_Texture = base.gameObject.AddComponent<UIDynamicLoadTexture>();
		}
		m_Texture.Path = reference.PicturePath;
	}
}
