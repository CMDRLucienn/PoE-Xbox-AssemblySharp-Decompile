using System;
using UnityEngine;

public class UICharacterCreationNavControl : UICharacterCreationElement
{
	public enum NavType
	{
		ACCEPT,
		CANCEL,
		DONE,
		ACCEPT_COLOR,
		EXIT
	}

	public NavType Control;

	public bool ReloadCharacterOnSelect;

	public Collider ExtraCollider;

	private static UIMessageBox s_messageBox;

	private UIMultiSpriteImageButton m_spriteButton;

	public UIMultiSpriteImageButton SpriteButton
	{
		get
		{
			if (m_spriteButton == null)
			{
				m_spriteButton = GetComponent<UIMultiSpriteImageButton>();
			}
			return m_spriteButton;
		}
	}

	protected override void Start()
	{
		base.Start();
		if ((bool)ExtraCollider)
		{
			UIEventListener uIEventListener = UIEventListener.Get(ExtraCollider.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnExtraClick));
		}
		Refresh();
	}

	private void OnExtraClick(GameObject go)
	{
		OnClick();
	}

	public void Refresh()
	{
		if (Control == NavType.CANCEL)
		{
			SpriteButton.enabled = UICharacterCreationManager.Instance.CanGoBack();
		}
		else if (Control == NavType.EXIT)
		{
			SpriteButton.enabled = UICharacterCreationManager.Instance.ActiveCharacter.StartingLevel > 0 || UICharacterCreationManager.Instance.CreationType != UICharacterCreationManager.CharacterCreationType.LevelUp;
		}
	}

	private void OnClick()
	{
		switch (Control)
		{
		case NavType.ACCEPT:
			UICharacterCreationManager.Instance.PressOkay();
			break;
		case NavType.CANCEL:
			UICharacterCreationManager.Instance.PressBack();
			break;
		case NavType.DONE:
		{
			bool flag = UICharacterCreationManager.Instance.TargetCharacter.GetComponent<CompanionInstanceID>() != null;
			UICharacterCreationManager.Instance.SetActiveController(null);
			bool num = UICharacterCreationManager.Instance.CreationType == UICharacterCreationManager.CharacterCreationType.NewCompanion || UICharacterCreationManager.Instance.CreationType == UICharacterCreationManager.CharacterCreationType.NewPlayer || (UICharacterCreationManager.Instance.CreationType == UICharacterCreationManager.CharacterCreationType.LevelUp && UICharacterCreationManager.Instance.ActiveCharacter.CoreData.Level == 0 && !flag);
			int remainingAttributePoints = UICharacterCreationManager.Instance.GetRemainingAttributePoints(UICharacterCreationManager.Instance.ActiveCharacter);
			if (num && remainingAttributePoints > 0)
			{
				if (s_messageBox == null)
				{
					string text2 = GUIUtils.GetText(1749);
					string text3 = GUIUtils.GetText(1750);
					UIMessageBox uIMessageBox2 = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, text2, StringUtility.Format(text3, remainingAttributePoints));
					uIMessageBox2.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox2.OnDialogEnd, new UIMessageBox.OnEndDialog(OnCompleteConfirm));
					s_messageBox = uIMessageBox2;
				}
			}
			else
			{
				UICharacterCreationManager.Instance.HandleCharacterCreationComplete();
			}
			break;
		}
		case NavType.ACCEPT_COLOR:
		{
			UIColorSelectorLine uIColorSelectorLine = UnityEngine.Object.FindObjectOfType<UIColorSelectorLine>();
			if (uIColorSelectorLine != null)
			{
				uIColorSelectorLine.Hide();
			}
			break;
		}
		case NavType.EXIT:
			if (s_messageBox == null)
			{
				string title;
				string text;
				switch (UICharacterCreationManager.Instance.CreationType)
				{
				case UICharacterCreationManager.CharacterCreationType.NewPlayer:
					title = GUIUtils.GetText(1557);
					text = GUIUtils.GetText(1558, UICharacterCreationManager.Instance.ActiveCharacter.Gender);
					break;
				case UICharacterCreationManager.CharacterCreationType.LevelUp:
					title = GUIUtils.GetText(1559);
					text = GUIUtils.Format(UICharacterCreationManager.Instance.ActiveCharacter.Gender, 1560, UICharacterCreationManager.Instance.ActiveCharacter.Name, UICharacterCreationManager.Instance.EndingLevel);
					break;
				case UICharacterCreationManager.CharacterCreationType.NewCompanion:
					title = GUIUtils.GetText(1561);
					text = GUIUtils.GetText(1562, UICharacterCreationManager.Instance.ActiveCharacter.Gender);
					break;
				default:
					title = "";
					text = "";
					break;
				}
				UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.YESNO_CANCEL, title, text);
				uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, new UIMessageBox.OnEndDialog(OnExitConfirm));
				s_messageBox = uIMessageBox;
			}
			break;
		}
		if (ReloadCharacterOnSelect)
		{
			base.Owner.SignalValueChanged(ValueType.Color);
		}
	}

	private void OnExitConfirm(UIMessageBox.Result result, UIMessageBox owner)
	{
		owner.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Remove(owner.OnDialogEnd, new UIMessageBox.OnEndDialog(OnExitConfirm));
		s_messageBox = null;
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			UICharacterCreationManager.Instance.HandleCharacterCreationCancel();
		}
	}

	private void OnCompleteConfirm(UIMessageBox.Result result, UIMessageBox owner)
	{
		owner.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Remove(owner.OnDialogEnd, new UIMessageBox.OnEndDialog(OnCompleteConfirm));
		s_messageBox = null;
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			UICharacterCreationManager.Instance.HandleCharacterCreationComplete();
		}
	}
}
