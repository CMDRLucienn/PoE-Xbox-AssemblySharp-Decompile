using UnityEngine;

[AddComponentMenu("Attacks/Melee")]
public class AttackMelee : AttackBase
{
	public float EngagementRadius = 1f;

	public bool Unarmed;

	public override float TotalIdealAttackDistance
	{
		get
		{
			CharacterStats component = base.Owner.GetComponent<CharacterStats>();
			if (component == null)
			{
				return base.TotalIdealAttackDistance;
			}
			float num = base.TotalIdealAttackDistance * component.MeleeAttackDistanceMultiplier;
			if (num < 0.15f)
			{
				return 0.15f;
			}
			return num;
		}
	}

	public override void OnImpact(GameObject self, Vector3 hitPosition)
	{
		if (!m_cancelled)
		{
			base.OnImpact(self, hitPosition);
			Stealth.SetInStealthMode(base.Owner, inStealth: false);
		}
	}

	public override void OnImpact(GameObject self, GameObject enemy, bool isMainTarget)
	{
		base.OnImpact(self, enemy, isMainTarget);
		Stealth.SetInStealthMode(base.Owner, inStealth: false);
	}

	public override void OnImpact(GameObject self, GameObject enemy)
	{
		base.OnImpact(self, enemy);
		Stealth.SetInStealthMode(base.Owner, inStealth: false);
	}

	public override float GetTotalAttackDistance(GameObject character)
	{
		float num = base.GetTotalAttackDistance(character);
		if (character != null)
		{
			CharacterStats component = character.GetComponent<CharacterStats>();
			if (component != null)
			{
				num *= component.MeleeAttackDistanceMultiplier;
			}
		}
		return num;
	}

	public override bool IsAutoAttack()
	{
		if (!base.IsAutoAttack())
		{
			return Unarmed;
		}
		return true;
	}
}
