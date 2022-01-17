using UnityEngine;

public class HeartOfFury : GenericAbility
{
	private bool m_attackingEnemies;

	public float FuryAoeRadius = 2f;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
		}
	}

	protected override void HandleStatsOnPreDamageDealt(GameObject source, CombatEventArgs args)
	{
		if (!m_attackingEnemies)
		{
			ApplyStatusEffectsNow();
		}
	}

	protected override void HandleStatsOnPostDamageDealt(GameObject source, CombatEventArgs args)
	{
		if (m_attackingEnemies)
		{
			return;
		}
		Equipment component = Owner.GetComponent<Equipment>();
		if (component == null)
		{
			return;
		}
		AttackBase primaryAttack = component.PrimaryAttack;
		AttackBase secondaryAttack = component.SecondaryAttack;
		bool flag = !(primaryAttack == null) && primaryAttack is AttackMelee;
		bool flag2 = !(secondaryAttack == null) && secondaryAttack is AttackMelee;
		if (!flag && !flag2)
		{
			return;
		}
		GameObject[] array = GameUtilities.CreaturesInRange(Owner.transform.position, FuryAoeRadius, Owner, includeUnconscious: false);
		if (array == null)
		{
			return;
		}
		m_attackingEnemies = true;
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (!(gameObject == args.Victim))
			{
				if (flag && primaryAttack.IsValidTarget(gameObject))
				{
					primaryAttack.SkipAnimation = true;
					primaryAttack.LaunchingDirectlyToImpact = false;
					primaryAttack.Launch(gameObject, this);
				}
				if (flag2 && secondaryAttack.IsValidTarget(gameObject))
				{
					secondaryAttack.SkipAnimation = true;
					secondaryAttack.LaunchingDirectlyToImpact = false;
					secondaryAttack.Launch(gameObject, this);
				}
			}
		}
		m_attackingEnemies = false;
	}

	public override void Deactivate(GameObject target)
	{
		base.Deactivate(target);
		Equipment component = Owner.GetComponent<Equipment>();
		AttackBase primaryAttack = component.PrimaryAttack;
		AttackBase secondaryAttack = component.SecondaryAttack;
		if ((bool)primaryAttack && primaryAttack is AttackMelee)
		{
			primaryAttack.SkipAnimation = false;
		}
		if ((bool)secondaryAttack && secondaryAttack is AttackMelee)
		{
			secondaryAttack.SkipAnimation = false;
		}
	}

	public void ApplyStatusEffectsNow()
	{
		if (m_ownerStats == null)
		{
			return;
		}
		foreach (StatusEffect effect in m_effects)
		{
			m_ownerStats.ApplyStatusEffectImmediate(effect);
		}
	}

	protected override void ActivateStatusEffects()
	{
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		string text = AttackBase.FormatWC(GUIUtils.GetText(1590), GUIUtils.Format(1533, FuryAoeRadius.ToString("#0.0")));
		string additionalEffects = base.GetAdditionalEffects(stringEffects, mode, ability, character);
		return (text + "\n" + additionalEffects).Trim();
	}
}
