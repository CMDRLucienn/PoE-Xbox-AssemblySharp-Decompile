using System.Collections.Generic;
using UnityEngine;

public class SoundSet : ScriptableObject
{
	public enum SoundAction
	{
		Invalid = -1,
		Selected,
		Movement,
		Attack,
		Idle,
		Leading,
		InjuredStamina,
		InjuredSevereStamina,
		InjuredHealth,
		InjuredSevereHealth,
		PlayerDeath,
		Rest,
		DeathComrade,
		Humor,
		TargetImmune,
		CriticalHit,
		CriticalMiss,
		InventoryFull,
		Scouting,
		FriendlyFire,
		ChatterOnEnter,
		ChatterOnExit,
		NOT_USED,
		FighterAbility,
		RogueAbility,
		PriestAbility,
		WizardAbility,
		BarbarianAbility,
		RangerAbility,
		DruidAbility,
		PaladinAbility,
		MonkAbility,
		CipherAbility,
		ChanterAbility,
		ImHit,
		ImDead,
		IAttack,
		Hello,
		Goodbye,
		Hello2,
		DetectableFound,
		LockPick,
		TaskComplete,
		Immobilized,
		PlayerKO,
		SpellCastFailure,
		Poisoned,
		EnemySpottedInStealth,
		CompanionDeath_Eder,
		CompanionDeath_Priest,
		CompanionDeath_Caroc,
		CompanionDeath_Aloth,
		CompanionDeath_Kana,
		CompanionDeath_Sagani,
		CompanionDeath_Pallegina,
		CompanionDeath_Mother,
		CompanionDeath_Hiravias,
		CompanionDeath_Calisca,
		CompanionDeath_Heodan,
		CompanionDeath_Generic,
		CompanionKO_Eder,
		CompanionKO_Priest,
		CompanionKO_Caroc,
		CompanionKO_Aloth,
		CompanionKO_Kana,
		CompanionKO_Sagani,
		CompanionKO_Pallegina,
		CompanionKO_Mother,
		CompanionKO_Hiravias,
		CompanionKO_Calisca,
		CompanionKO_Heodan,
		CompanionKO_Generic,
		CompanionDeath_Zahua,
		CompanionKO_Zahua,
		CompanionDeath_Maneha,
		CompanionKO_Maneha,
		PartyMemberPolymorphed,
		EnemyPolymorphed
	}

	public GUIDatabaseString DisplayName;

	public ConversationObject DialogOverride;

	private Dictionary<SoundAction, List<ClipNode>> m_clipTable = new Dictionary<SoundAction, List<ClipNode>>();

	public static float s_VeryShortVODelay = 2f;

	public static float s_ShortVODelay = 5f;

	public static float s_MediumVODelay = 12f;

	public static float s_LongVODelay = 20f;

	private float m_CooldownVOTimeReamining = -1f;

	private bool m_Initialized;

	private static AudioSource s_currentSound = null;

	public SoundSetClipList FighterAbility;

	public SoundSetClipList RogueAbility;

	public SoundSetClipList PriestAbility;

	public SoundSetClipList WizardAbility;

	public SoundSetClipList BarbarianAbility;

	public SoundSetClipList RangerAbility;

	public SoundSetClipList DruidAbility;

	public SoundSetClipList PaladinAbility;

	public SoundSetClipList MonkAbility;

	public SoundSetClipList CipherAbility;

	public SoundSetClipList ChanterAbility;

	public SoundSetClipList ImHit;

	public SoundSetClipList ImDead;

	public SoundSetClipList IAttack;

	public SoundSetClipList Hello;

	public SoundSetClipList Goodbye;

	public SoundSetClipList Hello2;

	private Dictionary<SoundAction, SoundSetClipList> m_FallbackClips;

	public float VOCooldownRemaining => m_CooldownVOTimeReamining;

	private void Initialize()
	{
		if (m_Initialized)
		{
			return;
		}
		m_Initialized = true;
		if (DialogOverride != null)
		{
			Object[] array = GameResources.LoadDialogueAudio(DialogOverride.Filename);
			Object[] array2 = array;
			if (array2 != null)
			{
				m_clipTable.Clear();
				array = array2;
				for (int i = 0; i < array.Length; i++)
				{
					VOAsset vOAsset = (VOAsset)array[i];
					if (vOAsset == null)
					{
						continue;
					}
					if (!m_clipTable.ContainsKey(vOAsset.VOAction))
					{
						m_clipTable.Add(vOAsset.VOAction, new List<ClipNode>());
					}
					ClipNode clipNode = new ClipNode();
					clipNode.Clip = vOAsset.VOClip;
					IntUtils.TryParseInvariant(vOAsset.name.Substring(vOAsset.name.Length - 4), out clipNode.NodeNumber);
					m_clipTable[vOAsset.VOAction].Add(clipNode);
					SoundAction[] additionalActions = vOAsset.AdditionalActions;
					foreach (SoundAction soundAction in additionalActions)
					{
						if (soundAction != SoundAction.Invalid)
						{
							if (!m_clipTable.ContainsKey(soundAction))
							{
								m_clipTable.Add(soundAction, new List<ClipNode>());
							}
							m_clipTable[soundAction].Add(clipNode);
						}
					}
				}
				foreach (SoundAction key in m_clipTable.Keys)
				{
					List<ClipNode> list = m_clipTable[key];
					float num = 0f;
					for (int k = 0; k < list.Count; k++)
					{
						num += list[k].Clip.PlayFrequency;
					}
					for (int l = 0; l < list.Count; l++)
					{
						list[l].Clip.PlayFrequency = list[l].Clip.PlayFrequency / num;
					}
				}
			}
		}
		AddFallbackSoundEffect(SoundAction.FighterAbility, FighterAbility);
		AddFallbackSoundEffect(SoundAction.RogueAbility, RogueAbility);
		AddFallbackSoundEffect(SoundAction.PriestAbility, PriestAbility);
		AddFallbackSoundEffect(SoundAction.WizardAbility, WizardAbility);
		AddFallbackSoundEffect(SoundAction.BarbarianAbility, BarbarianAbility);
		AddFallbackSoundEffect(SoundAction.RangerAbility, RangerAbility);
		AddFallbackSoundEffect(SoundAction.DruidAbility, DruidAbility);
		AddFallbackSoundEffect(SoundAction.PaladinAbility, PaladinAbility);
		AddFallbackSoundEffect(SoundAction.MonkAbility, MonkAbility);
		AddFallbackSoundEffect(SoundAction.CipherAbility, CipherAbility);
		AddFallbackSoundEffect(SoundAction.ChanterAbility, CipherAbility);
		AddFallbackSoundEffect(SoundAction.ImHit, ImHit);
		AddFallbackSoundEffect(SoundAction.ImDead, ImDead);
		AddFallbackSoundEffect(SoundAction.IAttack, IAttack);
		AddFallbackSoundEffect(SoundAction.Hello, Hello);
		AddFallbackSoundEffect(SoundAction.Goodbye, Goodbye);
		AddFallbackSoundEffect(SoundAction.Hello2, Hello2);
	}

	public void MyUpdate(float deltaTime)
	{
		if (m_CooldownVOTimeReamining > 0f)
		{
			m_CooldownVOTimeReamining -= deltaTime;
		}
	}

	private void OnDisable()
	{
		if (DialogOverride != null)
		{
			GameResources.UnloadDialogueAudio(DialogOverride.Filename);
		}
	}

	public bool PlaySound(GameObject owner, SoundAction action)
	{
		return PlaySound(owner, action, -1, skipIfConversing: true, ignoreListenerVolume: false);
	}

	public bool PlaySound(GameObject owner, SoundAction action, int idx)
	{
		return PlaySound(owner, action, idx, skipIfConversing: true, ignoreListenerVolume: false);
	}

	public bool PlaySound(GameObject owner, SoundAction action, int idx, bool skipIfConversing)
	{
		return PlaySound(owner, action, idx, skipIfConversing, ignoreListenerVolume: false);
	}

	public bool PlaySound(GameObject owner, SoundAction action, int idx, bool skipIfConversing, bool ignoreListenerVolume)
	{
		if (skipIfConversing && ConversationManager.Instance.IsConversationOrSIRunning())
		{
			Debug.Log("Conversation or scripted interaction active, skipping " + action);
			return false;
		}
		if (s_currentSound != null && s_currentSound.isPlaying)
		{
			return false;
		}
		Initialize();
		s_currentSound = null;
		if (m_clipTable.ContainsKey(action))
		{
			List<ClipNode> list = m_clipTable[action];
			if (idx < 0 || idx >= list.Count)
			{
				float num = OEIRandom.FloatValue();
				float num2 = 0f;
				for (int i = 0; i < list.Count; i++)
				{
					num2 += list[i].Clip.PlayFrequency;
					if (num < num2)
					{
						idx = i;
						break;
					}
				}
				if (idx < 0 || idx >= list.Count)
				{
					return false;
				}
			}
			VOBankClip clip = list[idx].Clip;
			if (clip == null || clip.clip == null)
			{
				return false;
			}
			if (owner != null)
			{
				float randomVolume = clip.RandomVolume;
				s_currentSound = GlobalAudioPlayer.StreamClipAtPoint(clip.clip, owner.transform.position, randomVolume, ignoreListenerVolume, bIs3DSound: true, MusicManager.SoundCategory.VOICE);
			}
		}
		else if (m_FallbackClips != null && m_FallbackClips.ContainsKey(action))
		{
			SoundSetClipList soundSetClipList = m_FallbackClips[action];
			if (idx < 0 || idx >= soundSetClipList.m_clips.Length)
			{
				idx = OEIRandom.Index(soundSetClipList.m_clips.Length);
			}
			if (soundSetClipList.m_clips[idx] != null)
			{
				string text = soundSetClipList.m_clips[idx];
				if (!string.IsNullOrEmpty(text) && owner != null)
				{
					s_currentSound = GlobalAudioPlayer.StreamClipAtPoint(text, owner.transform.position, 1f, ignoreListenerVolume: false, bIs3DSound: true, MusicManager.SoundCategory.VOICE);
				}
			}
		}
		return s_currentSound != null;
	}

	public static bool TryPlayVoiceEffectWithLocalCooldown(GameObject speaker, SoundAction voHook, float voiceCooldownInSeconds, bool forceInterrupt)
	{
		if (speaker == null)
		{
			return false;
		}
		SoundSetComponent component = speaker.GetComponent<SoundSetComponent>();
		if ((bool)component && (bool)component.SoundSet)
		{
			if (forceInterrupt)
			{
				component.SoundSet.InterruptAudio();
				component.SoundSet.m_CooldownVOTimeReamining = -1f;
			}
			if (component.SoundSet.m_CooldownVOTimeReamining <= 0f)
			{
				if (OEIRandom.FloatValue() < GameState.Option.VoiceFrequency)
				{
					bool result = component.SoundSet.PlaySound(speaker, voHook);
					component.SoundSet.m_CooldownVOTimeReamining = voiceCooldownInSeconds;
					return result;
				}
				return true;
			}
		}
		else
		{
			AudioBank component2 = speaker.GetComponent<AudioBank>();
			if ((bool)component2)
			{
				return component2.PlayFrom(voHook.ToString());
			}
		}
		return false;
	}

	public bool DoesClipExist(SoundAction soundHookToPlay)
	{
		Initialize();
		return m_clipTable.ContainsKey(soundHookToPlay);
	}

	public bool IsPlayingAudio()
	{
		return s_currentSound != null;
	}

	public void InterruptAudio()
	{
		if (s_currentSound != null)
		{
			s_currentSound.Stop();
		}
	}

	public void PlayBark(GameObject speaker, SoundAction action)
	{
		Initialize();
		if (m_clipTable.ContainsKey(action))
		{
			List<ClipNode> list = m_clipTable[action];
			int index = OEIRandom.Index(list.Count);
			ConversationManager.Instance.StartConversation(DialogOverride.Filename, list[index].NodeNumber, speaker, FlowChartPlayer.DisplayMode.Standard);
		}
	}

	private void AddFallbackSoundEffect(SoundAction actionID, SoundSetClipList clipList)
	{
		if (clipList != null)
		{
			if (m_FallbackClips == null)
			{
				m_FallbackClips = new Dictionary<SoundAction, SoundSetClipList>();
			}
			if (m_FallbackClips != null && !m_FallbackClips.ContainsKey(actionID))
			{
				m_FallbackClips.Add(actionID, clipList);
			}
		}
	}

	public void PlayBark(GameObject speaker, SoundAction action, int idx)
	{
		Initialize();
		if (m_clipTable.ContainsKey(action))
		{
			List<ClipNode> list = m_clipTable[action];
			if (idx >= list.Count)
			{
				idx = OEIRandom.Index(list.Count);
			}
			ConversationManager.Instance.StartConversation(DialogOverride.Filename, list[idx].NodeNumber, speaker, FlowChartPlayer.DisplayMode.Standard);
		}
	}

	public static SoundAction GetSoundActionForCompanionDeath(CompanionNames.Companions companionID)
	{
		return companionID switch
		{
			CompanionNames.Companions.Aloth => SoundAction.CompanionDeath_Aloth, 
			CompanionNames.Companions.Calisca => SoundAction.CompanionDeath_Calisca, 
			CompanionNames.Companions.Caroc => SoundAction.CompanionDeath_Caroc, 
			CompanionNames.Companions.Eder => SoundAction.CompanionDeath_Eder, 
			CompanionNames.Companions.Heodan => SoundAction.CompanionDeath_Heodan, 
			CompanionNames.Companions.Hiravias => SoundAction.CompanionDeath_Hiravias, 
			CompanionNames.Companions.Kana => SoundAction.CompanionDeath_Kana, 
			CompanionNames.Companions.Mother => SoundAction.CompanionDeath_Mother, 
			CompanionNames.Companions.Pallegina => SoundAction.CompanionDeath_Pallegina, 
			CompanionNames.Companions.Priest => SoundAction.CompanionDeath_Priest, 
			CompanionNames.Companions.Sagani => SoundAction.CompanionDeath_Sagani, 
			CompanionNames.Companions.Monk => SoundAction.CompanionDeath_Zahua, 
			CompanionNames.Companions.Maneha => SoundAction.CompanionDeath_Maneha, 
			_ => SoundAction.DeathComrade, 
		};
	}

	public static SoundAction GetSoundActionForCompanionKO(CompanionNames.Companions companionID)
	{
		return companionID switch
		{
			CompanionNames.Companions.Aloth => SoundAction.CompanionKO_Aloth, 
			CompanionNames.Companions.Calisca => SoundAction.CompanionKO_Calisca, 
			CompanionNames.Companions.Caroc => SoundAction.CompanionKO_Caroc, 
			CompanionNames.Companions.Eder => SoundAction.CompanionKO_Eder, 
			CompanionNames.Companions.Heodan => SoundAction.CompanionKO_Heodan, 
			CompanionNames.Companions.Hiravias => SoundAction.CompanionKO_Hiravias, 
			CompanionNames.Companions.Kana => SoundAction.CompanionKO_Kana, 
			CompanionNames.Companions.Mother => SoundAction.CompanionKO_Mother, 
			CompanionNames.Companions.Pallegina => SoundAction.CompanionKO_Pallegina, 
			CompanionNames.Companions.Priest => SoundAction.CompanionKO_Priest, 
			CompanionNames.Companions.Sagani => SoundAction.CompanionKO_Sagani, 
			CompanionNames.Companions.Monk => SoundAction.CompanionKO_Zahua, 
			CompanionNames.Companions.Maneha => SoundAction.CompanionKO_Maneha, 
			_ => SoundAction.CompanionKO_Generic, 
		};
	}
}
