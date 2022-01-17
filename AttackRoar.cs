using UnityEngine;

[DesignerObsolete("This class doesn't do anything special. You should just use Summon or AttackAOE instead.")]
public class AttackRoar : AttackAOE
{
	public Summon SummonPrefab;

	public override void OnImpact(GameObject projectile, Vector3 hitPosition)
	{
		base.OnImpact(projectile, hitPosition);
		CheckSummoning();
	}

	public override void OnImpact(GameObject projectile, GameObject enemy)
	{
		base.OnImpact(projectile, enemy);
		CheckSummoning();
	}

	private void CheckSummoning()
	{
		if (SummonPrefab != null)
		{
			Summon summon = GameResources.Instantiate<Summon>(SummonPrefab);
			summon.Owner = base.Owner;
			summon.transform.parent = base.Owner.transform;
			summon.SkipAnimation = true;
			summon.DestroyAfterSummonEnds = true;
			summon.Launch(m_destination, m_enemy);
		}
	}

	public override string GetDurationString(GenericAbility ability)
	{
		string text = base.GetDurationString(ability);
		if ((bool)SummonPrefab)
		{
			text = text + "\n" + SummonPrefab.GetDurationString(ability);
		}
		return text.Trim();
	}

	public override void GetAdditionalEffects(StringEffects stringEffects, GenericAbility ability, GameObject character)
	{
		if ((bool)SummonPrefab)
		{
			SummonPrefab.AddSummonEffects(stringEffects, character);
		}
		base.GetAdditionalEffects(stringEffects, ability, character);
	}
}
