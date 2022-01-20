// IEMod.QuickControls.QuickDropdown<T>
using System.Collections.Generic;
using System.Linq;
using Patchwork.Attributes;
using UnityEngine;

[NewType(null, null)]
[PatchedByType("IEMod.QuickControls.QuickDropdown`1")]
public class QuickDropdown<T> : QuickControl
{
	public GUIStringLabel LabelComponent
	{
		[PatchedByMember("GUIStringLabel IEMod.QuickControls.QuickDropdown`1::get_LabelComponent()")]
		get
		{
			return base.GameObject.transform.ComponentInDescendants<GUIStringLabel>();
		}
	}

	public string LabelText
	{
		[PatchedByMember("System.String IEMod.QuickControls.QuickDropdown`1::get_LabelText()")]
		get
		{
			GUIDatabaseString databaseString = LabelComponent.DatabaseString;
			return databaseString.GetText();
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickDropdown`1::set_LabelText(System.String)")]
		set
		{
			LabelComponent.DatabaseString.MaybeUnregister();
			LabelComponent.DatabaseString = IEModString.Register(value);
			Refresh();
		}
	}

	public int LabelWidth
	{
		[PatchedByMember("System.Int32 IEMod.QuickControls.QuickDropdown`1::get_LabelWidth()")]
		get
		{
			return LabelComponent.Component<UILabel>().lineWidth;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickDropdown`1::set_LabelWidth(System.Int32)")]
		set
		{
			if (value == 0)
			{
				LabelText = "";
			}
			LabelComponent.Component<UILabel>().lineWidth = value;
			Refresh();
		}
	}

	public string TooltipText
	{
		[PatchedByMember("System.String IEMod.QuickControls.QuickDropdown`1::get_TooltipText()")]
		get
		{
			return base.GameObject.Component<UIOptionsTag>().TooltipString?.GetText();
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickDropdown`1::set_TooltipText(System.String)")]
		set
		{
			UIOptionsTag uIOptionsTag = base.GameObject.Component<UIOptionsTag>();
			uIOptionsTag.TooltipString.MaybeUnregister();
			uIOptionsTag.TooltipString = IEModString.Register(value);
			Refresh();
		}
	}

	public int Width
	{
		[PatchedByMember("System.Int32 IEMod.QuickControls.QuickDropdown`1::get_Width()")]
		get
		{
			return (int)DropdownComponent.DropdownBackground.transform.localScale.x;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickDropdown`1::set_Width(System.Int32)")]
		set
		{
			UIWidget dropdownBackground = DropdownComponent.DropdownBackground;
			dropdownBackground.transform.localScale = new Vector3(value, 32f, 1f);
			DropdownComponent.BaseCollider.transform.localScale = new Vector3(value, 32f, 1f);
			Transform arrowPivot = DropdownComponent.ArrowPivot;
			arrowPivot.transform.localPosition = new Vector3(value - 27, 10f, 0f);
			UIDropdownMenu dropdownComponent = DropdownComponent;
			UILabel[] array = dropdownComponent.OptionGrid.ComponentsInDescendants<UILabel>();
			foreach (UILabel uILabel in array)
			{
				uILabel.lineWidth = value;
			}
			dropdownComponent.SelectedText.lineWidth = value;
			dropdownComponent.OptionGrid.cellWidth = value;
			Refresh();
		}
	}

	public UIDropdownMenu DropdownComponent
	{
		[PatchedByMember("UIDropdownMenu IEMod.QuickControls.QuickDropdown`1::get_DropdownComponent()")]
		get
		{
			return base.GameObject.ComponentInDescendants<UIDropdownMenu>();
		}
	}

	public IEnumerable<DropdownChoice<T>> Options
	{
		[PatchedByMember("System.Collections.Generic.IEnumerable`1<IEMod.QuickControls.DropdownChoice`1<T>> IEMod.QuickControls.QuickDropdown`1::get_Options()")]
		get
		{
			UIDropdownMenu dropdownComponent = DropdownComponent;
			return dropdownComponent.Options.Cast<DropdownChoice<T>>();
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickDropdown`1::set_Options(System.Collections.Generic.IEnumerable`1<IEMod.QuickControls.DropdownChoice`1<T>>)")]
		set
		{
			UIDropdownMenu dropdownComponent = DropdownComponent;
			dropdownComponent.Options = value.Cast<object>().ToArray();
			DropdownChoice<T> dropdownChoice = value.FirstOrDefault();
			SelectedValue.Value = ((dropdownChoice == null) ? default(T) : dropdownChoice.Value);
			Refresh();
		}
	}

	private T selectedValue
	{
		[PatchedByMember("T IEMod.QuickControls.QuickDropdown`1::get_selectedValue()")]
		get
		{
			DropdownChoice<T> dropdownChoice = (DropdownChoice<T>)DropdownComponent.SelectedItem;
			return (dropdownChoice == null) ? default(T) : dropdownChoice.Value;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickDropdown`1::set_selectedValue(T)")]
		set
		{
			UIDropdownMenu dropdownComponent = DropdownComponent;
			DropdownChoice<T> dropdownChoice = dropdownComponent.Options.Cast<DropdownChoice<T>>().SingleOrDefault((DropdownChoice<T> x) => object.Equals(x.Value, value));
			dropdownComponent.SelectedItem = dropdownChoice ?? ((dropdownComponent.Options.Length == 0) ? null : dropdownComponent.Options[0]);
			Refresh();
		}
	}

	public Bindable<T> SelectedValue
	{
		[PatchedByMember("IEMod.QuickControls.Bindable`1<T> IEMod.QuickControls.QuickDropdown`1::get_SelectedValue()")]
		get;
	}

	[PatchedByMember("System.Void IEMod.QuickControls.QuickDropdown`1::.ctor(UnityEngine.Transform,System.String,UnityEngine.GameObject)")]
	public QuickDropdown(Transform parent = null, string name = "QuickDropdown", GameObject altPrototype = null)
	{
		GameObject gameObject = altPrototype ?? Prefabs.QuickDropdown;
		if (gameObject == null)
		{
			throw IEDebug.Exception(null, "You must initialize the ExampleDropdown to create a dropdown.", null);
		}
		base.GameObject = Object.Instantiate(gameObject);
		base.GameObject.transform.parent = parent;
		base.GameObject.name = name;
		base.GameObject.transform.localPosition = Vector3.zero;
		base.GameObject.transform.localScale = Vector3.one;
		SelectedValue = BindingValue.Member(() => selectedValue).ToBindable();
		Options = new List<DropdownChoice<T>>();
		DropdownComponent.OnDropdownOptionChanged += delegate
		{
			SelectedValue.NotifyChange();
		};
		IEDebug.Log("Created: " + name);
	}

	[PatchedByMember("System.Void IEMod.QuickControls.QuickDropdown`1::Refresh()")]
	public void Refresh()
	{
		UIDropdownMenu dropdownComponent = DropdownComponent;
		dropdownComponent.RefreshDropdown();
		dropdownComponent.RefreshSelected();
		LabelComponent.RefreshText();
	}
}
