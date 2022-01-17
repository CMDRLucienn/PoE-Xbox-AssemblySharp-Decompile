using System;

[Serializable]
public class CurrencyValue
{
	public float v;

	public override string ToString()
	{
		return v.ToString();
	}

	public override int GetHashCode()
	{
		return v.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return v.Equals(obj);
	}

	public static implicit operator float(CurrencyValue val)
	{
		return val?.v ?? 0f;
	}

	public static explicit operator CurrencyValue(float val)
	{
		return new CurrencyValue
		{
			v = val
		};
	}
}
