using UnityEngine;

public class UICharsetSpecificContent : MonoBehaviour
{
	public enum MatchMode
	{
		AnyOf,
		NoneOf
	}

	public Language.CharacterSet[] Charsets;

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
		Language.CharacterSet[] charsets = Charsets;
		for (int i = 0; i < charsets.Length; i++)
		{
			if (charsets[i] == newLang.Charset)
			{
				(Target ? Target : base.gameObject).SetActive(Mode == MatchMode.AnyOf);
				return;
			}
		}
		(Target ? Target : base.gameObject).SetActive(Mode == MatchMode.NoneOf);
	}
}
