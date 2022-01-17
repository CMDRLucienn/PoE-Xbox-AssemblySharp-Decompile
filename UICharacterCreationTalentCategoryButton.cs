using UnityEngine;

public class UICharacterCreationTalentCategoryButton : UICharacterCreationElement
{
	public UIPanel AbilityButtonPanel;

	public UIGrid AbilitiesGrid;

	public GenericTalent.TalentCategory TalentCategory;

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

	public void ShowTalentCategory()
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
				if ((bool)uICharacterCreationEnumSetter && uICharacterCreationEnumSetter.UnlockableAbility != null && !(uICharacterCreationEnumSetter.UnlockableAbility.Ability == null))
				{
					gameObject.gameObject.SetActive(AbilityProgressionTable.GetGenericTalent(uICharacterCreationEnumSetter.UnlockableAbility.Ability).Category == TalentCategory);
				}
			}
		}
		UICharacterCreationTalentCategoryButton[] array = Object.FindObjectsOfType<UICharacterCreationTalentCategoryButton>();
		foreach (UICharacterCreationTalentCategoryButton uICharacterCreationTalentCategoryButton in array)
		{
			UIImageButtonRevised component = uICharacterCreationTalentCategoryButton.GetComponent<UIImageButtonRevised>();
			if ((bool)component)
			{
				component.ForceDown(uICharacterCreationTalentCategoryButton == this);
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
			ShowTalentCategory();
		}
	}
}
