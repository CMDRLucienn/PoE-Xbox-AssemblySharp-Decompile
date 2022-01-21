using UnityEngine;
using Object = UnityEngine.Object;

public class QuickCheckbox : QuickControl
{
	private static UIOptionsTag _prototype;
	private static UIOptionsTag Prototype
	{
		get
		{
			if (_prototype == null)
			{
				_prototype = Prefabs.QuickCheckbox;
			}
			return _prototype;
		}
	}
	public QuickCheckbox(string name = "QuickCheckbox", Transform parent = null, UIOptionsTag prototype = null)
	{
		IEDebug.Log("Creating Checkbox : {0}", name);
		var chBox = (UIOptionsTag)Object.Instantiate(prototype ?? Prototype);
		chBox.transform.parent = parent;
		chBox.name = name;

		chBox.transform.localScale = new Vector3(1, 1, 1);
		chBox.transform.localPosition = new Vector3(0, 0, 0);
		GameObject = chBox.gameObject;
		GameObject.name = name;
		IEDebug.Log("IEMod created: " + chBox.name);
		IsChecked = BindingValue.Member(() => this.isChecked).ToBindable();
		chBox.Checkbox.onStateChange += (a, b) => {
			IsChecked.NotifyChange();
		};

	}
	public UIOptionsTag OptionsTagComponent
	{
		get
		{
			return GameObject.Component<UIOptionsTag>();
		}
	}

	public string Label
	{
		get
		{
			return OptionsTagComponent.CheckboxLabel.GetText();
		}
		set
		{
			OptionsTagComponent.CheckboxLabel.MaybeUnregister();
			OptionsTagComponent.CheckboxLabel = IEModString.Register(value);
			OptionsTagComponent.UpdateLabel();
		}
	}

	public string Tooltip
	{
		get
		{
			return OptionsTagComponent.TooltipString.GetText();
		}
		set
		{
			OptionsTagComponent.TooltipString.MaybeUnregister();
			OptionsTagComponent.TooltipString = IEModString.Register(value);
		}
	}

	public Bindable<bool> IsChecked
	{
		get;
	}

	private bool isChecked
	{
		get
		{
			return OptionsTagComponent.Checkbox.isChecked;
		}
		set
		{

			OptionsTagComponent.Checkbox.isChecked = value;
		}
	}
}
