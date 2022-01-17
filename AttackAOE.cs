using System.Collections.Generic;
using System.Text;
using UnityEngine;

[AddComponentMenu("Attacks/AOE")]
public class AttackAOE : AttackRanged
{
	public float BlastRadius = 5f;

	[Range(0f, 360f)]
	public float DamageAngleDegrees = 360f;

	public bool ExcludeTarget;

	[Range(0f, 1000f)]
	public float BlastPhysicsForce = 500f;

	public AttackAOE SecondAOE;

	private readonly FormattableTarget AOE_TARGET = new FormattableTarget(1606, 1603);

	public override bool Channeled => true;

	public bool IsCone => DamageAngleDegrees < 360f;

	public override bool IsRangeUsed
	{
		get
		{
			if (base.IsRangeUsed)
			{
				return !IsCone;
			}
			return false;
		}
	}

	public float AdjustedBlastRadius => GetAdjustedBlastRadius(base.Owner);

	public override void OnImpact(GameObject self, GameObject enemy)
	{
		if (!m_cancelled)
		{
			m_can_cancel = false;
			TriggerNoise();
			base.OnImpact(self, enemy);
			OnImpactShared(self, enemy.transform.position, enemy);
		}
	}

	public override void OnImpact(GameObject self, Vector3 hitPosition)
	{
		if (!m_cancelled)
		{
			m_can_cancel = false;
			TriggerNoise();
			base.OnImpact(self, hitPosition);
			OnImpactShared(self, hitPosition, null);
		}
	}

	public void OnImpactShared(GameObject self, Vector3 hitPosition, GameObject excludedObject)
	{
		GameObject owner = base.Owner;
		if (owner != null)
		{
			Transform parent = AttackBase.GetTransform(owner, OnHitAttackerAttach);
			GameUtilities.LaunchEffect(OnHitAttackerVisualEffect, 1f, parent, m_ability);
		}
		List<GameObject> list = FindAoeTargets(owner, owner.transform.forward, hitPosition, forUi: false);
		if (self != null && list.Count == 0 && !(this is AttackPulsedAOE))
		{
			PlayCriticalMiss();
		}
		foreach (GameObject item in list)
		{
			SharedStats component = item.GetComponent<SharedStats>();
			if (component != null && list.Contains(component.SharedCharacter))
			{
				component.NotifySimultaneousHitStart(this);
			}
		}
		int num = 0;
		foreach (GameObject item2 in list)
		{
			if (item2 == owner && m_ability != null)
			{
				m_ability.ApplyAbilityModAttackStatusEffectsOnCasterOnly(null, deleteOnClear: true, null);
			}
			if (item2 == excludedObject || (ExcludeTarget && item2 == owner))
			{
				continue;
			}
			List<StatusEffect> appliedEffects = new List<StatusEffect>();
			List<StatusEffect> list2 = new List<StatusEffect>();
			Health component2 = item2.GetComponent<Health>();
			if (component2 != null)
			{
				CalcDamage(item2, self, out var damage);
				DamageInfo damageInfo = null;
				CharacterStats component3 = owner.GetComponent<CharacterStats>();
				CharacterStats component4 = item2.GetComponent<CharacterStats>();
				if (component3 != null)
				{
					if (damage.IsMiss)
					{
						component3.TriggerWhenMisses(item2, damage);
					}
					else
					{
						component3.TriggerWhenHits(item2, damage);
					}
				}
				if (!damage.IsMiss && (bool)component4)
				{
					component4.TriggerWhenHit(owner, damage);
				}
				if (damage.IsMiss)
				{
					num++;
					GameUtilities.LaunchEffect(OnMissVisualEffect, 1f, component2.transform, m_ability);
				}
				else
				{
					PushEnemy(item2, PushDistance, hitPosition, damage, appliedEffects);
					ApplyStatusEffects(item2, damage, deleteOnClear: true, appliedEffects);
					damageInfo = ApplyAfflictions(item2, damage, deleteOnClear: true, appliedEffects, list2);
					Transform hitTransform = GetHitTransform(item2);
					GameUtilities.LaunchEffect(OnHitVisualEffect, 1f, hitTransform, m_ability);
				}
				if (!IsFakeAttack)
				{
					component2.DoDamage(damage, owner);
				}
				if (FogOfWar.Instance == null || FogOfWar.Instance.PointVisible(owner.transform.position) || FogOfWar.Instance.PointVisible(item2.transform.position))
				{
					AttackBase.PostAttackMessages(item2, damage, appliedEffects, primaryAttack: true);
					if (damageInfo != null)
					{
						AttackBase.PostAttackMessagesSecondaryEffect(item2, damageInfo, list2);
					}
				}
				if (!damage.IsMiss && ExtraAOE != null && !ExtraAOEOnBounce)
				{
					AttackAOE attackAOE = Object.Instantiate(ExtraAOE);
					attackAOE.DestroyAfterImpact = true;
					attackAOE.Owner = owner;
					attackAOE.transform.parent = owner.transform;
					attackAOE.SkipAnimation = true;
					attackAOE.m_ability = m_ability;
					attackAOE.ShowImpactEffect(item2.transform.position);
					GameObject excludedObject2 = null;
					if (attackAOE.ExcludeTarget)
					{
						excludedObject2 = item2;
					}
					attackAOE.OnImpactShared(null, item2.transform.position, excludedObject2);
				}
			}
			if (self != null && (num == list.Count || num >= 5))
			{
				PlayCriticalMiss();
			}
		}
		float adjustedBlastRadius = AdjustedBlastRadius;
		Collider[] array = Physics.OverlapSphere(hitPosition, adjustedBlastRadius);
		for (int i = 0; i < array.Length; i++)
		{
			if ((bool)array[i].GetComponent<Rigidbody>())
			{
				array[i].GetComponent<Rigidbody>().AddExplosionForce(BlastPhysicsForce, hitPosition, adjustedBlastRadius);
			}
		}
		foreach (GameObject item3 in list)
		{
			SharedStats component5 = item3.GetComponent<SharedStats>();
			if (component5 != null && list.Contains(component5.SharedCharacter))
			{
				component5.NotifySimultaneousHitEnd();
			}
		}
		if (SecondAOE != null)
		{
			AttackAOE attackAOE2 = Object.Instantiate(SecondAOE);
			attackAOE2.DestroyAfterImpact = true;
			attackAOE2.Owner = owner;
			attackAOE2.transform.parent = owner.transform;
			attackAOE2.SkipAnimation = true;
			attackAOE2.m_ability = m_ability;
			attackAOE2.ShowImpactEffect(hitPosition);
			GameObject excludedObject3 = null;
			if (attackAOE2.ExcludeTarget)
			{
				excludedObject3 = excludedObject;
			}
			attackAOE2.OnImpactShared(null, hitPosition, excludedObject3);
		}
		if (base.DestroyAfterImpact)
		{
			GameUtilities.Destroy(base.gameObject, 1f);
			base.DestroyAfterImpact = false;
		}
	}

	public override void ShowImpactEffect(Vector3 position)
	{
		Quaternion orientation = Quaternion.identity;
		if (base.Owner != null)
		{
			orientation = base.Owner.transform.rotation;
		}
		GameUtilities.LaunchEffect(ImpactEffect, 1f, position, orientation, m_ability);
	}

	public virtual List<GameObject> FindAoeTargets(GameObject caster, Vector3 parentForward, Vector3 hitPosition, bool forUi)
	{
		List<GameObject> list = new List<GameObject>();
		float num = AdjustedBlastRadius;
		float num2 = num;
		if (ValidTargets == TargetType.All && BlastRadius < AdjustedBlastRadius)
		{
			num = BlastRadius;
		}
		foreach (Faction activeFactionComponent in Faction.ActiveFactionComponents)
		{
			if (activeFactionComponent == null || (activeFactionComponent.gameObject == base.Owner && DamageAngleDegrees > 0f && DamageAngleDegrees < 360f))
			{
				continue;
			}
			Vector3 vector = activeFactionComponent.transform.position - hitPosition;
			float sqrMagnitude = vector.sqrMagnitude;
			if (ExcludeTarget && !(sqrMagnitude > float.Epsilon))
			{
				continue;
			}
			float cachedRadius = activeFactionComponent.CachedRadius;
			float num3 = (num + cachedRadius) * (num + cachedRadius);
			float num4 = (num2 + cachedRadius) * (num2 + cachedRadius);
			Vector3 vector2 = vector.normalized;
			if (TargetAngle > 0f)
			{
				vector2 = Quaternion.Euler(0f, TargetAngle, 0f) * vector2;
			}
			bool flag = Vector3.Angle(parentForward, vector2) <= DamageAngleDegrees * 0.5f;
			bool num5 = sqrMagnitude < float.Epsilon || (flag && sqrMagnitude <= num4);
			bool flag2 = sqrMagnitude < float.Epsilon || (flag && sqrMagnitude <= num3);
			if (num5 && GameUtilities.LineofSight(hitPosition, activeFactionComponent.gameObject.transform.position, 1f, includeDynamics: false, wallsOnly: true))
			{
				if (ValidTargets == TargetType.All && IsValidTarget(activeFactionComponent.gameObject, caster, TargetType.Hostile))
				{
					list.Add(activeFactionComponent.gameObject);
				}
				else if (flag2 && IsValidTarget(activeFactionComponent.gameObject, caster))
				{
					list.Add(activeFactionComponent.gameObject);
				}
			}
		}
		int num6 = list.IndexOf(base.Owner);
		if (num6 > 0)
		{
			list[num6] = list[0];
			list[0] = base.Owner;
		}
		return list;
	}

	public override bool IsCharacterImmuneToAnyAffliction(CharacterStats character)
	{
		if ((bool)SecondAOE && SecondAOE.IsCharacterImmuneToAnyAffliction(character))
		{
			return true;
		}
		return base.IsCharacterImmuneToAnyAffliction(character);
	}

	public float GetAdjustedBlastRadius(GameObject character)
	{
		float num = BlastRadius;
		if ((bool)character)
		{
			CharacterStats component = character.GetComponent<CharacterStats>();
			if (component != null && !IgnoreCharacterStats)
			{
				num *= component.StatEffectRadiusMultiplier * component.AoERadiusMult;
			}
		}
		return num;
	}

	protected override string GetRangeString(GenericAbility ability, GameObject character)
	{
		if (IsCone)
		{
			return "";
		}
		return base.GetRangeString(ability, character);
	}

	public override void GetAdditionalEffects(StringEffects stringEffects, GenericAbility ability, GameObject character)
	{
		if ((bool)SecondAOE)
		{
			SecondAOE.GetString(ability, character, stringEffects);
		}
		base.GetAdditionalEffects(stringEffects, ability, character);
	}

	protected override string GetAoeString(GenericAbility ability, GameObject character)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (BlastRadius > 0f && DamageAngleDegrees > 0f)
		{
			GameObject character2 = (character ? character : base.Owner);
			stringBuilder.AppendGuiFormat((DamageAngleDegrees < 360f) ? 1541 : 1593, TextUtils.FormatBase(BlastRadius, GetAdjustedBlastRadius(character2), (float v) => GUIUtils.Format(1533, v.ToString("#0.0#"))));
			if (DamageAngleDegrees < 360f)
			{
				stringBuilder.Append(" ");
				stringBuilder.AppendGuiFormat(1536, DamageAngleDegrees.ToString("#0"));
			}
		}
		if (ApplyToSelfOnly)
		{
			return StringUtility.Format(GUIUtils.GetText(1592, CharacterStats.GetGender(character)), stringBuilder.ToString());
		}
		_ = ValidPrimaryTargets;
		_ = 19;
		return stringBuilder.ToString();
	}

	public override FormattableTarget GetMainTargetString(GenericAbility ability)
	{
		return AOE_TARGET;
	}
}
