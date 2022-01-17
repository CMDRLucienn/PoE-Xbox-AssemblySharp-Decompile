using System;
using UnityEngine;

public class UIStrongholdActionAttack : UIStrongholdActionItem
{
	public UIMultiSpriteImageButton ResolveButton;

	public UIMultiSpriteImageButton ManualButton;

	public UILabel Title;

	public UILabel Description;

	private void Start()
	{
		UIMultiSpriteImageButton resolveButton = ResolveButton;
		resolveButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(resolveButton.onClick, new UIEventListener.VoidDelegate(OnResolve));
		UIMultiSpriteImageButton manualButton = ManualButton;
		manualButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(manualButton.onClick, new UIEventListener.VoidDelegate(OnManual));
	}

	private void OnResolve(GameObject sender)
	{
		UIStrongholdManager.Instance.Stronghold.AutoResolveAttack();
	}

	private void OnManual(GameObject sender)
	{
		if (GameState.Instance.CurrentMapIsStronghold)
		{
			UIStrongholdManager.Instance.Stronghold.ManualResolveAttack();
		}
		else
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(678), GUIUtils.GetText(679));
		}
	}

	public override void Reload()
	{
		if ((bool)UIStrongholdManager.Instance)
		{
			string text = UIStrongholdManager.Instance.Stronghold.PendingAttack().Description.GetText();
			if (text != null)
			{
				Description.text = text;
			}
			Title.text = GUIUtils.GetText(678) + " (" + Stronghold.Format(61, UIStrongholdManager.Instance.Stronghold.PendingAttackTimeLeft().FormatNonZero(2)) + ")";
		}
	}
}
