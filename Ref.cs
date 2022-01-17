using System;

public class Ref<T>
{
	private Func<T> getter;

	private Action<T> setter;

	public T Val
	{
		get
		{
			return getter();
		}
		set
		{
			setter(value);
		}
	}

	public Ref(Func<T> getter, Action<T> setter)
	{
		this.getter = getter;
		this.setter = setter;
	}

	public override string ToString()
	{
		return Val.ToString();
	}

	public override int GetHashCode()
	{
		return Val.GetHashCode();
	}
}
