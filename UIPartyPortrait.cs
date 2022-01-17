using System;
using UnityEngine;

public class UIPartyPortrait : MonoBehaviour, ISelectACharacter
{
	private enum ClassCounterType
	{
		None,
		Focus,
		Phrases,
		Wounds
	}

	[HideInInspector]
	public int CurrentSlot;

	private bool m_NeedsUndisplace;

	private ClassCounterType m_ClassCounterType;

	public UIPartyPortraitMinion Minion;

	public UIPartyPortraitIcon LevelUp;

	public UIPartyPortraitIcon WantsToTalk;

	public UIGrid LevelTalkGrid;

	public UISlider Health;

	public UISlider Stamina;

	public UISlider StaminaCap;

	public UIWidget StaminaEdge;

	private UICharacterGetEndurance m_EnduranceValues;

	public UISlider HealthPulse;

	public UITexture HealthOverlay;

	public UISlider StaminaPulse;

	private TweenColor m_HealthOverlayTween;

	private UISprite m_HealthPulseSprite;

	private UISprite m_StaminaPulseSprite;

	public UICharacterActionIcon ActionIcon;

	public UITexture PortraitTexture;

	public UISprite Border;

	public UIStatusEffectStrip StatusEffectStrip;

	public GameObject ClassCounter;

	public UISprite ClassCounterIcon;

	public UILabel ClassCounterLabel;

	public GameObject ClassCounterPingEffect;

	public UIHealthObfuscator HealthObfuscator;

	private UITweener[] m_classCounterPingTweeners;

	public GameObject BackCollider;

	public Color HighHealthColor;

	public Color HighHealthColorColorBlind;

	public Color MediumHealthColor;

	public Color LowHealthColor1;

	public Color LowHealthColor2;

	private CharacterStats m_characterStats;

	private ChanterTrait m_characterChanter;

	private PartyMemberAI m_partyMemberAI;

	private NPCDialogue m_partyMemberDiag;

	private Health m_health;

	private GameObject m_LevelUpVfx;

	private GameObject m_CipherFocusVfx;

	private int gridContentCounter;

	private int oldGridContentCounter;

	public float PulsePeriodSeconds = 0.5f;

	public float PulseMinAlpha = 0.5f;

	public float PulseMaxAlpha = 0.7f;

	private int m_ClassCount = -1;

	private static int m_SaturationMinV;

	private static int m_Saturation;

	public CharacterStats SelectedCharacter => m_characterStats;

	private int ClassCount
	{
		get
		{
			return m_ClassCount;
		}
		set
		{
			if (m_ClassCount == value)
			{
				return;
			}
			if (m_characterStats.CharacterClass == CharacterStats.Class.Chanter && value >= 3)
			{
				for (int i = 0; i < m_classCounterPingTweeners.Length; i++)
				{
					m_classCounterPingTweeners[i].Reset();
					m_classCounterPingTweeners[i].Play(forward: true);
				}
			}
			m_ClassCount = value;
			ClassCounterLabel.text = m_ClassCount.ToString();
		}
	}

	public PartyMemberAI PartyMemberAI => m_partyMemberAI;

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	private void Start()
	{
		m_SaturationMinV = Shader.PropertyToID("_SaturationMinV");
		m_Saturation = Shader.PropertyToID("_Saturation");
		m_HealthPulseSprite = HealthPulse.GetComponentInChildren<UISprite>();
		m_StaminaPulseSprite = StaminaPulse.GetComponentInChildren<UISprite>();
		m_EnduranceValues = GetComponentInChildren<UICharacterGetEndurance>();
		m_HealthOverlayTween = HealthOverlay.GetComponent<TweenColor>();
		if ((bool)BackCollider)
		{
			BackCollider.transform.localScale = new Vector3(UIPartyPortraitBar.Instance.PortraitWidth + UIPartyPortraitBar.Instance.Spacing, BackCollider.transform.localScale.y, 1f);
		}
		GameState.OnLevelLoaded += LevelLoaded;
		GameState.OnCombatStart += CombatStart;
		GameState.OnCombatEnd += CombatEnd;
		UIEventListener uIEventListener = UIEventListener.Get(ClassCounterIcon);
		uIEventListener.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onTooltip, new UIEventListener.BoolDelegate(OnClassCounterTooltip));
		m_classCounterPingTweeners = ClassCounterPingEffect.GetComponents<UITweener>();
	}

	private void OnDestroy()
	{
		GameState.OnLevelLoaded -= LevelLoaded;
		GameState.OnCombatStart -= CombatStart;
		GameState.OnCombatEnd -= CombatEnd;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (m_partyMemberAI == null)
		{
			return;
		}
		m_EnduranceValues.LineBreak = Minion.gameObject.activeSelf;
		ActionIcon.Visible = GameState.InCombat && GameState.Option.GetOption(GameOption.BoolOption.SHOW_PORTRAIT_ACTION_ICONS);
		UpdateLevelTalkGrid();
		if (m_ClassCounterType == ClassCounterType.Focus)
		{
			ClassCount = Mathf.FloorToInt(m_characterStats.Focus);
		}
		else if (m_ClassCounterType == ClassCounterType.Phrases)
		{
			if (!m_characterChanter)
			{
				m_characterChanter = m_characterStats.GetChanterTrait();
			}
			if ((bool)m_characterChanter)
			{
				ClassCount = m_characterChanter.PhraseCount;
			}
		}
		if ((bool)m_CipherFocusVfx)
		{
			m_CipherFocusVfx.transform.localPosition = new Vector3(m_CipherFocusVfx.transform.localPosition.x, m_CipherFocusVfx.transform.localPosition.y);
		}
		float num = PulseMinAlpha + (PulseMaxAlpha - PulseMinAlpha) * Mathf.Sin((float)Math.PI * TimeController.sUnscaledDelta / PulsePeriodSeconds);
		if ((bool)m_StaminaPulseSprite && (bool)m_HealthPulseSprite)
		{
			float num4 = (m_StaminaPulseSprite.alpha = (m_HealthPulseSprite.alpha = num));
		}
		int num5 = 0;
		for (int i = 0; i < m_characterStats.ActiveStatusEffects.Count; i++)
		{
			num5 += -Mathf.RoundToInt(m_characterStats.ActiveStatusEffects[i].DotExpectedDamage(m_partyMemberAI.gameObject));
		}
		Transform transform = base.transform;
		float desiredXPosition = UIPartyPortraitBar.Instance.GetDesiredXPosition(CurrentSlot);
		if (transform.localPosition.x != desiredXPosition)
		{
			float num6 = Mathf.Sign(desiredXPosition - transform.localPosition.x);
			float num7 = Mathf.Max(1f, Mathf.Floor(Mathf.Abs((desiredXPosition - transform.localPosition.x) / UIPartyPortraitBar.Instance.PortraitWidth)));
			transform.localPosition += new Vector3(num7 * UIPartyPortraitBar.Instance.PortraitSlideSpeed * TimeController.sUnscaledDelta * num6, 0f, 0f);
			if (num6 != Mathf.Sign(desiredXPosition - transform.localPosition.x))
			{
				transform.localPosition = new Vector3(desiredXPosition, transform.localPosition.y, transform.localPosition.z);
				if (m_NeedsUndisplace)
				{
					m_NeedsUndisplace = false;
					Undisplace();
				}
			}
		}
		bool healthVisible = m_health.HealthVisible;
		UpdateHealthBar();
		Stamina.gameObject.SetActive(healthVisible);
		StaminaPulse.gameObject.SetActive(healthVisible);
		StaminaEdge.gameObject.SetActive(healthVisible);
		HealthObfuscator.gameObject.SetActive(!healthVisible);
		if (healthVisible)
		{
			float b = Mathf.Max(0f, m_health.CurrentHealth);
			float num8 = Mathf.Max(0f, m_health.CurrentStamina);
			float maxStamina = m_health.MaxStamina;
			maxStamina = Mathf.Min(maxStamina, b);
			float num9 = Mathf.Clamp01((m_characterStats.BaseMaxStamina - maxStamina) / m_characterStats.BaseMaxStamina);
			float a = 1f - Mathf.Clamp01(num8 / maxStamina);
			a = Mathf.Min(a, 1f - num9);
			Stamina.sliderValue = a;
			StaminaCap.sliderValue = num9;
			PortraitTexture.material.SetFloat(m_Saturation, 0f);
			PortraitTexture.material.SetFloat(m_SaturationMinV, 1f - StaminaCap.sliderValue);
		}
		else
		{
			PortraitTexture.material.SetFloat(m_Saturation, 0f);
			PortraitTexture.material.SetFloat(m_SaturationMinV, 0f);
		}
	}

	private void UpdateHealthBar()
	{
		float num = m_health.CurrentHealth / m_health.MaxHealth;
		if (m_health.HealthVisible)
		{
			HealthPulse.sliderValue = num;
			Health.sliderValue = Mathf.Max(0f, m_health.CurrentHealth) / m_health.MaxHealth;
		}
		else
		{
			Health.sliderValue = 1f;
		}
		HealthPulse.gameObject.SetActive(m_health.HealthVisible);
		Color color = HighHealthColor;
		bool flag = false;
		if (m_health.HealthVisible)
		{
			if (num >= 0.66f)
			{
				color = (GameState.Option.GetOption(GameOption.BoolOption.COLORBLIND_MODE) ? HighHealthColorColorBlind : HighHealthColor);
			}
			else if (num >= 0.33f)
			{
				color = MediumHealthColor;
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (num < 0.33f)
		{
			TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.PARTYMEM_LOW_HEALTH);
		}
		if (flag)
		{
			StatusEffect statusEffect = m_characterStats.FindFirstStatusEffectOfType(StatusEffect.ModifiedStat.HidesHealthStamina);
			if (statusEffect != null)
			{
				int num2 = (int)statusEffect.ParamsExtraValue();
				if (num2 < 0 || num2 >= UIPartyPortraitBar.Instance.ObfusticatorColors.Length)
				{
					Debug.LogError(string.Concat("HideHealthStamina effect from '", statusEffect.Origin, "' has a bad index in ExtraValue (", num2, ")."));
					num2 = 0;
				}
				m_HealthOverlayTween.from = UIPartyPortraitBar.Instance.ObfusticatorColors[num2].PulseColor1;
				m_HealthOverlayTween.to = UIPartyPortraitBar.Instance.ObfusticatorColors[num2].PulseColor2;
			}
			else
			{
				m_HealthOverlayTween.from = LowHealthColor1;
				m_HealthOverlayTween.to = LowHealthColor2;
			}
			m_HealthOverlayTween.from.a = (m_HealthOverlayTween.to.a = 1f);
			if (!m_HealthOverlayTween.enabled)
			{
				m_HealthOverlayTween.Play(forward: true);
			}
		}
		else
		{
			m_HealthOverlayTween.enabled = false;
			HealthOverlay.color = color;
		}
	}

	public void LetGo()
	{
		float desiredXPosition = UIPartyPortraitBar.Instance.GetDesiredXPosition(CurrentSlot);
		if (base.transform.localPosition.x != desiredXPosition)
		{
			m_NeedsUndisplace = true;
		}
		else
		{
			Undisplace();
		}
	}

	public void Grab()
	{
		Displace();
	}

	private void Displace()
	{
		base.transform.localPosition += new Vector3(0f, UIPartyPortraitBar.Instance.OffsetOnDrag, 0f);
	}

	private void Undisplace()
	{
		base.transform.localPosition -= new Vector3(0f, UIPartyPortraitBar.Instance.OffsetOnDrag, 0f);
	}

	private void AddStatusEffect(GameObject sender, StatusEffect effect, bool isFromAura)
	{
		if (m_ClassCounterType == ClassCounterType.Wounds && effect.AbilityOrigin is WoundsTrait)
		{
			ClassCount++;
		}
		if (!effect.Params.HideFromUi)
		{
			bool flag = (bool)effect.AbilityOrigin && effect.AbilityOrigin.Passive;
			bool flag2 = (bool)effect.AbilityOrigin && effect.AbilityOrigin.HideFromUi;
			bool flag3 = ((bool)effect.AbilityOrigin && m_characterStats.gameObject != effect.AbilityOrigin.Owner) || ((bool)effect.EquipmentOrigin && m_characterStats.gameObject != effect.EquipmentOrigin.EquippedOwner);
			if ((effect.Params.DontHideFromLog || !(((flag || (bool)effect.EquipmentOrigin) && !isFromAura && !effect.HasTriggerActivation && effect.Params.Duration == 0f && !flag3) || flag2)) && !effect.Params.IsInstantApplication && !effect.Params.OneHitUse)
			{
				StatusEffectStrip.AddStatusEffect(effect, isFromAura);
			}
		}
	}

	private void RemoveStatusEffect(GameObject sender, StatusEffect effect)
	{
		if ((bool)StatusEffectStrip)
		{
			StatusEffectStrip.RemoveStatusEffect(effect);
		}
		if (m_ClassCounterType == ClassCounterType.Wounds && effect.AbilityOrigin is WoundsTrait)
		{
			ClassCount--;
		}
	}

	public void RebundleEffects()
	{
		StatusEffectStrip.RebundleEffects();
	}

	private void LevelLoaded(object sender, EventArgs e)
	{
		ReloadPartyMember();
	}

	private void CombatStart(object sender, EventArgs e)
	{
		if (!m_CipherFocusVfx && m_characterStats.CharacterClass == CharacterStats.Class.Cipher)
		{
			m_CipherFocusVfx = UnityEngine.Object.Instantiate(UIPartyPortraitBar.Instance.CipherFocusVfx);
			m_CipherFocusVfx.transform.parent = base.transform;
			m_CipherFocusVfx.AddComponent<UIAnchor>().widgetContainer = ClassCounterIcon;
		}
		else if ((bool)m_CipherFocusVfx)
		{
			GameUtilities.RestartLoopingEffect(m_CipherFocusVfx);
		}
	}

	private void CombatEnd(object sender, EventArgs e)
	{
		if ((bool)m_CipherFocusVfx)
		{
			GameUtilities.ShutDownLoopingEffect(m_CipherFocusVfx);
		}
	}

	public void ReloadPartyMember()
	{
		StatusEffectStrip.Clear();
		if (m_partyMemberAI == null || m_partyMemberAI.gameObject == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		ClassCount = 0;
		m_ClassCounterType = ClassCounterType.None;
		if (m_characterStats.CharacterClass == CharacterStats.Class.Cipher)
		{
			m_ClassCounterType = ClassCounterType.Focus;
			ClassCounterIcon.spriteName = "ICO_focus";
			ClassCounterLabel.color = Color.white;
		}
		else if (m_characterStats.CharacterClass == CharacterStats.Class.Monk)
		{
			m_ClassCounterType = ClassCounterType.Wounds;
			ClassCounterIcon.spriteName = "ICO_wounds";
			ClassCounterLabel.color = new Color(84f / 85f, 69f / 85f, 0.8f);
		}
		else if (m_characterStats.CharacterClass == CharacterStats.Class.Chanter)
		{
			m_ClassCounterType = ClassCounterType.Phrases;
			ClassCounterIcon.spriteName = "ICO_phrases";
			ClassCounterLabel.color = Color.black;
		}
		ClassCounterIcon.MakePixelPerfect();
		ClassCounter.SetActive(m_ClassCounterType != ClassCounterType.None);
		if (m_characterStats.CharacterClass != CharacterStats.Class.Cipher && (bool)m_CipherFocusVfx)
		{
			GameUtilities.Destroy(m_CipherFocusVfx);
		}
		foreach (StatusEffect activeStatusEffect in m_characterStats.ActiveStatusEffects)
		{
			AddStatusEffect(m_characterStats.gameObject, activeStatusEffect, isFromAura: false);
		}
		SetPartyMemberIcons();
	}

	private void OnClassCounterTooltip(GameObject sender, bool bShow)
	{
		if (bShow)
		{
			if (m_ClassCounterType == ClassCounterType.Focus)
			{
				UIActionBarTooltip.GlobalShow(ClassCounterIcon, GUIUtils.GetText(415));
			}
			else if (m_ClassCounterType == ClassCounterType.Wounds)
			{
				UIActionBarTooltip.GlobalShow(ClassCounterIcon, GUIUtils.GetText(1503));
			}
			else if (m_ClassCounterType == ClassCounterType.Phrases)
			{
				UIActionBarTooltip.GlobalShow(ClassCounterIcon, GUIUtils.GetText(1434));
			}
		}
		else
		{
			UIActionBarTooltip.GlobalHide();
		}
	}

	public void SetPartyMember(PartyMemberAI partyMemberAI)
	{
		if (partyMemberAI != m_partyMemberAI)
		{
			GameObject gameObject = partyMemberAI.gameObject;
			if (m_characterStats != null)
			{
				m_characterStats.OnClearStatusEffect -= RemoveStatusEffect;
				m_characterStats.OnAddStatusEffect -= AddStatusEffect;
			}
			m_characterStats = gameObject.GetComponent<CharacterStats>();
			m_partyMemberAI = partyMemberAI;
			m_health = gameObject.GetComponent<Health>();
			m_characterChanter = m_characterStats.GetChanterTrait();
			m_partyMemberDiag = gameObject.GetComponent<NPCDialogue>();
			HealthObfuscator.LoadCharacter(m_characterStats);
			if (this.OnSelectedCharacterChanged != null)
			{
				this.OnSelectedCharacterChanged(SelectedCharacter);
			}
			m_characterStats.OnClearStatusEffect += RemoveStatusEffect;
			m_characterStats.OnAddStatusEffect += AddStatusEffect;
			ReloadPartyMember();
		}
	}

	public void SetPartyMemberIcons()
	{
		LevelUp.Stats = m_characterStats;
		LevelUp.Dialogue = m_partyMemberDiag;
		LevelUp.AI = m_partyMemberAI;
		WantsToTalk.Stats = m_characterStats;
		WantsToTalk.Dialogue = m_partyMemberDiag;
		WantsToTalk.AI = m_partyMemberAI;
	}

	private void UpdateLevelTalkGrid()
	{
		if ((m_characterStats.LevelUpAvailable() || m_characterStats.MaxMasteredAbilitiesAllowed() > m_characterStats.GetNumMasteredAbilities()) && !GameState.InCombat)
		{
			LevelUp.gameObject.SetActive(value: true);
			if (!m_LevelUpVfx)
			{
				m_LevelUpVfx = UnityEngine.Object.Instantiate(UIPartyPortraitBar.Instance.LevelUpVfx);
				m_LevelUpVfx.transform.parent = base.transform;
				m_LevelUpVfx.AddComponent<UIAnchor>().widgetContainer = LevelUp.GetComponentInChildren<UIWidget>();
			}
			else
			{
				GameUtilities.RestartLoopingEffect(m_LevelUpVfx);
			}
			gridContentCounter++;
		}
		else
		{
			LevelUp.gameObject.SetActive(value: false);
			if ((bool)m_LevelUpVfx)
			{
				GameUtilities.ShutDownLoopingEffect(m_LevelUpVfx);
			}
		}
		if ((bool)m_partyMemberDiag && m_partyMemberDiag.wantsToTalk && !GameState.InCombat)
		{
			WantsToTalk.gameObject.SetActive(value: true);
			gridContentCounter++;
		}
		else
		{
			WantsToTalk.gameObject.SetActive(value: false);
		}
		if (oldGridContentCounter != gridContentCounter)
		{
			LevelTalkGrid.Reposition();
		}
		oldGridContentCounter = gridContentCounter;
		gridContentCounter = 0;
	}
}
