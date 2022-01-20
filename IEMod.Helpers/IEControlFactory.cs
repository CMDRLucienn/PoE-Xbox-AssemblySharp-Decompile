// IEMod.Helpers.IEControlFactory
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Patchwork.Attributes;
using UnityEngine;

[PatchedByType("IEMod.Helpers.IEControlFactory")]
[NewType(null, null)]
[Obsolete("Use QuickFactory instead.")]
public class IEControlFactory
{
	public UIOptionsTag ExampleCheckbox;

	public UIOptionsTag ExampleDropdown;

	public Transform CurrentParent;

	public GameObject ExamplePage;

	public GameObject ExampleButton;

	[PatchedByMember("IEMod.Helpers.IEDropdownChoice[] IEMod.Helpers.IEControlFactory::EnumToChoices(System.Type)")]
	private static IEDropdownChoice[] EnumToChoices(Type enumType)
	{
		List<IEDropdownChoice> list = new List<IEDropdownChoice>();
		foreach (object value in Enum.GetValues(enumType))
		{
			FieldInfo field = enumType.GetField(value.ToString());
			string text = ReflectHelper.GetCustomAttribute<DescriptionAttribute>(field)?.Description;
			text = text ?? value.ToString();
			list.Add(new IEDropdownChoice(value, text));
		}
		return list.ToArray();
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.Helpers.IEControlFactory::Page(System.String)")]
	public GameObject Page(string name)
	{
		if (ExamplePage == null)
		{
			throw IEDebug.Exception(null, "You must initialize the ExamplePage to create a Page", null);
		}
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = ExamplePage.transform.parent;
		gameObject.transform.localScale = ExamplePage.transform.localScale;
		gameObject.transform.localPosition = ExamplePage.transform.localPosition;
		gameObject.name = name;
		return gameObject;
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.Helpers.IEControlFactory::Dropdown(System.Linq.Expressions.Expression`1<System.Func`1<T>>,System.Collections.Generic.IEnumerable`1<IEMod.Helpers.IEDropdownChoice>,System.Int32,System.Int32,System.String,System.String)")]
	public GameObject Dropdown<T>(Expression<Func<T>> memberAccessExpr, IEnumerable<IEDropdownChoice> choices, int width, int labelWidth, string label = null, string tooltip = null)
	{
		if (ExampleDropdown == null)
		{
			throw IEDebug.Exception(null, "You must initialize the ExampleDropdown to create a dropdown.", null);
		}
		Vector3 localPosition = new Vector3(0f, 0f, 0f);
		Func<T> getter = ReflectHelper.CreateGetter(memberAccessExpr);
		Action<T> setter = ReflectHelper.CreateSetter(memberAccessExpr);
		MemberExpression memberExpression = (MemberExpression)memberAccessExpr.Body;
		UIOptionsTag uIOptionsTag = UnityEngine.Object.Instantiate(ExampleDropdown);
		uIOptionsTag.transform.parent = CurrentParent;
		uIOptionsTag.name = memberExpression.Member.Name;
		uIOptionsTag.transform.localPosition = localPosition;
		uIOptionsTag.transform.localScale = new Vector3(1f, 1f, 1f);
		UIDropdownMenu uIDropdownMenu = uIOptionsTag.transform.ComponentsInDescendants<UIDropdownMenu>().Single();
		label = label ?? ReflectHelper.GetLabelInfo(memberExpression.Member);
		tooltip = tooltip ?? ReflectHelper.GetDescriptionInfo(memberExpression.Member);
		GUIStringLabel gUIStringLabel = uIOptionsTag.transform.ComponentsInDescendants<GUIStringLabel>().Single();
		if (labelWidth > 0)
		{
			gUIStringLabel.DatabaseString = IEModString.Register(label);
			UILabel uILabel = gUIStringLabel.gameObject.Component<UILabel>();
			uILabel.lineWidth = labelWidth;
		}
		else
		{
			gUIStringLabel.DatabaseString = IEModString.Register("");
			UILabel uILabel2 = gUIStringLabel.gameObject.Component<UILabel>();
			uILabel2.lineWidth = labelWidth;
		}
		UIOptionsTag uIOptionsTag2 = uIOptionsTag.Component<UIOptionsTag>();
		uIOptionsTag2.TooltipString = IEModString.Register(tooltip);
		IEDropdownChoice[] source = choices.ToArray();
		object[] array = (uIDropdownMenu.Options = source.ToArray());
		uIDropdownMenu.SelectedItem = source.SingleOrDefault((IEDropdownChoice x) => object.Equals(x.Value, getter())) ?? uIDropdownMenu.Options[0];
		GameObject gameObject = uIOptionsTag.Descendant("Background");
		gameObject.transform.localScale = new Vector3(width, 32f, 1f);
		GameObject gameObject2 = uIOptionsTag.Descendant("ArrowPivot");
		gameObject2.transform.localPosition = new Vector3(width - 27, 10f, 0f);
		uIDropdownMenu.SelectedText.lineWidth = width;
		uIDropdownMenu.OptionGrid.cellWidth = width;
		UIGrid optionGrid = uIDropdownMenu.OptionGrid;
		uIDropdownMenu.OnDropdownOptionChanged += delegate (object option)
		{
			IEDropdownChoice iEDropdownChoice = (IEDropdownChoice)option;
			setter((T)iEDropdownChoice.Value);
		};
		return uIOptionsTag.gameObject;
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.Helpers.IEControlFactory::EnumBoundDropdown(System.Linq.Expressions.Expression`1<System.Func`1<T>>,System.Int32,System.Int32)")]
	public GameObject EnumBoundDropdown<T>(Expression<Func<T>> enumMemberAccessExpr, int width, int labelWidth) where T : struct, IConvertible, IFormattable, IComparable
	{
		if (!typeof(T).IsEnum)
		{
			throw IEDebug.Exception(null, "Expected an enum type, but got {0}", typeof(T));
		}
		return Dropdown(enumMemberAccessExpr, EnumToChoices(typeof(T)), width, labelWidth);
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.Helpers.IEControlFactory::Checkbox(System.Linq.Expressions.Expression`1<System.Func`1<System.Boolean>>)")]
	public GameObject Checkbox(Expression<Func<bool>> memberAccessExpr)
	{
		if (ExampleCheckbox == null)
		{
			throw IEDebug.Exception(null, "You must initialize the ExampleCheckbox to create a check box.", null);
		}
		MemberExpression memberExpression = (MemberExpression)memberAccessExpr.Body;
		MemberInfo member = memberExpression.Member;
		IEDebug.Log("Creating Checkbox : {0}", member.Name);
		Action<bool> setter = ReflectHelper.CreateSetter(memberAccessExpr);
		UIOptionsTag uIOptionsTag = UnityEngine.Object.Instantiate(ExampleCheckbox);
		uIOptionsTag.transform.parent = CurrentParent;
		Func<bool> func = ReflectHelper.CreateGetter(memberAccessExpr);
		uIOptionsTag.name = memberExpression.Member.Name;
		uIOptionsTag.transform.localScale = new Vector3(1f, 1f, 1f);
		uIOptionsTag.transform.localPosition = new Vector3(0f, 0f, 0f);
		string labelInfo = ReflectHelper.GetLabelInfo(member);
		string descriptionInfo = ReflectHelper.GetDescriptionInfo(member);
		uIOptionsTag.CheckboxLabel = IEModString.Register(labelInfo);
		uIOptionsTag.TooltipString = IEModString.Register(descriptionInfo);
		uIOptionsTag.Checkbox.startsChecked = func();
		UICheckbox checkbox = uIOptionsTag.Checkbox;
		checkbox.onStateChange = (UICheckbox.OnStateChange)Delegate.Combine(checkbox.onStateChange, (UICheckbox.OnStateChange)delegate (GameObject sender, bool state)
		{
			setter(state);
		});
		IEDebug.Log("IEMod created: " + uIOptionsTag.name);
		return uIOptionsTag.gameObject;
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.Helpers.IEControlFactory::Button(System.String,System.String,System.Nullable`1<UnityEngine.Vector3>)")]
	public GameObject Button(string caption, string name = null, Vector3? localPos = null)
	{
		if (ExampleButton == null)
		{
			throw IEDebug.Exception(null, "You must initialize ExampleButton to create one.");
		}
		string[] replaceWhat = " :\t\n\r/1#$%^&*().".ToCharArray().Select(char.ToString).ToArray();
		name = name ?? caption.ReplaceAll("_", replaceWhat);
		GameObject gameObject = UnityEngine.Object.Instantiate(ExampleButton);
		gameObject.name = name;
		gameObject.transform.parent = CurrentParent;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = localPos ?? Vector3.zero;
		gameObject.ComponentInDescendants<UIMultiSpriteImageButton>().Label.Component<GUIStringLabel>().FormatString = caption;
		return gameObject;
	}

	[PatchedByMember("System.Void IEMod.Helpers.IEControlFactory::.ctor()")]
	public IEControlFactory()
	{
	}
}
