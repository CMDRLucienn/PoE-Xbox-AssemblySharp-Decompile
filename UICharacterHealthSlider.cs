using UnityEngine;

[RequireComponent(typeof(UISlider))]
public class UICharacterHealthSlider : UIParentSelectorListener
{
	public enum HealthType
	{
		Stamina,
		Health
	}

	private Health m_Health;

	private UISlider m_Slider;

	public HealthType Type;

	private void Awake()
	{
		m_Slider = GetComponent<UISlider>();
	}

	private void Update()
	{
		if ((bool)m_Health)
		{
			if (Type == HealthType.Stamina)
			{
				m_Slider.sliderValue = 1f - m_Health.CurrentStamina / m_Health.BaseMaxStamina;
			}
			else
			{
				m_Slider.sliderValue = 1f - m_Health.CurrentHealth / m_Health.MaxHealth;
			}
		}
		else
		{
			m_Slider.sliderValue = 0f;
		}
	}

	public override void NotifySelectionChanged(CharacterStats selection)
	{
		m_Health = (selection ? selection.GetComponent<Health>() : null);
	}
}
