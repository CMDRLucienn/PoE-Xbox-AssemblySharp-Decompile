using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIStrongholdCompanionPicker : UIHudWindow
{
	public delegate void OnEndDialog(UIMessageBox.Result result, GameObject selected);

	public UIMultiSpriteImageButton ConfirmButton;

	public UIMultiSpriteImageButton CancelButton;

	public UIWidget CloseButton;

	public UILabel DescriptionLabel;

	public UIStrongholdCompanionBox RootPartyMember;

	private List<UIStrongholdCompanionBox> m_Members = new List<UIStrongholdCompanionBox>();

	public UIGrid Grid;

	public GameObject NoCompanions;

	public OnEndDialog OnDialogEnd;

	private UIMessageBox.Result m_Result;

	private GameObject m_Selected;

	public static UIStrongholdCompanionPicker Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		RootPartyMember.gameObject.SetActive(value: false);
		UIMultiSpriteImageButton confirmButton = ConfirmButton;
		confirmButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(confirmButton.onClick, new UIEventListener.VoidDelegate(OnConfirm));
		UIMultiSpriteImageButton cancelButton = CancelButton;
		cancelButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(cancelButton.onClick, new UIEventListener.VoidDelegate(OnCancel));
		UIEventListener uIEventListener = UIEventListener.Get(CloseButton.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnCancel));
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

	public void Select(GameObject member)
	{
		ConfirmButton.enabled = member;
		m_Selected = member;
		foreach (UIStrongholdCompanionBox member2 in m_Members)
		{
			member2.Select(member);
		}
	}

	private void OnConfirm(GameObject sender)
	{
		m_Result = UIMessageBox.Result.AFFIRMATIVE;
		HideWindow();
	}

	private void OnCancel(GameObject sender)
	{
		m_Result = UIMessageBox.Result.CANCEL;
		HideWindow();
	}

	protected override bool Hide(bool forced)
	{
		if (OnDialogEnd != null)
		{
			OnDialogEnd(m_Result, m_Selected);
		}
		return base.Hide(forced);
	}

	protected override void Show()
	{
		m_Result = UIMessageBox.Result.CANCEL;
		Select(null);
		List<StoredCharacterInfo> companions = GameState.Stronghold.GetCompanions();
		RootPartyMember.gameObject.SetActive(value: false);
		int i = 0;
		foreach (StoredCharacterInfo item in companions)
		{
			GetBox(i).Load(item.gameObject);
			i++;
		}
		NoCompanions.gameObject.SetActive(i == 0);
		for (; i < m_Members.Count; i++)
		{
			m_Members[i].gameObject.SetActive(value: false);
		}
		IEnumerable<StoredCharacterInfo> source = companions.Where((StoredCharacterInfo info) => UIStrongholdManager.Instance.Stronghold.IsAvailable(info));
		if (source.Any())
		{
			Select(source.First().gameObject);
		}
		else
		{
			Select(null);
		}
		Grid.repositionNow = true;
	}

	private UIStrongholdCompanionBox GetBox(int index)
	{
		if (index < m_Members.Count)
		{
			m_Members[index].gameObject.SetActive(value: true);
			return m_Members[index];
		}
		UIStrongholdCompanionBox component = NGUITools.AddChild(RootPartyMember.transform.parent.gameObject, RootPartyMember.gameObject).GetComponent<UIStrongholdCompanionBox>();
		component.gameObject.SetActive(value: true);
		m_Members.Add(component);
		return component;
	}
}
