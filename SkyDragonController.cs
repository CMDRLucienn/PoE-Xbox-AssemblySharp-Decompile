using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("AI/Sky Dragon Controller")]
public class SkyDragonController : DragonController
{
	public List<Waypoint> JumpPoints = new List<Waypoint>();

	public GenericAbility LeapAbility;

	private int m_leapPointIndex;

	private GenericAbility m_leapAbility;

	private AttackBase m_leapAttack;

	private SpellCastData m_leapData;

	public override void Start()
	{
		m_animController = GetComponent<AnimationController>();
		if (m_animController != null)
		{
			m_animController.OnTargetableToggled += m_animController_OnTargetableToggled;
		}
		CharacterStats component = GetComponent<CharacterStats>();
		if ((bool)component)
		{
			m_leapAbility = UnityEngine.Object.Instantiate(LeapAbility);
			m_leapAbility.Owner = base.gameObject;
			component.ActiveAbilities.Add(m_leapAbility);
			m_leapAttack = m_leapAbility.GetComponent<AttackBase>();
			m_leapData = new SpellCastData();
			m_leapData.CastInstruction = SpellCastData.Instruction.Cast;
			m_leapData.Spell = m_leapAbility;
			m_leapData.CooldownTime = m_leapAbility.Cooldown;
		}
	}

	private void m_animController_OnTargetableToggled(object sender, EventArgs e)
	{
		base.transform.position = JumpPoints[m_leapPointIndex].transform.position;
		base.transform.rotation = JumpPoints[m_leapPointIndex].transform.rotation;
	}

	public override SpellCastData GetNextInstruction()
	{
		GameObject currentTarget = m_ai.CurrentTarget;
		if (currentTarget != null && m_leapData.Ready)
		{
			float num = Vector3.Distance(currentTarget.transform.position, base.transform.position);
			if (num > m_leapAttack.MinAttackDistance)
			{
				float num2 = float.MaxValue;
				for (int i = 0; i < JumpPoints.Count; i++)
				{
					if (!(JumpPoints[i] == null))
					{
						float sqrMagnitude = (JumpPoints[i].transform.position - currentTarget.transform.position).sqrMagnitude;
						if (sqrMagnitude < num2)
						{
							num2 = sqrMagnitude;
							m_leapPointIndex = i;
						}
					}
				}
				if (num2 < 100f)
				{
					return m_leapData;
				}
			}
			return base.GetNextInstruction();
		}
		return base.GetNextInstruction();
	}

	public override bool BeingKited()
	{
		return false;
	}
}
