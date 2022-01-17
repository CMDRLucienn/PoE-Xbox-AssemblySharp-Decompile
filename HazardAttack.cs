using System.Collections.Generic;
using System.Text;
using UnityEngine;

[AddComponentMenu("Attacks/Hazard")]
public class HazardAttack : AttackAbility
{
	public Trap TrapPrefab;

	private readonly FormattableTarget HAZARD_TARGET = new FormattableTarget(1589);

	private readonly FormattableTarget AOE_TARGET = new FormattableTarget(1606, 1603);

	public float LargestAuraRadius
	{
		get
		{
			float num = 1f;
			GenericAbility component = GetComponent<GenericAbility>();
			if (component != null && component.FriendlyRadius > num)
			{
				num = component.FriendlyRadius;
			}
			return num;
		}
	}

	public override void OnImpact(GameObject self, GameObject enemy)
	{
		OnImpact(self, enemy.transform.position);
	}

	public override void OnImpact(GameObject self, Vector3 hitPosition)
	{
		if (m_cancelled)
		{
			return;
		}
		m_can_cancel = false;
		TriggerNoise();
		ResetAttackVars();
		if (m_ability != null)
		{
			m_ability.Activate(hitPosition);
		}
		if (TrapPrefab != null)
		{
			PlaceTrap(hitPosition);
			if (m_ability != null)
			{
				m_ability.AttackComplete = true;
			}
			return;
		}
		if (StatusEffects.Count == 0 && Afflictions.Count == 0)
		{
			Debug.LogWarning(base.name + " doesn't have any status effects or afflictions to apply! It's useless!", base.gameObject);
			if (m_ability != null)
			{
				m_ability.AttackComplete = true;
			}
			return;
		}
		GameObject gameObject = new GameObject(base.name + "_spawned_hazard");
		CharacterStats characterStats = gameObject.AddComponent<CharacterStats>();
		Health health = gameObject.AddComponent<Health>();
		health.Targetable = false;
		health.CanBeTargeted = false;
		Faction faction = gameObject.AddComponent<Faction>();
		faction.ModifyToMatch(m_parent.GetComponent<Faction>());
		faction.DrawSelectionCircle = false;
		faction.ShowTooltips = false;
		gameObject.transform.position = hitPosition;
		float num = 1f;
		CharacterStats component = m_parent.GetComponent<CharacterStats>();
		if (component != null)
		{
			num *= component.StatEffectRadiusMultiplier * component.AoERadiusMult;
		}
		GameUtilities.LaunchLoopingEffect(OnHitVisualEffect, num, gameObject.transform, m_ability);
		characterStats.ImmuneToParticleEffects = true;
		ApplyStatusEffects(gameObject, null, deleteOnClear: false, null);
		ApplyAfflictions(gameObject, null, deleteOnClear: false, null, null);
		float hazardMaximumDuration = GetHazardMaximumDuration();
		GameUtilities.Destroy(gameObject, hazardMaximumDuration + 2f);
		GameUtilities.AddFadingEffect(gameObject);
		if (m_ability != null)
		{
			m_ability.AttackComplete = true;
		}
	}

	private float GetHazardMaximumDuration()
	{
		float num = 0.033f;
		GenericAbility component = GetComponent<GenericAbility>();
		if (component != null && component.DurationOverride > num)
		{
			return component.DurationOverride;
		}
		foreach (StatusEffectParams statusEffect in StatusEffects)
		{
			float duration = statusEffect.GetDuration(m_ownerStats);
			if (duration > num)
			{
				num = duration;
			}
		}
		foreach (AfflictionParams affliction in Afflictions)
		{
			if (affliction.Duration > num)
			{
				num = affliction.Duration;
			}
		}
		return num;
	}

	protected virtual void PlaceTrap(Vector3 position)
	{
		bool flag = GetComponent<Consumable>() != null;
		Trap trap = PlaceTrap(TrapPrefab, position, m_parent, flag);
		if (!(trap != null) || !(m_parent != null))
		{
			return;
		}
		GenericSpell genericSpell = m_ability as GenericSpell;
		if (!flag && (bool)genericSpell && genericSpell.SpellClass == CharacterStats.Class.Priest)
		{
			for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
			{
				if (!(Faction.ActiveFactionComponents[i] == null))
				{
					Trap component = Faction.ActiveFactionComponents[i].GetComponent<Trap>();
					if (component != null && component != trap && !component.FromItem && component.Owner == base.Owner)
					{
						GameUtilities.Destroy(component.gameObject, 0.1f);
					}
				}
			}
		}
		if (!m_parent.GetComponent<PartyMemberAI>())
		{
			return;
		}
		float triggerRadius = trap.TriggerRadius;
		for (int j = 0; j < Faction.ActiveFactionComponents.Count; j++)
		{
			Faction faction = Faction.ActiveFactionComponents[j];
			if (!faction)
			{
				continue;
			}
			Trap component2 = faction.GetComponent<Trap>();
			if ((bool)component2 && component2 != trap)
			{
				float num = triggerRadius + component2.TriggerRadius - 0.5f;
				if (flag && component2.FromItem && component2.Owner == base.Owner)
				{
					component2.Disarm(base.Owner);
				}
				else if ((faction.transform.position - position).sqrMagnitude <= num * num && (bool)component2.Owner && (bool)component2.Owner.GetComponent<PartyMemberAI>())
				{
					component2.Disarm(base.Owner);
				}
			}
		}
	}

	public override bool IsHostile(GameObject enemy, DamagePacket damage)
	{
		if ((bool)TrapPrefab)
		{
			if ((bool)TrapPrefab.AbilityOverride)
			{
				AttackBase attackBase = TrapPrefab.AbilityOverride.Attack ?? TrapPrefab.AbilityOverride.GetComponent<AttackBase>();
				if ((bool)attackBase && attackBase.IsHostile(enemy, attackBase.DamageData))
				{
					return true;
				}
			}
			if (TrapPrefab.AbilityOverrides != null)
			{
				for (int i = 0; i < TrapPrefab.AbilityOverrides.Length; i++)
				{
					AttackBase attackBase2 = TrapPrefab.AbilityOverrides[i].Attack ?? TrapPrefab.AbilityOverrides[i].GetComponent<AttackBase>();
					if ((bool)attackBase2 && attackBase2.IsHostile(enemy, attackBase2.DamageData))
					{
						return true;
					}
				}
			}
		}
		return base.IsHostile(enemy, damage);
	}

	public static Trap PlaceTrap(Trap trapPrefab, Vector3 position, GameObject parent, bool fromItem = false)
	{
		Quaternion rotation = Quaternion.LookRotation((position - parent.transform.position).normalized);
		int trapID = PlacedTrapID();
		return PlaceTrap(trapPrefab, position, rotation, parent, trapID, fromItem);
	}

	public static Trap PlaceTrap(Trap trapPrefab, Vector3 position, Quaternion rotation, GameObject parent, int trapID, bool fromItem = false)
	{
		if (trapPrefab == null)
		{
			return null;
		}
		Trap component = GameResources.Instantiate<GameObject>(trapPrefab.gameObject, position, rotation).GetComponent<Trap>();
		component.Owner = parent;
		Faction component2 = component.GetComponent<Faction>();
		Faction component3 = parent.GetComponent<Faction>();
		if (component2 != null && component3 != null)
		{
			component2.ModifyToMatch(component3);
		}
		component.FromItem = fromItem;
		component.TrapID = trapID;
		return component;
	}

	public static int PlacedTrapID()
	{
		return OEIRandom.NonNegativeInt();
	}

	public override void GetAllDefenses(CharacterStats attacker, GenericAbility ability, bool[] defenses, IList<int> accuracies)
	{
		base.GetAllDefenses(attacker, ability, defenses, accuracies);
		if (!TrapPrefab)
		{
			return;
		}
		if ((bool)TrapPrefab.AbilityOverride)
		{
			AttackBase component = TrapPrefab.AbilityOverride.GetComponent<AttackBase>();
			if ((bool)component)
			{
				component.GetAllDefenses(attacker, TrapPrefab.AbilityOverride, defenses, accuracies);
			}
		}
		if (TrapPrefab.AbilityOverrides == null)
		{
			return;
		}
		for (int i = 0; i < TrapPrefab.AbilityOverrides.Length; i++)
		{
			if ((bool)TrapPrefab.AbilityOverrides[i])
			{
				AttackBase component2 = TrapPrefab.AbilityOverrides[i].GetComponent<AttackBase>();
				if ((bool)component2)
				{
					component2.GetAllDefenses(attacker, TrapPrefab.AbilityOverrides[i], defenses, accuracies);
				}
			}
		}
	}

	protected override string GetAoeString(GenericAbility ability, GameObject character)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendGuiFormat(1593, GUIUtils.Format(1533, LargestAuraRadius.ToString("####0.0#")));
		string text = GUIUtils.Format(1594, stringBuilder.ToString());
		if (ApplyToSelfOnly)
		{
			return GUIUtils.Format(1592, text);
		}
		_ = ValidPrimaryTargets;
		_ = 19;
		return text;
	}

	public override string GetDurationString(GenericAbility ability)
	{
		string text = base.GetDurationString(ability);
		float num = GetHazardMaximumDuration();
		if ((bool)TrapPrefab)
		{
			num = Mathf.Max(TrapPrefab.SelfDestructTime, num);
		}
		if (num > 0f)
		{
			text = text + "\n" + AttackBase.FormatWC(GUIUtils.GetText(1634), GUIUtils.Format(211, num.ToString("#0")));
		}
		return text.Trim();
	}

	public override FormattableTarget GetMainTargetString(GenericAbility ability)
	{
		if (IsHostile(null, DamageData))
		{
			return HAZARD_TARGET;
		}
		return AOE_TARGET;
	}

	public override void GetAdditionalEffects(StringEffects stringEffects, GenericAbility ability, GameObject character)
	{
		if ((bool)TrapPrefab)
		{
			if ((bool)TrapPrefab.AbilityOverride)
			{
				AttackBase attackBase = TrapPrefab.AbilityOverride.Attack;
				if (!attackBase)
				{
					attackBase = TrapPrefab.AbilityOverride.GetComponent<AttackBase>();
				}
				if ((bool)attackBase)
				{
					attackBase.AddEffects(GetMainTargetString(ability), ability, character, 1f, stringEffects);
				}
			}
			if (TrapPrefab.AbilityOverrides != null)
			{
				if (TrapPrefab.OneRandomAbility)
				{
					for (int i = 0; i < TrapPrefab.AbilityOverrides.Length; i++)
					{
						GenericAbility genericAbility = TrapPrefab.AbilityOverrides[i];
						AttackBase attackBase2 = genericAbility.Attack;
						if (!attackBase2)
						{
							attackBase2 = genericAbility.GetComponent<AttackBase>();
						}
						if ((bool)attackBase2)
						{
							attackBase2.AddEffects(new FormattableTarget(genericAbility.DisplayName, attackBase2.ValidTargets), genericAbility, character, 1f, stringEffects);
						}
					}
				}
				else
				{
					for (int j = 0; j < TrapPrefab.AbilityOverrides.Length; j++)
					{
						GenericAbility genericAbility2 = TrapPrefab.AbilityOverrides[j];
						AttackBase attackBase3 = genericAbility2.Attack;
						if (!attackBase3)
						{
							attackBase3 = genericAbility2.GetComponent<AttackBase>();
						}
						if ((bool)attackBase3)
						{
							attackBase3.AddEffects(GetMainTargetString(genericAbility2), genericAbility2, character, 1f, stringEffects);
						}
					}
				}
			}
		}
		base.GetAdditionalEffects(stringEffects, ability, character);
	}
}
