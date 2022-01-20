// IEMod.QuickControls.Prefabs
using Patchwork.Attributes;
using UnityEngine;

[NewType(null, null)]
[PatchedByType("IEMod.QuickControls.Prefabs")]
public static class Prefabs
{
	private static UIOptionsTag _quickCheckbox;

	private static GameObject _quickDropdown;

	private static GameObject _quickButton;

	private static GameObject _page;

	public static UIOptionsTag QuickCheckbox
	{
		[PatchedByMember("UIOptionsTag IEMod.QuickControls.Prefabs::get_QuickCheckbox()")]
		get
		{
			if (_quickCheckbox == null)
			{
				throw NotInitialized("QuickCheckbox");
			}
			return _quickCheckbox;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.Prefabs::set_QuickCheckbox(UIOptionsTag)")]
		set
		{
			_quickCheckbox = value;
		}
	}

	public static GameObject QuickDropdown
	{
		[PatchedByMember("UnityEngine.GameObject IEMod.QuickControls.Prefabs::get_QuickDropdown()")]
		get
		{
			if (_quickCheckbox == null)
			{
				throw NotInitialized("QuickDropdown");
			}
			return _quickDropdown;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.Prefabs::set_QuickDropdown(UnityEngine.GameObject)")]
		set
		{
			_quickDropdown = value;
		}
	}

	public static GameObject QuickButton
	{
		[PatchedByMember("UnityEngine.GameObject IEMod.QuickControls.Prefabs::get_QuickButton()")]
		get
		{
			if (_quickButton == null)
			{
				throw NotInitialized("QuickButton");
			}
			return _quickButton;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.Prefabs::set_QuickButton(UnityEngine.GameObject)")]
		set
		{
			_quickButton = value;
		}
	}

	public static GameObject QuickPage
	{
		[PatchedByMember("UnityEngine.GameObject IEMod.QuickControls.Prefabs::get_QuickPage()")]
		get
		{
			if (_quickButton == null)
			{
				throw NotInitialized("Page");
			}
			return _page;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.Prefabs::set_QuickPage(UnityEngine.GameObject)")]
		set
		{
			_page = value;
		}
	}

	[PatchedByMember("IEMod.Helpers.IEModException IEMod.QuickControls.Prefabs::NotInitialized(System.String)")]
	private static IEModException NotInitialized(string what)
	{
		return IEDebug.Exception(null, "The {0} prefab hasn't been initialized! You have to initialize it before creating any {0}s.", what);
	}
}
