using System;
using System.Collections.Generic;
using AI.Player;
using UnityEngine;

public class SceneTransition : Usable
{
	public MapType LinkedScene;

	public string LinkedMapTag;

	public MapType LinkedSceneForUi;

	public bool ShowSceneName;

	public Texture2D DisplayIcon;

	public Texture2D DisplayIconActive;

	public const float SceneTransitionDistance = 10f;

	public float SFXFadeOutTime = 1f;

	public string[] sfx_to_ignore_fade;

	private UITriggerIcon m_ActiveIcon;

	[Range(-4f, 4f)]
	public float IconXOffset;

	[Range(-4f, 4f)]
	public float IconYOffset;

	[Range(-4f, 4f)]
	public float IconZOffset;

	public StartPoint.PointLocation StartPointLink;

	public string StartPointOverride = string.Empty;

	public GameCursor.CursorType MouseOverCursor = GameCursor.CursorType.Normal;

	public Transform[] PartyInteractionPoints = new Transform[6];

	private GameObject[] m_partyTargetMarkers;

	private PE_Collider2D m_2dCollider;

	private bool m_mouseOver;

	private bool m_safetyShow;

	private List<GameObject> m_partyWaitList = new List<GameObject>();

	public PartyWaypoint SameMapTransitionPoint;

	public bool ShowPartyManager;

	[Tooltip("If true, perform a 3D distance check when determining if within range. Only check this if object position is significantly above the ground plane.")]
	public bool Use3DDistanceCheck;

	private bool m_isActive;

	private bool m_playedTransitionSound;

	public static List<SceneTransition> s_activeSceneTransitionsList = new List<SceneTransition>();

	private bool m_UsedPartyManager;

	public override float UsableRadius => 0.1f;

	public override float ArrivalRadius => 0f;

	public override bool IsUsable => base.IsRevealed;

	private void OnEnable()
	{
		if (InGameHUD.Instance != null)
		{
			InGameHUD instance = InGameHUD.Instance;
			instance.OnHighlightBegin = (InGameHUD.HighlightEvent)Delegate.Combine(instance.OnHighlightBegin, new InGameHUD.HighlightEvent(RefreshIcon));
			InGameHUD instance2 = InGameHUD.Instance;
			instance2.OnHighlightEnd = (InGameHUD.HighlightEvent)Delegate.Combine(instance2.OnHighlightEnd, new InGameHUD.HighlightEvent(RefreshIcon));
		}
		RefreshIcon();
		if (!s_activeSceneTransitionsList.Contains(this))
		{
			s_activeSceneTransitionsList.Add(this);
		}
	}

	private void OnDisable()
	{
		if (InGameHUD.Instance != null)
		{
			InGameHUD instance = InGameHUD.Instance;
			instance.OnHighlightBegin = (InGameHUD.HighlightEvent)Delegate.Remove(instance.OnHighlightBegin, new InGameHUD.HighlightEvent(RefreshIcon));
			InGameHUD instance2 = InGameHUD.Instance;
			instance2.OnHighlightEnd = (InGameHUD.HighlightEvent)Delegate.Remove(instance2.OnHighlightEnd, new InGameHUD.HighlightEvent(RefreshIcon));
		}
		RefreshIcon(onDisable: true);
		s_activeSceneTransitionsList.Remove(this);
	}

	protected override void Start()
	{
		base.Start();
		if (string.IsNullOrEmpty(LinkedMapTag))
		{
			LinkedMapTag = "world";
		}
		m_2dCollider = GetComponent<PE_Collider2D>();
	}

	private void Update()
	{
		if ((bool)GetComponent<Collider>())
		{
			m_safetyShow = UIScreenEdgeBlocker.UiDoesOverlap(GetComponent<Collider>());
			RefreshIcon();
		}
		if (m_partyWaitList.Count > 0)
		{
			RefreshIcon();
			if (GameState.InCombat)
			{
				Console.AddMessage(GUIUtils.GetTextWithLinks(992));
				CancelTransition();
				return;
			}
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < PartyMemberAI.PartyMembers.Length; i++)
			{
				if (!(PartyMemberAI.PartyMembers[i] != null))
				{
					continue;
				}
				Health component = PartyMemberAI.PartyMembers[i].GetComponent<Health>();
				if (component == null || component.ShowDead)
				{
					flag = true;
				}
				if (!flag2)
				{
					if (PartyMemberAI.PartyMembers[i].StateManager.FindState(typeof(UseObject)) is UseObject useObject && useObject.UsableObject == this)
					{
						flag2 = true;
					}
					if (PartyMemberAI.PartyMembers[i].StateManager.FindState(typeof(WaitForSceneTransition)) is WaitForSceneTransition waitForSceneTransition && waitForSceneTransition.TransitionObject == this)
					{
						flag2 = true;
					}
				}
				Vector3 markerPosition = GetMarkerPosition(PartyMemberAI.PartyMembers[i], reverse: false);
				float num3 = 0f;
				num3 = ((!Use3DDistanceCheck) ? GameUtilities.V3SqrDistance2D(PartyMemberAI.PartyMembers[i].transform.position, markerPosition) : (PartyMemberAI.PartyMembers[i].transform.position - markerPosition).sqrMagnitude);
				if (num3 < 100f)
				{
					num++;
				}
				num2++;
			}
			if (!flag2)
			{
				flag = true;
			}
			for (int j = 0; j < m_partyWaitList.Count; j++)
			{
				if (m_partyWaitList[j] == null)
				{
					flag = true;
				}
			}
			if (flag)
			{
				CancelTransition();
			}
			else if (!GameState.Paused && num >= num2 && IsGameStateValidToTransition())
			{
				TriggerSceneTransition();
			}
		}
		else if ((bool)m_2dCollider)
		{
			m_mouseOver = m_2dCollider.MouseOver;
			RefreshIcon();
		}
		if (InGameHUD.TravelEnabled && !UIWorldMapManager.Instance.IsVisible && !UIAreaMapManager.Instance.IsVisible)
		{
			InGameHUD.TravelEnabled = false;
		}
	}

	protected override void OnDestroy()
	{
		DestroyPartyMarkers();
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnExtraHover(GameObject sender, bool over)
	{
		m_mouseOver = over;
		RefreshIcon();
	}

	private void OnMouseOver()
	{
		if (!(m_2dCollider != null) && UIWindowManager.MouseInputAvailable)
		{
			m_mouseOver = true;
			RefreshIcon();
		}
	}

	private void OnMouseExit()
	{
		if (!(m_2dCollider != null))
		{
			m_mouseOver = false;
			RefreshIcon();
		}
	}

	public static void CancelAllSceneTransitions()
	{
		SceneTransition[] array = UnityEngine.Object.FindObjectsOfType<SceneTransition>();
		foreach (SceneTransition sceneTransition in array)
		{
			if ((bool)sceneTransition)
			{
				sceneTransition.CancelTransition();
			}
		}
	}

	public void CancelTransition()
	{
		CancelTransitionNoVisual();
		SwitchIcon(active: false);
	}

	protected void CancelTransitionNoVisual()
	{
		for (int i = 0; i < m_partyWaitList.Count; i++)
		{
			if (m_partyWaitList[i] != null)
			{
				PartyMemberAI component = m_partyWaitList[i].GetComponent<PartyMemberAI>();
				if ((bool)component && component.StateManager.FindState(typeof(WaitForSceneTransition)) is WaitForSceneTransition waitForSceneTransition)
				{
					waitForSceneTransition.Cancel();
				}
			}
		}
		m_isActive = false;
		m_partyWaitList.Clear();
		RefreshIcon();
		DestroyPartyMarkers();
	}

	private void DestroyPartyMarkers()
	{
		if (m_partyTargetMarkers == null)
		{
			return;
		}
		for (int i = 0; i < m_partyTargetMarkers.Length; i++)
		{
			if (m_partyTargetMarkers[i] != null)
			{
				GameUtilities.Destroy(m_partyTargetMarkers[i]);
			}
			m_partyTargetMarkers[i] = null;
		}
	}

	private void DestroyPartyMarker(GameObject obj)
	{
		PartyMemberAI component = obj.GetComponent<PartyMemberAI>();
		if (!component)
		{
			return;
		}
		Vector3 markerPosition = GetMarkerPosition(component, reverse: false);
		if (m_partyTargetMarkers == null)
		{
			return;
		}
		for (int i = 0; i < m_partyTargetMarkers.Length; i++)
		{
			if (m_partyTargetMarkers[i] != null)
			{
				float num = 0f;
				num = ((!Use3DDistanceCheck) ? GameUtilities.V3SqrDistance2D(markerPosition, m_partyTargetMarkers[i].transform.position) : (markerPosition - m_partyTargetMarkers[i].transform.position).sqrMagnitude);
				if (num < 0.1f)
				{
					GameUtilities.Destroy(m_partyTargetMarkers[i]);
					m_partyTargetMarkers[i] = null;
				}
			}
		}
	}

	public override bool Use(GameObject user)
	{
		DestroyPartyMarker(user);
		if (!m_partyWaitList.Contains(user))
		{
			return false;
		}
		m_partyWaitList.Remove(user);
		if (ValidToTransition())
		{
			TriggerSceneTransition();
			return false;
		}
		return true;
	}

	private bool ValidToTransition()
	{
		if (m_partyWaitList.Count > 0)
		{
			return false;
		}
		if (GameState.s_playerCharacter == null)
		{
			return false;
		}
		Health component = GameState.s_playerCharacter.GetComponent<Health>();
		if (component == null || component.CurrentStamina <= 0f || component.CurrentHealth <= 0f)
		{
			return false;
		}
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI != null)
			{
				Health component2 = partyMemberAI.GetComponent<Health>();
				if ((bool)component2 && (component2.CurrentStamina <= 0f || component2.CurrentHealth <= 0f))
				{
					return false;
				}
			}
		}
		if (!IsGameStateValidToTransition())
		{
			return false;
		}
		return true;
	}

	private bool IsGameStateValidToTransition()
	{
		if (GameState.GameOver || GameState.InCombat || Cutscene.CutsceneActive)
		{
			return false;
		}
		return true;
	}

	private void TransitionToNewScene()
	{
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(TransitionToNewScene));
		if (!ValidToTransition())
		{
			CancelTransition();
			FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.AreaTransition, 0.35f);
			return;
		}
		ScriptEvent component = base.gameObject.GetComponent<ScriptEvent>();
		if ((bool)component)
		{
			component.ExecuteScript(ScriptEvent.ScriptEvents.OnTriggered);
			SpecialCharacterInstanceID.Add(GameState.s_playerCharacter.gameObject, SpecialCharacterInstanceID.SpecialCharacterInstance.User);
			component.ExecuteScript(ScriptEvent.ScriptEvents.OnUsed);
		}
		if (SameMapTransitionPoint == null)
		{
			GameState.s_playerCharacter.StartPointLink = StartPointLink;
			GameState.s_playerCharacter.StartPointName = StartPointOverride;
			if (GameState.Instance.CurrentMapIsStronghold == Stronghold.Instance.Activated && !m_UsedPartyManager && ShowPartyManager && UIPartyManager.Instance.ChoicesAvailable)
			{
				UIPartyManager.Instance.ToggleAlt();
				UIPartyManager instance2 = UIPartyManager.Instance;
				instance2.OnWindowHidden = (UIHudWindow.WindowHiddenDelegate)Delegate.Combine(instance2.OnWindowHidden, new UIHudWindow.WindowHiddenDelegate(OnPartyManagerHidden));
			}
			else if (LinkedScene == MapType.Map)
			{
				InGameHUD.MapTag = LinkedMapTag;
				InGameHUD.TravelEnabled = true;
				UIWorldMapManager.Instance.ShowWindow();
			}
			else
			{
				GameState.LoadedGame = false;
				GameState.ChangeLevel(LinkedScene);
			}
		}
		else
		{
			FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.AreaTransition, 0.75f);
			SameMapTransitionPoint.TeleportPartyToLocation();
		}
	}

	private void OnPartyManagerHidden(UIHudWindow window)
	{
		try
		{
			UIPartyManager instance = UIPartyManager.Instance;
			instance.OnWindowHidden = (UIHudWindow.WindowHiddenDelegate)Delegate.Remove(instance.OnWindowHidden, new UIHudWindow.WindowHiddenDelegate(OnPartyManagerHidden));
			m_UsedPartyManager = true;
			TransitionToNewScene();
		}
		finally
		{
			m_UsedPartyManager = false;
		}
	}

	private void TriggerSceneTransition()
	{
		FireUseAudio();
		CancelTransitionNoVisual();
		if (!FadeManager.Instance.IsFadeActive())
		{
			if ((LinkedScene != 0 && !ShowPartyManager) || SameMapTransitionPoint != null)
			{
				FadeManager instance = FadeManager.Instance;
				instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(TransitionToNewScene));
				FadeManager.Instance.FadeToBlack(FadeManager.FadeType.AreaTransition, 0.35f, AudioFadeMode.Fx);
			}
			else
			{
				TransitionToNewScene();
			}
		}
	}

	public void UseTransition(GameObject go)
	{
		GameInput.HandleAllClicks();
		if (m_partyWaitList == null)
		{
			return;
		}
		if (m_partyWaitList.Count > 0)
		{
			Console.AddMessage(GUIUtils.GetTextWithLinks(101));
			if (!m_playedTransitionSound)
			{
				m_playedTransitionSound = true;
				PlayTransitionSound("data/audio/vocalization/vo wav files/narrator/msg_no_transition.wav");
			}
		}
		else if (GameState.InCombat)
		{
			Console.AddMessage(GUIUtils.GetTextWithLinks(992));
			if (!m_playedTransitionSound)
			{
				m_playedTransitionSound = true;
				PlayTransitionSound("data/audio/vocalization/vo wav files/narrator/msg_no_transition_combat.wav");
			}
		}
		else
		{
			if (GameState.PartyDead || GameState.GameOver)
			{
				return;
			}
			SwitchIcon(active: true);
			PartyMemberAI.SelectAll();
			GameState.s_playerCharacter.ObjectClicked(this);
			int num = 0;
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI in partyMembers)
			{
				if (partyMemberAI != null)
				{
					num++;
					AIController component = partyMemberAI.GetComponent<AIController>();
					if ((bool)component && component.StateManager != null)
					{
						m_partyWaitList.Add(partyMemberAI.gameObject);
					}
				}
			}
			DestroyPartyMarkers();
			m_partyTargetMarkers = new GameObject[num];
		}
	}

	public void HoverTransition(GameObject go, bool over)
	{
		if (over)
		{
			if (base.IsRevealed)
			{
				GameCursor.GenericUnderCursor = base.gameObject;
				GameCursor.CursorOverride = MouseOverCursor;
			}
		}
		else if (GameCursor.GenericUnderCursor == base.gameObject)
		{
			GameCursor.GenericUnderCursor = null;
		}
	}

	public override Vector3 GetClosestInteractionPoint(Vector3 worldPos)
	{
		return base.transform.position;
	}

	public void RefreshIcon()
	{
		RefreshIcon(onDisable: false);
	}

	private void RefreshIcon(bool onDisable)
	{
		if ((bool)m_ActiveIcon && m_ActiveIcon.Hiding)
		{
			m_ActiveIcon = null;
		}
		if (!DisplayIcon || (!GetComponent<Collider>() && !m_2dCollider) || !UITriggerManager.Instance)
		{
			return;
		}
		bool flag = m_safetyShow && (!UITriggerManager.Instance.SafetyIcon || UITriggerManager.Instance.SafetyIcon == m_ActiveIcon);
		if ((m_mouseOver || flag || ((bool)InGameHUD.Instance && InGameHUD.Instance.HighlightActive) || m_partyWaitList.Count > 0) && base.IsRevealed && base.isActiveAndEnabled && !onDisable)
		{
			if (!(m_ActiveIcon == null))
			{
				return;
			}
			Detectable component = base.gameObject.GetComponent<Detectable>();
			if (component == null || component.Detected)
			{
				m_ActiveIcon = UITriggerManager.Instance.Show(GetIconWorldPosition(), m_isActive ? DisplayIconActive : DisplayIcon, UseTransition, HoverTransition, avoidHud: true);
				MapType linkedSceneForUi = GetLinkedSceneForUi();
				if (linkedSceneForUi != 0)
				{
					m_ActiveIcon.SetString(WorldMap.Instance.GetMap(linkedSceneForUi).DisplayName.GetText());
				}
				if (flag)
				{
					UITriggerManager.Instance.SafetyIcon = m_ActiveIcon;
				}
			}
		}
		else if (m_ActiveIcon != null)
		{
			UITriggerManager.Instance.Hide(m_ActiveIcon);
			m_ActiveIcon = null;
		}
	}

	private Vector3 GetIconWorldPosition()
	{
		Vector3 position = base.transform.position;
		Vector3 one = Vector3.one;
		if (m_2dCollider != null)
		{
			float y = m_2dCollider.bounds.size.y;
			position.y = base.transform.position.y - y * 0.5f + y * IconYOffset;
			float z = m_2dCollider.bounds.size.z;
			position.z = base.transform.position.z - z * 0.5f + z * IconZOffset;
			position.x = base.transform.position.x - z * 0.5f + z * IconXOffset;
		}
		else if (GetComponent<Collider>() != null)
		{
			one = GetComponent<Collider>().bounds.extents;
			position.x = base.transform.position.x + one.x * IconXOffset;
			position.y = base.transform.position.y + one.y * IconYOffset;
			position.z = base.transform.position.z + one.z * IconZOffset;
		}
		return position;
	}

	private void SwitchIcon(bool active)
	{
		if (m_ActiveIcon != null && DisplayIconActive != null && DisplayIcon != null)
		{
			UITriggerManager.Instance.Hide(m_ActiveIcon);
			m_isActive = active;
			m_ActiveIcon = UITriggerManager.Instance.Show(GetIconWorldPosition(), ((bool)m_ActiveIcon && m_isActive) ? DisplayIconActive : DisplayIcon, UseTransition, HoverTransition, avoidHud: true);
			MapType linkedSceneForUi = GetLinkedSceneForUi();
			if (linkedSceneForUi != 0)
			{
				m_ActiveIcon.SetString(WorldMap.Instance.GetMap(linkedSceneForUi).DisplayName.GetText());
			}
		}
	}

	public bool IsObjectOnWaitList(GameObject obj)
	{
		return m_partyWaitList.Contains(obj);
	}

	public Vector3 GetMarkerPosition(PartyMemberAI partyMemberAI, bool reverse)
	{
		int slot = partyMemberAI.Slot;
		bool secondary = false;
		if (partyMemberAI.Secondary)
		{
			secondary = true;
			slot = partyMemberAI.Summoner.GetComponent<PartyMemberAI>().Slot;
		}
		return GetMarkerPositionBySlot(reverse ? (5 - slot) : slot, secondary);
	}

	private Vector3 GetMarkerPositionBySlot(int slot, bool secondary)
	{
		if (PartyInteractionPoints[slot] == null)
		{
			return base.transform.position;
		}
		if (secondary)
		{
			Vector3 vector = PartyInteractionPoints[slot].position + PartyInteractionPoints[slot].right;
			if (Physics.Raycast(vector + Vector3.up * 2f, Vector3.down, out var hitInfo))
			{
				vector = hitInfo.point;
			}
			return vector;
		}
		return PartyInteractionPoints[slot].position;
	}

	public MapType GetLinkedSceneForUi()
	{
		if (!ShowSceneName)
		{
			return MapType.Map;
		}
		if (LinkedSceneForUi != 0)
		{
			return LinkedSceneForUi;
		}
		return LinkedScene;
	}

	public void MakePartyMarkers()
	{
		int[] array = new int[6] { -1, 1, -1, 1, -1, 1 };
		int[] array2 = new int[6] { -1, -1, 0, 0, 1, 1 };
		for (int i = 0; i < 6; i++)
		{
			GameObject gameObject = new GameObject("pm" + (i + 1));
			gameObject.AddComponent<SceneTransitionMarker>().slot = i;
			gameObject.transform.parent = base.gameObject.transform;
			gameObject.transform.localPosition = new Vector3(array[i], 0f, array2[i]);
			if (Physics.Raycast(gameObject.transform.position + Vector3.up * 2f, Vector3.down, out var hitInfo))
			{
				gameObject.transform.position = hitInfo.point;
			}
			PartyInteractionPoints[i] = gameObject.transform;
		}
	}

	public void ForcePartyMarkersToWalkmesh()
	{
		for (int i = 0; i < 6; i++)
		{
			if (Physics.Raycast(PartyInteractionPoints[i].position + Vector3.up * 2f, Vector3.down, out var hitInfo))
			{
				PartyInteractionPoints[i].position = hitInfo.point;
			}
		}
	}

	public void OnDrawGizmos()
	{
		if (DisplayIcon != null)
		{
			if (m_2dCollider == null)
			{
				m_2dCollider = GetComponent<PE_Collider2D>();
			}
			if ((bool)m_2dCollider)
			{
				m_2dCollider.CalculateBounds();
			}
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(GetIconWorldPosition(), 0.3f);
		}
	}

	private void PlayTransitionSound(string clipName)
	{
		AudioSource component = GameState.s_playerCharacter.gameObject.GetComponent<AudioSource>();
		if (!GameState.s_playerCharacter.gameObject.GetComponent<VolumeAsCategory>())
		{
			VolumeAsCategory volumeAsCategory = GameState.s_playerCharacter.gameObject.AddComponent<VolumeAsCategory>();
			volumeAsCategory.Category = MusicManager.SoundCategory.VOICE;
			volumeAsCategory.Source = component;
			volumeAsCategory.Init();
		}
		GlobalAudioPlayer.StreamClipAtSource(component, clipName, bIs3DSound: false);
	}
}
