using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UICharacterGetEndurance : UIParentSelectorListener
{
	public enum Style
	{
		Stamina,
		Health
	}

	public Style Stat;

	private UILabel m_Label;

	private Health m_SelectedHealth;

	private float m_LastKnownEndurance;

	private float m_LastKnownMaxEndurance;

	private bool m_LastKnownVision = true;

	[Tooltip("If set, this widget will show or hide based on PORTRAIT_SHOW_ENDURANCE_VALUE.")]
	public bool UsePortraitOption;

	private bool m_LineBreak;

	public bool LineBreak
	{
		get
		{
			return m_LineBreak;
		}
		set
		{
			if (m_LineBreak != value)
			{
				m_LineBreak = value;
				m_Label.maxLineCount = ((!value) ? 1 : 2);
				m_Label.lineWidth = (value ? 30 : 50);
				UpdateText(force: true);
			}
		}
	}

	private void OnEnable()
	{
		UpdateText(force: true);
	}

	private void Update()
	{
		UpdateText(force: false);
	}

	private void UpdateText(bool force)
	{
		if (m_Label == null)
		{
			m_Label = GetComponent<UILabel>();
		}
		m_Label.alpha = ((!UsePortraitOption || GameState.Option.GetOption(GameOption.BoolOption.PORTRAIT_SHOW_ENDURANCE_VALUE)) ? 1f : 0f);
		if (m_Label.alpha <= 0f)
		{
			return;
		}
		if (!m_SelectedHealth)
		{
			m_Label.text = "";
			return;
		}
		float num = 0f;
		float num2 = 0f;
		switch (Stat)
		{
		case Style.Health:
			num = m_SelectedHealth.CurrentHealth;
			num2 = m_SelectedHealth.MaxHealth;
			break;
		case Style.Stamina:
			num = m_SelectedHealth.CurrentStamina;
			num2 = m_SelectedHealth.MaxStamina;
			break;
		}
		bool healthVisible = m_SelectedHealth.HealthVisible;
		if (force || num != m_LastKnownEndurance || num2 != m_LastKnownMaxEndurance || healthVisible != m_LastKnownVision || string.IsNullOrEmpty(m_Label.text))
		{
			string text = ((!healthVisible) ? GUIUtils.GetText(1980) : Mathf.CeilToInt(Mathf.Max(0f, num)).ToString("#0"));
			m_Label.text = GUIUtils.Format(451, text, (LineBreak ? "\n" : "") + Mathf.CeilToInt(num2).ToString("#0"));
			m_LastKnownEndurance = num;
			m_LastKnownMaxEndurance = num2;
			m_LastKnownVision = healthVisible;
		}
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		m_SelectedHealth = (stats ? stats.GetComponent<Health>() : null);
		Update();
	}
}
