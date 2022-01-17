using System;
using UnityEngine;

public class UIFormationsSlot : MonoBehaviour
{
	public UITexture Icon;

	public GameObject Frame;

	public GameObject PortraitBorder;

	[HideInInspector]
	public int CurrentPartyMember = -1;

	private void Start()
	{
		ReloadMember();
		UIMultiSpriteImageButton component = GetComponent<UIMultiSpriteImageButton>();
		component.onDrop = (UIEventListener.ObjectDelegate)Delegate.Combine(component.onDrop, new UIEventListener.ObjectDelegate(OnChildDrop));
		component.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(component.onDrag, new UIEventListener.VectorDelegate(OnChildDrag));
		component.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(component.onPress, new UIEventListener.BoolDelegate(OnPress));
	}

	private void OnChildDrag(GameObject go, Vector2 delta)
	{
		if (Icon.gameObject.activeInHierarchy)
		{
			Icon.gameObject.SetActive(value: false);
			PortraitBorder.gameObject.SetActive(value: false);
			UIFormationsManager.Instance.BeginDrag(CurrentPartyMember);
		}
	}

	private void OnChildDrop(GameObject go, GameObject dragged)
	{
		UIFormationsSlot componentInChildren = dragged.transform.parent.gameObject.GetComponentInChildren<UIFormationsSlot>();
		if ((bool)componentInChildren)
		{
			int currentPartyMember = CurrentPartyMember;
			SetPartyMember(componentInChildren.CurrentPartyMember);
			componentInChildren.SetPartyMember(currentPartyMember);
			UIFormationsManager.Instance.Save();
		}
	}

	private void OnPress(GameObject go, bool pressed)
	{
		if (!pressed)
		{
			ReloadMember();
		}
	}

	public void SetPartyMember(int paiIndex)
	{
		CurrentPartyMember = paiIndex;
		ReloadMember();
	}

	public void ReloadMember()
	{
		if (CurrentPartyMember >= 0 && CurrentPartyMember < PartyMemberAI.PartyMembers.Length)
		{
			PartyMemberAI partyMemberAtFormationIndex = PartyMemberAI.GetPartyMemberAtFormationIndex(CurrentPartyMember);
			if ((bool)partyMemberAtFormationIndex)
			{
				Icon.mainTexture = Portrait.GetTextureSmall(partyMemberAtFormationIndex);
				Icon.gameObject.SetActive(value: true);
			}
			else
			{
				Icon.gameObject.SetActive(value: false);
				PortraitBorder.SetActive(value: false);
			}
		}
		else
		{
			Icon.gameObject.SetActive(value: false);
			PortraitBorder.SetActive(value: false);
		}
	}
}
