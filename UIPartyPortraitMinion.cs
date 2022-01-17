using System;
using UnityEngine;

public class UIPartyPortraitMinion : MonoBehaviour, ISelectACharacter
{
	public UISprite Selected;

	public UIPortraitOnClick Button;

	private bool m_Hovered;

	private CharacterStats m_SelectedCharacter;

	public PartyMemberAI SelectedPartyMemberAI { get; private set; }

	public CharacterStats SelectedCharacter
	{
		get
		{
			return m_SelectedCharacter;
		}
		set
		{
			m_SelectedCharacter = value;
			SelectedPartyMemberAI = (m_SelectedCharacter ? m_SelectedCharacter.GetComponent<PartyMemberAI>() : null);
		}
	}

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Button.gameObject);
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnChildHovered));
	}

	private void OnChildHovered(GameObject sender, bool over)
	{
		m_Hovered = over;
	}

	private void Update()
	{
		if (m_Hovered && (bool)SelectedCharacter)
		{
			GameCursor.CharacterUnderCursor = SelectedCharacter.gameObject;
		}
		if ((bool)SelectedPartyMemberAI && SelectedPartyMemberAI.Selected)
		{
			Selected.spriteName = "sel_circle_selected";
		}
		else
		{
			Selected.spriteName = "sel_circle";
		}
	}

	public void SetPartyMember(GameObject selectObject)
	{
		GameObject gameObject = (SelectedCharacter ? SelectedCharacter.gameObject : null);
		if (selectObject != gameObject || selectObject == null)
		{
			SelectedCharacter = (selectObject ? selectObject.GetComponent<CharacterStats>() : null);
			if (this.OnSelectedCharacterChanged != null)
			{
				this.OnSelectedCharacterChanged(SelectedCharacter);
			}
		}
		base.gameObject.SetActive(SelectedCharacter != null);
	}
}
