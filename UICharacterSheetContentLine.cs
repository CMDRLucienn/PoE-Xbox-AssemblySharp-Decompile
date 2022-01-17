using UnityEngine;

public abstract class UICharacterSheetContentLine : MonoBehaviour
{
	public virtual void Initialize()
	{
	}

	public virtual void Load(CharacterStats stats)
	{
	}

	public static string FormatPrefixed(string prefix, string text)
	{
		return UICharacterSheetContentManager.FormatPrefixed(prefix, text);
	}
}
