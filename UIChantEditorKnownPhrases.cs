using System;
using System.Collections.Generic;
using UnityEngine;

public class UIChantEditorKnownPhrases : UIParentSelectorListener
{
	public UIGrid Grid;

	public UIChantEditorPhrase RootIcon;

	public GameObject None;

	public UIChantEditorChants Chants;

	private List<UIChantEditorPhrase> elements;

	protected override void Start()
	{
		base.Start();
		Init();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Init()
	{
		if (elements == null)
		{
			elements = new List<UIChantEditorPhrase>();
			RootIcon.gameObject.SetActive(value: false);
			UIEventListener uIEventListener = UIEventListener.Get(RootIcon.Icon.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
			elements.Add(RootIcon);
		}
	}

	private void OnChildClick(GameObject sender)
	{
		UIChantEditorPhrase component = sender.transform.parent.GetComponent<UIChantEditorPhrase>();
		if (!component.Disabled && InGameHUD.Instance.CursorMode == InGameHUD.ExclusiveCursorMode.None)
		{
			Chants.SelectedChant.AddPhrase(component.Phrase);
		}
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		Init();
		if (!stats)
		{
			return;
		}
		ChanterTrait chanterTrait = stats.GetChanterTrait();
		if (chanterTrait == null)
		{
			Debug.LogError("Character '" + stats.name + "' has no ChanterTrait in UIChantEditor.");
			return;
		}
		int num = 0;
		Phrase[] knownPhrases = chanterTrait.GetKnownPhrases();
		foreach (Phrase phrase in knownPhrases)
		{
			num++;
			UIChantEditorPhrase icon = GetIcon(num);
			icon.gameObject.SetActive(value: true);
			icon.SetPhrase(phrase);
			icon.Disabled = GameState.InCombat;
		}
		None.SetActive(num == 0);
		for (num++; num < elements.Count; num++)
		{
			elements[num].gameObject.SetActive(value: false);
		}
		Grid.Reposition();
	}

	private UIChantEditorPhrase GetIcon(int index)
	{
		Init();
		if (index < elements.Count)
		{
			return elements[index];
		}
		UIChantEditorPhrase component = NGUITools.AddChild(RootIcon.transform.parent.gameObject, RootIcon.gameObject).GetComponent<UIChantEditorPhrase>();
		component.gameObject.SetActive(value: true);
		UIEventListener uIEventListener = UIEventListener.Get(component.Icon.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
		elements.Add(component);
		return component;
	}
}
