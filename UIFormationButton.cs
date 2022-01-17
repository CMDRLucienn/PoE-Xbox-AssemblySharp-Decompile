using System;
using UnityEngine;

public class UIFormationButton : MonoBehaviour
{
	public UILabel NumberLabel;

	public UIWidget Collider;

	[HideInInspector]
	public string TooltipText;

	public UIMultiSpriteImageButton Button;

	private int m_Formation = -1;

	private GameObject m_ModalActiveVfx;

	private Transform m_IconTransform;

	private Vector3 m_lastIconPosition;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Collider.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildLeftClick));
		UIEventListener uIEventListener2 = UIEventListener.Get(Collider.gameObject);
		uIEventListener2.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onTooltip, new UIEventListener.BoolDelegate(OnChildTooltip));
		UIEventListener uIEventListener3 = UIEventListener.Get(Collider.gameObject);
		uIEventListener3.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onRightClick, new UIEventListener.VoidDelegate(OnChildRightClick));
		m_IconTransform = base.transform.Find("Icon");
	}

	private void Update()
	{
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI != null) || !(m_IconTransform != null))
			{
				continue;
			}
			if (partyMemberAI.FormationStyle == m_Formation)
			{
				if (m_lastIconPosition != m_IconTransform.position)
				{
					if ((bool)m_ModalActiveVfx)
					{
						GameUtilities.Destroy(m_ModalActiveVfx);
					}
				}
				else if (!m_ModalActiveVfx || !m_ModalActiveVfx.activeSelf)
				{
					if (!m_ModalActiveVfx)
					{
						m_ModalActiveVfx = UIAbilityBar.Instance.InstantiateModalVfx(base.transform, new Vector3((0f - m_IconTransform.transform.localScale.x) / 2f - 1f, (0f - m_IconTransform.transform.localScale.y) / 2f - 1f, -3f));
					}
					m_ModalActiveVfx.SetActive(value: true);
				}
			}
			else if (m_ModalActiveVfx != null)
			{
				m_ModalActiveVfx.SetActive(value: false);
			}
			break;
		}
		m_lastIconPosition = m_IconTransform.position;
	}

	private void OnChildLeftClick(GameObject sender)
	{
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if ((bool)partyMemberAI)
			{
				partyMemberAI.FormationStyle = m_Formation;
			}
		}
	}

	private void OnChildRightClick(GameObject sender)
	{
		UIFormationsManager.Instance.ShowFormation(m_Formation);
	}

	private void OnChildTooltip(GameObject sender, bool over)
	{
		if (over)
		{
			UIActionBarTooltip.GlobalShow(Collider, TooltipText);
		}
		else
		{
			UIActionBarTooltip.GlobalHide();
		}
	}

	public void SetFormation(int index)
	{
		int numStandardSets = UIFormationsManager.Instance.NumStandardSets;
		if (index >= numStandardSets)
		{
			NumberLabel.gameObject.SetActive(value: true);
			NumberLabel.text = (index - numStandardSets + 1).ToString();
		}
		else
		{
			NumberLabel.gameObject.SetActive(value: false);
		}
		m_Formation = index;
	}
}
