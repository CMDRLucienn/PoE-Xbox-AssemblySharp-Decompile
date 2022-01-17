using System;
using UnityEngine;

public class UIWorldMapManager : UIHudWindow
{
	private static UIWorldMapManager s_Instance;

	public UILabel TitleLabel;

	public GameObject ShowOnPlay;

	public GameObject TutorialText;

	public Vector2 IconMouseoverOffset = new Vector2(0f, 2f);

	public UIWorldMapMasterLinks MasterLinks;

	private UIWorldMapIcons[] m_IconManagers;

	private int m_CurrentManager;

	private UIDraggablePanel[] m_DragPanels;

	public static UIWorldMapManager Instance => s_Instance;

	public UIWorldMapIcons CurrentIconManager => m_IconManagers[m_CurrentManager];

	public MapData GetCurrentMap()
	{
		return GameState.Instance.CurrentMap;
	}

	private void Awake()
	{
		s_Instance = this;
		ShowOnPlay.SetActive(value: true);
	}

	private void Start()
	{
		m_IconManagers = GetComponentsInChildren<UIWorldMapIcons>(includeInactive: true);
		UIWorldMapIcons[] iconManagers = m_IconManagers;
		for (int i = 0; i < iconManagers.Length; i++)
		{
			iconManagers[i].LoadMapData();
		}
		MasterLinks.RebuildTravelTimes();
	}

	public UIWorldMapIcon[] GetAllIcons()
	{
		return GetComponentsInChildren<UIWorldMapIcon>(includeInactive: true);
	}

	public UIWorldMapIcon IconForScene(MapData data)
	{
		UIWorldMapIcon[] allIcons = GetAllIcons();
		foreach (UIWorldMapIcon uIWorldMapIcon in allIcons)
		{
			if (uIWorldMapIcon.GetName().Equals(data.SceneName))
			{
				return uIWorldMapIcon;
			}
		}
		return null;
	}

	protected override void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (m_DragPanels == null)
		{
			m_DragPanels = GetComponentsInChildren<UIDraggablePanel>(includeInactive: true);
		}
		if (!GameState.Instance.UiWorldMapTutorialFinished && m_DragPanels != null)
		{
			bool flag = false;
			UIDraggablePanel[] dragPanels = m_DragPanels;
			foreach (UIDraggablePanel uIDraggablePanel in dragPanels)
			{
				flag = flag || (uIDraggablePanel.isActiveAndEnabled && !uIDraggablePanel.IsDragging);
			}
			if (!flag)
			{
				GameState.Instance.UiWorldMapTutorialFinished = true;
				UpdateTutorial();
			}
		}
		if (m_CurrentManager >= 0 && base.IsVisible)
		{
			m_IconManagers[m_CurrentManager].UpdateVisibility();
		}
	}

	private void UpdateTutorial()
	{
		TutorialText.SetActive(!GameState.Instance.UiWorldMapTutorialFinished);
		UITweenAtPanelEdge[] componentsInChildren = GetComponentsInChildren<UITweenAtPanelEdge>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].WhileNotDrag = GameState.Instance.UiWorldMapTutorialFinished;
		}
	}

	public void DoTransition(string mapName, string mapTag)
	{
		HideWindow();
		MapData map = WorldMap.Instance.GetMap(mapName);
		MapData mapData = GetCurrentMap();
		if (mapData == null)
		{
			mapData = map;
			UIDebug.Instance.LogOnScreenWarning("CurrentMap is not defined (detected by UIWorldMapManager). Travel time will be 0.", UIDebug.Department.Programming, 10f);
		}
		UIWorldMapIcon uIWorldMapIcon = MasterLinks.LastStopBetween(mapData, map);
		if (uIWorldMapIcon == null)
		{
			UIDebug.Instance.LogOnScreenWarning("Attempted to travel from a map that was not on the World Map ('" + mapData.DisplayName.GetText() + "'). Travel time will be invalid.", UIDebug.Department.Design, 10f);
		}
		else
		{
			GameState.s_playerCharacter.StartPointLink = MasterLinks.GetStartPoint(uIWorldMapIcon.GetData(), map);
		}
		UIDifficultyScaling.PromptScalersAndChangeLevel(map, OnContinueTransition);
	}

	private void OnContinueTransition(object sender, EventArgs e)
	{
		if (!(e is TransitionEventArgs transitionEventArgs))
		{
			Debug.LogError("OnContinueTransition recieved event args of the wrong type.");
			return;
		}
		MapData targetMap = transitionEventArgs.TargetMap;
		if (!WorldTime.Instance)
		{
			Debug.LogError("WorldTime.Instance is null. World time was not incremented.", this);
			return;
		}
		if (!MasterLinks)
		{
			Debug.LogError("MasterLinks is null. World time was not incremented.", this);
			return;
		}
		if (!MasterLinks.GetConnected(GetCurrentMap(), targetMap))
		{
			Debug.LogError("Error in UIWorldMapManager: the party managed to travel a link with no travel time data. World time was not incremented.", this);
			return;
		}
		EternityTimeInterval interval = MasterLinks.TravelTimeTo(targetMap);
		WorldTime.Instance.AdvanceTime(interval, isTravel: true, isResting: false);
	}

	protected override bool Hide(bool forced)
	{
		InGameHUD.MapTag = "";
		return base.Hide(forced);
	}

	protected override void Show()
	{
		UpdateTutorial();
		if (string.IsNullOrEmpty(InGameHUD.MapTag))
		{
			InGameHUD.MapTag = GameState.Instance.CurrentMap.MapTag.Split(',')[0];
		}
		ReloadMapTag();
	}

	public void ReloadMapTag()
	{
		m_CurrentManager = -1;
		ClickOffCloses = false;
		for (int i = 0; i < m_IconManagers.Length; i++)
		{
			if (InGameHUD.MapTag.Equals(m_IconManagers[i].MapTag))
			{
				m_IconManagers[i].DisableParent.SetActive(value: true);
				m_CurrentManager = i;
				ClickOffCloses = true;
				TitleLabel.text = m_IconManagers[i].MapTitle.GetText();
			}
			else
			{
				m_IconManagers[i].DisableParent.SetActive(value: false);
			}
		}
		if (m_CurrentManager < 0)
		{
			if (!InGameHUD.MapTag.Equals("all"))
			{
				Debug.LogError("WorldMap has no content for map tag '" + InGameHUD.MapTag + "'.");
			}
			m_CurrentManager = 0;
		}
		else
		{
			m_IconManagers[m_CurrentManager].UpdateVisibility();
		}
	}
}
