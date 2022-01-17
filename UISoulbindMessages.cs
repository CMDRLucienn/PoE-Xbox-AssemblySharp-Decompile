using System;
using UnityEngine;

public static class UISoulbindMessages
{
	public static void TryUnbind(EquipmentSoulbind soulbind)
	{
		if (!soulbind.IsBound)
		{
			return;
		}
		Item component = soulbind.GetComponent<Item>();
		if (!component)
		{
			return;
		}
		UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.YESNO, "", GUIUtils.Format(2033, CharacterStats.Name(soulbind.BoundGuid), component.Name));
		uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, (UIMessageBox.OnEndDialog)delegate(UIMessageBox.Result result, UIMessageBox sender)
		{
			if (result == UIMessageBox.Result.AFFIRMATIVE)
			{
				soulbind.Unbind();
			}
		});
	}

	public static void TryBind(EquipmentSoulbind soulbind, GameObject target)
	{
		if (soulbind.IsBound)
		{
			return;
		}
		CharacterStats component = target.GetComponent<CharacterStats>();
		if (!component)
		{
			return;
		}
		if (soulbind.CanBindClass(component.CharacterClass))
		{
			Item component2 = soulbind.GetComponent<Item>();
			if (!component2)
			{
				return;
			}
			UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.YESNO, "", GUIUtils.Format(2032, component2.Name, CharacterStats.Name(target)));
			uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, (UIMessageBox.OnEndDialog)delegate(UIMessageBox.Result result, UIMessageBox sender)
			{
				if (result == UIMessageBox.Result.AFFIRMATIVE)
				{
					soulbind.Bind(target);
				}
			});
		}
		else
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.Format(2035, TextUtils.FuncJoin((CharacterStats.Class cl) => GUIUtils.GetClassString(cl, Gender.Neuter), soulbind.BindableClasses, GUIUtils.Comma())));
		}
	}
}
