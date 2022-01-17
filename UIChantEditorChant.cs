using System;
using System.Collections.Generic;
using UnityEngine;

public class UIChantEditorChant : UIParentSelectorListener
{
	public UIChantEditorChantButton Button;

	public UIChantEditorPhrase RootTopPhrase;

	public UIChantEditorPhrase RootBottomPhrase;

	public UILabel NameLabel;

	public Collider NameChangeCollider;

	[HideInInspector]
	public UIChantEditorChants Owner;

	public UIWidget TopLeader;

	public UIWidget BottomLeader;

	private List<UIChantEditorPhrase> m_Phrases;

	private Chant m_LoadedChant;

	public UIPanel Panel;

	public UIDraggablePanel DragPanel;

	private Vector3 m_TopLeaderRootScale;

	public int Index;

	private float m_Left;

	public Chant Chant => m_LoadedChant;

	public bool IsNew { get; private set; }

	protected override void Start()
	{
		base.Start();
		Button.Owner = this;
		RootTopPhrase.gameObject.SetActive(value: false);
		RootBottomPhrase.gameObject.SetActive(value: false);
		Init();
		UIEventListener uIEventListener = UIEventListener.Get(NameChangeCollider.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(BeginChangeName));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void BeginChangeName(GameObject sender)
	{
		if ((bool)Chant)
		{
			UIStringPromptBox uIStringPromptBox = UIWindowManager.ShowStringPrompt(1886, GenericAbility.Name(Chant));
			uIStringPromptBox.OnDialogEnd = (UIStringPromptBox.OnEndDialog)Delegate.Combine(uIStringPromptBox.OnDialogEnd, new UIStringPromptBox.OnEndDialog(EndChangeName));
		}
	}

	private void EndChangeName(UIMessageBox.Result result, UIStringPromptBox sender)
	{
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			Chant.OverrideName = sender.ResultString;
			NameLabel.text = GenericAbility.Name(Chant);
		}
	}

	private void OnChildClick(GameObject sender)
	{
		if (!m_LoadedChant || InGameHUD.Instance.CursorMode != 0)
		{
			return;
		}
		UIChantEditorPhrase component = sender.transform.parent.GetComponent<UIChantEditorPhrase>();
		if (!component || component.Disabled)
		{
			return;
		}
		if (GlobalAudioPlayer.Instance != null)
		{
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.RemoveChant);
		}
		int index = m_Phrases.IndexOf(component);
		m_LoadedChant.Phrases.RemoveAt(index);
		m_LoadedChant.Phrases = ArrayExtender.Compress(m_LoadedChant.Phrases);
		if (m_LoadedChant.Phrases.Length == 0)
		{
			ParentSelector.SelectedCharacter.ActiveAbilities.Remove(m_LoadedChant);
			Persistence component2 = m_LoadedChant.GetComponent<Persistence>();
			if ((bool)component2)
			{
				component2.SetForDestroy();
			}
			GameUtilities.Destroy(m_LoadedChant.gameObject);
			Owner.ReloadChants();
		}
		else
		{
			Chant.InstantiatePhrases();
		}
		Reload();
	}

	public void AddPhrase(Phrase p)
	{
		if (GlobalAudioPlayer.Instance != null)
		{
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.AddChant);
		}
		if (IsNew)
		{
			m_LoadedChant = GameResources.Instantiate<Chant>(UIChantEditor.Instance.EmptyChantPrefab);
			m_LoadedChant.OverrideName = GUIUtils.Format(1632, Index + 1);
			m_LoadedChant.UiIndex = Index;
			ParentSelector.SelectedCharacter.ActiveAbilities.Add(m_LoadedChant);
			Chant.Owner = ParentSelector.SelectedCharacter.gameObject;
			Owner.ReloadChants();
			IsNew = false;
		}
		if (Chant.Phrases.Length < 10)
		{
			Phrase[] array = new Phrase[Chant.Phrases.Length + 1];
			Chant.Phrases.CopyTo(array, 0);
			array[array.Length - 1] = p;
			Chant.Phrases = array;
			Chant.InstantiatePhrases();
			Reload();
			float scroll = DragPanel.GetRealMax() - DragPanel.GetVisibleWidth();
			DragPanel.SetScroll(scroll);
		}
	}

	public void Select()
	{
		Owner.SelectionChanged(this);
	}

	private void Init()
	{
		if (m_Phrases == null)
		{
			m_TopLeaderRootScale = TopLeader.transform.localScale;
			m_Left = RootTopPhrase.transform.localPosition.x;
			m_Phrases = new List<UIChantEditorPhrase>();
		}
	}

	private void Reload()
	{
		Init();
		int i = 0;
		float num = m_Left;
		if ((bool)m_LoadedChant)
		{
			Phrase[] phrases = m_LoadedChant.Phrases;
			foreach (Phrase phrase in phrases)
			{
				UIChantEditorPhrase phrase2 = GetPhrase(i);
				phrase2.SetPhrase(phrase);
				phrase2.Disabled = GameState.InCombat;
				phrase2.transform.localPosition = new Vector3(num, phrase2.transform.localPosition.y, phrase2.transform.localPosition.z);
				num += phrase2.Width;
				if (i > 0)
				{
					GetPhrase(i - 1).SubsequentWidth = ((i == m_LoadedChant.Phrases.Length - 1) ? phrase2.EntireWidth : phrase2.Width);
				}
				i++;
			}
		}
		if (i > 0)
		{
			GetPhrase(i - 1).SubsequentWidth = GetPhrase(i - 1).LingerWidth;
		}
		if ((bool)Chant)
		{
			NameLabel.text = GenericAbility.Name(Chant);
		}
		else
		{
			NameLabel.text = "";
		}
		if (i == 0)
		{
			TopLeader.transform.localScale = new Vector3(Panel.clipRange.z, TopLeader.transform.localScale.y, TopLeader.transform.localScale.z);
		}
		else
		{
			TopLeader.transform.localScale = m_TopLeaderRootScale;
		}
		if (i <= 1)
		{
			BottomLeader.transform.localScale = new Vector3(Panel.clipRange.z, TopLeader.transform.localScale.y, TopLeader.transform.localScale.z);
		}
		else
		{
			BottomLeader.transform.localScale = new Vector3(TopLeader.transform.localScale.x + GetPhrase(0).Width, BottomLeader.transform.localScale.y, BottomLeader.transform.localScale.z);
		}
		for (; i < m_Phrases.Count; i++)
		{
			m_Phrases[i].gameObject.SetActive(value: false);
		}
	}

	private UIChantEditorPhrase GetPhrase(int index)
	{
		while (index >= m_Phrases.Count)
		{
			UIChantEditorPhrase component;
			if (m_Phrases.Count % 2 == 1)
			{
				GameObject obj = NGUITools.AddChild(RootBottomPhrase.transform.parent.gameObject, RootBottomPhrase.gameObject);
				obj.transform.localPosition = RootBottomPhrase.transform.localPosition;
				component = obj.GetComponent<UIChantEditorPhrase>();
				m_Phrases.Add(component);
			}
			else
			{
				GameObject obj2 = NGUITools.AddChild(RootTopPhrase.transform.parent.gameObject, RootTopPhrase.gameObject);
				obj2.transform.localPosition = RootTopPhrase.transform.localPosition;
				component = obj2.GetComponent<UIChantEditorPhrase>();
				m_Phrases.Add(component);
			}
			UIEventListener uIEventListener = UIEventListener.Get(component.Icon.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
		}
		m_Phrases[index].gameObject.SetActive(value: true);
		return m_Phrases[index];
	}

	public void LoadNew()
	{
		IsNew = true;
		m_LoadedChant = null;
		Reload();
	}

	public void Load(Chant chant)
	{
		IsNew = false;
		m_LoadedChant = chant;
		Reload();
	}
}
