using System.Collections.Generic;
using AI.Plan;
using AI.Player;
using UnityEngine;

public class CoordinatedPositioning : GenericAbility
{
	private const float MAX_ABDUCTION_DISTANCE_SQ = 400f;

	private const float MIN_ABDUCTION_DISTANCE_SQ = 25f;

	private const float ABDUCTION_POINT_CLEAR_DISTANCE = 2.5f;

	private List<bool> m_hostile;

	[Tooltip("If true, both the attacker and target are teleported to a nearby abduction waypoint.")]
	public bool TeleportToAbductionWaypoint;

	[Tooltip("If true, Abduction has unlimited maximum range.")]
	public bool UnlimitedAbductionRange;

	protected override void Init()
	{
		if (m_initialized)
		{
			return;
		}
		base.Init();
		if (m_attackBase != null)
		{
			m_hostile = new List<bool>(m_attackBase.StatusEffects.Count);
			for (int i = 0; i < m_attackBase.StatusEffects.Count; i++)
			{
				m_hostile.Add(m_attackBase.StatusEffects[i].IsHostile);
			}
		}
	}

	protected override void HandleStatsOnPostDamageDealt(GameObject source, CombatEventArgs args)
	{
		base.HandleStatsOnPostDamageDealt(source, args);
		if (args.Victim != null)
		{
			Faction component = args.Victim.GetComponent<Faction>();
			if (component != null && component.IsHostile(m_owner) && (args.Damage.IsMiss || args.Damage.IsGraze))
			{
				ResetHostility();
				return;
			}
			Mover component2 = m_owner.GetComponent<Mover>();
			Mover component3 = args.Victim.GetComponent<Mover>();
			if (component2 == null || component3 == null || component3.IsImmobile)
			{
				ResetHostility();
				return;
			}
			if (!TeleportToAbductionWaypoint)
			{
				Vector3 position = m_owner.transform.position;
				m_owner.transform.position = args.Victim.transform.position;
				args.Victim.transform.position = position;
				ResetStates(args.Victim);
			}
			else
			{
				Waypoint waypoint = null;
				float num = float.MinValue;
				foreach (Waypoint s_ActiveWayPoint in Waypoint.s_ActiveWayPoints)
				{
					if (!s_ActiveWayPoint.IsAbductionWaypoint)
					{
						continue;
					}
					float num2 = GameUtilities.V3SqrDistance2D(m_owner.transform.position, s_ActiveWayPoint.transform.position);
					if ((UnlimitedAbductionRange || num2 < 400f) && num2 > 25f && num2 > num && (UnlimitedAbductionRange || GameUtilities.LineofSight(m_owner.transform.position, s_ActiveWayPoint.transform.position, 1f, includeDynamics: false)))
					{
						float overlapDistance = 0f;
						if (!(Mover.AreaOccupiedBy(s_ActiveWayPoint.transform.position, null, 2.5f, out overlapDistance) != null))
						{
							waypoint = s_ActiveWayPoint;
							num = num2;
						}
					}
				}
				if (waypoint != null)
				{
					m_owner.transform.position = GameUtilities.NearestUnoccupiedLocation(waypoint.transform.position, component2.Radius, 20f, component2);
					args.Victim.transform.position = GameUtilities.NearestUnoccupiedLocation(m_owner.transform.position, component3.Radius, 20f, component3);
					ResetStates(args.Victim);
					AIController aIController = GameUtilities.FindActiveAIController(Owner);
					if (aIController != null)
					{
						aIController.SafePopAllStates();
					}
					AIController aIController2 = GameUtilities.FindActiveAIController(args.Victim);
					if (aIController2 != null)
					{
						AIState aIState = aIController2.StateManager.FindState(typeof(Move));
						if (aIState != null)
						{
							aIController2.StateManager.PopState(aIState);
						}
					}
				}
			}
		}
		ResetHostility();
	}

	protected void ResetHostility()
	{
		if (m_attackBase != null)
		{
			for (int i = 0; i < m_attackBase.StatusEffects.Count; i++)
			{
				m_attackBase.StatusEffects[i].IsHostile = m_hostile[i];
			}
		}
	}

	protected void ResetStates(GameObject victim)
	{
		AIController aIController = GameUtilities.FindActiveAIController(Owner);
		if (aIController != null)
		{
			for (int i = 0; i < aIController.EngagedBy.Count; i++)
			{
				GameObject gameObject = aIController.EngagedBy[i];
				if (gameObject == null)
				{
					continue;
				}
				AIController aIController2 = GameUtilities.FindActiveAIController(gameObject);
				if (aIController2 != null)
				{
					if (aIController2.StateManager.CurrentState is ApproachTarget)
					{
						aIController2.SafePopAllStates();
					}
					else
					{
						aIController2.StateManager.ClearQueuedStates();
					}
				}
			}
		}
		AIController.BreakAllEngagements(Owner);
		AIController.BreakAllEngagements(victim);
	}

	protected override void HandleStatsOnAttackLaunch(GameObject source, CombatEventArgs args)
	{
		base.HandleStatsOnAttackLaunch(source, args);
		if (!Owner || Owner.GetComponent<Faction>().GetRelationship(args.Victim) != Faction.Relationship.Friendly || !(m_attackBase != null))
		{
			return;
		}
		foreach (StatusEffectParams statusEffect in m_attackBase.StatusEffects)
		{
			statusEffect.IsHostile = false;
		}
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		AttackBase.AddStringEffect(GetAbilityTarget().GetText(), new AttackBase.AttackEffect(Name(), base.Attack), stringEffects);
		return base.GetAdditionalEffects(stringEffects, mode, ability, character);
	}
}
