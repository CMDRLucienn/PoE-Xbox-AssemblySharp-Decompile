// IEMod.Helpers.MemberAccess<T>
using System;
using System.Linq.Expressions;
using System.Reflection;
using Patchwork.Attributes;

[PatchedByType("IEMod.Helpers.MemberAccess`1")]
[NewType(null, null)]
public class MemberAccess<T>
{
	private readonly MemberExpression _expression;

	public Action<T> Setter
	{
		[PatchedByMember("System.Action`1<T> IEMod.Helpers.MemberAccess`1::get_Setter()")]
		get;
		[PatchedByMember("System.Void IEMod.Helpers.MemberAccess`1::set_Setter(System.Action`1<T>)")]
		private set;
	}

	public Func<T> Getter
	{
		[PatchedByMember("System.Func`1<T> IEMod.Helpers.MemberAccess`1::get_Getter()")]
		get;
		[PatchedByMember("System.Void IEMod.Helpers.MemberAccess`1::set_Getter(System.Func`1<T>)")]
		private set;
	}

	public MemberInfo TopmostMember
	{
		[PatchedByMember("System.Reflection.MemberInfo IEMod.Helpers.MemberAccess`1::get_TopmostMember()")]
		get;
		[PatchedByMember("System.Void IEMod.Helpers.MemberAccess`1::set_TopmostMember(System.Reflection.MemberInfo)")]
		private set;
	}

	public Func<object> InstanceGetter
	{
		[PatchedByMember("System.Func`1<System.Object> IEMod.Helpers.MemberAccess`1::get_InstanceGetter()")]
		get;
		[PatchedByMember("System.Void IEMod.Helpers.MemberAccess`1::set_InstanceGetter(System.Func`1<System.Object>)")]
		private set;
	}

	[PatchedByMember("System.Void IEMod.Helpers.MemberAccess`1::.ctor(System.Linq.Expressions.Expression`1<System.Func`1<T>>)")]
	public MemberAccess(Expression<Func<T>> expression)
	{
		MemberExpression memberExpression = (_expression = (MemberExpression)expression.Body);
		Getter = ReflectHelper.CreateGetter(expression);
		Getter = expression.Compile();
		Action<T> setter = ReflectHelper.CreateSetter(expression);
		Setter = delegate (T v)
		{
			setter(v);
		};
		TopmostMember = memberExpression.Member;
		InstanceGetter = ReflectHelper.CreateGetter(memberExpression.Expression);
	}
}
