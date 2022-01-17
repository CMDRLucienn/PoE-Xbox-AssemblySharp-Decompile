using System;
using UnityEngine;

public class UIStrongholdActionDebt : UIStrongholdActionItem
{
	public UIMultiSpriteImageButton PayButton;

	public UILabel DebtLabel;

	private void Start()
	{
		UIMultiSpriteImageButton payButton = PayButton;
		payButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(payButton.onClick, new UIEventListener.VoidDelegate(OnPay));
	}

	private void OnPay(GameObject sender)
	{
		UIStrongholdManager.Instance.Stronghold.PaydownDebt(UIStrongholdManager.Instance.Stronghold.Debt);
	}

	public override void Reload()
	{
		if ((bool)UIStrongholdManager.Instance)
		{
			DebtLabel.text = GUIUtils.Format(708, GUIUtils.Format(466, UIStrongholdManager.Instance.Stronghold.Debt));
		}
	}
}
