using System;
using System.Collections.Generic;
using UnityEngine;

public class ChanterTrait : GenericAbility
{
	private Chant m_chant;

	[Persistent(Persistent.ConversionType.GUIDLink)]
	private Chant m_lastChant;

	private int m_phraseCount;

	private int m_phraseCountMax = 1;

	[Persistent]
	private List<Phrase> m_phrasesKnown = new List<Phrase>();

	private bool m_FirstUpdate = true;

	public int PhraseCount
	{
		get
		{
			return m_phraseCount;
		}
		set
		{
			int phraseCountMax = PhraseCountMax;
			if (value > phraseCountMax)
			{
				m_phraseCount = phraseCountMax;
			}
			else
			{
				m_phraseCount = value;
			}
		}
	}

	public int PhraseCountMax
	{
		get
		{
			return m_phraseCountMax;
		}
		set
		{
			m_phraseCountMax = value;
		}
	}

	public Chant Chant => m_chant;

	public Chant DesiredChant
	{
		get
		{
			return m_lastChant;
		}
		set
		{
			m_lastChant = value;
		}
	}

	protected override void OnDestroy()
	{
		GameState.OnCombatStart -= OnCombatStart;
		GameState.OnCombatEnd -= OnCombatEnd;
		base.OnDestroy();
	}

	protected override void Update()
	{
		base.Update();
		if (m_FirstUpdate)
		{
			m_FirstUpdate = false;
			if (m_ownerStats != null)
			{
				for (int num = m_ownerStats.ActiveAbilities.Count - 1; num >= 0; num--)
				{
					Chant chant = m_ownerStats.ActiveAbilities[num] as Chant;
					if ((bool)chant && chant.PhraseInstances.Length == 0)
					{
						m_ownerStats.ActiveAbilities.RemoveAt(num);
						Persistence component = chant.GetComponent<Persistence>();
						if ((bool)component)
						{
							component.SetForDestroy();
						}
						GameUtilities.Destroy(chant.gameObject);
					}
				}
			}
		}
		if (GameState.InCombat && !GameState.IsInTrapTriggeredCombat && !IsChanting() && !GameState.Paused)
		{
			Chant chant2 = FindPendingChant();
			if (chant2 == null)
			{
				StartLastChant();
			}
			else if (chant2.BeginChant())
			{
				m_chant = m_lastChant;
			}
		}
	}

	protected override void HandleStatsOnPreApply(GameObject source, CombatEventArgs args)
	{
		Chant component = source.GetComponent<Chant>();
		if (component != null)
		{
			if (m_chant != null)
			{
				m_chant.InterruptChant();
			}
			m_chant = component;
			m_lastChant = component;
			return;
		}
		AttackBase component2 = source.GetComponent<AttackBase>();
		if (component2 != null && component2.IsInvocation)
		{
			if (m_chant != null)
			{
				m_chant.DelayChant();
			}
			PhraseCount -= component2.PhraseCost;
		}
	}

	protected override void HandleStatsOnPostDamageApplied(GameObject source, CombatEventArgs args)
	{
		if (m_chant != null && args.Damage != null && args.Damage.Attack != null && args.Damage.Attack.IsInvocation)
		{
			m_chant.DelayChant();
		}
	}

	protected override void HandleStatsOnDeactivate(GameObject source, CombatEventArgs args)
	{
		Chant component = source.GetComponent<Chant>();
		if (component != null && m_chant == component)
		{
			m_chant = null;
		}
	}

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			GameState.OnCombatStart += OnCombatStart;
			GameState.OnCombatEnd += OnCombatEnd;
			m_permanent = true;
		}
	}

	private void OnCombatStart(object sender, EventArgs e)
	{
		if (!GameState.IsInTrapTriggeredCombat)
		{
			StartLastChant();
		}
		PhraseCountMax = FindMaxRoarCost();
	}

	protected void StartLastChant()
	{
		PhraseCount = 0;
		if (m_lastChant == null)
		{
			m_lastChant = FindFirstChant();
			if (m_lastChant == null)
			{
				return;
			}
		}
		if (m_lastChant.BeginChant())
		{
			m_chant = m_lastChant;
		}
	}

	private Chant FindFirstChant()
	{
		if (m_ownerStats != null)
		{
			foreach (GenericAbility activeAbility in m_ownerStats.ActiveAbilities)
			{
				if (activeAbility is Chant)
				{
					return activeAbility as Chant;
				}
			}
		}
		return null;
	}

	private Chant FindPendingChant()
	{
		if (m_ownerStats != null)
		{
			foreach (GenericAbility activeAbility in m_ownerStats.ActiveAbilities)
			{
				if (activeAbility is Chant && activeAbility.UiActivated)
				{
					return activeAbility as Chant;
				}
			}
		}
		return null;
	}

	private int FindMaxRoarCost()
	{
		int num = 1;
		if (m_ownerStats != null)
		{
			foreach (GenericAbility activeAbility in m_ownerStats.ActiveAbilities)
			{
				if (activeAbility != null && activeAbility.Attack != null)
				{
					int phraseCost = activeAbility.Attack.PhraseCost;
					if (phraseCost > num)
					{
						num = phraseCost;
					}
				}
			}
			return num;
		}
		return num;
	}

	private void OnCombatEnd(object sender, EventArgs e)
	{
		PhraseCount = 0;
	}

	protected override void ActivateStatusEffects()
	{
	}

	public bool IsChanting()
	{
		if (m_chant == null)
		{
			return false;
		}
		return m_chant.IsActive();
	}

	public void AddKnownPhrase(Phrase phrase)
	{
		if (!HasKnownPhrase(phrase))
		{
			m_phrasesKnown.Add(phrase);
		}
	}

	public bool HasKnownPhrase(Phrase phrase)
	{
		return m_phrasesKnown.Contains(phrase);
	}

	public Phrase[] GetKnownPhrases()
	{
		return m_phrasesKnown.ToArray();
	}
}
