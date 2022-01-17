using UnityEngine;

[ClassTooltip("Attacks in a beam between the caster and the target, and teleports the caster to a location near the target.")]
public class FlagellentsPathAttack : AttackMelee
{
	[Tooltip("If set, the caster will also hit with his primary attack in a beam.")]
	public bool HitWithPrimaryAttack;

	[Tooltip("If set, the caster will also hit with his secondary attack in a beam.")]
	public bool HitWithSecondaryAttack;

	[Tooltip("If set, the caster will also hit the target with his primary attack.")]
	public bool HitTargetWithPrimaryAttack;

	[Tooltip("If set, the caster will also hit the target with his secondary attack.")]
	public bool HitTargetWithSecondaryAttack;

	public override void OnImpact(GameObject self, GameObject enemy, bool isMainTarget)
	{
		if (isMainTarget)
		{
			HandleBeam(base.Owner, enemy, Vector3.zero);
			if (HitWithPrimaryAttack || HitWithSecondaryAttack)
			{
				Equipment component = base.Owner.GetComponent<Equipment>();
				if ((bool)component)
				{
					AttackBase primaryAttack = component.PrimaryAttack;
					if (HitWithPrimaryAttack && (bool)primaryAttack)
					{
						primaryAttack.HandleBeam(base.Owner, enemy, Vector3.zero);
					}
					AttackBase secondaryAttack = component.SecondaryAttack;
					if (HitWithSecondaryAttack && (bool)secondaryAttack)
					{
						secondaryAttack.HandleBeam(base.Owner, enemy, Vector3.zero);
					}
				}
			}
			if (HitTargetWithPrimaryAttack || HitTargetWithSecondaryAttack)
			{
				Equipment component2 = base.Owner.GetComponent<Equipment>();
				if ((bool)component2)
				{
					AttackBase primaryAttack2 = component2.PrimaryAttack;
					if (HitTargetWithPrimaryAttack && (bool)primaryAttack2)
					{
						primaryAttack2.OnImpact(self, enemy);
					}
					AttackBase secondaryAttack2 = component2.SecondaryAttack;
					if (HitTargetWithSecondaryAttack && (bool)secondaryAttack2)
					{
						secondaryAttack2.OnImpact(self, enemy);
					}
				}
			}
			TeleportToEnemy(enemy);
		}
		base.OnImpact(self, enemy, isMainTarget);
	}

	private void TeleportToEnemy(GameObject enemy)
	{
		Mover component = base.Owner.GetComponent<Mover>();
		Vector3 position = enemy.transform.position;
		position -= (position - base.Owner.transform.position).normalized * component.Radius;
		base.Owner.transform.position = GameUtilities.NearestUnoccupiedLocation(position, component.Radius, 10f, component);
		component.MoveToGround();
	}
}
