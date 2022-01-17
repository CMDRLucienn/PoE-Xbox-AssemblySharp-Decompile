using System;
using UnityEngine;

public class UILanguageSpecificContent : MonoBehaviour
{
	public enum MatchMode
	{
		AnyOf,
		NoneOf
	}

	public string[] LanguageNames;

	public MatchMode Mode;

	public GameObject Target;

	private void Start()
	{
		StringTableManager.OnLanguageChanged += OnLanguageChanged;
		OnLanguageChanged(StringTableManager.CurrentLanguage);
	}

	private void OnDestroy()
	{
		StringTableManager.OnLanguageChanged -= OnLanguageChanged;
		Target = null;
	}

	private void OnLanguageChanged(Language newLang)
	{
		string[] languageNames = LanguageNames;
		for (int i = 0; i < languageNames.Length; i++)
		{
			if (languageNames[i].Equals(newLang.Name, StringComparison.OrdinalIgnoreCase))
			{
				(Target ? Target : base.gameObject).SetActive(Mode == MatchMode.AnyOf);
				return;
			}
		}
		(Target ? Target : base.gameObject).SetActive(Mode == MatchMode.NoneOf);
	}
}
