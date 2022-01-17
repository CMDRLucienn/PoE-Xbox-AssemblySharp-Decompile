using UnityEngine;

public class UICharacterCreationAbilityLevelButton : UICharacterCreationElement
{
	public UIPanel AbilityButtonPanel;

	public UIGrid AbilitiesGrid;

	public int AbilityLevel;

	public UIDraggablePanel DragPanel;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void DisableButton()
	{
		GetComponent<UIImageButtonRevised>().enabled = false;
		base.transform.parent.GetComponentInChildren<UILabel>().color = new Color(14f / 51f, 14f / 51f, 14f / 51f);
	}

	public void EnableButton()
	{
		GetComponent<UIImageButtonRevised>().enabled = true;
		base.transform.parent.GetComponentInChildren<UILabel>().color = Color.white;
	}

	private bool IsButtonEnabled()
	{
		return GetComponent<UIImageButtonRevised>().enabled;
	}

	public void ShowLevelGroup()
	{
		for (int i = 0; i < AbilitiesGrid.transform.childCount; i++)
		{
			GameObject gameObject = AbilitiesGrid.transform.GetChild(i).gameObject;
			if (!gameObject)
			{
				continue;
			}
			UICharacterCreationEnumSetter[] componentsInChildren = gameObject.GetComponentsInChildren<UICharacterCreationEnumSetter>(includeInactive: true);
			foreach (UICharacterCreationEnumSetter uICharacterCreationEnumSetter in componentsInChildren)
			{
				if ((bool)uICharacterCreationEnumSetter && !(uICharacterCreationEnumSetter.AbilityObject == null))
				{
					gameObject.gameObject.SetActive(AbilityProgressionTable.GetSpellLevel(uICharacterCreationEnumSetter.AbilityObject) == AbilityLevel);
				}
			}
		}
		UICharacterCreationAbilityLevelButton[] array = Object.FindObjectsOfType<UICharacterCreationAbilityLevelButton>();
		foreach (UICharacterCreationAbilityLevelButton uICharacterCreationAbilityLevelButton in array)
		{
			UIImageButtonRevised component = uICharacterCreationAbilityLevelButton.GetComponent<UIImageButtonRevised>();
			if ((bool)component)
			{
				component.ForceDown(uICharacterCreationAbilityLevelButton == this);
			}
		}
		if ((bool)AbilitiesGrid)
		{
			AbilitiesGrid.Reposition();
		}
		if ((bool)DragPanel)
		{
			DragPanel.ResetPosition();
		}
	}

	private void OnClick()
	{
		if (IsButtonEnabled())
		{
			ShowLevelGroup();
		}
	}
}
