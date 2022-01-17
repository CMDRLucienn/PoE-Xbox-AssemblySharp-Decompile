using AI.Achievement;
using AI.Plan;
using AI.Player;
using UnityEngine;

public class UICharacterActionIcon : UIParentSelectorListener
{
	public enum ActionIconType
	{
		None,
		Moving,
		Attacking,
		Casting,
		Reloading,
		Idle
	}

	private AIController m_AI;

	private CharacterStats m_Stats;

	public UISprite ActionIcon;

	public UITexture AbilityIcon;

	private UIClippedTexture m_ClippedAbilityTexture;

	private UITooltipContentTrigger m_AbilityTooltipTrigger;

	private bool m_Visible = true;

	public bool IsInteresting { get; private set; }

	public bool Visible
	{
		get
		{
			return m_Visible;
		}
		set
		{
			m_Visible = value;
			AbilityIcon.gameObject.SetActive(Visible);
			ActionIcon.gameObject.SetActive(Visible);
		}
	}

	private void Awake()
	{
		m_ClippedAbilityTexture = AbilityIcon.GetComponent<UIClippedTexture>();
		m_AbilityTooltipTrigger = AbilityIcon.GetComponent<UITooltipContentTrigger>();
	}

	private void Update()
	{
		UpdateIcon();
	}

	private void UpdateIcon()
	{
		IsInteresting = false;
		if (!(m_AI != null) || m_AI.StateManager == null)
		{
			return;
		}
		AIState aIState = m_AI.StateManager.CurrentState;
		if (aIState is HitReact || aIState is PathToPosition)
		{
			aIState = m_AI.StateManager.QueuedState;
		}
		if (aIState == null)
		{
			return;
		}
		if (aIState is AI.Achievement.Attack || aIState is AI.Player.Attack || aIState is TargetedAttack || aIState is ApproachTarget || UsePotionIcon())
		{
			GenericAbility genericAbility = aIState.CurrentAbility;
			if (genericAbility == null)
			{
				genericAbility = GetQueuedAbility();
			}
			if ((bool)genericAbility)
			{
				SetActionIcon(ActionIconType.Casting);
				SetAbilityIcon(genericAbility);
				IsInteresting = true;
			}
			else if (!UsePotionIcon())
			{
				SetActionIcon(ActionIconType.Attacking);
				SetAbilityIcon();
			}
		}
		else if (aIState is PathToPosition || aIState is Move)
		{
			SetActionIcon(ActionIconType.Moving);
			SetAbilityIcon();
		}
		else if (aIState is AI.Achievement.ReloadWeapon || aIState is AI.Player.ReloadWeapon)
		{
			SetActionIcon(ActionIconType.Reloading);
			SetAbilityIcon();
		}
		else if (aIState is Paralyzed)
		{
			Texture2D texture2D = null;
			texture2D = ((!m_Stats.HasStatusEffectFromAffliction(AfflictionData.Paralyzed)) ? AfflictionData.Instance.PetrifiedPrefab.Icon : AfflictionData.Instance.ParalyzedPrefab.Icon);
			if (texture2D != null)
			{
				SetActionIcon(ActionIconType.None);
				SetAbilityIcon(texture2D);
				IsInteresting = true;
			}
		}
		else if (aIState is Stunned)
		{
			SetActionIcon(ActionIconType.None);
			SetAbilityIcon(AfflictionData.Stunned.Icon);
			IsInteresting = true;
		}
		else if (aIState is Idle || aIState is AI.Player.Wait || aIState is PerformAction)
		{
			SetActionIcon(ActionIconType.Idle);
			SetAbilityIcon();
			IsInteresting = m_AI is PartyMemberAI && !GameState.IsCombatWaitingToEnd;
		}
		else
		{
			SetActionIcon(ActionIconType.None);
			SetAbilityIcon();
		}
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		m_Stats = stats;
		m_AI = (stats ? GameUtilities.FindActiveAIController(stats.gameObject) : null);
		UpdateIcon();
	}

	private bool UsePotionIcon()
	{
		if (m_AI != null)
		{
			if (m_AI.StateManager.CurrentState is ConsumePotion)
			{
				return true;
			}
			GenericAbility queuedAbility = GetQueuedAbility();
			if (queuedAbility != null)
			{
				Consumable component = queuedAbility.GetComponent<Consumable>();
				if (component != null && component.IsFoodDrugOrPotion)
				{
					return true;
				}
			}
		}
		return false;
	}

	private GenericAbility GetQueuedAbility()
	{
		PartyMemberAI partyMemberAI = m_AI as PartyMemberAI;
		if (partyMemberAI != null)
		{
			return partyMemberAI.QueuedAbility;
		}
		return null;
	}

	private void SetAbilityIcon()
	{
		SetAbilityIcon((Texture2D)null);
	}

	private void SetAbilityIcon(GenericAbility ability)
	{
		if (!ability || !ability.Icon)
		{
			AbilityIcon.gameObject.SetActive(value: false);
			return;
		}
		SetActionIcon(ActionIconType.None);
		AbilityIcon.gameObject.SetActive(Visible);
		if (AbilityIcon.mainTexture != ability.Icon)
		{
			if (UICombatTooltipManager.Instance.ShowAbilityTooltips)
			{
				m_AbilityTooltipTrigger.Set(ability, ability);
			}
			AbilityIcon.mainTexture = ability.Icon;
			m_ClippedAbilityTexture.OnTextureChanged();
		}
	}

	private void SetAbilityIcon(Texture2D icon)
	{
		if (!icon)
		{
			AbilityIcon.gameObject.SetActive(value: false);
			return;
		}
		AbilityIcon.gameObject.SetActive(Visible);
		if (AbilityIcon.mainTexture != icon)
		{
			m_AbilityTooltipTrigger.Clear();
			AbilityIcon.mainTexture = icon;
			m_ClippedAbilityTexture.OnTextureChanged();
		}
	}

	public void SetActionIcon(ActionIconType actionType)
	{
		ActionIcon.gameObject.SetActive(Visible);
		switch (actionType)
		{
		case ActionIconType.Idle:
			ActionIcon.spriteName = "ICO_field_idle";
			break;
		case ActionIconType.Attacking:
			ActionIcon.spriteName = "ICO_field_attack";
			break;
		case ActionIconType.Moving:
			ActionIcon.spriteName = "ICO_field_path";
			break;
		case ActionIconType.Casting:
			ActionIcon.spriteName = "ICO_field_ability";
			break;
		case ActionIconType.Reloading:
			ActionIcon.spriteName = "ICO_field_reload";
			break;
		default:
			ActionIcon.spriteName = "ICO_field_empty";
			break;
		}
	}
}
