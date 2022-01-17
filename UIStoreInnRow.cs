using System;
using UnityEngine;

public class UIStoreInnRow : MonoBehaviour
{
	public UITexture ImageTexture;

	public UILabel NameLabel;

	public UILabel PriceLabel;

	public UILabel DescLabel;

	public UIWidget HoverDisplay;

	public Collider Collider;

	private Inn.InnRoom m_Room;

	private Inn m_Inn;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Collider.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClick));
		UIEventListener uIEventListener2 = UIEventListener.Get(Collider.gameObject);
		uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnHover));
		HoverDisplay.alpha = 0f;
	}

	public void Set(Inn.InnRoom room, Inn inn)
	{
		m_Room = room;
		m_Inn = inn;
		if (m_Room == null || m_Inn == null)
		{
			return;
		}
		ImageTexture.mainTexture = room.DisplayImage;
		NameLabel.text = room.DisplayName.GetText();
		PriceLabel.text = GUIUtils.Format(466, room.ActualCost(m_Inn));
		PriceLabel.color = (inn.CanPurchaseRoom(room) ? Color.white : UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.ERROR));
		PriceLabel.alpha = ((!inn.IsPlayerOwned) ? 1 : 0);
		if (room != null && (bool)room.RestBonus && room.RestBonus.StatusEffects.Length != 0)
		{
			int maxRestCycles = room.RestBonus.StatusEffects[0].MaxRestCycles;
			if (room.RestBonus.StatusEffects[0].LastsUntilRest)
			{
				DescLabel.text = GUIUtils.Format((maxRestCycles == 1) ? 1866 : 1867, maxRestCycles) + ": ";
			}
			DescLabel.text += StatusEffectParams.ListToString(room.RestBonus.StatusEffects, null);
		}
		else
		{
			DescLabel.text = "";
		}
		base.gameObject.SetActive(room.CanSee());
	}

	public void Reload()
	{
		Set(m_Room, m_Inn);
	}

	private void OnHover(GameObject sender, bool over)
	{
		HoverDisplay.alpha = (over ? 1 : 0);
	}

	private void OnClick(GameObject sender)
	{
		if (m_Inn.CanPurchaseRoom(m_Room))
		{
			UIRestConfirmBox.OnDialogEnd = (UIRestConfirmBox.OnEndDialog)Delegate.Combine(UIRestConfirmBox.OnDialogEnd, new UIRestConfirmBox.OnEndDialog(OnConfirmCamp));
			UIRestConfirmBox.Instance.SetData((!m_Inn.IsPlayerOwned) ? UIRestConfirmBox.RestType.INN : UIRestConfirmBox.RestType.PLAYER_HOUSE, GUIUtils.Format(m_Inn.IsPlayerOwned ? 888 : 755, m_Room.DisplayName.GetText(), GUIUtils.Format(294, m_Room.ActualCost(m_Inn))));
		}
		else
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(m_Inn.IsPlayerOwned ? 887 : 754), GUIUtils.GetText(721));
		}
	}

	private void OnConfirmCamp(UIRestConfirmBox.Result result)
	{
		UIRestConfirmBox.OnDialogEnd = (UIRestConfirmBox.OnEndDialog)Delegate.Remove(UIRestConfirmBox.OnDialogEnd, new UIRestConfirmBox.OnEndDialog(OnConfirmCamp));
		switch (result)
		{
		case UIRestConfirmBox.Result.REST:
			UIStoreManager.Instance.CancelTransaction();
			UIStoreManager.Instance.HideWindow();
			m_Inn.PurchaseRoom(m_Room);
			break;
		case UIRestConfirmBox.Result.STASH:
		{
			UIWindowManager.Instance.CloseAllWindows();
			UIInventoryManager.Instance.ImmediateStashAccess();
			UIInventoryManager instance = UIInventoryManager.Instance;
			instance.OnWindowHidden = (UIHudWindow.WindowHiddenDelegate)Delegate.Combine(instance.OnWindowHidden, new UIHudWindow.WindowHiddenDelegate(CampFromStash));
			break;
		}
		}
	}

	private void CampFromStash(UIHudWindow window)
	{
		UIInventoryManager instance = UIInventoryManager.Instance;
		instance.OnWindowHidden = (UIHudWindow.WindowHiddenDelegate)Delegate.Remove(instance.OnWindowHidden, new UIHudWindow.WindowHiddenDelegate(CampFromStash));
		m_Inn.PurchaseRoom(m_Room);
		UIStoreManager.Instance.HideWindow();
	}
}
