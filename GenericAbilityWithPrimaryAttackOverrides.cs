using UnityEngine;

public class GenericAbilityWithPrimaryAttackOverrides : GenericAbility
{
	public CharacterStats.DefenseType DefendedBy;

	protected override void HandleStatsOnPreDamageDealt(GameObject source, CombatEventArgs args)
	{
		args.Damage.DefendedBy = DefendedBy;
	}
}
