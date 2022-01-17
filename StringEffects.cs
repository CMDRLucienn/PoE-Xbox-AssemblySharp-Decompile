using System.Collections.Generic;

public class StringEffects
{
	public Dictionary<string, List<AttackBase.AttackEffect>> Effects;

	public int Count => Effects.Count;

	public List<AttackBase.AttackEffect> this[string key]
	{
		get
		{
			return Effects[key];
		}
		set
		{
			Effects[key] = value;
		}
	}

	public StringEffects()
	{
		Effects = new Dictionary<string, List<AttackBase.AttackEffect>>();
	}

	public static implicit operator Dictionary<string, List<AttackBase.AttackEffect>>(StringEffects other)
	{
		return other.Effects;
	}
}
