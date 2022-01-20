// IEMod.Helpers.ReflectHelper
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Patchwork.Attributes;
using UnityEngine;

[NewType(null, null)]
[PatchedByType("IEMod.Helpers.ReflectHelper")]
public static class ReflectHelper
{
	[PatchedByMember("System.Collections.Generic.IEnumerable`1<T> IEMod.Helpers.ReflectHelper::GetCustomAttributes(System.Reflection.ICustomAttributeProvider)")]
	public static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeProvider provider)
	{
		return provider.GetCustomAttributes(typeof(T), inherit: true).OfType<T>();
	}

	[PatchedByMember("T IEMod.Helpers.ReflectHelper::GetCustomAttribute(System.Reflection.ICustomAttributeProvider)")]
	public static T GetCustomAttribute<T>(this ICustomAttributeProvider provider)
	{
		return provider.GetCustomAttributes<T>().SingleOrDefault();
	}

	[PatchedByMember("System.Action`1<System.Object> IEMod.Helpers.ReflectHelper::CreateSetter(System.Linq.Expressions.MemberExpression)")]
	public static Action<object> CreateSetter(MemberExpression expr)
	{
		if (expr == null)
		{
			throw IEDebug.Exception(null, "The expression is not allowed to be null.");
		}
		Func<object> targetGetter = CreateGetter(expr.Expression);
		if (expr.Member is FieldInfo)
		{
			FieldInfo asFieldInfo = (FieldInfo)expr.Member;
			return delegate (object value)
			{
				asFieldInfo.SetValue(targetGetter(), value);
			};
		}
		if (expr.Member is PropertyInfo)
		{
			PropertyInfo asPropertyInfo = (PropertyInfo)expr.Member;
			return delegate (object value)
			{
				asPropertyInfo.SetValue(targetGetter(), value, null);
			};
		}
		throw new IEModException("Expected PropertyInfo or FieldInfo member, but got: " + expr.Member);
	}

	[PatchedByMember("System.Action`1<T> IEMod.Helpers.ReflectHelper::CreateSetter(System.Linq.Expressions.Expression`1<System.Func`1<T>>)")]
	public static Action<T> CreateSetter<T>(Expression<Func<T>> memberAccess)
	{
		if (!(memberAccess.Body is MemberExpression))
		{
			throw IEDebug.Exception(null, "The topmost expression must be a simple member access expression.");
		}
		return delegate (T v)
		{
			CreateSetter((MemberExpression)memberAccess.Body)(v);
		};
	}

	[PatchedByMember("System.Func`1<T> IEMod.Helpers.ReflectHelper::CreateGetter(System.Linq.Expressions.Expression`1<System.Func`1<T>>)")]
	public static Func<T> CreateGetter<T>(Expression<Func<T>> memberAccess)
	{
		return () => (T)CreateGetter(memberAccess.Body)();
	}

	[PatchedByMember("System.Func`1<System.Object> IEMod.Helpers.ReflectHelper::CreateBaseGetter(System.Linq.Expressions.Expression`1<System.Func`1<T>>)")]
	public static Func<object> CreateBaseGetter<T>(Expression<Func<T>> memberAccess)
	{
		if (!(memberAccess.Body is MemberExpression))
		{
			throw IEDebug.Exception(null, "The topmost expression must be a simple member access expression.");
		}
		MemberExpression memberExpression = (MemberExpression)memberAccess.Body;
		return CreateGetter(memberExpression.Expression);
	}

	[PatchedByMember("System.String IEMod.Helpers.ReflectHelper::TryGetName(System.Object)")]
	public static string TryGetName(object o)
	{
		IGameObjectWrapper gameObjectWrapper = o as IGameObjectWrapper;
		GameObject gameObject = o as GameObject;
		return (gameObjectWrapper != null) ? gameObjectWrapper.Name : gameObject?.name;
	}

	[PatchedByMember("IEMod.Helpers.MemberAccess`1<T> IEMod.Helpers.ReflectHelper::AnalyzeMember(System.Linq.Expressions.Expression`1<System.Func`1<T>>)")]
	public static MemberAccess<T> AnalyzeMember<T>(Expression<Func<T>> memberAccessExpr)
	{
		return new MemberAccess<T>(memberAccessExpr);
	}

	[PatchedByMember("System.Func`1<System.Object> IEMod.Helpers.ReflectHelper::CreateGetter(System.Linq.Expressions.Expression)")]
	public static Func<object> CreateGetter(Expression expr)
	{
		if (expr == null)
		{
			return () => null;
		}
		switch (expr.NodeType)
		{
			case ExpressionType.Constant:
				{
					ConstantExpression asConst = (ConstantExpression)expr;
					return () => asConst.Value;
				}
			case ExpressionType.MemberAccess:
				{
					MemberExpression memberExpression = (MemberExpression)expr;
					Func<object> targetGetter = CreateGetter(memberExpression.Expression);
					if (memberExpression.Member is FieldInfo)
					{
						FieldInfo field = (FieldInfo)memberExpression.Member;
						return () => field.GetValue(targetGetter());
					}
					if (memberExpression.Member is PropertyInfo)
					{
						PropertyInfo prop = (PropertyInfo)memberExpression.Member;
						return () => prop.GetValue(targetGetter(), null);
					}
					throw IEDebug.Exception(null, "Unexpected member type ", memberExpression.Member.GetType());
				}
			default:
				throw IEDebug.Exception(null, "Unexpected node type {0}", expr.NodeType);
		}
	}

	[PatchedByMember("System.String IEMod.Helpers.ReflectHelper::GetLabelInfo(System.Reflection.MemberInfo)")]
	public static string GetLabelInfo(MemberInfo provider)
	{
		string text = GetCustomAttribute<LabelAttribute>(provider)?.Label;
		return text ?? provider.Name;
	}

	[PatchedByMember("System.String IEMod.Helpers.ReflectHelper::GetDescriptionInfo(System.Reflection.MemberInfo)")]
	public static string GetDescriptionInfo(MemberInfo provider)
	{
		string text = GetCustomAttribute<DescriptionAttribute>(provider)?.Description;
		return text ?? provider.Name;
	}
}
