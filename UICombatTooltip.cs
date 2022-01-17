using UnityEngine;

public class UICombatTooltip : MonoBehaviour, ISelectACharacter
{
	public enum FadeState
	{
		None,
		In,
		Out,
		Full
	}

	private GameObject m_Target;

	private CharacterStats m_Stats;

	private Health m_Health;

	private Faction m_Faction;

	private PartyMemberAI m_PartyMember;

	public UISprite[] HealthSprites = new UISprite[5];

	public UICharacterActionIcon ActionIcon;

	public UIPanel ActionPanel;

	public UIPanel Panel;

	private Vector3 m_ActionPanelRootPosition;

	private UIAnchorToWorld WorldAnchor;

	private bool m_Init;

	private FadeState m_fadeState = FadeState.Full;

	private float m_currentAlpha = 1f;

	private float m_alphaTimer;

	private bool m_isSelected;

	private FadeState m_selectionState;

	private float m_selectionAlpha;

	private float m_selectionTimer;

	[HideInInspector]
	public bool activeThisFrame;

	private bool m_fadeToKill;

	public static float MasterFadeAlpha;

	private const float FADE_TIME = 0.3f;

	private const float SELECTION_TIME = 0.2f;

	public CharacterStats SelectedCharacter => m_Stats;

	public GameObject Target => m_Target;

	public bool IsPartyMember => m_PartyMember != null;

	public Health TargetHealth => m_Health;

	public Faction TargetFaction => m_Faction;

	public PartyMemberAI TargetPartyMember => m_PartyMember;

	public bool IsSelected
	{
		get
		{
			return m_isSelected;
		}
		set
		{
			m_isSelected = value;
			if (value)
			{
				if (m_selectionState != FadeState.In && m_selectionState != FadeState.Full)
				{
					m_selectionState = FadeState.In;
					m_selectionTimer = 0.2f;
				}
			}
			else if (m_selectionState != FadeState.Out && m_selectionState != 0)
			{
				m_selectionState = FadeState.Out;
				m_selectionTimer = 0.2f;
			}
		}
	}

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

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
			m_ActionPanelRootPosition = ActionPanel.transform.localPosition;
		}
	}

	private void Update()
	{
		if (!m_Faction || !m_Health || m_Health.ShowDead)
		{
			FadeOut();
		}
		else if (GameCursor.CharacterUnderCursor == m_Faction.gameObject || ((bool)m_PartyMember && m_PartyMember.Selected) || InGameHUD.Instance.HighlightActive)
		{
			FadeIn();
		}
		else
		{
			FadeOutNoKill();
		}
		UpdateFade();
		Panel.alpha = m_currentAlpha * (m_selectionAlpha + 0.5f) * MasterFadeAlpha;
		if (GameState.InCombat && ActionIcon.IsInteresting && (bool)TargetFaction && TargetFaction.isFowVisible && InGameHUD.Instance.ShowHUD)
		{
			ActionPanel.alpha = 1f;
		}
		else
		{
			ActionPanel.alpha = Panel.alpha;
		}
		if (Panel.alpha == 0f)
		{
			ActionPanel.transform.localPosition = Vector3.zero;
		}
		else
		{
			ActionPanel.transform.localPosition = m_ActionPanelRootPosition;
		}
		if (m_Target == null)
		{
			ActionPanel.alpha = Panel.alpha;
			return;
		}
		RefreshDynamicContent();
		WorldAnchor.UpdatePosition();
		base.transform.localPosition = WorldAnchor.Position;
	}

	private void LateUpdate()
	{
		activeThisFrame = false;
	}

	public void Set(GameObject target)
	{
		if (!m_Init)
		{
			Initialize();
		}
		Reset();
		m_Target = target;
		m_Health = m_Target.GetComponent<Health>();
		m_Stats = m_Target.GetComponent<CharacterStats>();
		m_Faction = m_Target.GetComponent<Faction>();
		m_PartyMember = m_Target.GetComponent<PartyMemberAI>();
		if (this.OnSelectedCharacterChanged != null)
		{
			this.OnSelectedCharacterChanged(m_Stats);
		}
		WorldAnchor.SetAnchor(target);
		RefreshContent();
	}

	public void Reset()
	{
		m_Target = null;
		m_Stats = null;
		m_Health = null;
		m_Faction = null;
		ActionPanel.alpha = 0f;
		if (this.OnSelectedCharacterChanged != null)
		{
			this.OnSelectedCharacterChanged(m_Stats);
		}
	}

	public void RefreshContent()
	{
		RefreshDynamicContent();
	}

	private void RefreshDynamicContent()
	{
		Color color = InGameHUD.GetFriendlyColor();
		if ((bool)m_Faction && m_Faction.RelationshipToPlayer == Faction.Relationship.Hostile)
		{
			color = InGameHUD.GetFoeColor();
		}
		if (m_Health != null)
		{
			int num = HealthSprites.Length - InGameHUD.GetHealthStage(m_Health.CurrentStamina, m_Health.MaxStamina);
			for (int i = 0; i < HealthSprites.Length; i++)
			{
				if (HealthSprites[i] != null)
				{
					if (i < num && TargetHealth.HealthVisible)
					{
						HealthSprites[i].alpha = 1f;
						HealthSprites[i].color = color;
					}
					else
					{
						HealthSprites[i].alpha = 0f;
					}
				}
			}
		}
		Panel.Refresh();
	}

	private void UpdateFade()
	{
		if (m_alphaTimer > 0f)
		{
			m_alphaTimer -= Time.unscaledDeltaTime;
		}
		if (m_selectionTimer > 0f)
		{
			m_selectionTimer -= Time.unscaledDeltaTime;
		}
		switch (m_fadeState)
		{
		case FadeState.In:
			if (m_alphaTimer <= 0f)
			{
				m_fadeState = FadeState.Full;
				m_currentAlpha = 1f;
			}
			else
			{
				m_currentAlpha = Mathf.SmoothStep(0f, 1f, 1f - m_alphaTimer / 0.3f);
			}
			break;
		case FadeState.Out:
			if (m_alphaTimer <= 0f)
			{
				m_fadeState = FadeState.None;
				m_currentAlpha = 0f;
				if (m_fadeToKill)
				{
					UICombatTooltipManager.Instance.Remove(m_Target);
				}
			}
			else
			{
				m_currentAlpha = Mathf.SmoothStep(0f, 1f, m_alphaTimer / 0.3f);
			}
			break;
		}
		switch (m_selectionState)
		{
		case FadeState.In:
			if (m_selectionTimer < 0f)
			{
				m_selectionState = FadeState.Full;
				m_selectionAlpha = 0.5f;
			}
			else
			{
				m_selectionAlpha = Mathf.SmoothStep(0f, 0.5f, 1f - m_selectionTimer / 0.2f);
			}
			break;
		case FadeState.Out:
			if (m_selectionTimer < 0f)
			{
				m_selectionState = FadeState.None;
				m_selectionAlpha = 0f;
			}
			else
			{
				m_selectionAlpha = Mathf.SmoothStep(0f, 0.5f, m_selectionTimer / 0.2f);
			}
			break;
		case FadeState.None:
		case FadeState.Full:
			break;
		}
	}

	public void FadeIn()
	{
		if (m_fadeState != FadeState.In && m_fadeState != FadeState.Full)
		{
			m_fadeToKill = false;
			m_fadeState = FadeState.In;
			m_alphaTimer = 0.3f;
		}
	}

	public void FadeOut()
	{
		if (m_fadeState != FadeState.Out && m_fadeState != 0)
		{
			m_fadeToKill = true;
			m_fadeState = FadeState.Out;
			m_alphaTimer = 0.3f;
		}
	}

	public void FadeOutNoKill()
	{
		if (m_fadeState != FadeState.Out && m_fadeState != 0)
		{
			m_fadeToKill = false;
			m_fadeState = FadeState.Out;
			m_alphaTimer = 0.3f;
		}
	}
}
