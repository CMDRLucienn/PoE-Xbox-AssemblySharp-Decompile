// IEMod.Helpers.UnityPrinter
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Patchwork.Attributes;
using UnityEngine;

[PatchedByType("IEMod.Helpers.UnityPrinter")]
[NewType(null, null)]
public class UnityPrinter
{
	[PatchedByType("IEMod.Helpers.UnityPrinter/RecursiveObjectPrinter")]
	[NewType(null, null)]
	private class RecursiveObjectPrinter
	{
		private readonly IndentedTextWriter _writer;

		private readonly UnityPrinter _parent;

		private readonly IDictionary<object, bool> _visited = new Dictionary<object, bool>();

		[PatchedByMember("System.Void IEMod.Helpers.UnityPrinter/RecursiveObjectPrinter::.ctor(System.IO.TextWriter,IEMod.Helpers.UnityPrinter)")]
		public RecursiveObjectPrinter(TextWriter writer, UnityPrinter parent)
		{
			_parent = parent;
			_writer = new IndentedTextWriter(writer);
		}

		[PatchedByMember("System.Void IEMod.Helpers.UnityPrinter/RecursiveObjectPrinter::PrintValue(System.String,System.Func`1<System.Object>)")]
		private void PrintValue(string key, Func<object> getter)
		{
			object obj;
			try
			{
				obj = getter();
			}
			catch (Exception ex)
			{
				_writer.WriteLine("{0} = !! {1} !! //an exception occured while evaluating this", key, ex.GetType());
				return;
			}
			if (obj == null)
			{
				_writer.WriteLine("{0} = {1}", key, "(null)");
				return;
			}
			string text = obj.ToString();
			List<string> list = text.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			if (list.Count <= 1)
			{
				_writer.WriteLine("{0} = {1}", key, text);
				return;
			}
			_writer.WriteLine("{0} = ", key);
			_writer.Indent++;
			list.ForEach(_writer.WriteLine);
			_writer.Indent--;
		}

		[PatchedByMember("System.Void IEMod.Helpers.UnityPrinter/RecursiveObjectPrinter::PrintObjectMembers(System.Object)")]
		private void PrintObjectMembers(object o)
		{
			if (o == null)
			{
				_writer.WriteLine("(null)");
				return;
			}
			Type type = o.GetType();
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo[] array = properties;
			foreach (PropertyInfo prop in array)
			{
				Type propertyType = prop.PropertyType;
				PrintValue($"{propertyType.Name} {prop.Name}", () => prop.GetValue(o, null));
			}
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			FieldInfo[] array2 = fields;
			foreach (FieldInfo field in array2)
			{
				Type fieldType = field.FieldType;
				PrintValue($"{fieldType.Name} {field.Name}", () => field.GetValue(o));
			}
		}

		[PatchedByMember("System.Void IEMod.Helpers.UnityPrinter/RecursiveObjectPrinter::PrintObject(UnityEngine.Object,System.Int32)")]
		public void PrintObject(UnityEngine.Object o, int recursionDepth = 0)
		{
			Component component = o as Component;
			GameObject gameObject = o as GameObject;
			if (component == null && gameObject == null)
			{
				_writer.WriteLine("{0} : {1}", o.GetType(), o.name);
				_writer.Indent++;
				PrintObjectMembers(o);
				_writer.Indent--;
			}
			else
			{
				if (component != null)
				{
					gameObject = component.gameObject;
				}
				PrintUnityGameObject(gameObject, recursionDepth);
			}
		}

		[PatchedByMember("System.Void IEMod.Helpers.UnityPrinter/RecursiveObjectPrinter::PrintUnityGameObject(UnityEngine.GameObject,System.Int32)")]
		public void PrintUnityGameObject(GameObject o, int recursionDepth = 0)
		{
			if (o == null)
			{
				_writer.WriteLine("(null)");
				return;
			}
			_writer.Write("{0} : {1}", o.GetType(), o.name);
			if (_visited.ContainsKey(o))
			{
				_writer.WriteLine(" (already dumped)");
				return;
			}
			if (recursionDepth >= _parent.MaxRecursionDepth)
			{
				_writer.WriteLine(" (recursion depth exceeded)");
				return;
			}
			_writer.WriteLine();
			_writer.Indent++;
			_writer.WriteLine("Parent: " + o.transform.parent?.name);
			_writer.WriteLine("Components:");
			_writer.Indent++;
			_visited[o] = true;
			Component[] array = o.Components(typeof(Component));
			foreach (Component component in array)
			{
				_writer.WriteLine("[Component] {0}", component.GetType().Name);
				if (_parent.ComponentFilter(component))
				{
					_writer.Indent++;
					PrintObjectMembers(component);
					_writer.Indent--;
				}
			}
			_writer.Indent--;
			_writer.WriteLine("Children:");
			int childCount = o.transform.childCount;
			for (int j = 0; j < childCount; j++)
			{
				Transform child = o.transform.GetChild(j);
				_writer.Write("{0}.\t [Child] ", j);
				if (_parent.GameObjectFilter(child.gameObject))
				{
					PrintUnityGameObject(child.gameObject, recursionDepth + 1);
				}
			}
			_writer.Indent--;
		}
	}

	private readonly IDictionary<object, double> _timestamps = new Dictionary<object, double>();

	public static UnityPrinter FullPrinter = new UnityPrinter
	{
		MillisecondInterval = 1000.0
	};

	public static UnityPrinter HierarchyPrinter = new UnityPrinter
	{
		ComponentFilter = (Component x) => false,
		MillisecondInterval = 1000.0
	};

	public static readonly UnityPrinter ShallowPrinter = new UnityPrinter
	{
		ComponentFilter = (Component x) => false,
		MaxRecursionDepth = 1,
		MillisecondInterval = 1000.0
	};

	public static readonly UnityPrinter ComponentPrinter = new UnityPrinter
	{
		ComponentFilter = (Component x) => true,
		MaxRecursionDepth = 1,
		MillisecondInterval = 1000.0
	};

	public Func<GameObject, bool> GameObjectFilter
	{
		[PatchedByMember("System.Func`2<UnityEngine.GameObject,System.Boolean> IEMod.Helpers.UnityPrinter::get_GameObjectFilter()")]
		get;
		[PatchedByMember("System.Void IEMod.Helpers.UnityPrinter::set_GameObjectFilter(System.Func`2<UnityEngine.GameObject,System.Boolean>)")]
		set;
	}

	public Func<Component, bool> ComponentFilter
	{
		[PatchedByMember("System.Func`2<UnityEngine.Component,System.Boolean> IEMod.Helpers.UnityPrinter::get_ComponentFilter()")]
		get;
		[PatchedByMember("System.Void IEMod.Helpers.UnityPrinter::set_ComponentFilter(System.Func`2<UnityEngine.Component,System.Boolean>)")]
		set;
	}

	public int MaxRecursionDepth
	{
		[PatchedByMember("System.Int32 IEMod.Helpers.UnityPrinter::get_MaxRecursionDepth()")]
		get;
		[PatchedByMember("System.Void IEMod.Helpers.UnityPrinter::set_MaxRecursionDepth(System.Int32)")]
		set;
	}

	public double MillisecondInterval
	{
		[PatchedByMember("System.Double IEMod.Helpers.UnityPrinter::get_MillisecondInterval()")]
		get;
		[PatchedByMember("System.Void IEMod.Helpers.UnityPrinter::set_MillisecondInterval(System.Double)")]
		set;
	}

	[PatchedByMember("System.Void IEMod.Helpers.UnityPrinter::.ctor()")]
	public UnityPrinter()
	{
		GameObjectFilter = (GameObject x) => true;
		ComponentFilter = (Component x) => true;
		MaxRecursionDepth = 1024;
		MillisecondInterval = 0.0;
	}

	[PatchedByMember("System.Void IEMod.Helpers.UnityPrinter::Print(UnityEngine.Object)")]
	public void Print(UnityEngine.Object o)
	{
		IEDebug.Log(PrintString(o));
	}

	[PatchedByMember("System.String IEMod.Helpers.UnityPrinter::PrintString(UnityEngine.Object)")]
	public string PrintString(UnityEngine.Object o)
	{
		double totalMilliseconds = TimeSpan.FromTicks(Environment.TickCount).TotalMilliseconds;
		if (_timestamps.ContainsKey(o))
		{
			double num = _timestamps[o];
			if (totalMilliseconds - num < MillisecondInterval)
			{
				return "";
			}
		}
		_timestamps[o] = totalMilliseconds;
		StringWriter stringWriter = new StringWriter();
		IndentedTextWriter indentedTextWriter = new IndentedTextWriter(stringWriter);
		RecursiveObjectPrinter recursiveObjectPrinter = new RecursiveObjectPrinter(indentedTextWriter, this);
		recursiveObjectPrinter.PrintObject(o);
		indentedTextWriter.Flush();
		stringWriter.Flush();
		_timestamps[o] = TimeSpan.FromTicks(Environment.TickCount).TotalMilliseconds;
		return stringWriter.ToString();
	}
}
