using System;

[Serializable]
public class Tuple<A, B>
{
	public A First;

	public B Second;

	public Tuple(A First, B Second)
	{
		this.First = First;
		this.Second = Second;
	}

	public override bool Equals(object obj)
	{
		if (obj is Tuple<A, B> tuple)
		{
			if (tuple.First.Equals(First))
			{
				return tuple.Second.Equals(Second);
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return 17 * First.GetHashCode() + 31 * Second.GetHashCode();
	}
}
