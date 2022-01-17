using System;
using UnityEngine;

public class UIOptionsSliderGroup : MonoBehaviour
{
	public delegate void OnSettingChanged(UIOptionsSliderGroup sender, float newSetting);

	public UILabel NumberLabel;

	public UIOptionsSlider Slider;

	public float NumberMultiplier = 1f;

	public float NumberAdd;

	public OnSettingChanged OnChanged;

	public GUIDatabaseString FormatString;

	public float Setting
	{
		get
		{
			return (float)Slider.Setting * NumberMultiplier + NumberAdd;
		}
		set
		{
			Slider.Setting = Mathf.RoundToInt((value - NumberAdd) / NumberMultiplier);
		}
	}

	private void Start()
	{
		UIOptionsSlider slider = Slider;
		slider.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider.OnChanged, new UIOptionsSlider.OnSettingChanged(OnSliderChanged));
		OnSliderChanged(Slider, Slider.Setting);
		StringTableManager.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		StringTableManager.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged(Language newLang)
	{
		UpdateText();
	}

	private void OnSliderChanged(UIOptionsSlider sender, int setting)
	{
		UpdateText();
		if (OnChanged != null)
		{
			OnChanged(this, Setting);
		}
	}

	private void UpdateText()
	{
		if ((bool)NumberLabel)
		{
			if (FormatString.IsValidString)
			{
				NumberLabel.text = GUIUtils.Format(FormatString.StringID, Setting);
			}
			else
			{
				NumberLabel.text = Setting.ToString("#0.##");
			}
		}
	}
}
