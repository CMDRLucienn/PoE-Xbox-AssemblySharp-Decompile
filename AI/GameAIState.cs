using UnityEngine;

namespace AI;

public class GameAIState : AIState
{
	public enum StatePriority
	{
		Normal,
		ConsumePotion,
		Attack,
		Interrupt,
		HitReact,
		Stunned,
		Cinematic,
		Paralyzed,
		PushedBack,
		KnockDown,
		Unconscious,
		Dead,
		Invulnerable
	}

	protected CharacterStats m_stats;

	protected Faction m_faction;

	protected AIPackageController PackageController;

	public override int Priority => 0;

	public override void Reset()
	{
		base.Reset();
		m_stats = null;
		PackageController = null;
		m_faction = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_stats = m_owner.GetComponent<CharacterStats>();
		m_faction = m_owner.GetComponent<Faction>();
		PackageController = m_owner.GetComponent<AIPackageController>();
	}

	protected virtual bool AvoidanceOverride(Mover other, out Vector3 force)
	{
		force = Vector3.zero;
		Faction component = other.GetComponent<Faction>();
		if (component == null)
		{
			return false;
		}
		Faction component2 = m_owner.GetComponent<Faction>();
		if (component2 == null)
		{
			return false;
		}
		if (!component2.IsHostile(component))
		{
			return component.IsHostile(component2);
		}
		return true;
	}

	public override void Update()
	{
		base.Update();
		if (m_animation != null)
		{
			m_animation.IsInCombatMode = GameState.InCombat && !GameState.IsInTrapTriggeredCombat;
		}
	}

	public bool IsHostile(GameObject obj)
	{
		Faction component = m_owner.GetComponent<Faction>();
		if ((bool)component)
		{
			return component.IsHostile(obj);
		}
		return false;
	}
}
