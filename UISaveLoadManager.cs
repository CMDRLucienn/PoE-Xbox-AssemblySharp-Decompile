public class UISaveLoadManager : UIHudWindow
{
	public UISaveLoadList SaveList;

	private bool m_LoadMode;

	private bool m_SaveMode = true;

	public UILabel Title;

	public GUIDatabaseString SaveGameTitle;

	public GUIDatabaseString LoadGameTitle;

	public GUIDatabaseString SaveGameButton;

	public GUIDatabaseString LoadGameButton;

	public static UISaveLoadManager Instance { get; private set; }

	public bool LoadMode
	{
		get
		{
			return m_LoadMode;
		}
		set
		{
			m_LoadMode = value;
			m_SaveMode = !value;
		}
	}

	public bool SaveMode
	{
		get
		{
			return m_SaveMode;
		}
		set
		{
			m_SaveMode = value;
			m_LoadMode = !value;
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public string GetButtonText()
	{
		if (SaveMode)
		{
			return SaveGameButton.GetText();
		}
		return LoadGameButton.GetText();
	}

	protected override void Show()
	{
		if (base.AlternateMode)
		{
			SetModeLoad();
		}
		else
		{
			if (GameState.CannotSaveBecauseInCombat || PartyMemberAI.IsPartyMemberUnconscious())
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.GetText(713));
				HideWindow();
				return;
			}
			SetModeSave();
		}
		SaveList.Reload();
	}

	private void SetModeLoad()
	{
		LoadMode = true;
		GUIStringLabel.Get(Title).SetString(LoadGameTitle);
	}

	private void SetModeSave()
	{
		SaveMode = true;
		GUIStringLabel.Get(Title).SetString(SaveGameTitle);
	}
}
