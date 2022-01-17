using UnityEngine;

public class UICharacterSheetCompanionContent : UIParentSelectorListener, ISelectACharacter
{
	private UITable m_Table;

	public CharacterStats SelectedCharacter { get; private set; }

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	public void Reposition()
	{
		if (!m_Table)
		{
			m_Table = GetComponent<UITable>();
		}
		if ((bool)m_Table)
		{
			m_Table.Reposition();
		}
	}

	public override void NotifySelectionChanged(CharacterStats selection)
	{
		if ((bool)selection)
		{
			GameObject gameObject = GameUtilities.FindAnimalCompanion(selection.gameObject);
			SelectedCharacter = (gameObject ? gameObject.GetComponent<CharacterStats>() : null);
		}
		else
		{
			SelectedCharacter = null;
		}
		if (this.OnSelectedCharacterChanged != null)
		{
			this.OnSelectedCharacterChanged(SelectedCharacter);
		}
	}
}
