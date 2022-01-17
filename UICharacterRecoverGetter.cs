using UnityEngine;

[RequireComponent(typeof(UISlider))]
public class UICharacterRecoverGetter : UIParentSelectorListener
{
	private UISlider m_Slider;

	private void Awake()
	{
		m_Slider = GetComponent<UISlider>();
	}

	private void Update()
	{
		if (ParentSelector != null && (bool)ParentSelector.SelectedCharacter && (bool)m_Slider)
		{
			if (ParentSelector.SelectedCharacter.TotalRecoveryTime != 0f)
			{
				float sliderValue = ParentSelector.SelectedCharacter.RecoveryTimer / ParentSelector.SelectedCharacter.TotalRecoveryTime;
				m_Slider.sliderValue = sliderValue;
			}
			else
			{
				m_Slider.sliderValue = 0f;
			}
		}
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		Update();
	}
}
