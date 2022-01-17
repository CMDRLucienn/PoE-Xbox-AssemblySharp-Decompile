using System;
using UnityEngine;

public class UIRestConfirmBox : UIHudWindow
{
	public enum Result
	{
		STASH,
		REST,
		CANCEL
	}

	public enum RestType
	{
		CAMP,
		INN,
		PLAYER_HOUSE
	}

	public delegate void OnEndDialog(Result result);

	public UIWidget CloseButton;

	public static OnEndDialog OnDialogEnd;

	private Result m_Result = Result.CANCEL;

	public UILabel Title;

	public UILabel Text;

	public UIWidget Background;

	public GameObject CampingOnlyContent;

	public UIGrid PartyMemberGrid;

	public UIRestBoxPartyMember GridRootObject;

	private UIRestBoxPartyMember[] m_PartyMemberObjects;

	public UIGrid ButtonGrid;

	public UIMultiSpriteImageButton StashButton;

	public UIMultiSpriteImageButton RestButton;

	public UIMultiSpriteImageButton CancelButton;

	public static UIRestConfirmBox Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		UIMultiSpriteImageButton stashButton = StashButton;
		stashButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(stashButton.onClick, new UIEventListener.VoidDelegate(OnStash));
		UIMultiSpriteImageButton restButton = RestButton;
		restButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(restButton.onClick, new UIEventListener.VoidDelegate(OnRest));
		UIMultiSpriteImageButton cancelButton = CancelButton;
		cancelButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(cancelButton.onClick, new UIEventListener.VoidDelegate(OnCloseWindow));
		UIEventListener uIEventListener = UIEventListener.Get(CloseButton.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnCloseWindow));
	}

	private void InitPartyGrid()
	{
		if (m_PartyMemberObjects == null)
		{
			m_PartyMemberObjects = new UIRestBoxPartyMember[6];
			for (int i = 0; i < m_PartyMemberObjects.Length; i++)
			{
				UIRestBoxPartyMember uIRestBoxPartyMember = ((i != 0) ? NGUITools.AddChild(GridRootObject.transform.parent.gameObject, GridRootObject.gameObject).GetComponent<UIRestBoxPartyMember>() : GridRootObject);
				m_PartyMemberObjects[i] = uIRestBoxPartyMember;
			}
		}
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		OnDialogEnd = null;
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnStash(GameObject sender)
	{
		m_Result = Result.STASH;
		HideWindow();
	}

	private void OnRest(GameObject sender)
	{
		m_Result = Result.REST;
		HideWindow();
	}

	private void OnCloseWindow(GameObject sender)
	{
		m_Result = Result.CANCEL;
		HideWindow();
	}

	protected override void Show()
	{
		m_Result = Result.CANCEL;
		base.Show();
	}

	protected override bool Hide(bool forced)
	{
		if (OnDialogEnd != null)
		{
			OnDialogEnd(m_Result);
		}
		return base.Hide(forced);
	}

	public void SetData(RestType type, string text)
	{
		ShowWindow();
		InitPartyGrid();
		StashButton.gameObject.SetActive(!GameState.Option.GetOption(GameOption.BoolOption.DONT_RESTRICT_STASH));
		CampingOnlyContent.gameObject.SetActive(type == RestType.CAMP);
		Background.transform.localScale = new Vector3(Background.transform.localScale.x, (type == RestType.CAMP) ? 540f : 328f, Background.transform.localScale.z);
		switch (type)
		{
		case RestType.CAMP:
			Title.text = GUIUtils.GetText(855);
			break;
		case RestType.INN:
			Title.text = GUIUtils.GetText(754);
			break;
		case RestType.PLAYER_HOUSE:
			Title.text = GUIUtils.GetText(887);
			break;
		}
		ButtonGrid.Reposition();
		Text.text = text;
		for (int i = 0; i < m_PartyMemberObjects.Length; i++)
		{
			m_PartyMemberObjects[i].LoadPartyMember(PartyMemberAI.PartyMembers[i]);
		}
		PartyMemberGrid.Reposition();
		UIWidgetUtils.UpdateDependents(base.gameObject, 4);
	}
}
