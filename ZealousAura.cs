[ClassTooltip("Same as Generic Ability, but the owner's Zealous Aura radius bonuses apply to it.")]
public class ZealousAura : GenericAbility
{
	public override float RadiusMultiplier
	{
		get
		{
			float num = base.RadiusMultiplier;
			if (m_ownerStats != null)
			{
				num *= m_ownerStats.ZealousAuraRadiusMult;
			}
			return num;
		}
	}

	public void RestoreFixUp()
	{
		if (Activated)
		{
			ForceDeactivate(null);
		}
		bool flag = false;
		for (int num = m_effects.Count - 1; num >= 0; num--)
		{
			if (m_effects[num].Params.AffectsStat == StatusEffect.ModifiedStat.Accuracy)
			{
				if (flag)
				{
					m_effects.RemoveAt(num);
				}
				else
				{
					flag = true;
				}
			}
		}
		for (int num2 = m_effects.Count - 1; num2 >= 0; num2--)
		{
			if (m_effects[num2].IsSuppressed)
			{
				m_effects[num2].Unsuppress();
			}
		}
	}
}
