using System;
using UnityEngine;

[ExecuteInEditMode]
public class UIOptionsTag : MonoBehaviour
{
	public GUIDatabaseString TooltipString;

	public GUIDatabaseString CheckboxLabel;

	public UILabel Label;

	public UICheckbox Checkbox;

	public UIOptionsManager.Option Option;

	public GameOption.BoolOption BoolSuboption;

	public AutoPauseOptions.PauseEvent AutopauseSuboption;

	public GameDifficulty Difficulty;

	public bool Autoslow;

	public bool Enabled = true;

	public bool NotBackerBeta;

	public bool ExpertMode;

	public bool Inverted;

	public bool GreyLabelWhenDisabled;

	private static Color s_GreyLabelColor = new Color(0.698039234f, 0.698039234f, 0.698039234f);

	private void Start()
	{
		StringTableManager.OnLanguageChanged += OnLanguageChanged;
		UpdateLabel();
		if (!Application.isPlaying)
		{
			return;
		}
		if ((bool)Checkbox && !Checkbox.GetComponent<UICheckboxAudio>())
		{
			Checkbox.gameObject.AddComponent<UICheckboxAudio>();
		}
		if ((bool)Label && (bool)Checkbox)
		{
			GameObject obj = new GameObject("TextCollider");
			obj.transform.parent = base.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.AddComponent<BoxCollider>();
			obj.AddComponent<UINoClick>();
			UIStretch uIStretch = obj.AddComponent<UIStretch>();
			uIStretch.widgetContainer = Label;
			uIStretch.style = UIStretch.Style.Both;
			UIAnchor uIAnchor = obj.AddComponent<UIAnchor>();
			uIAnchor.widgetContainer = Label;
			uIStretch.pixelAdjustment.x = 6f;
			uIAnchor.pixelOffset.x = -3f;
			UIMultiSpriteImageButton component = GetComponent<UIMultiSpriteImageButton>();
			if ((bool)component)
			{
				component.ReFindChildren();
			}
		}
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UIEventListener uIEventListener = UIEventListener.Get(componentsInChildren[i].gameObject);
			uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnHoverChild));
		}
		if (Label != null)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(Label.gameObject);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnLabelClick));
		}
	}

	private void OnDestroy()
	{
		StringTableManager.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged(Language newlang)
	{
		UpdateLabel();
	}

	public void UpdateLabel()
	{
		if (Label != null)
		{
			if (CheckboxLabel.IsValidString)
			{
				Label.text = CheckboxLabel.GetText();
			}
			else if (Option == UIOptionsManager.Option.AUTOPAUSE)
			{
				Label.text = AutoPauseOptions.GetDisplayName(AutopauseSuboption);
			}
		}
	}

	private void OnLabelClick(GameObject go)
	{
		if (Checkbox.enabled)
		{
			Checkbox.isChecked = !Checkbox.isChecked;
		}
	}

	private void OnHoverChild(GameObject go, bool over)
	{
		if (over)
		{
			UIOptionsTooltip.Show(TooltipString.GetText());
		}
	}

	public void Disable()
	{
		Enabled = false;
		if ((bool)Checkbox)
		{
			UIImageButtonRevised component = Checkbox.GetComponent<UIImageButtonRevised>();
			if ((bool)component)
			{
				component.StateLocked = !Enabled;
			}
			Checkbox.enabled = Enabled;
		}
		UpdateColors();
	}

	public void Enable()
	{
		Enabled = true;
		if ((bool)Checkbox)
		{
			UIImageButtonRevised component = Checkbox.GetComponent<UIImageButtonRevised>();
			if ((bool)component)
			{
				component.StateLocked = !Enabled;
			}
			Checkbox.enabled = Enabled;
		}
		UpdateColors();
	}

	public void Enable(bool state)
	{
		if (state)
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	public void ForceHover()
	{
		UIOptionsTooltip.Show(TooltipString.GetText());
	}

	public void Reload(GameMode mode)
	{
		if (ExpertMode)
		{
			if (mode.Expert)
			{
				Disable();
			}
			else
			{
				Enable();
			}
		}
		if (Option == UIOptionsManager.Option.DIFFICULTY)
		{
			if (UIOptionsManager.Instance.NormalSubWindow.activeSelf)
			{
				if (GameState.Instance.Difficulty == GameDifficulty.PathOfTheDamned)
				{
					Enable(Difficulty == GameDifficulty.PathOfTheDamned);
				}
				else
				{
					Enable(Difficulty != GameDifficulty.PathOfTheDamned);
				}
			}
			else
			{
				Enable();
			}
		}
		UpdateColors();
	}

	private void UpdateColors()
	{
		bool flag = Enabled && (!NotBackerBeta || !Conditionals.s_TestCommandLineArgs.Contains("bb"));
		if (Checkbox != null)
		{
			Checkbox.enabled = flag;
		}
		if ((bool)Label)
		{
			if (flag)
			{
				Label.color = ((GreyLabelWhenDisabled && (bool)Checkbox && !Checkbox.isChecked) ? s_GreyLabelColor : Color.white);
			}
			else
			{
				Label.color = Color.gray;
			}
		}
	}
}
