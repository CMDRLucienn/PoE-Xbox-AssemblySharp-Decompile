// IEMod.QuickControls.QuickCheckbox
using System;
using Patchwork.Attributes;
using UnityEngine;

[PatchedByType("IEMod.QuickControls.QuickCheckbox")]
[NewType(null, null)]
public class QuickCheckbox : QuickControl
{
	private static UIOptionsTag _prototype;

	private static UIOptionsTag Prototype
	{
		[PatchedByMember("UIOptionsTag IEMod.QuickControls.QuickCheckbox::get_Prototype()")]
		get
		{
			if (_prototype == null)
			{
				_prototype = Prefabs.QuickCheckbox;
			}
			return _prototype;
		}
	}

	public UIOptionsTag OptionsTagComponent
	{
		[PatchedByMember("UIOptionsTag IEMod.QuickControls.QuickCheckbox::get_OptionsTagComponent()")]
		get
		{
			return base.GameObject.Component<UIOptionsTag>();
		}
	}

	public string Label
	{
		[PatchedByMember("System.String IEMod.QuickControls.QuickCheckbox::get_Label()")]
		get
		{
			return OptionsTagComponent.CheckboxLabel.GetText();
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickCheckbox::set_Label(System.String)")]
		set
		{
			OptionsTagComponent.CheckboxLabel.MaybeUnregister();
			OptionsTagComponent.CheckboxLabel = IEModString.Register(value);
			OptionsTagComponent.UpdateLabel();
		}
	}

	public string Tooltip
	{
		[PatchedByMember("System.String IEMod.QuickControls.QuickCheckbox::get_Tooltip()")]
		get
		{
			return OptionsTagComponent.TooltipString.GetText();
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickCheckbox::set_Tooltip(System.String)")]
		set
		{
			OptionsTagComponent.TooltipString.MaybeUnregister();
			OptionsTagComponent.TooltipString = IEModString.Register(value);
		}
	}

	public Bindable<bool> IsChecked
	{
		[PatchedByMember("IEMod.QuickControls.Bindable`1<System.Boolean> IEMod.QuickControls.QuickCheckbox::get_IsChecked()")]
		get;
	}

	private bool isChecked
	{
		[PatchedByMember("System.Boolean IEMod.QuickControls.QuickCheckbox::get_isChecked()")]
		get
		{
			return OptionsTagComponent.Checkbox.isChecked;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickCheckbox::set_isChecked(System.Boolean)")]
		set
		{
			OptionsTagComponent.Checkbox.isChecked = value;
		}
	}

	[PatchedByMember("System.Void IEMod.QuickControls.QuickCheckbox::.ctor(System.String,UnityEngine.Transform,UIOptionsTag)")]
	public QuickCheckbox(string name = "QuickCheckbox", Transform parent = null, UIOptionsTag prototype = null)
	{
		IEDebug.Log("Creating Checkbox : {0}", name);
		UIOptionsTag uIOptionsTag = UnityEngine.Object.Instantiate(prototype ?? Prototype);
		uIOptionsTag.transform.parent = parent;
		uIOptionsTag.name = name;
		uIOptionsTag.transform.localScale = new Vector3(1f, 1f, 1f);
		uIOptionsTag.transform.localPosition = new Vector3(0f, 0f, 0f);
		base.GameObject = uIOptionsTag.gameObject;
		base.GameObject.name = name;
		IEDebug.Log("IEMod created: " + uIOptionsTag.name);
		IsChecked = BindingValue.Member(() => isChecked).ToBindable();
		UICheckbox checkbox = uIOptionsTag.Checkbox;
		checkbox.onStateChange = (UICheckbox.OnStateChange)Delegate.Combine(checkbox.onStateChange, (UICheckbox.OnStateChange)delegate
		{
			IsChecked.NotifyChange();
		});
	}
}
