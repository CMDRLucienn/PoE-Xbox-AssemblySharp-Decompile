using System;
using UnityEngine;

public class Affliction : MonoBehaviour, ITooltipContent
{
	public DatabaseString DisplayName = new DatabaseString(DatabaseString.StringTableType.Afflictions);

	public DatabaseString Description = new DatabaseString(DatabaseString.StringTableType.Afflictions);

	public string Tag;

	public Texture2D Icon;

	[Tooltip("If set, only one of these can be on a target. A new one clears out any existing one.")]
	public bool Exclusive;

	[Tooltip("The status effects that make up this affliction.")]
	public StatusEffectParams[] StatusEffects;

	[Tooltip("This affliction overrides the listed ones. Those afflictions are still present on their target and still tick down, but their effects are suppressed.")]
	public Affliction[] Overrides;

	[Tooltip("Does this affliction come from resting somewhere?")]
	public bool FromResting;

	[Tooltip("If true, cancel all combat engagements when applied.")]
	public bool DisengageAll;

	[Tooltip("Overrides material settings on afflicted characters.")]
	public MaterialReplacement Material;

	[Tooltip("If set, UI will not bundle the effects of this affliction.")]
	public bool ThinUI;

	[Tooltip("If set, this affliction will never be shown in the UI.")]
	public bool HideFromUI;

	public static string Name(GameObject go)
	{
		if (!go)
		{
			Debug.LogError("Tried to get the name of a null game object.");
			return "*NameError*";
		}
		Affliction component = go.GetComponent<Affliction>();
		if ((bool)component)
		{
			return component.Name();
		}
		Debug.LogError("Tried to get the name of something that wasn't an affliction (" + go.name + ")");
		return "*NameError*";
	}

	public static string Name(MonoBehaviour mb)
	{
		if ((bool)mb)
		{
			return Name(mb.gameObject);
		}
		Debug.LogError("Tried to get the name of a null behaviour.");
		return "*NameError*";
	}

	public string Name()
	{
		return DisplayName.GetText();
	}

	public bool OverridesAffliction(Affliction aff)
	{
		if (Overrides == null)
		{
			return false;
		}
		Affliction[] overrides = Overrides;
		for (int i = 0; i < overrides.Length; i++)
		{
			if (overrides[i] == aff)
			{
				return true;
			}
		}
		return false;
	}

	public override string ToString()
	{
		return DisplayName.ToString();
	}

	public override bool Equals(object other)
	{
		Affliction affliction = other as Affliction;
		if ((bool)affliction)
		{
			if ((object)this == affliction)
			{
				return true;
			}
			if (Tag.Equals(affliction.Tag, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Tag.GetHashCode();
	}

	public static bool operator ==(Affliction aff1, Affliction aff2)
	{
		return aff1?.Equals(aff2) ?? ((object)aff2 == null);
	}

	public static bool operator !=(Affliction aff1, Affliction aff2)
	{
		if ((object)aff1 == null)
		{
			return (object)aff2 != null;
		}
		return !aff1.Equals(aff2);
	}

	public string GetTooltipContent(GameObject owner)
	{
		return StatusEffectParams.ListToString(StatusEffects);
	}

	public string GetTooltipName(GameObject owner)
	{
		return Name();
	}

	public Texture GetTooltipIcon()
	{
		return Icon;
	}
}
