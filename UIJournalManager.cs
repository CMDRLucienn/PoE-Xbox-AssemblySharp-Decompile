using System;
using System.Collections.Generic;
using OEIFormats.FlowCharts.Quests;
using UnityEngine;

public class UIJournalManager : UIHudWindow
{
	public enum JournalScreen
	{
		QUESTS,
		BIOGRAPHY,
		CYCLOPEDIA,
		NOTES,
		Count
	}

	public delegate void BestiarySelectionChanged(BestiaryReference newref);

	private JournalScreen m_CurrentScreen = JournalScreen.Count;

	private ITreeListContent[] m_ContentListContentByPage = new ITreeListContent[4];

	public UIMultiSpriteImageButton TabButtonPrefab;

	public float TabButtonSpace;

	public int[] TabButtonLabels;

	private UIMultiSpriteImageButton[] m_TabButtons = new UIMultiSpriteImageButton[0];

	public UIRadioButtonGroup TabButtonGroup;

	public UIDraggablePanel ContentPanel;

	public Color QuestObjectiveDisabledColor;

	public Color QuestObjectiveColor;

	public Color QuestObjectiveTitleColor;

	public Color QuestTextColor;

	public UITreeList ContentList;

	public UILabel TitleLabel;

	public GameObject ContentQuest;

	public UIJournalContentText ContentQuestDescLabel;

	public GameObject QuestObjectivePrefab;

	public int QuestObjectiveSpacing = 22;

	public GameObject ContentJournal;

	public UIJournalBioText ContentJournalText;

	public GameObject ContentGlossary;

	public UIJournalContentText ContentGlossaryBodyLabel;

	public UIJournalContentText ContentGlossaryLinksLabel;

	public GameObject ContentBestiary;

	public UIJournalSimpleBestiary ContentBestiaryParent;

	public GameObject LayoutBestiaryWithImage;

	public GameObject LayoutBestiaryNoImage;

	public GameObject BestiaryDescription;

	public GameObject ContentCyclopedia;

	public UIJournalCyclopediaContent CyclopediaContent;

	public GameObject ContentNote;

	public UIJournalContentText ContentNoteLabel;

	private List<UIJournalQuestObjective> m_QuestObjectives = new List<UIJournalQuestObjective>();

	public BestiarySelectionChanged OnBestiarySelectionChanged;

	private ITreeListContent[] m_Selections = new ITreeListContent[4];

	private bool m_Init;

	public static UIJournalManager Instance { get; private set; }

	public BestiaryReference CyclopediaCurrentBestiary { get; private set; }

	public override int CyclePosition => 2;

	public void OnCreateNewNote(GameObject sender1)
	{
		UIStringPromptBox uIStringPromptBox = UIWindowManager.ShowStringPrompt(1886, "");
		uIStringPromptBox.Input.defaultText = "";
		uIStringPromptBox.OnDialogEnd = (UIStringPromptBox.OnEndDialog)Delegate.Combine(uIStringPromptBox.OnDialogEnd, (UIStringPromptBox.OnEndDialog)delegate(UIMessageBox.Result result, UIStringPromptBox sender)
		{
			if (result == UIMessageBox.Result.AFFIRMATIVE)
			{
				NotesPage notesPage = NotesManager.Instance.NewNote();
				if (!string.IsNullOrEmpty(sender.Text.text))
				{
					notesPage.UserTitle = sender.Text.text;
				}
				else
				{
					notesPage.LocalizedTitle = new GUIDatabaseString(171);
				}
				RefreshItems();
				ContentList.SelectItem(notesPage);
			}
		});
	}

	public void SetSelectedItem(ITreeListContent item)
	{
		SetSelectedItem(item, m_CurrentScreen);
	}

	public ITreeListContent GetSelectedItem()
	{
		return GetSelectedItem(m_CurrentScreen);
	}

	public void SetSelectedItem(ITreeListContent item, JournalScreen page)
	{
		if (page < JournalScreen.Count)
		{
			m_Selections[(int)page] = item;
			if (page == m_CurrentScreen)
			{
				SetContent(item);
				ContentList.SelectItem(item);
			}
		}
	}

	public ITreeListContent GetSelectedItem(JournalScreen page)
	{
		if (page < JournalScreen.Count)
		{
			return m_Selections[(int)page];
		}
		return null;
	}

	private void Awake()
	{
		Instance = this;
		GameResources.OnLoadedSave += OnLoadedSave;
	}

	private void OnLoadedSave()
	{
		for (int i = 0; i < m_Selections.Length; i++)
		{
			m_Selections[i] = null;
		}
	}

	private void Start()
	{
		QuestObjectivePrefab.gameObject.SetActive(value: false);
		m_QuestObjectives.Add(QuestObjectivePrefab.GetComponent<UIJournalQuestObjective>());
		m_TabButtons = new UIMultiSpriteImageButton[4];
		m_TabButtons[0] = TabButtonPrefab;
		for (int i = 0; i < m_TabButtons.Length; i++)
		{
			if (i > 0)
			{
				m_TabButtons[i] = NGUITools.AddChild(TabButtonPrefab.transform.parent.gameObject, TabButtonPrefab.gameObject).GetComponent<UIMultiSpriteImageButton>();
				m_TabButtons[i].transform.localPosition = TabButtonPrefab.transform.localPosition + new Vector3(TabButtonSpace * (float)i, 0f, 0f);
			}
			m_TabButtons[i].Label.GetComponent<GUIStringLabel>().SetString(TabButtonLabels[i]);
		}
		UIMultiSpriteImageButton obj = m_TabButtons[0];
		obj.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(obj.onClick, new UIEventListener.VoidDelegate(OnQuests));
		UIMultiSpriteImageButton obj2 = m_TabButtons[1];
		obj2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(obj2.onClick, new UIEventListener.VoidDelegate(OnBiography));
		UIMultiSpriteImageButton obj3 = m_TabButtons[2];
		obj3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(obj3.onClick, new UIEventListener.VoidDelegate(OnCyclopedia));
		UIMultiSpriteImageButton obj4 = m_TabButtons[3];
		obj4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(obj4.onClick, new UIEventListener.VoidDelegate(OnNotes));
		m_ContentListContentByPage[0] = new JournalTreeListQuestPage();
		m_ContentListContentByPage[1] = new JournalTreeListBiographyPage();
		m_ContentListContentByPage[2] = new JournalTreeListCyclopediaPage();
		m_ContentListContentByPage[3] = new JournalTreeListNotesPage();
		ContentList.OnSelectedItemChanged += OnSelectedItemChanged;
		UITable[] componentsInChildren = ContentBestiary.GetComponentsInChildren<UITable>(includeInactive: true);
		foreach (UITable obj5 in componentsInChildren)
		{
			obj5.onReposition = (UITable.OnReposition)Delegate.Combine(obj5.onReposition, new UITable.OnReposition(OnBestiaryTableReposition));
		}
		ChangeScreen(JournalScreen.QUESTS);
		ClearContent();
		ContentNoteLabel.ContentUpdated += OnNoteTextUpdate;
	}

	private void OnSelectedItemChanged(UITreeList sender, UITreeListItem selected)
	{
		SetSelectedItem(selected ? selected.Data : null);
	}

	public void ClearContent(GameObject except = null)
	{
		ContentNote.SetActive(except == ContentNote);
		ContentBestiary.SetActive(except == ContentBestiary);
		ContentBestiaryParent.gameObject.SetActive(except == ContentBestiaryParent.gameObject);
		ContentCyclopedia.SetActive(except == ContentCyclopedia);
		ContentQuest.SetActive(except == ContentQuest);
		ContentGlossary.SetActive(except == ContentGlossary);
		ContentJournal.SetActive(except == ContentJournal);
		ContentJournalText.Load(UIJournalBioText.Mode.None);
		TitleLabel.text = "";
	}

	public void RefreshContent()
	{
		SetContent(GetSelectedItem());
	}

	public void RefreshItems()
	{
		if ((int)m_CurrentScreen < m_ContentListContentByPage.Length)
		{
			ContentList.Load(m_ContentListContentByPage[(int)m_CurrentScreen]);
		}
		else
		{
			ContentList.Load(null);
		}
	}

	public void HintShowQuest(Quest quest)
	{
	}

	public void SetContent(ITreeListContent content)
	{
		ContentNoteLabel.SetClear();
		if (content is UIJournalBioText.BiographyCategory)
		{
			SetContentBiography(((UIJournalBioText.BiographyCategory)content).mode);
		}
		else if (content is Quest)
		{
			SetContentQuest((Quest)content);
		}
		else if (content is NotesPage)
		{
			SetContentNote((NotesPage)content);
		}
		else if (content is BestiaryReference)
		{
			SetContentBestiary((BestiaryReference)content);
		}
		else if (content is BestiaryParent)
		{
			SetContentBestiaryParent((BestiaryParent)content);
		}
		else if (content is CyclopediaEntry)
		{
			SetContentCyclopedia((CyclopediaEntry)content);
		}
		else if (content is GlossaryEntry)
		{
			SetContentGlossary((GlossaryEntry)content);
		}
		else
		{
			ClearContent();
		}
	}

	public void SetContentQuest(Quest quest)
	{
		if (quest.IsQuestFailed())
		{
			ClearContent();
			return;
		}
		bool activeSelf = ContentQuest.activeSelf;
		if (quest == null)
		{
			ClearContent();
			return;
		}
		ClearContent(ContentQuest);
		if (!activeSelf)
		{
			ContentPanel.ResetPosition();
		}
		else
		{
			ContentPanel.InvalidateBounds();
		}
		if (GetSelectedItem() != null && GetSelectedItem() != quest)
		{
			foreach (UIJournalQuestObjective questObjective in m_QuestObjectives)
			{
				questObjective.Expanded = false;
			}
		}
		TitleLabel.text = " " + quest.GetQuestTitle() + " ";
		string questEndState = quest.GetQuestEndState();
		if (!quest.IsQuestFailed())
		{
			if (!string.IsNullOrEmpty(questEndState))
			{
				ContentQuestDescLabel.SetText(quest.GetQuestDescription() + Environment.NewLine + Environment.NewLine + questEndState);
			}
			else
			{
				ContentQuestDescLabel.SetText(quest.GetQuestDescription());
			}
		}
		else if (quest.IsStarted())
		{
			ContentQuestDescLabel.SetText(quest.GetQuestDescription() + Environment.NewLine + Environment.NewLine + questEndState);
		}
		else
		{
			ContentQuestDescLabel.SetText(questEndState);
		}
		List<int> list = new List<int>();
		for (int i = 0; i <= quest.GetHighestNodeID(); i++)
		{
			if (quest.GetNode(i) is ObjectiveNode && (quest.IsQuestStateActive(i) || QuestManager.Instance.IsStateVisited(quest, i)))
			{
				list.Add(i);
			}
		}
		Comparison<int> comparison = (int x, int y) => (QuestManager.Instance.GetStateTimestamp(quest, y) == null) ? 1 : QuestManager.Instance.GetStateTimestamp(quest, y).CompareTo(QuestManager.Instance.GetStateTimestamp(quest, x));
		list.Sort(comparison);
		int num = 0;
		float num2 = (ContentQuestDescLabel.Label.relativeSize.y + 1f) * ContentQuestDescLabel.transform.localScale.y - ContentQuestDescLabel.transform.localPosition.y;
		foreach (int item in list)
		{
			num++;
			if (num > m_QuestObjectives.Count)
			{
				UIJournalQuestObjective component = UnityEngine.Object.Instantiate(QuestObjectivePrefab).GetComponent<UIJournalQuestObjective>();
				component.transform.parent = QuestObjectivePrefab.transform.parent;
				component.transform.localPosition = QuestObjectivePrefab.transform.localPosition;
				component.transform.localScale = new Vector3(1f, 1f, 1f);
				m_QuestObjectives.Add(component);
			}
			UIJournalQuestObjective uIJournalQuestObjective = m_QuestObjectives[num - 1];
			uIJournalQuestObjective.SetContent(quest, item);
			uIJournalQuestObjective.gameObject.SetActive(value: true);
			uIJournalQuestObjective.transform.localPosition = new Vector3(uIJournalQuestObjective.transform.localPosition.x, 0f - num2, uIJournalQuestObjective.transform.localPosition.z);
			num2 += uIJournalQuestObjective.ContentHeight + (float)QuestObjectiveSpacing;
		}
		for (int j = num; j < m_QuestObjectives.Count; j++)
		{
			m_QuestObjectives[j].gameObject.SetActive(value: false);
		}
		if (!activeSelf)
		{
			ContentPanel.ResetPosition();
		}
		else
		{
			ContentPanel.InvalidateBounds();
		}
	}

	public void SetContentNote(NotesPage note)
	{
		if (note == null)
		{
			ClearContent();
			return;
		}
		ClearContent(ContentNote);
		TitleLabel.text = " " + note.DisplayTitle + " ";
		ContentNoteLabel.SetNote(note);
		ContentPanel.ResetPosition();
		OnNoteTextUpdate(ContentNoteLabel);
	}

	public void SetContentBestiary(BestiaryReference reference)
	{
		if (reference == null)
		{
			ClearContent();
			return;
		}
		ClearContent(ContentBestiary);
		TitleLabel.text = CharacterStats.Name(reference.gameObject);
		CyclopediaCurrentBestiary = reference;
		if (OnBestiarySelectionChanged != null)
		{
			OnBestiarySelectionChanged(reference);
		}
		bool flag = !string.IsNullOrEmpty(CyclopediaCurrentBestiary.PicturePath);
		LayoutBestiaryNoImage.SetActive(!flag);
		LayoutBestiaryWithImage.SetActive(flag);
		UITable[] componentsInChildren = ContentBestiary.GetComponentsInChildren<UITable>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Reposition();
		}
		ContentPanel.ResetPosition();
	}

	public void SetContentBestiaryParent(BestiaryParent reference)
	{
		if (reference == null)
		{
			ClearContent();
			return;
		}
		ClearContent(ContentBestiaryParent.gameObject);
		TitleLabel.text = reference.Name.GetText(Gender.Neuter);
		ContentBestiaryParent.Load(reference);
		ContentPanel.ResetPosition();
	}

	private void OnBestiaryTableReposition()
	{
		float num = 0f;
		if (LayoutBestiaryWithImage.activeSelf)
		{
			foreach (Transform item in LayoutBestiaryWithImage.transform)
			{
				num = Mathf.Min(num, NGUIMath.CalculateRelativeWidgetBounds(item).min.y);
			}
		}
		if (LayoutBestiaryNoImage.activeSelf)
		{
			foreach (Transform item2 in LayoutBestiaryNoImage.transform)
			{
				num = Mathf.Min(num, NGUIMath.CalculateRelativeWidgetBounds(item2).min.y);
			}
		}
		BestiaryDescription.transform.localPosition = new Vector3(BestiaryDescription.transform.localPosition.x, num - 20f, BestiaryDescription.transform.localPosition.z);
	}

	public void SetContentBiography(UIJournalBioText.Mode mode)
	{
		ClearContent(ContentJournal);
		TitleLabel.text = CharacterStats.Name(GameState.s_playerCharacter);
		ContentJournalText.Load(mode);
	}

	public void SetContentGlossary(GlossaryEntry term)
	{
		if (term == null)
		{
			ClearContent();
			return;
		}
		ClearContent(ContentGlossary);
		TitleLabel.text = " " + term.Title.ToString();
		string text = (term.Body.IsValidString ? term.Body.GetText() : string.Empty);
		ContentGlossaryBodyLabel.SetText(Glossary.Instance.AddUrlTags(text, term, carefulReplace: false));
		string text2 = string.Empty;
		GlossaryEntry[] visibleLinkedEntries = term.VisibleLinkedEntries;
		if (visibleLinkedEntries.Length != 0)
		{
			text2 = ((!string.IsNullOrEmpty(text)) ? (text2 + GUIUtils.GetText(1654)) : ((visibleLinkedEntries.Length <= 1) ? GUIUtils.Format(1702, Glossary.Instance.AddUrlTags(visibleLinkedEntries[0].Title.GetText())) : (text2 + GUIUtils.GetText(1703))));
			if (!string.IsNullOrEmpty(text) || visibleLinkedEntries.Length > 1)
			{
				text2 += " ";
				for (int i = 0; i < visibleLinkedEntries.Length; i++)
				{
					GlossaryEntry glossaryEntry = visibleLinkedEntries[i];
					text2 += Glossary.Instance.AddUrlTags(glossaryEntry.Title.GetText());
					if (i < visibleLinkedEntries.Length - 1)
					{
						text2 += ", ";
					}
				}
			}
		}
		ContentGlossaryLinksLabel.SetText(text2);
		ContentPanel.ResetPosition();
	}

	public void SetContentCyclopedia(CyclopediaEntry cyclo)
	{
		if (cyclo == null)
		{
			ClearContent();
			return;
		}
		ClearContent(ContentCyclopedia);
		CyclopediaContent.Load(cyclo);
		ContentPanel.ResetPosition();
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameResources.OnLoadedSave -= OnLoadedSave;
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override void HandleInput()
	{
		int num = (int)(m_CurrentScreen + 1);
		if (num >= 4)
		{
			num = 0;
		}
		if (GameInput.GetControlDownWithRepeat(MappedControl.NEXT_TAB, handle: true))
		{
			ChangeScreen((JournalScreen)num);
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
		}
		if (m_CurrentScreen == JournalScreen.NOTES && GetSelectedItem() != null && GameInput.GetKeyUp(KeyCode.Delete) && GetSelectedItem() is NotesPage notesPage)
		{
			UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, "", GUIUtils.Format(1920, notesPage.DisplayTitle));
			uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, new UIMessageBox.OnEndDialog(OnDeleteNote));
		}
		base.HandleInput();
	}

	private void Update()
	{
		if (!m_Init)
		{
			m_Init = true;
			SelectTab(m_CurrentScreen);
			Hide(forced: false);
		}
	}

	private void OnDeleteNote(UIMessageBox.Result result, UIMessageBox owner)
	{
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			NotesManager.Instance.DeleteNote(GetSelectedItem() as NotesPage);
			SetSelectedItem(null, JournalScreen.NOTES);
			RefreshItems();
		}
	}

	private void OnNoteTextUpdate(UIJournalContentText source)
	{
		ContentPanel.InvalidateBounds();
		ContentPanel.SetScroll(0f - ContentPanel.bounds.size.y - 30f);
		ContentPanel.RestrictWithinBounds(instant: true);
	}

	public void ShowGlossaryEntry(GlossaryEntry entry)
	{
		ChangeScreen(JournalScreen.CYCLOPEDIA);
		ShowWindow();
		SetSelectedItem(entry, JournalScreen.CYCLOPEDIA);
	}

	public void ShowBestiaryReference(BestiaryReference prefab)
	{
		ChangeScreen(JournalScreen.CYCLOPEDIA);
		ShowWindow();
		SetSelectedItem(prefab, JournalScreen.CYCLOPEDIA);
	}

	protected override void Show()
	{
		GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.WindowShowJournal);
		RefreshItems();
		SetSelectedItem(GetSelectedItem());
	}

	public void ChangeScreen(JournalScreen newscreen)
	{
		if (newscreen != m_CurrentScreen)
		{
			m_CurrentScreen = newscreen;
			SelectTab(newscreen);
			RefreshItems();
			SetSelectedItem(GetSelectedItem());
		}
	}

	private void SelectLatestQuest()
	{
		SetSelectedItem(QuestManager.Instance.LastUpdatedQuest, JournalScreen.QUESTS);
	}

	private void OnQuests(GameObject go)
	{
		ChangeScreen(JournalScreen.QUESTS);
	}

	private void OnBiography(GameObject go)
	{
		ChangeScreen(JournalScreen.BIOGRAPHY);
	}

	private void OnCyclopedia(GameObject go)
	{
		ChangeScreen(JournalScreen.CYCLOPEDIA);
	}

	private void OnNotes(GameObject go)
	{
		ChangeScreen(JournalScreen.NOTES);
	}

	private void SelectTab(JournalScreen screen)
	{
		if ((int)screen < m_TabButtons.Length)
		{
			TabButtonGroup.DoSelect(m_TabButtons[(int)screen].gameObject);
		}
	}
}
