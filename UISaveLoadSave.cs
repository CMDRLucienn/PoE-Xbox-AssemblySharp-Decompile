using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UISaveLoadSave : MonoBehaviour
{
	private UISaveLoadPlaythroughGroup m_Parent;

	public UITexture ScreenshotTexture;

	public UILabel AreaName;

	public UILabel PlayTime;

	public UILabel SaveDate;

	public UIWidget SelectedFrame;

	public UIInput InputUserSaveName;

	public GameObject InputHighlightParent;

	public GameObject Collider;

	public UIWidget TrialOfIronIcon;

	public UIGrid ButtonGrid;

	public UIMultiSpriteImageButton SaveLoadButton;

	public UIMultiSpriteImageButton DeleteButton;

	public UIGrid PartyMemberGrid;

	public GameObject RootPartyMember;

	private List<UITexture> m_Portraits = new List<UITexture>();

	private SaveGameInfo m_Metadata;

	private string m_CurUserString = string.Empty;

	private string m_LastUpdateUserString = string.Empty;

	private bool m_IsNew;

	private bool m_InputSelected;

	public int Height => (int)Collider.transform.localScale.y;

	public bool ShowSaveLoad
	{
		get
		{
			if (!m_IsNew)
			{
				if (!UISaveLoadManager.Instance.SaveMode || !(GameState.s_playerCharacter.SessionID != m_Metadata.SessionID))
				{
					if (UISaveLoadManager.Instance.LoadMode && GameState.Mode.TrialOfIron)
					{
						return !m_Metadata.FileName.Equals(GameState.LoadedFileName);
					}
					return true;
				}
				return false;
			}
			return true;
		}
	}

	public bool CanSaveLoad
	{
		get
		{
			if (!UISaveLoadManager.Instance.SaveMode || InGameHUD.Instance.QuicksaveAllowed)
			{
				return ShowSaveLoad;
			}
			return false;
		}
	}

	public SaveGameInfo MetaData => m_Metadata;

	public bool IsNew => m_IsNew;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Collider.gameObject);
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnHover));
		UIEventListener uIEventListener2 = UIEventListener.Get(Collider.gameObject);
		uIEventListener2.onDoubleClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onDoubleClick, new UIEventListener.VoidDelegate(OnDoubleClick));
		UIMultiSpriteImageButton saveLoadButton = SaveLoadButton;
		saveLoadButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(saveLoadButton.onClick, new UIEventListener.VoidDelegate(OnButtonClicked));
		UIMultiSpriteImageButton deleteButton = DeleteButton;
		deleteButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(deleteButton.onClick, new UIEventListener.VoidDelegate(OnDeleteClicked));
		InputUserSaveName.onSubmit = OnNameChangeSubmit;
		InputUserSaveName.OnSelectedChanged += OnInputSelectChanged;
		SelectedFrame.alpha = 0f;
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnHover(GameObject sender, bool over)
	{
		SelectedFrame.alpha = (over ? 1 : 0);
	}

	private void OnDoubleClick(GameObject sender)
	{
		OnButtonClicked(sender);
	}

	private void OnButtonClicked(GameObject sender)
	{
		if (!CanSaveLoad)
		{
			return;
		}
		if (InputUserSaveName.selected)
		{
			InputUserSaveName.selected = false;
		}
		if (UISaveLoadManager.Instance.SaveMode)
		{
			if (m_IsNew)
			{
				if (GameState.Mode.TrialOfIron)
				{
					UISaveLoadManager.Instance.HideWindow();
					GameState.LoadMainMenu(fadeOut: true);
				}
				else
				{
					GameResources.SaveGame(SaveGameInfo.GetSaveFileName(), m_CurUserString);
					UISaveLoadManager.Instance.HideWindow();
				}
			}
			else
			{
				UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, "", GUIUtils.Format(1452, m_Metadata.PlayerName, m_Metadata.RealTimestamp.ToShortDateString()));
				uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, new UIMessageBox.OnEndDialog(OnSaveOverEnd));
			}
		}
		else if (UISaveLoadManager.Instance.LoadMode && !FadeManager.Instance.IsFadeActive())
		{
			FadeManager instance = FadeManager.Instance;
			instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(LoadGameOnFadeEnd));
			FadeManager.Instance.FadeToBlack(FadeManager.FadeType.AreaTransition, 0.35f, AudioFadeMode.MusicAndFx);
		}
	}

	private void OnNameChangeSubmit(string newSaveName)
	{
		GetInputStringAsUserString();
		if (m_Metadata != null && m_CurUserString != m_LastUpdateUserString)
		{
			GameResources.UpdateUserStringSaveFile(m_Metadata, m_CurUserString);
			m_LastUpdateUserString = m_CurUserString;
		}
	}

	private void OnInputSelectChanged(UIInput source, bool willBeSelected)
	{
		m_InputSelected = willBeSelected;
		if (!m_InputSelected)
		{
			GetInputStringAsUserString();
		}
		UpdateSaveNameField();
	}

	private void GetInputStringAsUserString()
	{
		string text = InputUserSaveName.text;
		if (!m_IsNew || string.Compare(text, GUIUtils.GetText(569)) != 0)
		{
			m_CurUserString = InputUserSaveName.text;
		}
		else
		{
			m_CurUserString = string.Empty;
		}
	}

	private void LoadGameOnFadeEnd()
	{
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(LoadGameOnFadeEnd));
		UISaveLoadManager.Instance.HideWindow();
		if (!GameResources.LoadGame(m_Metadata.FileName))
		{
			FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.AreaTransition, 0.35f);
		}
	}

	private void OnSaveOverEnd(UIMessageBox.Result result, UIMessageBox sender)
	{
		if (result != 0)
		{
			return;
		}
		if (GameState.Mode.TrialOfIron)
		{
			UISaveLoadManager.Instance.HideWindow();
			GameState.LoadMainMenu(fadeOut: true);
			return;
		}
		if (GameResources.SaveGame(SaveGameInfo.GetSaveFileName(), m_CurUserString))
		{
			GameResources.DeleteSavedGame(m_Metadata.FileName);
		}
		UISaveLoadManager.Instance.HideWindow();
	}

	private void OnDeleteClicked(GameObject sender)
	{
		UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, "", GUIUtils.Format(1451, m_Metadata.PlayerName, m_Metadata.RealTimestamp.ToShortDateString()));
		uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, new UIMessageBox.OnEndDialog(OnDeleteEnd));
	}

	private void OnDeleteEnd(UIMessageBox.Result result, UIMessageBox sender)
	{
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			FindParent();
			GameResources.DeleteSavedGame(m_Metadata.FileName);
			m_Parent.RemoveSaveFile(this);
		}
	}

	private void FindParent()
	{
		Transform parent = base.transform.parent;
		while ((bool)parent && !m_Parent)
		{
			m_Parent = parent.GetComponent<UISaveLoadPlaythroughGroup>();
			parent = parent.parent;
		}
	}

	private void UpdateSaveNameField()
	{
		InputHighlightParent.gameObject.SetActive(m_IsNew || m_InputSelected);
		StringBuilder stringBuilder = new StringBuilder(m_CurUserString);
		if (m_InputSelected)
		{
			InputUserSaveName.maxChars = 50;
		}
		else
		{
			InputUserSaveName.maxChars = 0;
			if (m_IsNew)
			{
				stringBuilder.Append(string.IsNullOrEmpty(m_CurUserString) ? GUIUtils.GetText(569) : string.Empty);
			}
			else if (string.IsNullOrEmpty(m_CurUserString))
			{
				stringBuilder.Append((m_Metadata.SceneTitleId >= 0) ? StringTableManager.GetText(DatabaseString.StringTableType.Maps, m_Metadata.SceneTitleId) : m_Metadata.SceneTitle);
			}
			else
			{
				stringBuilder.AppendGuiFormat(1731, (m_Metadata.SceneTitleId >= 0) ? StringTableManager.GetText(DatabaseString.StringTableType.Maps, m_Metadata.SceneTitleId) : m_Metadata.SceneTitle);
			}
			if (!m_IsNew && m_Metadata != null)
			{
				if (m_Metadata.IsQuickSave())
				{
					stringBuilder.AppendGuiFormat(1731, GUIUtils.GetText(861).ToUpper());
				}
				else if (m_Metadata.IsAutoSave())
				{
					stringBuilder.AppendGuiFormat(1731, GUIUtils.GetText(1537).ToUpper());
				}
				else if (m_Metadata.IsBug())
				{
					stringBuilder.AppendGuiFormat(1731, "*BUG*");
				}
				else if (m_Metadata.IsPointOfNoReturnSave())
				{
					stringBuilder.AppendGuiFormat(1731, GUIUtils.GetText(1885).ToUpper());
				}
			}
		}
		InputUserSaveName.text = stringBuilder.ToString();
	}

	public void LoadMetadata(SaveGameInfo info)
	{
		SaveLoadButton.Label.text = UISaveLoadManager.Instance.GetButtonText();
		m_Metadata = info;
		m_CurUserString = m_Metadata.UserSaveName;
		m_LastUpdateUserString = m_CurUserString;
		ScreenshotTexture.mainTexture = info.Screenshot;
		PlayTime.text = GUIUtils.Format(554, info.Chapter, info.Playtime.FormatNonZero(2));
		int num = info.RealtimePlayDurationSeconds / 3600;
		int num2 = 0;
		int num3 = 0;
		if (num >= 10000)
		{
			num = 9999;
			num2 = 59;
			num3 = 59;
		}
		else
		{
			num2 = info.RealtimePlayDurationSeconds % 3600 / 60;
			num3 = info.RealtimePlayDurationSeconds % 60;
		}
		SaveDate.text = StringUtility.Format("{0} - {1} (" + GUIUtils.GetText(1914) + ")", info.RealTimestamp.ToShortDateString(), info.RealTimestamp.ToShortTimeString(), num, num2, num3);
		Texture2D[] partyPortraits = info.PartyPortraits;
		foreach (Texture2D texture2D in partyPortraits)
		{
			if (texture2D != null)
			{
				UITexture uITexture = NGUITools.AddChild(PartyMemberGrid.gameObject, RootPartyMember).GetComponentsInChildren<UITexture>(includeInactive: true)[0];
				uITexture.material = null;
				uITexture.mainTexture = texture2D;
				m_Portraits.Add(uITexture);
			}
		}
		RootPartyMember.SetActive(value: false);
		SaveLoadButton.gameObject.SetActive(ShowSaveLoad);
		SaveLoadButton.enabled = CanSaveLoad;
		DeleteButton.gameObject.SetActive(value: true);
		ButtonGrid.Reposition();
		PartyMemberGrid.Reposition();
		TrialOfIronIcon.gameObject.SetActive(info.TrialOfIron);
		UpdateSaveNameField();
	}

	public void SetNew()
	{
		SaveLoadButton.Label.text = UISaveLoadManager.Instance.GetButtonText();
		m_IsNew = true;
		m_Metadata = null;
		InputHighlightParent.SetActive(value: true);
		InputUserSaveName.defaultText = GUIUtils.GetText(569);
		InputUserSaveName.enabled = true;
		m_CurUserString = string.Empty;
		m_LastUpdateUserString = string.Empty;
		ScreenshotTexture.alpha = 0f;
		PlayTime.alpha = 0f;
		SaveDate.alpha = 0f;
		RootPartyMember.gameObject.SetActive(value: false);
		TrialOfIronIcon.gameObject.SetActive(GameState.Mode.TrialOfIron);
		SaveLoadButton.gameObject.SetActive(ShowSaveLoad);
		SaveLoadButton.enabled = CanSaveLoad;
		DeleteButton.gameObject.SetActive(value: false);
		ButtonGrid.Reposition();
	}
}
