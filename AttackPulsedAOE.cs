using System.Collections.Generic;
using UnityEngine;

public class AttackPulsedAOE : AttackAOE
{
	public float Duration;

	public bool OneValidTargetPerPulse;

	public GameObject PulseEffect;

	[Tooltip("If set, do not stop the pulse if the caster is unconscious.")]
	public bool OwnerMayBeUnconscious;

	[Tooltip("If set, the pulse stops if the target is not unconscious.")]
	public bool TargetMustBeUnconscious;

	private bool m_first_pulse;

	public override void OnImpact(GameObject self, GameObject enemy)
	{
		if (!m_cancelled)
		{
			m_can_cancel = false;
			TriggerNoise();
			m_first_pulse = true;
			base.OnImpact(self, enemy);
			BeginPulse(self, enemy.transform.position);
			m_first_pulse = false;
		}
	}

	public override void OnImpact(GameObject self, Vector3 hitPosition)
	{
		if (!m_cancelled)
		{
			m_can_cancel = false;
			TriggerNoise();
			m_first_pulse = true;
			base.OnImpact(self, hitPosition);
			BeginPulse(self, hitPosition);
			m_first_pulse = false;
		}
	}

	public override void ShowImpactEffect(Vector3 position)
	{
		if (m_first_pulse || PulseEffect == null)
		{
			base.ShowImpactEffect(position);
			return;
		}
		Quaternion orientation = Quaternion.identity;
		if (base.Owner != null)
		{
			orientation = base.Owner.transform.rotation;
		}
		GameUtilities.LaunchEffect(PulseEffect, 1f, position, orientation, m_ability);
	}

	public override List<GameObject> FindAoeTargets(GameObject caster, Vector3 parentForward, Vector3 hitPosition, bool forUi)
	{
		List<GameObject> list = base.FindAoeTargets(caster, parentForward, hitPosition, forUi);
		if (forUi || !OneValidTargetPerPulse || list == null || list.Count == 0)
		{
			return list;
		}
		List<GameObject> list2 = new List<GameObject>();
		int index = OEIRandom.Index(list.Count);
		list2.Add(list[index]);
		return list2;
	}

	private void BeginPulse(GameObject self, Vector3 hitPosition)
	{
		if (self != null)
		{
			Projectile component = self.GetComponent<Projectile>();
			if (component != null)
			{
				float duration = PulseDuration();
				component.TargetMustBeUnconscious = TargetMustBeUnconscious;
				component.OwnerMayBeUnconscious = OwnerMayBeUnconscious;
				component.SetPulse(duration, hitPosition);
			}
		}
		if (m_ability != null)
		{
			m_ability.AttackComplete = true;
		}
	}

	public float PulseDuration()
	{
		float num = Duration;
		if (m_ownerStats != null)
		{
			num *= m_ownerStats.StatEffectDurationMultiplier;
		}
		return num;
	}

	public void Pulse(GameObject self, Vector3 hitPosition)
	{
		if (base.gameObject.activeInHierarchy)
		{
			base.OnImpact(self, hitPosition);
		}
	}

	protected override string GetAoeString(GenericAbility ability, GameObject character)
	{
		string aoeString = base.GetAoeString(ability, character);
		string text = "";
		if (Duration > 0f && Duration < 99999f)
		{
			float adjustedValue = Duration;
			GameObject gameObject = (character ? character : base.Owner);
			if ((bool)ability && (bool)gameObject)
			{
				CharacterStats component = gameObject.GetComponent<CharacterStats>();
				if ((bool)component && component.StatEffectDurationMultiplier != 1f)
				{
					adjustedValue = Duration * component.StatEffectDurationMultiplier;
				}
			}
			text = AttackBase.FormatWC(GUIUtils.GetText(1775), TextUtils.FormatBase(Duration, adjustedValue, (float v) => GUIUtils.Format(211, v.ToString("0.#"))));
		}
		return (aoeString + "\n" + text).Trim();
	}
}
