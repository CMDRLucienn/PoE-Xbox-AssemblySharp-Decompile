using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

public class Phrase : MonoBehaviour, ITooltipContent
{
	public class NameComparer : IEqualityComparer<Phrase>
	{
		private static NameComparer s_instance;

		public static NameComparer Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = new NameComparer();
				}
				return s_instance;
			}
		}

		public bool Equals(Phrase a, Phrase b)
		{
			if (a == null || b == null)
			{
				return false;
			}
			if (a.DisplayName.StringID != b.DisplayName.StringID)
			{
				return false;
			}
			return a.name.Replace("(Clone)", "") == b.name.Replace("(Clone)", "");
		}

		public int GetHashCode(Phrase ability)
		{
			return ability.DisplayName.StringID;
		}
	}

	[Serializable]
	public class PhraseData
	{
		public bool IsHostile = true;

		public StatusEffect.ApplyType Apply;

		public StatusEffect.ModifiedStat AffectsStat;

		public DamagePacket.DamageType DmgType;

		public float Value;

		public float ExtraValue;

		public StatusEffectParams.IntervalRateType IntervalRate;

		public GameObject OnStartVisualEffect;

		public GameObject OnAppliedVisualEffect;

		public GameObject OnStopVisualEffect;

		public AttackBase.EffectAttachType VisualEffectAttach = AttackBase.EffectAttachType.Root;

		public Texture2D Icon;

		public Trap TrapPrefab;

		public Affliction AfflictionPrefab;
	}

	public class EffectData
	{
		public GameObject m_target;

		public uint m_effectID;

		public float m_durationMult;
	}

	public class HitData
	{
		public GameObject m_target;

		public HitType m_hitResult;
	}

	public Texture2D Icon;

	public DatabaseString DisplayName = new DatabaseString(DatabaseString.StringTableType.Abilities);

	public DatabaseString Description = new DatabaseString(DatabaseString.StringTableType.Abilities);

	public string Tag;

	[Range(1f, 5f)]
	public int Level;

	[Tooltip("Plays a cast vocalization based on character class. 0 prevents vocalization.")]
	[Range(0f, 10f)]
	public int VocalizationNumber;

	public CharacterStats.DefenseType DefendedBy = CharacterStats.DefenseType.None;

	public PhraseData[] m_phraseData;

	[Tooltip("Status Effects inflicted by this phrase.")]
	public StatusEffectParams[] StatusEffects;

	[Tooltip("Afflictions inflicted by this phrase.")]
	[FormerlySerializedAs("m_afflictionData")]
	public Affliction[] Afflictions;

	public GameObject OnChanterVisualEffect;

	public AttackBase.EffectAttachType ChanterVisualEffectAttach = AttackBase.EffectAttachType.Root;

	private List<EffectData> m_effectData;

	private List<StatusEffectParams> m_fxParams = new List<StatusEffectParams>();

	private List<DamageInfo> m_hitData;

	private GameObject[] m_hostileTargets;

	private GameObject m_chanterFX;

	private float m_fxTimer;

	private float m_phraseDelay;

	private readonly AttackBase.FormattableTarget AURA_TARGET = new AttackBase.FormattableTarget(1618, 1617);

	public CharacterStats PhraseOwner { get; set; }

	public float Recitation => CalculateRecitation(PhraseOwner);

	public float BaseRecitation => (float)Level * 2f + 2f;

	public float Linger => CalculateLinger(PhraseOwner);

	public float BaseLinger => (float)Level + 1f;

	public float Duration => Recitation + Linger;

	private void Update()
	{
		if (m_phraseDelay > 0f)
		{
			m_phraseDelay -= Time.deltaTime;
			if (m_phraseDelay <= 0f)
			{
				BeginPhrase(PhraseOwner.gameObject, m_phraseDelay);
				m_phraseDelay = 0f;
			}
		}
		if (m_fxTimer > 0f)
		{
			m_fxTimer -= Time.deltaTime;
			if (m_fxTimer <= 0f)
			{
				EndVfx();
			}
		}
	}

	private void EndVfx()
	{
		if ((bool)m_chanterFX)
		{
			GameUtilities.ShutDownLoopingEffect(m_chanterFX);
			GameUtilities.Destroy(m_chanterFX, 5f);
			m_chanterFX = null;
			m_fxTimer = 0f;
		}
	}

	public void BeginPhrase(GameObject owner, float delay)
	{
		if (owner == null)
		{
			return;
		}
		CharacterStats component = owner.GetComponent<CharacterStats>();
		if (PhraseOwner != component)
		{
			PhraseOwner = component;
		}
		if (delay > 0f)
		{
			m_phraseDelay = delay;
			return;
		}
		if ((bool)owner && FogOfWar.PointVisibleInFog(owner.transform.position))
		{
			Console.AddMessage(GUIUtils.FormatWithLinks(133, CharacterStats.NameColored(owner), DisplayName.ToString()), Color.white);
		}
		if (OnChanterVisualEffect != null)
		{
			EndVfx();
			Transform transform = AttackBase.GetTransform(owner, ChanterVisualEffectAttach);
			m_chanterFX = GameUtilities.LaunchLoopingEffect(OnChanterVisualEffect, transform.position, transform.rotation, 1f, transform, null);
			if (m_chanterFX != null)
			{
				m_fxTimer = Duration;
			}
		}
		CharacterStats component2 = owner.GetComponent<CharacterStats>();
		float range = AttackData.Instance.ChanterPhraseRadius;
		if (component2 != null)
		{
			range = component2.ChantRadius;
		}
		m_hostileTargets = GameUtilities.CreaturesInRange(owner.transform.position, range, owner, includeUnconscious: true);
		if (m_hostileTargets != null && component2 != null)
		{
			m_hitData = new List<DamageInfo>();
			GameObject[] hostileTargets = m_hostileTargets;
			foreach (GameObject enemy in hostileTargets)
			{
				DamageInfo damageInfo = component2.ComputeSecondaryAttack(null, enemy, DefendedBy);
				damageInfo.OtherOwner = owner;
				m_hitData.Add(damageInfo);
			}
		}
		GameObject[] array = GameUtilities.FriendsInRange(owner.transform.position, range, owner, includeUnconscious: false);
		m_effectData = new List<EffectData>();
		m_fxParams.Clear();
		if (m_phraseData != null)
		{
			for (int j = 0; j < m_phraseData.Length; j++)
			{
				m_fxParams.Add(new StatusEffectParams(m_phraseData[j], BaseLinger));
			}
		}
		if (StatusEffects != null)
		{
			m_fxParams.AddRange(StatusEffects);
		}
		List<StatusEffect> list = new List<StatusEffect>();
		if (m_hostileTargets != null)
		{
			for (int k = 0; k < m_hostileTargets.Length; k++)
			{
				list.Clear();
				ApplyPhraseHostileEffects(owner, m_hostileTargets[k], list);
				if (list.Count > 0)
				{
					AttackBase.PostAttackMessages(m_hostileTargets[k], m_hitData[k], list, primaryAttack: true);
				}
			}
		}
		if (array != null)
		{
			GameObject[] hostileTargets = array;
			foreach (GameObject target in hostileTargets)
			{
				list.Clear();
				ApplyPhraseFriendlyEffects(owner, target, list);
			}
		}
		list.Clear();
		ApplyPhraseFriendlyEffects(owner, owner, list);
	}

	private void ApplyPhraseHostileEffects(GameObject owner, GameObject target, List<StatusEffect> appliedEffects)
	{
		foreach (StatusEffectParams fxParam in m_fxParams)
		{
			if (fxParam.IsHostile)
			{
				ApplyPhraseEffect(owner, fxParam, target, null, appliedEffects);
			}
		}
		if (Afflictions == null)
		{
			return;
		}
		Affliction[] afflictions = Afflictions;
		foreach (Affliction affliction in afflictions)
		{
			if (affliction == null)
			{
				continue;
			}
			if (affliction.Exclusive && HitResult(target) != 0)
			{
				CharacterStats component = target.GetComponent<CharacterStats>();
				if ((bool)component)
				{
					component.ClearEffectFromAffliction(affliction);
				}
			}
			if (affliction.Overrides != null && HitResult(target) != 0)
			{
				CharacterStats component2 = target.GetComponent<CharacterStats>();
				if ((bool)component2)
				{
					Affliction[] overrides = affliction.Overrides;
					foreach (Affliction aff in overrides)
					{
						component2.SuppressEffectFromAffliction(aff);
					}
				}
			}
			if (affliction.StatusEffects != null)
			{
				StatusEffectParams[] statusEffects = affliction.StatusEffects;
				foreach (StatusEffectParams fx in statusEffects)
				{
					ApplyPhraseEffect(owner, fx, target, affliction, appliedEffects);
				}
			}
		}
	}

	private void ApplyPhraseFriendlyEffects(GameObject owner, GameObject target, List<StatusEffect> appliedEffects)
	{
		foreach (StatusEffectParams fxParam in m_fxParams)
		{
			if (!fxParam.IsHostile)
			{
				ApplyPhraseEffect(owner, fxParam, target, null, appliedEffects);
			}
		}
	}

	private void ApplyPhraseEffect(GameObject owner, StatusEffectParams fx, GameObject target, Affliction aff, List<StatusEffect> appliedEffects)
	{
		if (fx == null)
		{
			return;
		}
		CharacterStats component = owner.GetComponent<CharacterStats>();
		CharacterStats component2 = target.GetComponent<CharacterStats>();
		if (!component || !component2)
		{
			return;
		}
		float num = fx.AdjustDuration(component, BaseLinger);
		float num2 = 1f;
		if (fx.IsHostile)
		{
			num2 = GetDurationMultiplier(target);
			if (num2 == 0f)
			{
				return;
			}
			num *= num2;
		}
		StatusEffect statusEffect = StatusEffect.Create(owner, fx, GenericAbility.AbilityType.Ability, null, deleteOnClear: true, num);
		statusEffect.AfflictionOrigin = aff;
		statusEffect.PhraseOrigin = this;
		statusEffect.UnadjustedDurationAdd = Recitation;
		if (component2.ApplyStatusEffectImmediate(statusEffect))
		{
			appliedEffects?.Add(statusEffect);
		}
		EffectData effectData = new EffectData();
		effectData.m_target = target;
		effectData.m_effectID = statusEffect.EffectID;
		effectData.m_durationMult = num2;
		m_effectData.Add(effectData);
	}

	private float GetDurationMultiplier(GameObject target)
	{
		if (DefendedBy == CharacterStats.DefenseType.None)
		{
			return 1f;
		}
		HitType hitType = HitResult(target);
		float result = 1f;
		switch (hitType)
		{
		case HitType.CRIT:
			result = CharacterStats.CritMultiplier;
			break;
		case HitType.GRAZE:
			result = CharacterStats.GrazeMultiplier;
			break;
		case HitType.MISS:
			result = 0f;
			break;
		}
		return result;
	}

	private HitType HitResult(GameObject target)
	{
		if (m_hitData != null)
		{
			int num = -1;
			for (int i = 0; i < m_hostileTargets.Length; i++)
			{
				if (m_hostileTargets[i] == target)
				{
					num = i;
				}
			}
			if (num >= 0)
			{
				return m_hitData[num].HitType;
			}
		}
		return HitType.MISS;
	}

	public void InterruptPhrase()
	{
		if (m_effectData != null)
		{
			foreach (EffectData effectDatum in m_effectData)
			{
				if (effectDatum == null || effectDatum.m_target == null)
				{
					continue;
				}
				CharacterStats component = effectDatum.m_target.GetComponent<CharacterStats>();
				if ((bool)component)
				{
					StatusEffect statusEffect = component.FindStatusEffect(effectDatum.m_effectID);
					if (statusEffect != null)
					{
						statusEffect.Duration = Linger * effectDatum.m_durationMult;
						statusEffect.ResetTimer();
					}
				}
			}
		}
		if (m_chanterFX != null)
		{
			m_fxTimer = Linger;
		}
	}

	public float CalculateRecitation(GameObject owner)
	{
		return CalculateRecitation((owner != null) ? owner.GetComponent<CharacterStats>() : null);
	}

	public float CalculateRecitation(CharacterStats character)
	{
		float num = 1f;
		if ((bool)character)
		{
			num = character.GetStatusEffectValueMultiplier(StatusEffect.ModifiedStat.PhraseRecitationLengthMult);
		}
		return Mathf.Max(0.5f, num * BaseRecitation);
	}

	public float CalculateLinger(GameObject owner)
	{
		return CalculateLinger((owner != null) ? owner.GetComponent<CharacterStats>() : null);
	}

	public float CalculateLinger(CharacterStats charStats)
	{
		float num = 1f;
		if (charStats != null)
		{
			num = charStats.StatEffectDurationMultiplier;
		}
		return num * BaseLinger;
	}

	public string GetString(GenericAbility ability, GameObject character, StringEffects stringEffects)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string aoeString = GetAoeString(ability, character);
		if (!string.IsNullOrEmpty(aoeString))
		{
			stringBuilder.AppendLine(AttackBase.FormatWC(GUIUtils.GetText(1590), aoeString));
		}
		stringBuilder.AppendLine(AttackBase.FormatWC(GUIUtils.GetText(1634), TextUtils.FormatBase(BaseRecitation, CalculateRecitation(character), (float v) => GUIUtils.Seconds(v))));
		stringBuilder.Append(AttackBase.FormatWC(GUIUtils.GetText(1635), TextUtils.FormatBase(BaseLinger, CalculateLinger(character), (float v) => GUIUtils.Seconds(v))));
		stringBuilder.AppendLine();
		GetAdditionalEffects(stringEffects, ability, character);
		return stringBuilder.ToString().TrimEnd();
	}

	public void GetAdditionalEffects(StringEffects stringEffects, GenericAbility ability, GameObject character)
	{
		AddEffects(AURA_TARGET, character, 1f, stringEffects);
	}

	protected string GetAoeString(GenericAbility ability, GameObject character)
	{
		return "";
	}

	public void AddEffects(AttackBase.FormattableTarget formatTarget, GameObject character, float damageMult, StringEffects stringEffects, IEnumerable<StatusEffectParams> statusEffects = null)
	{
		CharacterStats source = null;
		if ((bool)character)
		{
			source = character.GetComponent<CharacterStats>();
		}
		List<StatusEffectParams> list = new List<StatusEffectParams>();
		for (int i = 0; i < m_phraseData.Length; i++)
		{
			list.Add(new StatusEffectParams(m_phraseData[i], 0f));
		}
		list.AddRange(StatusEffects);
		StatusEffectParams.CleanUp(list);
		string text = StatusEffectParams.ListToString(list.Where((StatusEffectParams sep) => !sep.IsHostile), source, this, StatusEffectFormatMode.InspectWindow);
		if (!string.IsNullOrEmpty(text))
		{
			AttackBase.AddStringEffect(formatTarget.GetText(AttackBase.TargetType.Friendly), new AttackBase.AttackEffect(text, null, hostile: false), stringEffects);
		}
		text = StatusEffectParams.ListToString(list.Where((StatusEffectParams sep) => sep.IsHostile), source, this, StatusEffectFormatMode.InspectWindow);
		if (!string.IsNullOrEmpty(text))
		{
			AttackBase.AddStringEffect(formatTarget.GetText(AttackBase.TargetType.Hostile), new AttackBase.AttackEffect(text, null, hostile: true), stringEffects);
		}
		Affliction[] afflictions = Afflictions;
		foreach (Affliction affliction in afflictions)
		{
			AttackBase.AddStringEffect(formatTarget.GetText(AttackBase.TargetType.Hostile), new AttackBase.AttackEffect(affliction.DisplayName.ToString(), null, hostile: true), stringEffects);
		}
	}

	public string GetTooltipContent(GameObject owner)
	{
		return Description.GetText();
	}

	public string GetTooltipName(GameObject owner)
	{
		return DisplayName.GetText();
	}

	public Texture GetTooltipIcon()
	{
		return Icon;
	}
}
