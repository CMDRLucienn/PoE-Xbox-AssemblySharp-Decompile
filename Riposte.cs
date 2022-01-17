using System.Collections.Generic;
using UnityEngine;

public class Riposte : GenericAbility
{
	public float OnMissTriggerChance = 0.2f;

	public float OnGrazeTriggerChance = 0.3f;

	private List<GameObject> m_riposteTargets = new List<GameObject>();

	public override bool ListenForDamageEvents => true;

	protected override void Update()
	{
		base.Update();
		if (m_riposteTargets.Count <= 0)
		{
			return;
		}
		try
		{
			for (int i = 0; i < m_riposteTargets.Count; i++)
			{
				Equipment component = m_owner.GetComponent<Equipment>();
				if (component != null)
				{
					AttackBase primaryAttack = component.PrimaryAttack;
					if (primaryAttack != null && primaryAttack is AttackMelee)
					{
						bool skipAnimation = primaryAttack.SkipAnimation;
						primaryAttack.SkipAnimation = true;
						primaryAttack.OnAttackComplete += PlayRiposteEffect;
						primaryAttack.Launch(m_riposteTargets[i], this);
						primaryAttack.SkipAnimation = skipAnimation;
						primaryAttack.OnAttackComplete -= PlayRiposteEffect;
					}
					primaryAttack = component.SecondaryAttack;
					if (primaryAttack != null && primaryAttack is AttackMelee)
					{
						bool skipAnimation2 = primaryAttack.SkipAnimation;
						primaryAttack.SkipAnimation = true;
						primaryAttack.OnAttackComplete += PlayRiposteEffect;
						primaryAttack.Launch(m_riposteTargets[i], this);
						primaryAttack.SkipAnimation = skipAnimation2;
						primaryAttack.OnAttackComplete -= PlayRiposteEffect;
					}
				}
			}
		}
		finally
		{
			m_riposteTargets.Clear();
		}
	}

	public override void HandleOnDamaged(GameObject myObject, GameEventArgs args)
	{
		DamageInfo damageInfo = (DamageInfo)args.GenericData[0];
		if (damageInfo != null && damageInfo.DefendedBy == CharacterStats.DefenseType.Deflect && damageInfo.Attack is AttackMelee && ((damageInfo.IsMiss && OEIRandom.FloatValue() <= OnMissTriggerChance) || (damageInfo.IsGraze && OEIRandom.FloatValue() <= OnGrazeTriggerChance)))
		{
			m_riposteTargets.Add(args.GameObjectData[0]);
		}
	}

	private void PlayRiposteEffect(object sender, CombatEventArgs e)
	{
		if (!(e.Attacker == null) && !(e.Victim == null) && !e.Damage.IsMiss && !(AttackData.Instance.DefaultRiposteFx == null))
		{
			Quaternion orientation = Quaternion.LookRotation(e.Victim.transform.position - e.Attacker.transform.position);
			GameUtilities.LaunchEffect(AttackData.Instance.DefaultRiposteFx, 1f, e.Attacker.transform.position, orientation, this);
		}
	}
}
