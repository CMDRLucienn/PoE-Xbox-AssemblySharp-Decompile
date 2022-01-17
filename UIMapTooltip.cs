using System;
using System.Text;
using UnityEngine;

public class UIMapTooltip : UIScreenRectangleItem, ISelectACharacter
{
	[HideInInspector]
	public bool RevealedByMouse;

	[HideInInspector]
	public bool RevealedByAttackCursor;

	private PartyMemberAI m_PartyAI;

	private Health m_Health;

	private Faction m_Faction;

	private BackerContent m_BackerContent;

	public UILabel NameLabel;

	public UILabel RaceLabel;

	public UILabel HealthLabel;

	public UITexture Background;

	public UISprite Divider;

	public GameObject DefenseParent;

	public GameObject DtParent;

	public UICharacterImmunitiesGetter ImmunitiesParent;

	public UICharacterResistancesGetter ResistancesParent;

	[Tooltip("Anchor that will be attached to the bottommost element of the top section.")]
	public UIAnchor SubTop;

	public UIPanel Panel;

	public UITable Table;

	private UIAnchorToWorld WorldAnchor;

	public UIAnchor PortraitAnchor;

	public UIStretchToContents ContentStretcher;

	public UIAnchor PointerAnchor;

	private UITweenerAggregator m_RootTweens;

	private float m_Opposition;

	private int m_OpposerCount;

	public bool AllowStats;

	private bool m_Init;

	private bool m_HideToPool;

	private static StringBuilder s_stringBuilder = new StringBuilder();

	public GameObject Target { get; private set; }

	public bool TargetIsDead
	{
		get
		{
			if ((bool)m_Health)
			{
				return m_Health.ShowDead;
			}
			return false;
		}
	}

	public CharacterStats SelectedCharacter { get; private set; }

	private bool TargetIsParty
	{
		get
		{
			if ((bool)m_PartyAI)
			{
				return m_PartyAI.IsActiveInParty;
			}
			return false;
		}
	}

	private bool ShowHealth
	{
		get
		{
			if ((bool)m_PartyAI)
			{
				return m_PartyAI.SummonType != AIController.AISummonType.AnimalCompanion;
			}
			return true;
		}
	}

	private bool ShowDefenses
	{
		get
		{
			if (!TargetIsParty && (bool)SelectedCharacter && GameState.Option.DisplayRelativeDefenses && RevealedByAttackCursor)
			{
				return AllowStats;
			}
			return false;
		}
	}

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	public override Rect GetScreenBounds()
	{
		return new Rect(BasePosition.x - Background.transform.localScale.x / 2f, BasePosition.y + Background.transform.localPosition.y, Background.transform.localScale.x, Background.transform.localScale.y);
	}

	private void OnDisable()
	{
		Target = null;
		SelectedCharacter = null;
		m_Health = null;
		m_Faction = null;
		m_BackerContent = null;
	}

	private void Awake()
	{
		m_RootTweens = GetComponent<UITweenerAggregator>();
		if ((bool)m_RootTweens)
		{
			UITweenerAggregator rootTweens = m_RootTweens;
			rootTweens.OnAllFinished = (UITweenerAggregator.OnFinished)Delegate.Combine(rootTweens.OnAllFinished, new UITweenerAggregator.OnFinished(OnTweenersFinished));
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Initialize()
	{
		if (!m_Init)
		{
			m_Init = true;
			WorldAnchor = GetComponent<UIAnchorToWorld>();
		}
	}

	private void Update()
	{
		if (!(Target == null))
		{
			UpdatePosition();
			RefreshDynamicContent();
		}
	}

	private void LateUpdate()
	{
		m_OpposerCount = 0;
		m_Opposition = 0f;
	}

	private void OnTweenersFinished(UITweenerAggregator tweener)
	{
		if (m_HideToPool)
		{
			UIMapTooltipManager.Instance.ReturnToPool(this);
		}
	}

	private void UpdatePosition()
	{
		if ((bool)PortraitAnchor && (bool)PortraitAnchor.widgetContainer)
		{
			PortraitAnchor.pixelOffset.y = PortraitAnchor.widgetContainer.transform.localScale.y / 2f + Background.transform.localScale.y / 2f;
		}
		if ((bool)WorldAnchor && WorldAnchor.enabled)
		{
			WorldAnchor.UpdatePosition();
			BasePosition = WorldAnchor.Position - new Vector2(0f, ContentStretcher.Bounds.min.y);
			base.transform.localPosition = new Vector3(base.ScreenPosition.x, base.ScreenPosition.y, base.transform.localPosition.z);
		}
	}

	public void Set(GameObject target)
	{
		if (!m_Init)
		{
			Initialize();
		}
		if (Target != target)
		{
			Reset();
		}
		if (target != Target)
		{
			Target = target;
			SelectedCharacter = Target.GetComponent<CharacterStats>();
			if (this.OnSelectedCharacterChanged != null)
			{
				this.OnSelectedCharacterChanged(SelectedCharacter);
			}
		}
		m_PartyAI = Target.GetComponent<PartyMemberAI>();
		m_Health = Target.GetComponent<Health>();
		m_Faction = Target.GetComponent<Faction>();
		m_BackerContent = Target.GetComponent<BackerContent>();
		if (PointerAnchor != null)
		{
			PointerAnchor.relativeOffset = new Vector2(0f, PointerAnchor.relativeOffset.y);
		}
		if (PortraitAnchor != null)
		{
			PortraitAnchor.relativeOffset = new Vector2(0f, PortraitAnchor.relativeOffset.y);
			PortraitAnchor.side = UIAnchor.Side.Center;
		}
		if (TargetIsParty && (bool)PortraitAnchor)
		{
			UIPartyPortrait portraitFor = UIPartyPortraitBar.Instance.GetPortraitFor(m_PartyAI);
			PortraitAnchor.widgetContainer = (portraitFor ? portraitFor.Border : null);
			PortraitAnchor.enabled = true;
			WorldAnchor.enabled = false;
			if (portraitFor.CurrentSlot == 0)
			{
				if (PointerAnchor != null)
				{
					PointerAnchor.relativeOffset = new Vector2(-0.44f, PointerAnchor.relativeOffset.y);
				}
				PortraitAnchor.relativeOffset = new Vector2(0.4f, PortraitAnchor.relativeOffset.y);
				PortraitAnchor.side = UIAnchor.Side.Right;
			}
			base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
		}
		else
		{
			if ((bool)PortraitAnchor)
			{
				PortraitAnchor.enabled = false;
			}
			if ((bool)WorldAnchor)
			{
				WorldAnchor.enabled = true;
				WorldAnchor.SetAnchor(target);
			}
			base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 1f);
		}
		RefreshDynamicContent();
	}

	public void Reset()
	{
		Target = null;
		SelectedCharacter = null;
		m_Health = null;
		m_Faction = null;
		m_BackerContent = null;
		if (this.OnSelectedCharacterChanged != null)
		{
			this.OnSelectedCharacterChanged(SelectedCharacter);
		}
		CorrectingOffset = Vector2.zero;
	}

	public void NotifyShown()
	{
		UpdatePosition();
		m_HideToPool = false;
		if ((bool)m_RootTweens)
		{
			m_RootTweens.Play(forward: true);
		}
	}

	public void Hide()
	{
		if ((bool)m_RootTweens)
		{
			if (!m_HideToPool)
			{
				m_HideToPool = true;
				m_RootTweens.Play(forward: false);
			}
		}
		else
		{
			UIMapTooltipManager.Instance.ReturnToPool(this);
		}
	}

	public void RefreshBackgroundColor()
	{
		if (!(Background == null))
		{
			Color color = Color.black;
			if ((bool)m_BackerContent)
			{
				color = UIMapTooltipManager.Instance.BackerColor;
			}
			else if ((bool)m_Faction)
			{
				color = ((m_Faction.RelationshipToPlayer == Faction.Relationship.Friendly) ? UIMapTooltipManager.Instance.FriendColor : ((m_Faction.RelationshipToPlayer != Faction.Relationship.Hostile) ? UIMapTooltipManager.Instance.NeutralColor : UIMapTooltipManager.Instance.FoeColor));
			}
			Background.color = color;
		}
	}

	private void RefreshDynamicContent()
	{
		Container component = Target.GetComponent<Container>();
		RefreshBackgroundColor();
		if (m_Health != null)
		{
			bool flag = false;
			if (!TargetIsParty && m_Health.Uninjured)
			{
				flag = true;
			}
			if (m_Health == null || flag)
			{
				HealthLabel.alpha = 0f;
			}
			else
			{
				HealthLabel.alpha = 1f;
			}
			if (HealthLabel.gameObject.activeInHierarchy)
			{
				if (TargetIsParty)
				{
					s_stringBuilder.Append(GUIUtils.GetText(1498, CharacterStats.GetGender(SelectedCharacter)));
					s_stringBuilder.Append(": ");
					s_stringBuilder.Append(InGameHUD.GetHealthColorString(m_Health.CurrentStamina, m_Health.MaxStamina));
					s_stringBuilder.Append(m_Health.CurrentStaminaString());
					s_stringBuilder.Append("[-]/");
					s_stringBuilder.Append(Mathf.CeilToInt(m_Health.MaxStamina).ToString("#0"));
					if (ShowHealth)
					{
						s_stringBuilder.AppendLine();
						s_stringBuilder.Append(GUIUtils.GetText(1469, CharacterStats.GetGender(SelectedCharacter)));
						s_stringBuilder.Append(": ");
						s_stringBuilder.Append(InGameHUD.GetHealthColorString(m_Health.CurrentHealth, m_Health.MaxHealth));
						s_stringBuilder.Append(m_Health.CurrentHealthString());
						s_stringBuilder.Append("[-]/");
						s_stringBuilder.Append(Mathf.CeilToInt(m_Health.MaxHealth).ToString("#0"));
					}
					HealthLabel.text = s_stringBuilder.ToString();
					s_stringBuilder.Remove(0, s_stringBuilder.Length);
				}
				else if (m_Health.HealthVisible)
				{
					HealthLabel.text = InGameHUD.GetHealthString(m_Health.CurrentStamina, m_Health.BaseMaxStamina, CharacterStats.GetGender(SelectedCharacter));
				}
				else
				{
					HealthLabel.text = "[888888]" + GUIUtils.GetText(1980) + "[-]";
				}
			}
		}
		else if (component != null && component.IsEmpty)
		{
			HealthLabel.alpha = 1f;
			s_stringBuilder.Append("[888888]");
			s_stringBuilder.Append(GUIUtils.GetText(262));
			s_stringBuilder.Append("[-]");
			HealthLabel.text = s_stringBuilder.ToString();
			s_stringBuilder.Remove(0, s_stringBuilder.Length);
		}
		else
		{
			HealthLabel.text = "";
			HealthLabel.alpha = 0f;
		}
		if ((bool)DtParent)
		{
			DtParent.SetActive(ShowDefenses);
		}
		if ((bool)DefenseParent)
		{
			DefenseParent.SetActive(ShowDefenses);
		}
		if ((bool)ImmunitiesParent)
		{
			ImmunitiesParent.ExternalActivation = ShowDefenses;
		}
		if ((bool)ResistancesParent)
		{
			ResistancesParent.ExternalActivation = ShowDefenses;
		}
		if ((bool)SubTop)
		{
			SubTop.widgetContainer = (((bool)RaceLabel && !string.IsNullOrEmpty(RaceLabel.text)) ? RaceLabel : NameLabel);
		}
		Divider.alpha = ((HealthLabel.alpha > 0f || ShowDefenses) ? 1 : 0);
		if ((bool)Table)
		{
			Table.Reposition();
		}
		Panel.Refresh();
	}

	public void ProcessOpposer(AIController ai)
	{
		AttackBase attackBase = ai.StateManager.GetCurrentAttack();
		if (attackBase == null)
		{
			Equipment component = ai.GetComponent<Equipment>();
			if (component != null)
			{
				attackBase = component.PrimaryAttack;
			}
		}
		if (!(attackBase != null) || attackBase.DefendedBy >= CharacterStats.DefenseType.Count)
		{
			return;
		}
		CharacterStats component2 = ai.GetComponent<CharacterStats>();
		float num = component2.CalculateAccuracy(attackBase, Target);
		m_Opposition = ((float)m_OpposerCount * m_Opposition + num) / (float)(m_OpposerCount + 1);
		m_OpposerCount++;
		ProcessDamThresh(attackBase.DamageData.Type, attackBase.DamageData.AverageBaseDamage(component2) * attackBase.DamageMultiplier);
		foreach (DamagePacket.DamageProcType item in attackBase.DamageData.DamageProc)
		{
			ProcessDamThresh(item.Type, item.PercentOfBaseDamage / 100f * attackBase.DamageData.AverageBaseDamage(component2) * attackBase.DamageMultiplier);
		}
	}

	private void ProcessDamThresh(DamagePacket.DamageType damtype, float dam)
	{
		_ = 7;
	}
}
