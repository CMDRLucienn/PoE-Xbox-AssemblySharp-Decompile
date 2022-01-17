using System;
using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class UIPartyMemberColorGetSet : UIParentSelectorListener
{
	public ColorListManager.ColorPickerType PickerType;

	public UIColorSelectorLine SelectorLine;

	private UIWidget m_Widget;

	private void Awake()
	{
		m_Widget = GetComponent<UIWidget>();
		if (SelectorLine != null)
		{
			UIColorSelectorLine selectorLine = SelectorLine;
			selectorLine.OnColorChanged = (UIColorSelectorLine.ColorChanged)Delegate.Combine(selectorLine.OnColorChanged, new UIColorSelectorLine.ColorChanged(OnColorChanged));
		}
	}

	private void OnClick()
	{
		SelectorLine.CurrentTarget = base.gameObject;
		SelectorLine.Show(ColorListManager.GetColorList(PickerType, CharacterStats.Subrace.Undefined), m_Widget.color);
	}

	private void OnColorChanged(Color color)
	{
		if (SelectorLine.CurrentTarget != base.gameObject)
		{
			return;
		}
		NPCAppearance component = UIInventoryManager.Instance.SelectedCharacter.GetComponent<NPCAppearance>();
		if ((bool)component)
		{
			switch (PickerType)
			{
			case ColorListManager.ColorPickerType.Hair:
				component.hairColor = color;
				break;
			case ColorListManager.ColorPickerType.Major:
				component.primaryColor = color;
				break;
			case ColorListManager.ColorPickerType.Minor:
				component.secondaryColor = color;
				break;
			case ColorListManager.ColorPickerType.Skin:
				component.skinColor = color;
				break;
			}
		}
		UIInventoryManager.Instance.ReloadTints();
		component.ApplyTints();
		NotifySelectionChanged(UIInventoryManager.Instance.SelectedCharacter);
	}

	public override void NotifySelectionChanged(CharacterStats character)
	{
		Color color = Color.white;
		if ((bool)character)
		{
			NPCAppearance component = character.GetComponent<NPCAppearance>();
			if ((bool)component)
			{
				switch (PickerType)
				{
				case ColorListManager.ColorPickerType.Hair:
					color = component.hairColor;
					break;
				case ColorListManager.ColorPickerType.Skin:
					color = component.skinColor;
					break;
				case ColorListManager.ColorPickerType.Major:
					color = component.primaryColor;
					break;
				case ColorListManager.ColorPickerType.Minor:
					color = component.secondaryColor;
					break;
				}
			}
		}
		m_Widget.color = color;
	}
}
