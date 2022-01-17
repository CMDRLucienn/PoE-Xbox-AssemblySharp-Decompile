using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour
{
	public enum Relationship
	{
		Neutral,
		Hostile,
		Friendly
	}

	[HideInInspector]
	public bool MousedOver;

	[HideInInspector]
	public float MousedOverTime;

	public Team CurrentTeam;

	private Team m_myTeam;

	private Team m_originalTeam;

	private bool m_unitHostileToPlayer;

	[Tooltip("If unchecked, the character will never draw a selection circle unless speaking.")]
	public bool DrawSelectionCircle = true;

	[Tooltip("If unchecked, the character will never draw a selection circle while speaking.")]
	public bool DrawConversationCircle = true;

	public bool ShowTooltips = true;

	[Tooltip("Used to determine the offset of tooltips for this character. If the value is <= 0, the max height of the character's Renderers will be used. This can usually be left at 0.")]
	public float TooltipHeightOverride;

	[Tooltip("Radius of fog-of-war revealed when this character is speaking. Default 5.")]
	public float SpeakFowRadius = 5f;

	public static List<Faction> ActiveFactionComponents = new List<Faction>();

	protected SelectionCircle m_selectionCircle;

	protected Health healthComponent;

	protected PartyMemberAI partyMemberAIComponent;

	protected AIPackageController packageController;

	protected Mover moverComponent;

	private FogOfWar.Revealer m_Revealer;

	protected static Team s_playerTeam = null;

	protected bool m_needsMurderedPenalty;

	public Conversation CurrentConversation { get; private set; }

	public static Team PlayerTeam => s_playerTeam;

	public bool isPartyMember
	{
		get
		{
			if ((bool)partyMemberAIComponent)
			{
				return partyMemberAIComponent.enabled;
			}
			return false;
		}
	}

	public bool isFowVisible
	{
		get
		{
			if ((bool)packageController)
			{
				return packageController.IsFogVisible;
			}
			if (FogOfWar.Instance != null)
			{
				return FogOfWar.Instance.PointVisible(base.transform.position);
			}
			return true;
		}
	}

	public bool CanShowTooltip
	{
		get
		{
			if (ShowTooltips && (!healthComponent || !healthComponent.ShowDead))
			{
				return isFowVisible;
			}
			return false;
		}
	}

	public Color SelectionColor
	{
		get
		{
			if (!m_selectionCircle)
			{
				return Color.clear;
			}
			return m_selectionCircle.GetSelectedColor();
		}
	}

	public SelectionCircle SelectionCircle => m_selectionCircle;

	public Mover Mover => moverComponent;

	public bool CanBeTargeted
	{
		get
		{
			if (!packageController || !packageController.IsBusy)
			{
				if ((bool)healthComponent && !healthComponent.CanBeTargeted && (bool)GameState.s_playerCharacter)
				{
					return !GameState.s_playerCharacter.IsInForceAttackMode;
				}
				return true;
			}
			return false;
		}
	}

	public bool UnitHostileToPlayer
	{
		get
		{
			return m_unitHostileToPlayer;
		}
		set
		{
			m_unitHostileToPlayer = value;
		}
	}

	[Persistent]
	public Team CurrentTeamInstance
	{
		get
		{
			if (CurrentTeam != null && m_myTeam == null)
			{
				CurrentTeam.Register();
				m_myTeam = Team.GetTeamByTag(CurrentTeam.ScriptTag);
				CurrentTeam = m_myTeam;
			}
			return m_myTeam;
		}
		set
		{
			if (value == null)
			{
				m_myTeam = null;
				CurrentTeam = null;
			}
			else
			{
				value.Register();
				m_myTeam = Team.GetTeamByTag(value.ScriptTag);
				CurrentTeam = m_myTeam;
			}
			partyMemberAIComponent = GetComponent<PartyMemberAI>();
			packageController = GetComponent<AIPackageController>();
		}
	}

	public Team OriginalTeamInstance => m_originalTeam;

	public bool IsInPlayerFaction
	{
		get
		{
			if (s_playerTeam == null)
			{
				s_playerTeam = Team.GetTeamByTag("player");
			}
			if (m_myTeam == null || s_playerTeam == null)
			{
				return false;
			}
			return m_myTeam.ScriptTag == s_playerTeam.ScriptTag;
		}
	}

	public Relationship RelationshipToPlayer
	{
		get
		{
			if (m_unitHostileToPlayer)
			{
				return Relationship.Hostile;
			}
			if (CurrentTeamInstance == null)
			{
				return Relationship.Neutral;
			}
			if (IsInPlayerFaction)
			{
				return Relationship.Friendly;
			}
			return CurrentTeamInstance.GetRelationship(s_playerTeam);
		}
		set
		{
			if (s_playerTeam == null)
			{
				s_playerTeam = Team.GetTeamByTag("player");
			}
			if (!(s_playerTeam == null))
			{
				if (CurrentTeamInstance == null)
				{
					Debug.LogError(base.name + " doesn't have a team assigned but is being told to change relationships with the player!", base.gameObject);
				}
				else
				{
					CurrentTeamInstance.SetRelationship(s_playerTeam, value, mutual: true);
				}
			}
		}
	}

	public Reputation Reputation
	{
		get
		{
			if (CurrentTeamInstance == null)
			{
				return null;
			}
			return ReputationManager.Instance.GetReputation(CurrentTeamInstance.GameFaction);
		}
	}

	public float CachedRadius
	{
		get
		{
			if (moverComponent == null && base.gameObject != null)
			{
				moverComponent = GetComponent<Mover>();
			}
			if (moverComponent != null)
			{
				return moverComponent.Radius;
			}
			return 0f;
		}
	}

	public event SelectionCircle.SharedMaterialChanged OnSelectionCircleMaterialChanged;

	public static void ClearPlayerTeam()
	{
		s_playerTeam = null;
	}

	public static bool IsFowVisible(GameObject go)
	{
		Faction component = go.GetComponent<Faction>();
		if ((bool)component)
		{
			return component.isFowVisible;
		}
		return false;
	}

	public static bool IsFowVisible(MonoBehaviour mb)
	{
		return IsFowVisible(mb.gameObject);
	}

	private void Start()
	{
		healthComponent = GetComponent<Health>();
		partyMemberAIComponent = GetComponent<PartyMemberAI>();
		packageController = GetComponent<AIPackageController>();
		moverComponent = GetComponent<Mover>();
		if (!GetComponent<HighlightCharacter>() && !GetComponent<Encounter>())
		{
			base.gameObject.AddComponent<HighlightCharacter>();
		}
		if (CurrentTeam != null)
		{
			CurrentTeam.Register();
			m_myTeam = Team.GetTeamByTag(CurrentTeam.ScriptTag);
			CurrentTeam = m_myTeam;
			m_originalTeam = CurrentTeam;
		}
		if (s_playerTeam == null)
		{
			s_playerTeam = Team.GetTeamByTag("player");
		}
		if ((bool)healthComponent)
		{
			healthComponent.OnDeath += healthComponent_OnDeath;
		}
		CheckInstantiateCircle();
	}

	private void healthComponent_OnDeath(GameObject myObject, GameEventArgs args)
	{
		if (m_needsMurderedPenalty)
		{
			ReputationManager.Instance.AddReputation(CurrentTeamInstance.GameFaction, Reputation.Axis.Negative, CurrentTeamInstance.MurderedReputationChange);
		}
	}

	public void NotifyAttackWitnessed()
	{
		if (CurrentTeamInstance == null)
		{
			return;
		}
		if (CurrentTeamInstance.GetRelationship(s_playerTeam) == Relationship.Hostile)
		{
			if (CurrentTeamInstance.GameFaction != 0)
			{
				m_needsMurderedPenalty = true;
			}
			return;
		}
		RelationshipToPlayer = Relationship.Hostile;
		if (!m_needsMurderedPenalty && CurrentTeamInstance.GameFaction != 0)
		{
			ReputationManager.Instance.AddReputation(CurrentTeamInstance.GameFaction, Reputation.Axis.Negative, CurrentTeamInstance.InjuredReputationChange);
			m_needsMurderedPenalty = true;
		}
	}

	public void OnDestroy()
	{
		if (m_Revealer != null && (bool)FogOfWar.Instance)
		{
			FogOfWar.Instance.RemoveRevealer(m_Revealer);
		}
		ActiveFactionComponents.Remove(this);
		if ((bool)m_selectionCircle)
		{
			m_selectionCircle.OnSharedMaterialChanged -= OnSharedMaterialChanged;
			GameUtilities.Destroy(m_selectionCircle.gameObject);
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void OnEnable()
	{
		ActiveFactionComponents.Add(this);
	}

	public void OnDisable()
	{
		ActiveFactionComponents.Remove(this);
		if ((bool)m_selectionCircle)
		{
			GameUtilities.Destroy(m_selectionCircle.gameObject);
		}
		m_selectionCircle = null;
	}

	public void TargetSelectionCircle()
	{
		if (m_selectionCircle != null && ShowSelectionCircle(elevate: true))
		{
			m_selectionCircle.CurrentMode = SelectionCircle.Mode.Targeted;
		}
	}

	public bool ShowSelectionCircle(bool elevate)
	{
		if (InGameHUD.Instance == null)
		{
			return false;
		}
		bool num = DrawSelectionCircle && InGameHUD.Instance.ShowHUD;
		bool flag = healthComponent == null || !healthComponent.ShowDead || GameInput.SelectDead;
		bool flag2 = !GameState.Option.GetOption(GameOption.BoolOption.HIDE_CIRCLES) || GameState.Paused;
		bool flag3 = RelationshipToPlayer == Relationship.Hostile || isPartyMember || elevate;
		if (num && flag && isFowVisible)
		{
			if (!(flag2 && flag3) && !MousedOver)
			{
				return InGameHUD.Instance.HighlightActive;
			}
			return true;
		}
		return false;
	}

	private void SetSelectionCircleColor()
	{
		if (!m_selectionCircle || !m_selectionCircle.IsVisible())
		{
			return;
		}
		bool isFoe = !IsInPlayerFaction && RelationshipToPlayer == Relationship.Hostile;
		bool isSelected = false;
		AIController aIController = GameUtilities.FindActiveAIController(base.gameObject);
		bool isDominated = (bool)aIController && aIController.IsFactionSwapped();
		if (IsInPlayerFaction)
		{
			PartyMemberAI partyMemberAI = aIController as PartyMemberAI;
			HighlightCharacter component = GetComponent<HighlightCharacter>();
			isSelected = ((bool)partyMemberAI && partyMemberAI.Selected) || ((bool)component && component.ShouldHighlight);
			if ((bool)component && component.LassoDeselected)
			{
				isSelected = false;
			}
		}
		Stealth component2 = GetComponent<Stealth>();
		bool isStealthed = (bool)component2 && component2.IsInStealthMode();
		m_selectionCircle.SetMaterial(isFoe, isSelected, isStealthed, isDominated);
	}

	public void Update()
	{
		if (MousedOver)
		{
			MousedOverTime += TimeController.sUnscaledDelta;
		}
		MousedOver = GameCursor.CharacterUnderCursor == base.gameObject;
		if (!MousedOver)
		{
			MousedOverTime = 0f;
		}
		if (MousedOver && GameInput.GetControlDown(MappedControl.SELECT) && RelationshipToPlayer == Relationship.Neutral)
		{
			TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.CLICK_FRIENDLY);
		}
		if (m_Revealer != null)
		{
			m_Revealer.WorldPos = base.transform.position;
			m_Revealer.RequiresRefresh = false;
		}
		if (ShowSelectionCircle(elevate: false))
		{
			CheckInstantiateCircle();
			if ((bool)m_selectionCircle && m_selectionCircle.CurrentMode == SelectionCircle.Mode.Hidden)
			{
				m_selectionCircle.CurrentMode = SelectionCircle.Mode.Normal;
			}
		}
		else if ((bool)m_selectionCircle && m_selectionCircle.CurrentMode == SelectionCircle.Mode.Normal)
		{
			m_selectionCircle.CurrentMode = SelectionCircle.Mode.Hidden;
		}
		SetSelectionCircleColor();
	}

	public void TryHideRevealer(Conversation conversation)
	{
		if (conversation == CurrentConversation && m_Revealer != null)
		{
			if ((bool)FogOfWar.Instance)
			{
				FogOfWar.Instance.RemoveRevealer(m_Revealer);
			}
			m_Revealer = null;
		}
	}

	public void NotifyBeginSpeaking(Conversation conversation, bool state)
	{
		CheckInstantiateCircle();
		if ((bool)m_selectionCircle)
		{
			m_selectionCircle.CurrentMode = (state ? SelectionCircle.Mode.Acting : SelectionCircle.Mode.Normal);
		}
		if ((bool)FogOfWar.Instance)
		{
			if (state && m_Revealer == null)
			{
				m_Revealer = FogOfWar.Instance.AddRevealer(triggersBoxColliders: false, SpeakFowRadius, base.gameObject.transform.position, null, revealOnly: false, respectLOS: true);
				m_Revealer.RequiresRefresh = true;
				CurrentConversation = conversation;
			}
			else if (!state && m_Revealer != null && CurrentConversation == null)
			{
				FogOfWar.Instance.RemoveRevealer(m_Revealer);
				m_Revealer = null;
			}
		}
	}

	private void CheckInstantiateCircle()
	{
		if (m_selectionCircle == null && (bool)InGameHUD.Instance)
		{
			m_selectionCircle = Object.Instantiate(InGameHUD.Instance.SelectionCircle, base.transform.position, base.transform.rotation).GetComponent<SelectionCircle>();
			m_selectionCircle.SetOwner(base.gameObject);
			m_selectionCircle.OnSharedMaterialChanged += OnSharedMaterialChanged;
			float num = base.transform.localScale.x;
			if (num <= 0f)
			{
				num = 1f;
			}
			Mover component = GetComponent<Mover>();
			if ((bool)component)
			{
				m_selectionCircle.SetRootScale(component.Radius * 2f / num);
			}
			else
			{
				m_selectionCircle.SetRootScale(1f);
			}
		}
	}

	private void OnSharedMaterialChanged(Material mat)
	{
		if (this.OnSelectionCircleMaterialChanged != null)
		{
			this.OnSelectionCircleMaterialChanged(mat);
		}
	}

	public Relationship GetRelationship(GameObject creature)
	{
		Faction component = ComponentUtils.GetComponent<Faction>(creature);
		return GetRelationship(component);
	}

	public virtual Relationship GetRelationship(Faction creatureFaction)
	{
		if (creatureFaction == null || m_myTeam == null)
		{
			return Relationship.Neutral;
		}
		if (creatureFaction.IsInPlayerFaction && m_unitHostileToPlayer)
		{
			return Relationship.Hostile;
		}
		return m_myTeam.GetRelationship(creatureFaction.m_myTeam);
	}

	public virtual bool IsHostile(GameObject creature)
	{
		return GetRelationship(creature) == Relationship.Hostile;
	}

	public virtual bool IsHostile(Faction creatureFaction)
	{
		return GetRelationship(creatureFaction) == Relationship.Hostile;
	}

	public virtual bool IsFriendly(GameObject creature)
	{
		return GetRelationship(creature) == Relationship.Friendly;
	}

	public virtual bool IsFriendly(Faction creatureFaction)
	{
		return GetRelationship(creatureFaction) == Relationship.Friendly;
	}

	public virtual void ModifyToMatch(Faction newSettings)
	{
		if (newSettings == null)
		{
			return;
		}
		if (newSettings != null && newSettings.CurrentTeamInstance != null)
		{
			Team teamByTag = Team.GetTeamByTag(newSettings.CurrentTeamInstance.ScriptTag);
			if (teamByTag == null)
			{
				newSettings.CurrentTeamInstance.Register();
				teamByTag = Team.GetTeamByTag(newSettings.CurrentTeamInstance.ScriptTag);
			}
			CurrentTeamInstance = teamByTag;
		}
		else if (newSettings.CurrentTeam != null)
		{
			Team teamByTag2 = Team.GetTeamByTag(newSettings.CurrentTeam.ScriptTag);
			if (teamByTag2 == null)
			{
				newSettings.CurrentTeam.Register();
				teamByTag2 = Team.GetTeamByTag(newSettings.CurrentTeam.ScriptTag);
			}
			CurrentTeamInstance = teamByTag2;
		}
		else
		{
			CurrentTeamInstance = null;
		}
	}

	private void OnDrawGizmos()
	{
		if (TooltipHeightOverride > 0f)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube(base.transform.position + Vector3.up * TooltipHeightOverride, new Vector3(2f, 0f, 2f));
		}
	}
}
