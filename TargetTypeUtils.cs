public static class TargetTypeUtils
{
	public static AttackBase.TargetType LayerTargetTypes(AttackBase.TargetType a, AttackBase.TargetType b)
	{
		if (a == b)
		{
			return a;
		}
		if (((a == AttackBase.TargetType.NotSelf || a == AttackBase.TargetType.AllyNotSelfOrHostile) && b == AttackBase.TargetType.Self) || ((b == AttackBase.TargetType.NotSelf || b == AttackBase.TargetType.AllyNotSelfOrHostile) && a == AttackBase.TargetType.Self))
		{
			return AttackBase.TargetType.None;
		}
		if (a == AttackBase.TargetType.All)
		{
			return b;
		}
		if (b == AttackBase.TargetType.All)
		{
			return a;
		}
		AttackBase.TargetType targetType = DecayTargetType(a);
		AttackBase.TargetType targetType2 = DecayTargetType(b);
		if (targetType == AttackBase.TargetType.All && targetType2 == AttackBase.TargetType.All)
		{
			return targetType;
		}
		if (targetType == AttackBase.TargetType.All)
		{
			return b;
		}
		if (targetType2 == AttackBase.TargetType.All)
		{
			return a;
		}
		if (targetType == AttackBase.TargetType.None || targetType2 == AttackBase.TargetType.None)
		{
			return AttackBase.TargetType.None;
		}
		switch (targetType)
		{
		case AttackBase.TargetType.Hostile:
			if (targetType2 == AttackBase.TargetType.Friendly || targetType2 == AttackBase.TargetType.Ally || targetType2 == AttackBase.TargetType.Self)
			{
				return AttackBase.TargetType.None;
			}
			return targetType;
		case AttackBase.TargetType.Ally:
			return targetType2 switch
			{
				AttackBase.TargetType.Hostile => AttackBase.TargetType.None, 
				AttackBase.TargetType.Self => b, 
				_ => targetType, 
			};
		case AttackBase.TargetType.Friendly:
			return targetType2 switch
			{
				AttackBase.TargetType.Hostile => AttackBase.TargetType.None, 
				AttackBase.TargetType.Ally => b, 
				AttackBase.TargetType.Self => b, 
				_ => targetType, 
			};
		case AttackBase.TargetType.Self:
			switch (targetType2)
			{
			case AttackBase.TargetType.Friendly:
			case AttackBase.TargetType.Ally:
				return a;
			case AttackBase.TargetType.Hostile:
				return AttackBase.TargetType.None;
			default:
				return targetType;
			}
		default:
			return AttackBase.TargetType.None;
		}
	}

	public static AttackBase.TargetType DecayTargetType(AttackBase.TargetType a)
	{
		if (ValidTargetAny(a))
		{
			return AttackBase.TargetType.All;
		}
		if (ValidTargetHostile(a))
		{
			return AttackBase.TargetType.Hostile;
		}
		if (ValidTargetAlly(a))
		{
			return AttackBase.TargetType.Ally;
		}
		if (ValidTargetFriendly(a))
		{
			return AttackBase.TargetType.Friendly;
		}
		if (ValidTargetSelf(a))
		{
			return AttackBase.TargetType.Self;
		}
		return AttackBase.TargetType.None;
	}

	public static AttackBase.TargetType Complement(AttackBase.TargetType a)
	{
		AttackBase.TargetType targetType = DecayTargetType(a);
		if (a == AttackBase.TargetType.NotSelf || a == AttackBase.TargetType.AllyNotSelfOrHostile)
		{
			return AttackBase.TargetType.Self;
		}
		switch (targetType)
		{
		case AttackBase.TargetType.Hostile:
			return AttackBase.TargetType.Friendly;
		case AttackBase.TargetType.Friendly:
		case AttackBase.TargetType.Ally:
			return AttackBase.TargetType.Hostile;
		case AttackBase.TargetType.All:
			return AttackBase.TargetType.None;
		case AttackBase.TargetType.Self:
			return AttackBase.TargetType.NotSelf;
		default:
			return AttackBase.TargetType.All;
		}
	}

	public static AttackBase.TargetType PrerequisiteToTarget(PrerequisiteType pt, AttackBase.TargetType mainTarget)
	{
		return pt switch
		{
			PrerequisiteType.Friendly => AttackBase.TargetType.Friendly, 
			PrerequisiteType.Hostile => AttackBase.TargetType.Hostile, 
			PrerequisiteType.ClosestAllyWithSameTarget => AttackBase.TargetType.Ally, 
			PrerequisiteType.MainTargetOnly => mainTarget, 
			PrerequisiteType.ExcludeMainTarget => Complement(mainTarget), 
			_ => AttackBase.TargetType.All, 
		};
	}

	public static bool ValidTargetNone(AttackBase.TargetType validTargets)
	{
		return DecayTargetType(validTargets) == AttackBase.TargetType.None;
	}

	public static bool ValidTargetSelf(AttackBase.TargetType validTargets)
	{
		return validTargets == AttackBase.TargetType.Self;
	}

	public static bool ValidTargetAny(AttackBase.TargetType validTargets)
	{
		if (validTargets != 0 && validTargets != AttackBase.TargetType.AllDeadOrUnconscious && validTargets != AttackBase.TargetType.AllyNotSelfOrHostile && validTargets != AttackBase.TargetType.NotSelf && validTargets != AttackBase.TargetType.Dead)
		{
			return validTargets == AttackBase.TargetType.AnyWithResonance;
		}
		return true;
	}

	public static bool ValidTargetAlly(AttackBase.TargetType ValidTargets)
	{
		if (ValidTargets != AttackBase.TargetType.Ally)
		{
			return ValidTargets == AttackBase.TargetType.AllyNotSelf;
		}
		return true;
	}

	public static bool ValidTargetHostile(AttackBase.TargetType ValidTargets)
	{
		if (ValidTargets != AttackBase.TargetType.Hostile && ValidTargets != AttackBase.TargetType.HostileBeast && ValidTargets != AttackBase.TargetType.HostileVessel && ValidTargets != AttackBase.TargetType.HostileWithGrimoire && ValidTargets != AttackBase.TargetType.AllyNotSelfOrHostile)
		{
			return ValidTargets == AttackBase.TargetType.HostileWithNpcAppearance;
		}
		return true;
	}

	public static bool ValidTargetFriendly(AttackBase.TargetType ValidTargets)
	{
		if (ValidTargets != AttackBase.TargetType.Friendly && ValidTargets != AttackBase.TargetType.FriendlyUnconscious && ValidTargets != AttackBase.TargetType.FriendlyIncludingCharmed)
		{
			return ValidTargets == AttackBase.TargetType.FriendlyNotVessel;
		}
		return true;
	}

	public static string GetValidTargetString(AttackBase.TargetType ValidTargets)
	{
		if (ValidTargetAlly(ValidTargets))
		{
			return GUIUtils.GetText(1597);
		}
		if (ValidTargetFriendly(ValidTargets))
		{
			return GUIUtils.GetText(1599);
		}
		if (ValidTargetHostile(ValidTargets))
		{
			return GUIUtils.GetText(1598);
		}
		if (ValidTargetSelf(ValidTargets))
		{
			return GUIUtils.GetText(1609);
		}
		return GUIUtils.GetText(343);
	}

	public static bool ValidTargetDead(AttackBase.TargetType ValidTargets)
	{
		if (ValidTargets != AttackBase.TargetType.Dead)
		{
			return ValidTargets == AttackBase.TargetType.AllDeadOrUnconscious;
		}
		return true;
	}

	public static bool ValidTargetUnconscious(AttackBase.TargetType ValidTargets)
	{
		if (ValidTargets != AttackBase.TargetType.AllDeadOrUnconscious)
		{
			return ValidTargets == AttackBase.TargetType.FriendlyUnconscious;
		}
		return true;
	}
}
