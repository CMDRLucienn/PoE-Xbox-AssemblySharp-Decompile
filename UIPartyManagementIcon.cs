using System;
using UnityEngine;

public class UIPartyManagementIcon : MonoBehaviour
{
	public GameObject HighlightObject;

	public UITexture Portrait;

	public Collider Collider;

	public UILabel StatusLabel;

	public bool Enabled;

	public bool isRoster;

	private GameObject m_CurrentPartyMember;

	private Coroutine m_portraitTextureLoadCoroutine;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Collider.gameObject);
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnColliderHover));
		UIEventListener uIEventListener2 = UIEventListener.Get(Collider.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClick));
	}

	private void OnClick(GameObject go)
	{
		if (!m_CurrentPartyMember)
		{
			return;
		}
		if (isRoster)
		{
			StoredCharacterInfo component = m_CurrentPartyMember.GetComponent<StoredCharacterInfo>();
			if (component == null || GameState.Stronghold.IsAvailable(component))
			{
				if (UIPartyManager.Instance.Party.PartySize < 6)
				{
					UIPartyManager.Instance.PartyCharacter(m_CurrentPartyMember);
				}
			}
			else
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(873), GUIUtils.Format(896, CharacterStats.Name(m_CurrentPartyMember), GameState.Stronghold.WhyNotAvailableString(component)));
			}
		}
		else if (m_CurrentPartyMember != GameState.s_playerCharacter.gameObject)
		{
			UIPartyManager.Instance.BenchCharacter(m_CurrentPartyMember);
			HighlightObject.SetActive(value: false);
		}
		UIPartyManager.Instance.Reload();
	}

	public void SetPartyMember(GameObject member)
	{
		m_CurrentPartyMember = member;
		Enabled = member != null;
		Portrait.alpha = (Enabled ? 1 : 0);
		if (!(member != null))
		{
			return;
		}
		StoredCharacterInfo component = member.GetComponent<StoredCharacterInfo>();
		Portrait component2 = member.GetComponent<Portrait>();
		if ((bool)component)
		{
			if ((bool)component.SmallPortrait)
			{
				Portrait.mainTexture = component.SmallPortrait;
			}
			else
			{
				LoadPortraitTextureFromPath(component.PortraitSmallPath);
			}
		}
		else if ((bool)component2)
		{
			if ((bool)component2.TextureSmall)
			{
				Portrait.mainTexture = component2.TextureSmall;
			}
			else
			{
				LoadPortraitTextureFromPath(component2.TextureSmallPath);
			}
		}
		else
		{
			Portrait.mainTexture = global::Portrait.GetTextureSmall(member);
		}
	}

	private void OnColliderHover(GameObject sender, bool over)
	{
		if (over)
		{
			UIPartyManager.Instance.SelectCharacter(m_CurrentPartyMember);
		}
		else
		{
			UIPartyManager.Instance.DeselectCharacter(m_CurrentPartyMember);
		}
	}

	private void OnDisable()
	{
		HighlightObject.SetActive(value: false);
	}

	public void Select(GameObject partyMember)
	{
		if (m_CurrentPartyMember == partyMember && partyMember != null)
		{
			if (m_CurrentPartyMember == GameState.s_playerCharacter.gameObject)
			{
				return;
			}
			HighlightObject.SetActive(value: true);
			if (isRoster)
			{
				StoredCharacterInfo component = m_CurrentPartyMember.GetComponent<StoredCharacterInfo>();
				if (component == null || GameState.Stronghold.IsAvailable(component))
				{
					StatusLabel.text = GUIUtils.GetText(403, CharacterStats.GetGender(partyMember));
				}
				else
				{
					StatusLabel.text = GUIUtils.GetText(709, CharacterStats.GetGender(partyMember));
				}
			}
			else
			{
				StatusLabel.text = GUIUtils.GetText(400, CharacterStats.GetGender(partyMember));
			}
		}
		else
		{
			HighlightObject.SetActive(value: false);
		}
	}

	private void LoadPortraitTextureFromPath(string texturePath)
	{
		if (!string.IsNullOrEmpty(texturePath))
		{
			if (m_portraitTextureLoadCoroutine != null)
			{
				StopCoroutine(m_portraitTextureLoadCoroutine);
			}
			m_portraitTextureLoadCoroutine = StartCoroutine(GUIUtils.LoadTexture2DFromPathCallback(texturePath, SmallPortraitTextureLoaded));
		}
	}

	private void SmallPortraitTextureLoaded(Texture2D loadedTexture)
	{
		m_portraitTextureLoadCoroutine = null;
		Portrait.mainTexture = loadedTexture;
	}
}
