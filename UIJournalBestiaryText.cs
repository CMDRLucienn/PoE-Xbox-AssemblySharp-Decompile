using System;
using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UIJournalBestiaryText : MonoBehaviour
{
	public string FormatString;

	private UILabel m_Label;

	public GUIDatabaseString Label;

	private UITable m_LayoutParent;

	private void Awake()
	{
		m_LayoutParent = NGUITools.FindInParents<UITable>(base.gameObject);
		UIJournalManager instance = UIJournalManager.Instance;
		instance.OnBestiarySelectionChanged = (UIJournalManager.BestiarySelectionChanged)Delegate.Combine(instance.OnBestiarySelectionChanged, new UIJournalManager.BestiarySelectionChanged(RefreshText));
	}

	private void Start()
	{
		RefreshText(UIJournalManager.Instance.CyclopediaCurrentBestiary);
	}

	private void OnDestroy()
	{
		if ((bool)UIJournalManager.Instance)
		{
			UIJournalManager instance = UIJournalManager.Instance;
			instance.OnBestiarySelectionChanged = (UIJournalManager.BestiarySelectionChanged)Delegate.Remove(instance.OnBestiarySelectionChanged, new UIJournalManager.BestiarySelectionChanged(RefreshText));
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void RefreshText(BestiaryReference reference)
	{
		if (m_Label == null)
		{
			m_Label = GetComponent<UILabel>();
		}
		if (reference == null)
		{
			m_Label.text = "";
			return;
		}
		string format = FormatString.Replace("{1}", Label.GetText());
		try
		{
			m_Label.text = string.Format(reference, format, reference);
		}
		catch (FormatException exception)
		{
			Debug.LogException(exception, this);
			m_Label.text = "";
		}
		if ((bool)m_LayoutParent)
		{
			m_LayoutParent.repositionNow = true;
		}
	}
}
