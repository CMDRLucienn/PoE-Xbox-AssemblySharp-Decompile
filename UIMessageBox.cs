using System;
using UnityEngine;

public class UIMessageBox : UIHudWindow
{
	public enum Result
	{
		AFFIRMATIVE,
		NEGATIVE,
		CANCEL,
		NONE
	}

	public enum ButtonStyle
	{
		ACCEPTCANCEL,
		YESNO,
		OK,
		LITE,
		YESNO_CANCEL
	}

	public delegate void OnEndDialog(Result result, UIMessageBox sender);

	public UIWidget[] DebugDisable;

	private int m_TwoButtonOffset = 140;

	public ButtonStyle ButtonLayout = ButtonStyle.OK;

	public GameObject CheckboxParent;

	public UICheckbox CheckboxComponent;

	public UILabel CheckboxLabel;

	public GameObject NumericParent;

	public UILabel NumericComponent;

	public UISprite NumericUpButton;

	public UISprite NumericDownButton;

	private int m_NumericValue;

	public KeyCode NumericUpKey = KeyCode.D;

	public KeyCode NumericDownKey = KeyCode.A;

	private int m_NumericMin;

	private int m_NumericMax = int.MaxValue;

	public OnEndDialog OnDialogEnd;

	private Result m_Result = Result.NONE;

	public UILabel Title;

	public UILabel Text;

	public UIMultiSpriteImageButton[] Buttons = new UIMultiSpriteImageButton[2];

	public int NumericValue
	{
		get
		{
			if (!NumericParent || !NumericParent.activeSelf)
			{
				return 0;
			}
			return m_NumericValue;
		}
	}

	public bool CheckboxActive
	{
		get
		{
			if ((bool)CheckboxParent && CheckboxParent.activeSelf)
			{
				return CheckboxComponent.isChecked;
			}
			return false;
		}
	}

	public object UserData { get; set; }

	private void Start()
	{
		UIMultiSpriteImageButton obj = Buttons[0];
		obj.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(obj.onClick, new UIEventListener.VoidDelegate(OnButton1));
		UIMultiSpriteImageButton obj2 = Buttons[1];
		obj2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(obj2.onClick, new UIEventListener.VoidDelegate(OnButton2));
		UIEventListener uIEventListener = UIEventListener.Get(NumericUpButton.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnNumericUp));
		UIEventListener uIEventListener2 = UIEventListener.Get(NumericDownButton.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnNumericDown));
		UIWindowManager.Instance.OnWindowHidden += OnWindowHiddenFromManager;
	}

	protected override void OnDestroy()
	{
		if ((bool)UIWindowManager.Instance)
		{
			UIWindowManager.Instance.WindowDestroyed(this);
			UIWindowManager.Instance.OnWindowHidden -= OnWindowHiddenFromManager;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override void HandleInput()
	{
		if (GameInput.GetKeyUp(NumericDownKey))
		{
			OnNumericDown(base.gameObject);
		}
		if (GameInput.GetKeyUp(NumericUpKey))
		{
			OnNumericUp(base.gameObject);
		}
		if (GameInput.GetControlDown(MappedControl.MB_CONFIRM))
		{
			Buttons[0].ForceDown(state: true);
		}
		if (GameInput.GetControlUp(MappedControl.MB_CONFIRM))
		{
			Buttons[0].ForceDown(state: false);
			OnButton1(base.gameObject);
		}
	}

	private void OnNumericUp(GameObject go)
	{
		if ((bool)NumericComponent)
		{
			m_NumericValue = Mathf.Min(m_NumericValue + 1, m_NumericMax);
			if ((bool)GlobalAudioPlayer.Instance)
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.Increment);
			}
			NumericComponent.text = m_NumericValue.ToString();
		}
	}

	private void OnNumericDown(GameObject go)
	{
		if ((bool)NumericComponent)
		{
			m_NumericValue = Mathf.Max(m_NumericValue - 1, m_NumericMin);
			if ((bool)GlobalAudioPlayer.Instance)
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.Decrement);
			}
			NumericComponent.text = m_NumericValue.ToString();
		}
	}

	public void SetCheckBoxLabel(int stringId)
	{
		if ((bool)CheckboxParent)
		{
			CheckboxParent.SetActive(stringId >= 0);
			if ((bool)CheckboxLabel)
			{
				CheckboxLabel.text = GUIUtils.GetText(stringId);
			}
		}
	}

	public void SetNumericValue(int value, int min, int max)
	{
		if ((bool)NumericComponent)
		{
			NumericParent.SetActive(value: true);
			m_NumericMin = min;
			m_NumericMax = max;
			m_NumericValue = Mathf.Clamp(value, min, max);
			NumericComponent.text = m_NumericValue.ToString();
		}
	}

	private void OnButton1(GameObject sender)
	{
		m_Result = Result.AFFIRMATIVE;
		HideWindow();
	}

	private void OnButton2(GameObject sender)
	{
		if (ButtonLayout == ButtonStyle.ACCEPTCANCEL)
		{
			m_Result = Result.CANCEL;
		}
		else
		{
			m_Result = Result.NEGATIVE;
		}
		HideWindow();
	}

	protected override void Show()
	{
		m_Result = Result.CANCEL;
		SetButtons(ButtonLayout);
		UIWidgetUtils.UpdateDependents(base.gameObject, 2);
		OverPanel.Refresh();
	}

	protected override bool Hide(bool forced)
	{
		if (m_Result == Result.NONE)
		{
			if (!forced && ButtonLayout == ButtonStyle.YESNO)
			{
				return false;
			}
			m_Result = Result.CANCEL;
		}
		bool num = base.Hide(forced);
		if (num)
		{
			if ((DimsBackground || DimsBackgroundTemp) && MessageBox)
			{
				UIWindowManager.Instance.SetMessageBoxBackgroundDepth();
			}
			UIWindowManager.Instance.PoolMessageBox(this);
		}
		return num;
	}

	private void OnWindowHiddenFromManager(UIHudWindow window)
	{
		if (window == this && OnDialogEnd != null)
		{
			OnDialogEnd(m_Result, this);
			Reset(clearCallback: false);
		}
	}

	public void SetData(string title, string text)
	{
		Title.text = title;
		Text.text = text;
	}

	public void SetButtons(ButtonStyle buttons)
	{
		if (ButtonLayout != ButtonStyle.LITE)
		{
			ButtonLayout = buttons;
			Buttons[0].GetComponent<UIAnchor>().pixelOffset.x = -m_TwoButtonOffset;
			Buttons[1].GetComponent<UIAnchor>().pixelOffset.x = m_TwoButtonOffset;
			Buttons[1].gameObject.SetActive(value: true);
			switch (ButtonLayout)
			{
			case ButtonStyle.ACCEPTCANCEL:
				Buttons[0].SetText(GUIUtils.GetText(157));
				Buttons[1].SetText(GUIUtils.GetText(158));
				break;
			case ButtonStyle.YESNO:
			case ButtonStyle.YESNO_CANCEL:
				Buttons[0].SetText(GUIUtils.GetText(159));
				Buttons[1].SetText(GUIUtils.GetText(160));
				break;
			case ButtonStyle.OK:
				Buttons[0].SetText(GUIUtils.GetText(161));
				Buttons[0].GetComponent<UIAnchor>().pixelOffset.x = 0f;
				Buttons[1].gameObject.SetActive(value: false);
				break;
			case ButtonStyle.LITE:
				break;
			}
		}
	}

	public void Reset(bool clearCallback)
	{
		CheckboxParent.SetActive(value: false);
		NumericParent.SetActive(value: false);
		if (clearCallback)
		{
			OnDialogEnd = null;
		}
		m_Result = Result.NONE;
		UserData = null;
		Buttons[0].ForceDown(state: false);
		Buttons[1].ForceDown(state: false);
	}

	public void OverrideButtonText(string text1, string text2)
	{
		Buttons[0].SetText(text1);
		Buttons[1].SetText(text2);
	}

	public void SetPosition(Vector3 position)
	{
		GetComponent<UIAnchor>().pixelOffset = position;
	}
}
