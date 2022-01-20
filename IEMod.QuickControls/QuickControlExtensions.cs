// IEMod.QuickControls.QuickControlExtensions
using System.Collections.Generic;
using Patchwork.Attributes;
using UnityEngine;

[NewType(null, null)]
[PatchedByType("IEMod.QuickControls.QuickControlExtensions")]
public static class QuickControlExtensions
{
	[PatchedByMember("System.Boolean IEMod.QuickControls.QuickControlExtensions::IsAlive(IEMod.QuickControls.IGameObjectWrapper)")]
	public static bool IsAlive(this IGameObjectWrapper gow)
	{
		return gow?.IsAlive ?? false;
	}

	[PatchedByMember("System.Collections.Generic.IEnumerable`1<T> IEMod.QuickControls.QuickControlExtensions::Components(IEMod.QuickControls.IGameObjectWrapper)")]
	public static IEnumerable<T> Components<T>(this IGameObjectWrapper gow) where T : Component
	{
		return (gow?.GameObject).Components<T>();
	}

	[PatchedByMember("System.Collections.Generic.IEnumerable`1<T> IEMod.QuickControls.QuickControlExtensions::ComponentsInDescendants(IEMod.QuickControls.IGameObjectWrapper)")]
	public static IEnumerable<T> ComponentsInDescendants<T>(this IGameObjectWrapper gow) where T : Component
	{
		return (gow?.GameObject).ComponentsInDescendants<T>();
	}

	[PatchedByMember("T IEMod.QuickControls.QuickControlExtensions::ComponentInDescendants(IEMod.QuickControls.IGameObjectWrapper)")]
	public static T ComponentInDescendants<T>(this IGameObjectWrapper gow) where T : Component
	{
		return (gow?.GameObject).ComponentInDescendants<T>();
	}

	[PatchedByMember("T IEMod.QuickControls.QuickControlExtensions::Component(IEMod.QuickControls.IGameObjectWrapper)")]
	public static T Component<T>(this IGameObjectWrapper gow) where T : Component
	{
		return (gow?.GameObject).Component<T>();
	}

	[PatchedByMember("System.Collections.Generic.IEnumerable`1<UnityEngine.GameObject> IEMod.QuickControls.QuickControlExtensions::Children(IEMod.QuickControls.IGameObjectWrapper,System.String)")]
	public static IEnumerable<GameObject> Children(this IGameObjectWrapper gow, string name = null)
	{
		return (gow?.GameObject).Children(name);
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.QuickControls.QuickControlExtensions::Child(IEMod.QuickControls.IGameObjectWrapper,System.String)")]
	public static GameObject Child(this IGameObjectWrapper gow, string name)
	{
		return (gow?.GameObject).Child(name);
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.QuickControls.QuickControlExtensions::Descendant(IEMod.QuickControls.IGameObjectWrapper,System.String)")]
	public static GameObject Descendant(this IGameObjectWrapper gow, string name)
	{
		return (gow?.GameObject).Descendant(name);
	}

	[PatchedByMember("System.Void IEMod.QuickControls.QuickControlExtensions::AddComponent(IEMod.QuickControls.IGameObjectWrapper)")]
	public static void AddComponent<T>(this IGameObjectWrapper gow) where T : Component
	{
		if (gow == null)
		{
			throw IEDebug.Exception(null, "GameObject cannot be null");
		}
		gow.GameObject.AddComponent<T>();
	}

	[PatchedByMember("System.Collections.Generic.IEnumerable`1<UnityEngine.GameObject> IEMod.QuickControls.QuickControlExtensions::Descendants(IEMod.QuickControls.IGameObjectWrapper,System.String)")]
	public static IEnumerable<GameObject> Descendants(this IGameObjectWrapper gow, string name = null)
	{
		return (gow?.GameObject).Descendants(name);
	}

	[PatchedByMember("System.Boolean IEMod.QuickControls.QuickControlExtensions::HasComponent(IEMod.QuickControls.IGameObjectWrapper)")]
	public static bool HasComponent<T>(this IGameObjectWrapper gow) where T : Component
	{
		return (gow?.GameObject).HasComponent<T>();
	}
}
