using System;

public class UICharacterCreationNameSetter : UICharacterCreationElement
{
	public enum CharacterCreationNameType
	{
		Character,
		Animal_Companion
	}

	public CharacterCreationNameType NameType;

	protected override void Start()
	{
		base.Start();
		UIInput component = GetComponent<UIInput>();
		if ((bool)component)
		{
			component.onSubmit = (UIInput.OnSubmit)Delegate.Combine(component.onSubmit, new UIInput.OnSubmit(AcceptName));
		}
	}

	private void OnEnable()
	{
		UIInput component = GetComponent<UIInput>();
		if ((bool)component)
		{
			component.selected = true;
		}
	}

	private void SetName(string inputString)
	{
		if (NameType == CharacterCreationNameType.Character)
		{
			base.Owner.Character.Name = inputString.Trim();
		}
		else if (NameType == CharacterCreationNameType.Animal_Companion)
		{
			base.Owner.Character.Animal_Name = inputString.Trim();
		}
		UICharacterCreationManager.Instance.RefreshNextButtons();
	}

	private void AcceptName(string inputString)
	{
		SetName(inputString);
		UICharacterCreationManager.Instance.EndStage();
		base.Owner.SignalValueChanged(ValueType.Name);
	}

	private void OnInputChanged()
	{
		UIInput component = GetComponent<UIInput>();
		if ((bool)component)
		{
			if (string.IsNullOrEmpty(component.text.Trim()))
			{
				component.text = component.text.Trim();
			}
			SetName(component.text);
		}
	}
}
