// IEMod.Helpers.UnityObjectExtensions
using System;
using System.Collections.Generic;
using System.Linq;
using Patchwork.Attributes;
using UnityEngine;

[NewType(null, null)]
[PatchedByType("IEMod.Helpers.UnityObjectExtensions")]
public static class UnityObjectExtensions
{
	[PatchedByMember("System.Void IEMod.Helpers.UnityObjectExtensions::MaybeUnregister(GUIDatabaseString)")]
	public static void MaybeUnregister(this GUIDatabaseString str)
	{
		if (str is IEModString iEModString)
		{
			iEModString.Unregister();
		}
	}

	[PatchedByMember("UnityEngine.Vector3 IEMod.Helpers.UnityObjectExtensions::ScaleBy(UnityEngine.Vector3,System.Single)")]
	public static Vector3 ScaleBy(this Vector3 self, float scalar)
	{
		self.Set(self.x * scalar, self.y * scalar, self.z * scalar);
		return self;
	}

	[PatchedByMember("System.Void IEMod.Helpers.UnityObjectExtensions::SetBehaviors(UnityEngine.GameObject,System.Boolean)")]
	public static void SetBehaviors<T>(this GameObject self, bool newState) where T : Behaviour
	{
		List<T> list = self.Components<T>().ToList();
		if (list.Count == 0)
		{
			IEDebug.Log(null, "WARNING: In GameObject {0}, told to set behaviors of type {1} to {2}, but no behaviors of this type were found.", self.name, typeof(T), newState);
		}
		else
		{
			list.ForEach(delegate (T x)
			{
				x.enabled = newState;
			});
		}
	}

	[PatchedByMember("System.Void IEMod.Helpers.UnityObjectExtensions::AddChild(UnityEngine.GameObject,UnityEngine.GameObject)")]
	public static void AddChild(this GameObject self, GameObject child)
	{
		child.transform.parent = self.transform;
	}

	[PatchedByMember("System.Void IEMod.Helpers.UnityObjectExtensions::AssertAlive(UnityEngine.GameObject)")]
	public static void AssertAlive(this GameObject o)
	{
		if (o == null)
		{
			throw IEDebug.Exception(null, "An attempt was made to access a GameObject, but it has been destroyed.");
		}
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.Helpers.UnityObjectExtensions::ChildPath(UnityEngine.GameObject,System.String)")]
	public static GameObject ChildPath(this GameObject start, string path)
	{
		string[] array = path.Split('/');
		GameObject gameObject = start;
		string[] array2 = array;
		foreach (string text in array2)
		{
			string text2 = text;
			if (!(text2 == ""))
			{
				bool flag = text2.StartsWith("#");
				if (flag)
				{
					text2 = text2.Substring(1);
				}
				gameObject = ((flag && int.TryParse(text2, out var result)) ? gameObject.Child(result) : gameObject.Child(text2));
			}
		}
		return gameObject;
	}

	[PatchedByMember("System.Boolean IEMod.Helpers.UnityObjectExtensions::HasChild(UnityEngine.GameObject,System.String)")]
	public static bool HasChild(this GameObject o, string name)
	{
		return o.Children(name).Any();
	}

	[PatchedByMember("System.Boolean IEMod.Helpers.UnityObjectExtensions::HasComponent(UnityEngine.GameObject)")]
	public static bool HasComponent<T>(this GameObject o) where T : Component
	{
		return (UnityEngine.Object)o.GetComponent<T>() != (UnityEngine.Object)null;
	}

	[PatchedByMember("UnityEngine.Component[] IEMod.Helpers.UnityObjectExtensions::Components(UnityEngine.GameObject,System.Type)")]
	public static Component[] Components(this GameObject o, Type t = null)
	{
		if (o == null)
		{
			throw IEDebug.Exception(null, "GameObject cannot be null.");
		}
		return o.GetComponents(t ?? typeof(Component));
	}

	[PatchedByMember("T[] IEMod.Helpers.UnityObjectExtensions::Components(UnityEngine.GameObject)")]
	public static T[] Components<T>(this GameObject o) where T : Component
	{
		return o.Components(typeof(T)).Cast<T>().ToArray();
	}

	[PatchedByMember("T IEMod.Helpers.UnityObjectExtensions::Component(UnityEngine.GameObject)")]
	public static T Component<T>(this GameObject o) where T : Component
	{
		if (o == null)
		{
			throw IEDebug.Exception(null, $"When trying to get component of type {typeof(T)}: GameObject cannot be null.");
		}
		T[] array = o.Components<T>();
		if (array.Length > 1 || array.Length == 0)
		{
			UnityPrinter.ShallowPrinter.Print(o);
			throw IEDebug.Exception(null, "GameObject '{0}' has {1} components of type {2}, but told to pick exactly one.", o.name, array.Length, typeof(T));
		}
		return array[0];
	}

	[PatchedByMember("T[] IEMod.Helpers.UnityObjectExtensions::ComponentsInDescendants(UnityEngine.Component,System.Boolean)")]
	public static T[] ComponentsInDescendants<T>(this Component c, bool inactive = true) where T : Component
	{
		if (c == null)
		{
			throw IEDebug.Exception(null, "Component cannot be null.");
		}
		return c.gameObject.ComponentsInDescendants<T>(inactive);
	}

	[PatchedByMember("T IEMod.Helpers.UnityObjectExtensions::ComponentInDescendants(UnityEngine.Component,System.Boolean)")]
	public static T ComponentInDescendants<T>(this Component c, bool inactive = true) where T : Component
	{
		if (c == null)
		{
			throw IEDebug.Exception(null, "Component cannot be null.");
		}
		return c.gameObject.ComponentInDescendants<T>(inactive);
	}

	[PatchedByMember("T[] IEMod.Helpers.UnityObjectExtensions::ComponentsInDescendants(UnityEngine.GameObject,System.Boolean)")]
	public static T[] ComponentsInDescendants<T>(this GameObject o, bool inactive = true) where T : Component
	{
		if (o == null)
		{
			throw IEDebug.Exception(null, "GameObject cannot be null.");
		}
		return o.GetComponentsInChildren<T>(inactive);
	}

	[PatchedByMember("T IEMod.Helpers.UnityObjectExtensions::ComponentInDescendants(UnityEngine.GameObject,System.Boolean)")]
	public static T ComponentInDescendants<T>(this GameObject o, bool inactive = true) where T : Component
	{
		if (o == null)
		{
			throw IEDebug.Exception(null, "GameObject cannot be null.");
		}
		T[] array = o.ComponentsInDescendants<T>(inactive);
		if (array.Length == 0 || array.Length > 1)
		{
			throw IEDebug.Exception(null, "GameObject '{0}' has {1} components of type {2} in its children, but told to pick exactly one.", o.name, array.Length, typeof(T));
		}
		return array[0];
	}

	[PatchedByMember("System.Collections.Generic.IList`1<T> IEMod.Helpers.UnityObjectExtensions::Components(UnityEngine.Component)")]
	public static IList<T> Components<T>(this Component o) where T : Component
	{
		if (o == null)
		{
			throw IEDebug.Exception(null, "Component cannot be null.");
		}
		return o.gameObject.Components<T>();
	}

	[PatchedByMember("T IEMod.Helpers.UnityObjectExtensions::Component(UnityEngine.Component)")]
	public static T Component<T>(this Component o) where T : Component
	{
		if (o == null)
		{
			throw IEDebug.Exception(null, "Component cannot be null.");
		}
		return o.gameObject.Component<T>();
	}

	[PatchedByMember("System.Boolean IEMod.Helpers.UnityObjectExtensions::HasComponent(UnityEngine.Component)")]
	public static bool HasComponent<T>(this Component o) where T : Component
	{
		return o.gameObject.HasComponent<T>();
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.Helpers.UnityObjectExtensions::Child(UnityEngine.Component,System.String)")]
	public static GameObject Child(this Component c, string name)
	{
		return c.gameObject.Child(name);
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.Helpers.UnityObjectExtensions::Child(UnityEngine.Component,System.Int32)")]
	public static GameObject Child(this Component c, int n)
	{
		return c.gameObject.Child(n);
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.Helpers.UnityObjectExtensions::Child(UnityEngine.GameObject,System.String)")]
	public static GameObject Child(this GameObject o, string name)
	{
		if (o == null)
		{
			throw IEDebug.Exception(null, "When trying to get Child with name '{0}': gameObject cannot be null.", name);
		}
		List<GameObject> list = (from x in o.Children()
								 where x.gameObject.name == name
								 select x).ToList();
		if (list.Count == 0 || list.Count > 1)
		{
			UnityPrinter.ShallowPrinter.Print(o);
			throw IEDebug.Exception(null, "GameObject '{0}' has {1} children with the name '{2}', but told to pick exactly one.", o.name, list.Count, name);
		}
		return list[0];
	}

	[PatchedByMember("UnityEngine.Vector3 IEMod.Helpers.UnityObjectExtensions::Plus(UnityEngine.Vector3,System.Single,System.Single,System.Single)")]
	public static Vector3 Plus(this Vector3 self, float x = 0f, float y = 0f, float z = 0f)
	{
		return new Vector3(self.x + x, self.y + y, self.z + z);
	}

	[PatchedByMember("UnityEngine.Vector2 IEMod.Helpers.UnityObjectExtensions::Plus(UnityEngine.Vector2,System.Single,System.Single)")]
	public static Vector2 Plus(this Vector2 self, float x = 0f, float y = 0f)
	{
		return new Vector2(self.x + x, self.y + y);
	}

	[PatchedByMember("UnityEngine.Vector4 IEMod.Helpers.UnityObjectExtensions::Plus(UnityEngine.Vector4,System.Single,System.Single,System.Single,System.Single)")]
	public static Vector4 Plus(this Vector4 self, float x = 0f, float y = 0f, float z = 0f, float w = 0f)
	{
		return new Vector4(self.x + x, self.y + y, self.z + z, self.w + w);
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.Helpers.UnityObjectExtensions::Child(UnityEngine.GameObject,System.Int32)")]
	public static GameObject Child(this GameObject o, int i)
	{
		if (o == null)
		{
			throw IEDebug.Exception(null, "The GameObject cannot be null.");
		}
		Transform child = o.transform.GetChild(i);
		if (child == null)
		{
			throw IEDebug.Exception(null, "The GameObject '{0}' has no child at index {1}", o.name, i);
		}
		return child.gameObject;
	}

	[PatchedByMember("System.Collections.Generic.IEnumerable`1<UnityEngine.GameObject> IEMod.Helpers.UnityObjectExtensions::Children(UnityEngine.GameObject,System.String)")]
	public static IEnumerable<GameObject> Children(this GameObject o, string name = null)
	{
		return from Transform child in o.transform
			   where name == null || name == child.name
			   select child.gameObject;
	}

	[PatchedByMember("System.Collections.Generic.IEnumerable`1<UnityEngine.GameObject> IEMod.Helpers.UnityObjectExtensions::Descendants(UnityEngine.GameObject,System.String)")]
	public static IEnumerable<GameObject> Descendants(this GameObject o, string name = null)
	{
		return from x in o.ComponentsInDescendants<Transform>()
			   select x.gameObject into x
			   where x.name == name
			   select x;
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.Helpers.UnityObjectExtensions::Descendant(UnityEngine.GameObject,System.String)")]
	public static GameObject Descendant(this GameObject o, string name)
	{
		if (o == null)
		{
			throw IEDebug.Exception(null, "GameObject cannot be null.");
		}
		IEnumerable<GameObject> source = o.Descendants(name);
		GameObject[] array = source.ToArray();
		if (array.Length == 0 || array.Length > 1)
		{
			throw IEDebug.Exception(null, "Game object '{0}' has {1} descendants with the name '{2}', but told to pick exactly one.", o.name, array.Count(), name);
		}
		return array[0];
	}

	[PatchedByMember("System.Collections.Generic.IEnumerable`1<UnityEngine.GameObject> IEMod.Helpers.UnityObjectExtensions::Descendants(UnityEngine.Component,System.String)")]
	public static IEnumerable<GameObject> Descendants(this Component o, string name = null)
	{
		if (o == null)
		{
			throw IEDebug.Exception(null, "Component cannot be null.");
		}
		return o.gameObject.Descendants(name);
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.Helpers.UnityObjectExtensions::Descendant(UnityEngine.Component,System.String)")]
	public static GameObject Descendant(this Component o, string name)
	{
		if (o == null)
		{
			throw IEDebug.Exception(null, "GameObject cannot be null.");
		}
		return o.gameObject.Descendant(name);
	}
}
