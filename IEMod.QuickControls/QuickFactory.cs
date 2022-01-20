// IEMod.QuickControls.QuickFactory
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Patchwork.Attributes;
using UnityEngine;

[NewType(null, null)]
[PatchedByType("IEMod.QuickControls.QuickFactory")]
public class QuickFactory
{
	public Transform CurrentParent
	{
		[PatchedByMember("UnityEngine.Transform IEMod.QuickControls.QuickFactory::get_CurrentParent()")]
		get;
		[PatchedByMember("System.Void IEMod.QuickControls.QuickFactory::set_CurrentParent(UnityEngine.Transform)")]
		set;
	}

	[PatchedByMember("System.Void IEMod.QuickControls.QuickFactory::.ctor(UnityEngine.Transform)")]
	public QuickFactory(Transform currentParent)
	{
		CurrentParent = currentParent;
	}

	[PatchedByMember("System.Void IEMod.QuickControls.QuickFactory::.ctor()")]
	public QuickFactory()
	{
	}

	[PatchedByMember("IEMod.QuickControls.QuickCheckbox IEMod.QuickControls.QuickFactory::Checkbox(System.Linq.Expressions.Expression`1<System.Func`1<System.Boolean>>,System.String)")]
	public QuickCheckbox Checkbox(Expression<Func<bool>> memberAccessExpr, string name = null)
	{
		MemberAccess<bool> memberAccess = ReflectHelper.AnalyzeMember(memberAccessExpr);
		string labelInfo = ReflectHelper.GetLabelInfo(memberAccess.TopmostMember);
		string descriptionInfo = ReflectHelper.GetDescriptionInfo(memberAccess.TopmostMember);
		Transform currentParent = CurrentParent;
		QuickCheckbox quickCheckbox = new QuickCheckbox(name ?? memberAccess.TopmostMember.Name, currentParent)
		{
			Label = labelInfo,
			Tooltip = descriptionInfo
		};
		quickCheckbox.IsChecked.Bind(memberAccessExpr);
		return quickCheckbox;
	}

	[PatchedByMember("IEMod.QuickControls.QuickDropdown`1<T> IEMod.QuickControls.QuickFactory::Dropdown(System.Linq.Expressions.Expression`1<System.Func`1<T>>,System.Collections.Generic.IEnumerable`1<IEMod.QuickControls.DropdownChoice`1<T>>,System.String)")]
	public QuickDropdown<T> Dropdown<T>(Expression<Func<T>> memberAccessExpr, IEnumerable<DropdownChoice<T>> choices, string name = null)
	{
		MemberAccess<T> memberAccess = ReflectHelper.AnalyzeMember(memberAccessExpr);
		string labelInfo = ReflectHelper.GetLabelInfo(memberAccess.TopmostMember);
		string descriptionInfo = ReflectHelper.GetDescriptionInfo(memberAccess.TopmostMember);
		QuickDropdown<T> quickDropdown = new QuickDropdown<T>(CurrentParent, name ?? memberAccess.TopmostMember.Name)
		{
			Options = choices.ToArray(),
			LabelText = labelInfo,
			TooltipText = descriptionInfo
		};
		quickDropdown.SelectedValue.Bind(memberAccessExpr);
		return quickDropdown;
	}

	[PatchedByMember("IEMod.QuickControls.QuickButton IEMod.QuickControls.QuickFactory::Button(System.String,System.String,System.Nullable`1<UnityEngine.Vector3>)")]
	public QuickButton Button(string caption = "", string name = null, Vector3? localPos = null)
	{
		string[] replaceWhat = " :\t\n\r/1#$%^&*().".ToCharArray().Select(char.ToString).ToArray();
		name = name ?? caption.ReplaceAll("_", replaceWhat);
		return new QuickButton(CurrentParent, name)
		{
			LocalPosition = (localPos ?? Vector3.zero),
			Caption = caption
		};
	}

	[PatchedByMember("IEMod.QuickControls.QuickDropdown`1<T> IEMod.QuickControls.QuickFactory::EnumDropdown(System.Linq.Expressions.Expression`1<System.Func`1<T>>,System.String)")]
	public QuickDropdown<T> EnumDropdown<T>(Expression<Func<T>> memberAccessExpr, string name = null) where T : struct, IConvertible, IComparable, IFormattable
	{
		return Dropdown(memberAccessExpr, DropdownChoice.FromEnum<T>(), name);
	}
}
