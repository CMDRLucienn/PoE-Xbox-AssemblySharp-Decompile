using System.Text;
using UnityEngine;

public class Chant : GenericAbility
{
	public const int MaxChantsPerCharacter = 4;

	public const int MaxPhrasesPerChant = 10;

	public const float PhraseStartDelay = 1f;

	[Persistent]
	public Phrase[] Phrases;

	[HideInInspector]
	public Phrase[] PhraseInstances = new Phrase[0];

	private float m_chantTimer;

	private float m_delayTimer;

	private int m_currentPhrase = -1;

	[HideInInspector]
	[Persistent]
	public int UiIndex = -1;

	public float Timer => m_chantTimer;

	public float TimeToNextPhrase
	{
		get
		{
			float num = 0f;
			for (int i = 0; i <= m_currentPhrase; i++)
			{
				if (PhraseInstances[i] != null)
				{
					num += PhraseInstances[i].Recitation;
				}
			}
			return num;
		}
	}

	public int CurrentPhrase => m_currentPhrase;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			CombatOnly = true;
			CannotActivateWhileInStealth = true;
			VocalizationClass = CharacterStats.Class.Chanter;
			InstantiatePhrases();
		}
	}

	public void SetPhraseOwner(CharacterStats owner)
	{
		for (int i = 0; i < PhraseInstances.Length; i++)
		{
			PhraseInstances[i].PhraseOwner = owner;
		}
	}

	public void InstantiatePhrases()
	{
		Phrases = ArrayExtender.Compress(Phrases);
		if (Phrases.Length == PhraseInstances.Length)
		{
			bool flag = true;
			for (int i = 0; i < Phrases.Length; i++)
			{
				if (!Phrase.NameComparer.Instance.Equals(Phrases[i], PhraseInstances[i]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return;
			}
		}
		for (int j = 0; j < PhraseInstances.Length; j++)
		{
			GameUtilities.Destroy(PhraseInstances[j].gameObject);
		}
		if (PhraseInstances.Length != Phrases.Length)
		{
			PhraseInstances = new Phrase[Phrases.Length];
		}
		for (int k = 0; k < Phrases.Length; k++)
		{
			PhraseInstances[k] = Object.Instantiate(Phrases[k].gameObject).GetComponent<Phrase>();
			Persistence component = PhraseInstances[k].GetComponent<Persistence>();
			if ((bool)component)
			{
				GameUtilities.DestroyComponent(component);
			}
			PhraseInstances[k].transform.parent = base.transform;
		}
	}

	public override void Activate(Vector3 target)
	{
		if (CanChant())
		{
			base.Activate(target);
		}
	}

	public override void Activate(GameObject target)
	{
		if (CanChant())
		{
			base.Activate(target);
		}
	}

	protected override void ReportActivation(bool overridePassive)
	{
		if (!HideFromCombatLog)
		{
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(131), CharacterStats.NameColored(m_owner), GenericAbility.Name(this)), Color.white);
		}
	}

	protected override void ReportDeactivation(bool overridePassive)
	{
		if (!HideFromCombatLog)
		{
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(132), CharacterStats.NameColored(m_owner), GenericAbility.Name(this)), Color.white);
		}
	}

	protected override void Update()
	{
		if (!GameState.Paused)
		{
			if (m_statusEffectsNeeded && !m_statusEffectsActivated)
			{
				BeginChant();
			}
			else if (!m_statusEffectsNeeded && m_statusEffectsActivated)
			{
				InterruptChant();
			}
		}
		base.Update();
		if (m_delayTimer > 0f)
		{
			m_delayTimer -= Time.deltaTime;
			if (m_delayTimer <= 0f)
			{
				BeginChant();
				m_delayTimer = 0f;
			}
		}
		else
		{
			if (m_currentPhrase < 0 || TimeController.Instance.Paused)
			{
				return;
			}
			if (!CanChant())
			{
				Deactivate(m_owner);
				return;
			}
			m_chantTimer += Time.deltaTime;
			float timeToNextPhrase = TimeToNextPhrase;
			if ((bool)m_ownerStats)
			{
				m_ownerStats.NoiseLevel = NoiseLevel;
			}
			if (m_chantTimer >= timeToNextPhrase)
			{
				m_currentPhrase++;
				if (m_currentPhrase >= PhraseInstances.Length)
				{
					m_currentPhrase = 0;
					m_chantTimer -= timeToNextPhrase;
				}
				if (PhraseInstances[m_currentPhrase] != null)
				{
					PhraseInstances[m_currentPhrase].BeginPhrase(m_owner, 0f);
				}
				IncrementPhraseCount();
			}
		}
	}

	public override void TriggerFromUI()
	{
		if (m_ownerStats != null)
		{
			ChanterTrait chanterTrait = m_ownerStats.GetChanterTrait();
			if (chanterTrait != null)
			{
				chanterTrait.DesiredChant = this;
			}
		}
		base.TriggerFromUI();
	}

	private void IncrementPhraseCount()
	{
		if (m_ownerStats != null)
		{
			ChanterTrait chanterTrait = m_ownerStats.GetChanterTrait();
			if (chanterTrait != null)
			{
				chanterTrait.PhraseCount++;
			}
		}
	}

	public bool CanChant()
	{
		if (m_delayTimer > 0f)
		{
			return false;
		}
		if (!GameState.InCombat || GameState.IsInTrapTriggeredCombat)
		{
			return false;
		}
		if (m_ownerHealth != null && (m_ownerHealth.Unconscious || m_ownerHealth.Dead))
		{
			return false;
		}
		if (m_ownerStats != null)
		{
			if (m_ownerStats.HasStatusEffectFromAffliction(AfflictionData.Paralyzed) || m_ownerStats.HasStatusEffectFromAffliction(AfflictionData.Petrified))
			{
				return false;
			}
			if (m_ownerStats.HasStatusEffectOfType(StatusEffect.ModifiedStat.Stunned))
			{
				return false;
			}
		}
		AIPackageController component = m_owner.GetComponent<AIPackageController>();
		if (component != null && component.gameObject.activeInHierarchy)
		{
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI in partyMembers)
			{
				if (partyMemberAI != null && GameUtilities.V3SqrDistance2D(component.transform.position, partyMemberAI.transform.position) < 400f)
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public bool BeginChant()
	{
		if (PhraseInstances.Length == 0)
		{
			return false;
		}
		if (!CanChant())
		{
			if (Activated)
			{
				Deactivate(m_owner);
			}
			return false;
		}
		if (!Activated)
		{
			base.Activate(m_owner);
			if (!Activated)
			{
				return false;
			}
		}
		m_chantTimer = 0f;
		m_currentPhrase = 0;
		if (PhraseInstances != null && PhraseInstances[m_currentPhrase] != null)
		{
			VocalizationNumber = PhraseInstances[m_currentPhrase].VocalizationNumber;
			PlayVocalization();
			PhraseInstances[m_currentPhrase].BeginPhrase(m_owner, 1f);
		}
		m_UITriggered = true;
		m_statusEffectsActivated = true;
		return true;
	}

	public void DelayChant()
	{
		InterruptChant();
		m_delayTimer = 2f;
	}

	public void InterruptChant()
	{
		if (m_currentPhrase != -1)
		{
			if (PhraseInstances != null && m_currentPhrase < PhraseInstances.Length && PhraseInstances[m_currentPhrase] != null)
			{
				PhraseInstances[m_currentPhrase].InterruptPhrase();
			}
			m_currentPhrase = -1;
			m_delayTimer = 0f;
			m_statusEffectsActivated = false;
		}
	}

	public bool IsActive()
	{
		if (m_currentPhrase == -1)
		{
			return false;
		}
		if (m_delayTimer > 0f)
		{
			return false;
		}
		return true;
	}

	protected override string GetResourceString()
	{
		string text = "";
		if (m_delayTimer > 0f)
		{
			text = GUIUtils.Format(1913, "[" + NGUITools.EncodeColor(Color.red) + "]" + GUIUtils.Format(211, m_delayTimer.ToString("#0.0")) + "[-]");
		}
		string resourceString = base.GetResourceString();
		return (text + "\n" + resourceString).Trim();
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GUIUtils.GetText(1434));
		stringBuilder.AppendLine(":");
		Phrase[] phraseInstances = PhraseInstances;
		foreach (Phrase phrase in phraseInstances)
		{
			stringBuilder.AppendLine("\r" + phrase.DisplayName.GetText());
		}
		if (PhraseInstances.Length == 0)
		{
			stringBuilder.AppendLine("\r" + GUIUtils.GetText(343));
		}
		string additionalEffects = base.GetAdditionalEffects(stringEffects, mode, ability, character);
		return (stringBuilder.ToString().Trim() + "\n" + additionalEffects).Trim();
	}
}
