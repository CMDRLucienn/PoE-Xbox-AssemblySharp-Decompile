using System.Collections;
using UnityEngine;

public class GenericSpellWithExtraAttackOnMissAndEnd : GenericSpell
{
	public AttackBase ExtraAttack;

	protected override void HandleStatsOnPostDamageDealt(GameObject source, CombatEventArgs args)
	{
		base.HandleStatsOnPostDamageDealt(source, args);
		if (ExtraAttack == null || args.Victim == null || Owner == null)
		{
			return;
		}
		if (args.Damage.IsMiss)
		{
			PerformExtraAttack(args.Victim);
			return;
		}
		float num = 1f;
		AttackBase component = GetComponent<AttackBase>();
		if (component != null && component.StatusEffects.Count > 0)
		{
			num = component.StatusEffects[0].GetDuration(m_ownerStats);
		}
		if (args.Damage.IsCriticalHit)
		{
			num *= CharacterStats.CritMultiplier;
		}
		else if (args.Damage.IsGraze)
		{
			num *= CharacterStats.GrazeMultiplier;
		}
		StartCoroutine(ExtraAttackDelay(num, args.Victim));
	}

	private IEnumerator ExtraAttackDelay(float time, GameObject enemy)
	{
		yield return new WaitForSeconds(time);
		PerformExtraAttack(enemy);
	}

	private void PerformExtraAttack(GameObject enemy)
	{
		if (enemy == null)
		{
			return;
		}
		GameObject[] array = GameUtilities.CreaturesInRange(enemy.transform.position, ExtraAttack.TotalAttackDistance, Owner, includeUnconscious: false);
		if (array == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i] == null) && !(array[i] == enemy) && (!(m_attackBase != null) || m_attackBase.IsValidTarget(array[i])))
			{
				AttackBase attackBase = Object.Instantiate(ExtraAttack);
				attackBase.transform.parent = Owner.transform;
				attackBase.SkipAnimation = true;
				attackBase.Launch(array[i], this);
				GameUtilities.Destroy(attackBase.gameObject, 5f);
				num++;
				if (num >= 2)
				{
					break;
				}
			}
		}
	}
}
