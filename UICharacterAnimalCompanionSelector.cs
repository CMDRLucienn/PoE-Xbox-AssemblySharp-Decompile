using UnityEngine;

public class UICharacterAnimalCompanionSelector : UIParentSelectorListener, ISelectACharacter
{
	public bool DeactivateIfNone;

	public CharacterStats SelectedCharacter { get; private set; }

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	public override void NotifySelectionChanged(CharacterStats character)
	{
		SelectedCharacter = null;
		if ((bool)character)
		{
			GameObject gameObject = GameUtilities.FindAnimalCompanion(character.gameObject);
			SelectedCharacter = (gameObject ? gameObject.GetComponent<CharacterStats>() : null);
		}
		if (DeactivateIfNone)
		{
			base.gameObject.SetActive(SelectedCharacter);
		}
		if (this.OnSelectedCharacterChanged != null)
		{
			this.OnSelectedCharacterChanged(SelectedCharacter);
		}
	}
}
