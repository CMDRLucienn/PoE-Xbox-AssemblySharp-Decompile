using System;
using System.Collections.Generic;
using UnityEngine;

public class InGameHUD : MonoBehaviour
{
	public delegate void HighlightEvent();

	[Serializable]
	public class HealthColor
	{
		public float HealthRatioMin;

		public Color DisplayColor;

		public int StringDatabaseId;

		private string m_NguiColorTag;

		public string NguiColorTag
		{
			get
			{
				if (m_NguiColorTag == null)
				{
					m_NguiColorTag = "[" + NGUITools.EncodeColor(DisplayColor) + "]";
				}
				return m_NguiColorTag;
			}
		}
	}

	public enum ExclusiveCursorMode
	{
		None,
		Inspect,
		Helwax
	}

	public delegate void HudVisibilityChanged(bool visible);

	[HideInInspector]
	public Vector2 m_CombatLogCurrentSize;

	[Persistent]
	[HideInInspector]
	public Vector2 CombatLogOutSize;

	public HighlightEvent OnHighlightBegin;

	public HighlightEvent OnHighlightEnd;

	private bool m_HighlightToggled;

	private bool m_HighlightHeld;

	public KeyCode MapKey = KeyCode.F11;

	public GameObject SelectionCircle;

	public GameObject DestinationCircle;

	public GameObject TargetCircle;

	public GameObject SceneTransitionCircle;

	public Material Friendly;

	public Material FriendlyColorBlind;

	public Material Dominated;

	public Material DominatedColorBlind;

	public Material FoeMaterial;

	public Material FoeColorBlind;

	public Material FriendlySelected;

	public Material FriendlySelectedColorBlind;

	public Material GhostCharacterMat;

	public Texture2D EngageTexture;

	public Texture2D FlankTexture;

	public Texture2D TargetArrowTexture;

	public Texture2D MoveTargetArrowTexture;

	private Color FriendlyColor;

	private Color FoeColor;

	private Color FriendlyColorColorBlind;

	public GameObject LevelUpVfx;

	public float PieDensity = 0.75f;

	public float PieWidth = 0.2f;

	public float PieMinScale = 0.6f;

	public float PieMaxScale = 1f;

	public float MovePieWidth = 0.18f;

	public float MovePieRotSpeed = 30f;

	public float MovePieAlpha = 0.8f;

	public float MovePieOffset = 0.75f;

	public float EngagedCircleWidth = 0.06f;

	[Tooltip("Length of a tiling segment of the engagement texture on the circle. Gets clamped to nearest vertex.")]
	public float EngagedCircleTileLength = 0.12f;

	public float SelectionCircleWidth = 0.03f;

	public float PieArc = 70f;

	public float SpikeWidth = 0.12f;

	public float SpikeDepth = 0.12f;

	public float SpikeCircleWidthInset = 2f;

	public float SpikeAbsoluteInset;

	private TweenColorIndependent m_HighlightPulseTween;

	public HealthColor[] CharacterHealthStages = new HealthColor[0];

	private static int s_hideHUD = 0;

	private static Vector3[] m_LocalVerts = new Vector3[12];

	private static Color[] m_LocalColor = new Color[12];

	public float HighlightAlphaMin = 0.2f;

	public float HighlightAlphaMax = 0.3f;

	public float HighlightTweenDuration = 2f;

	public float HighlightSaturation = 1f;

	public float HighlightTailDur = 0.15f;

	public AnimationCurve HighlightCurve;

	public Color HighlightFriend;

	public Color HighlightFoe;

	public Color HighlightFriendColorblind;

	public Color HighlightNeutral;

	public Color HighlightInteractable;

	private ExclusiveCursorMode m_CursorMode;

	private bool m_CursorProtect;

	[HideInInspector]
	public bool HidePause;

	public static string MapTag = "world";

	public static bool TravelEnabled = false;

	private static Vector2 s_scrollPos = Vector2.zero;

	public static InGameHUD Instance { get; private set; }

	[Persistent]
	public Vector2 CombatLogCurrentSize
	{
		get
		{
			return m_CombatLogCurrentSize;
		}
		set
		{
			m_CombatLogCurrentSize = value;
			UIConsole.Instance.SetSize(m_CombatLogCurrentSize);
		}
	}

	public bool HighlightActive => m_HighlightHeld != m_HighlightToggled;

	public SelectionCircleMaterials CircleMaterials { get; private set; }

	public Mesh SelectionCircleMesh { get; private set; }

	public Mesh EngagedSpikeMesh { get; private set; }

	public Mesh EngagedTargetedSpikeMesh { get; private set; }

	public bool UseColorBlindSettings => GameState.Option.GetOption(GameOption.BoolOption.COLORBLIND_MODE);

	public float HighlightTweenAlpha => (HighlightAlphaMax - HighlightAlphaMin) * (m_HighlightPulseTween ? m_HighlightPulseTween.sample : 0f) + HighlightAlphaMin;

	public bool QuicksaveAllowed
	{
		get
		{
			if ((!UIConversationManager.Instance || !UIConversationManager.Instance.WindowActive()) && (!ConversationManager.Instance || !ConversationManager.Instance.IsConversationOrSIRunning()) && (!UICharacterCreationManager.Instance || !UICharacterCreationManager.Instance.WindowActive()) && (!UIInterstitialManager.Instance || !UIInterstitialManager.Instance.WindowActive()) && !Cutscene.CutsceneActive && !GameState.IsLoading && !GameState.PartyDead && !GameState.GameOver)
			{
				if ((bool)FadeManager.Instance)
				{
					return !FadeManager.Instance.IsFadeActive();
				}
				return true;
			}
			return false;
		}
	}

	public ExclusiveCursorMode CursorMode
	{
		get
		{
			return m_CursorMode;
		}
		private set
		{
			m_CursorMode = value;
			m_CursorProtect = m_CursorMode != ExclusiveCursorMode.None;
		}
	}

	public HelwaxMold HelwaxSource { get; private set; }

	public int HudUserMode { get; private set; }

	public bool ShowHUD
	{
		get
		{
			return s_hideHUD == 0;
		}
		set
		{
			if (value)
			{
				s_hideHUD--;
			}
			else
			{
				s_hideHUD++;
			}
			if (s_hideHUD <= 0)
			{
				HudUserMode = 0;
				s_hideHUD = 0;
			}
			PartyMemberAI[] partyMembers;
			if (ShowHUD)
			{
				if (this.OnHudVisibilityChanged != null)
				{
					this.OnHudVisibilityChanged(ShowHUD);
				}
				InGameUILayout instance = InGameUILayout.Instance;
				if (instance != null)
				{
					instance.ShowHud();
				}
				partyMembers = PartyMemberAI.PartyMembers;
				foreach (PartyMemberAI partyMemberAI in partyMembers)
				{
					if (partyMemberAI != null)
					{
						partyMemberAI.ReinstateDestination();
					}
				}
				return;
			}
			if (this.OnHudVisibilityChanged != null)
			{
				this.OnHudVisibilityChanged(ShowHUD);
			}
			InGameUILayout instance2 = InGameUILayout.Instance;
			if (instance2 != null)
			{
				instance2.HideHud();
			}
			partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI2 in partyMembers)
			{
				if (partyMemberAI2 != null)
				{
					partyMemberAI2.SuspendDestination();
				}
			}
		}
	}

	public event HudVisibilityChanged OnHudVisibilityChanged;

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		if ((bool)GameInput.Instance)
		{
			GameInput.Instance.OnHandleInput -= HandleInput;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		m_HighlightPulseTween = base.gameObject.AddComponent<TweenColorIndependent>();
		m_HighlightPulseTween.delay = 0f;
		m_HighlightPulseTween.style = UITweener.Style.Loop;
		m_HighlightPulseTween.animationCurve = HighlightCurve;
		m_HighlightPulseTween.Play(forward: true);
		SelectionCircleMesh = new Mesh();
		SelectionCircleMesh.name = "SelectionSquareMesh";
		SelectionCircleMesh.vertices = new Vector3[4]
		{
			new Vector3(-0.5f, 0f, 0.5f),
			new Vector3(0.5f, 0f, 0.5f),
			new Vector3(0.5f, 0f, -0.5f),
			new Vector3(-0.5f, 0f, -0.5f)
		};
		SelectionCircleMesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f)
		};
		SelectionCircleMesh.triangles = new int[6] { 0, 1, 3, 1, 2, 3 };
		EngagedSpikeMesh = new Mesh();
		EngagedSpikeMesh.name = "EngagementCircleMesh";
		EngagedTargetedSpikeMesh = new Mesh();
		EngagedTargetedSpikeMesh.name = "EngagmentTargetedMesh";
		UpdateEngagedMesh();
		EngagedSpikeMesh.triangles = new int[12]
		{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 11
		};
		EngagedSpikeMesh.uv = new Vector2[12];
		CircleMaterials = new SelectionCircleMaterials(Friendly, FriendlyColorBlind, FoeMaterial, FoeColorBlind, FriendlySelected, FriendlySelectedColorBlind, Dominated, DominatedColorBlind);
		FetchColors();
		GameInput.Instance.OnHandleInput += HandleInput;
	}

	private void FetchColors()
	{
		FriendlyColor = FriendlySelected.color;
		FoeColor = FoeMaterial.color;
		FriendlyColorColorBlind = FriendlySelectedColorBlind.color;
	}

	private void UpdateEngagedMesh()
	{
		for (int i = 0; i < 4; i++)
		{
			Quaternion quaternion = Quaternion.AngleAxis(90f * (float)i, Vector3.up);
			m_LocalVerts[i * 3] = quaternion * new Vector3((0f - SpikeWidth) / 2f, 0f, 0.5f);
			m_LocalVerts[i * 3 + 1] = quaternion * new Vector3(SpikeWidth / 2f, 0f, 0.5f);
			m_LocalVerts[i * 3 + 2] = quaternion * new Vector3(0f, 0f, 0.5f - SpikeDepth);
			float a = 0.6f + 0.4f * (Mathf.Sin((float)i * (float)Math.PI / 2f) + 1f) / 2f;
			m_LocalColor[i * 3] = new Color(1f, 1f, 1f, a);
			m_LocalColor[i * 3 + 1] = new Color(1f, 1f, 1f, a);
			m_LocalColor[i * 3 + 2] = new Color(1f, 1f, 1f, a);
		}
		EngagedSpikeMesh.vertices = m_LocalVerts;
		EngagedSpikeMesh.colors = m_LocalColor;
	}

	private void HandleInput()
	{
		if (!GameInput.GetControlUp(MappedControl.QUICKLOAD) || UICharacterCreationManager.Instance.IsVisible)
		{
			return;
		}
		if (GameResources.SaveGameExists(SaveGameInfo.GetQuicksaveFileName()))
		{
			if (!FadeManager.Instance.IsFadeActive())
			{
				FadeManager instance = FadeManager.Instance;
				instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(QuickLoadGameOnFadeEnd));
				FadeManager.Instance.FadeToBlack(FadeManager.FadeType.AreaTransition, 0.35f, AudioFadeMode.MusicAndFx);
			}
		}
		else
		{
			UISystemMessager.Instance.PostMessage(GUIUtils.GetText(1500), Color.red);
		}
	}

	private void Update()
	{
		m_HighlightPulseTween.duration = HighlightTweenDuration;
		switch (m_CursorMode)
		{
		case ExclusiveCursorMode.Inspect:
			GameCursor.UiCursor = GameCursor.CursorType.Examine;
			break;
		case ExclusiveCursorMode.Helwax:
			GameCursor.UiCursor = GameCursor.CursorType.DuplicateItem;
			break;
		}
		bool highlightActive = HighlightActive;
		if (UIWindowManager.KeyInputAvailable)
		{
			m_HighlightHeld = GameInput.GetControl(MappedControl.HIGHLIGHT_HOLD);
			if (GameInput.GetControlUp(MappedControl.HIGHLIGHT_TOGGLE))
			{
				m_HighlightToggled = !m_HighlightToggled;
			}
		}
		else
		{
			m_HighlightHeld = false;
		}
		if (!highlightActive && HighlightActive && OnHighlightBegin != null)
		{
			OnHighlightBegin();
		}
		else if (highlightActive && !HighlightActive && OnHighlightEnd != null)
		{
			OnHighlightEnd();
		}
		if (QuicksaveAllowed && GameInput.GetControlUp(MappedControl.QUICKSAVE))
		{
			if (GameState.CannotSaveBecauseInCombat || PartyMemberAI.IsPartyMemberUnconscious())
			{
				UISystemMessager.Instance.PostMessage(GUIUtils.GetText(713), Color.red);
			}
			else if (GameState.Mode.TrialOfIron)
			{
				UISystemMessager.Instance.PostMessage(GUIUtils.GetText(1454), Color.red);
			}
			else
			{
				GameResources.SaveGame(SaveGameInfo.GetQuicksaveFileName());
				UISystemMessager.Instance.PostMessage(GUIUtils.GetText(601), Color.green);
				Console.AddMessage(GUIUtils.GetTextWithLinks(601), Color.green);
			}
		}
		if (GameInput.GetControlDown(MappedControl.TOGGLE_HUD, handle: true))
		{
			if (HudUserMode == 0)
			{
				ShowHUD = false;
			}
			HudUserMode++;
			if (HudUserMode > 2)
			{
				HudUserMode = 0;
			}
			if (HudUserMode == 0)
			{
				ShowHUD = true;
			}
			if ((bool)GameCursor.Instance)
			{
				GameCursor.Instance.SetShowCursor(this, HudUserMode != 2);
			}
			if (this.OnHudVisibilityChanged != null)
			{
				this.OnHudVisibilityChanged(ShowHUD);
			}
		}
		if (GameState.Instance.CheatsEnabled && GameInput.GetKeyDown(MapKey, setHandled: true))
		{
			MapTag = "all";
			UIWorldMapManager.Instance.Toggle();
		}
	}

	private void QuickLoadGameOnFadeEnd()
	{
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(QuickLoadGameOnFadeEnd));
		if (GameResources.LoadGame(SaveGameInfo.GetQuicksaveFileName()))
		{
			UISystemMessager.Instance.PostMessage(GUIUtils.GetText(602), Color.green);
			Console.AddMessage(GUIUtils.GetTextWithLinks(602), Color.green);
		}
		else
		{
			FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.AreaTransition, 0.35f);
		}
	}

	public void EnterInspectMode()
	{
		CursorMode = ExclusiveCursorMode.Inspect;
	}

	public void EnterHelwaxMode(HelwaxMold source)
	{
		CursorMode = ExclusiveCursorMode.Helwax;
		HelwaxSource = source;
	}

	public void EndExclusiveCursor()
	{
		CursorMode = ExclusiveCursorMode.None;
	}

	public void TargetHelwax(Equippable target)
	{
		if ((bool)HelwaxSource)
		{
			HelwaxSource.TargetHelwax(target);
		}
	}

	private void LateUpdate()
	{
		if (GameInput.GetMouseButtonUp(0, setHandled: false))
		{
			if (m_CursorProtect)
			{
				m_CursorProtect = false;
			}
			else
			{
				EndExclusiveCursor();
			}
		}
	}

	public static void BlockClicksInRect(Rect window)
	{
		if (Event.current.type == EventType.Repaint)
		{
			UnityGUIClickEater.EatInRect(window);
		}
	}

	private void OnGUI()
	{
		if ((bool)UIWorldMapManager.Instance && UIWorldMapManager.Instance.WindowActive() && MapTag == "all")
		{
			BlockClicksInRect(GUI.Window(60, new Rect(0f, 0f, Screen.width, (float)Screen.height - 200f), WorldMapUI, "World Map"));
		}
	}

	public void ForceShowHUD()
	{
		s_hideHUD = 0;
		HudUserMode = 0;
	}

	public static Color GetFriendlyColor()
	{
		if (Instance.UseColorBlindSettings)
		{
			return Instance.FriendlyColorColorBlind;
		}
		return Instance.FriendlyColor;
	}

	public static Color GetFriendlyHighlightColor()
	{
		if (Instance.UseColorBlindSettings)
		{
			return Instance.HighlightFriendColorblind;
		}
		return Instance.HighlightFriend;
	}

	public static Color GetFoeColor()
	{
		return Instance.FoeColor;
	}

	private HealthColor GetHealthColor(float current, float max)
	{
		float num = current / max;
		if (num < 0f)
		{
			num = 0f;
		}
		HealthColor healthColor = new HealthColor();
		HealthColor[] characterHealthStages = CharacterHealthStages;
		foreach (HealthColor healthColor2 in characterHealthStages)
		{
			if (num >= healthColor2.HealthRatioMin && healthColor2.HealthRatioMin >= healthColor.HealthRatioMin)
			{
				healthColor = healthColor2;
			}
		}
		return healthColor;
	}

	public static int GetHealthStage(float current, float max)
	{
		float num = current / max;
		if (num < 0f)
		{
			num = 0f;
		}
		for (int i = 0; i < Instance.CharacterHealthStages.Length; i++)
		{
			if (num >= Instance.CharacterHealthStages[i].HealthRatioMin)
			{
				return i;
			}
		}
		return 0;
	}

	public static string GetHealthString(float current, float max, Gender gender)
	{
		HealthColor healthColor = Instance.GetHealthColor(current, max);
		int id = 165;
		if ((double)current > 0.01)
		{
			id = healthColor.StringDatabaseId;
		}
		return healthColor.NguiColorTag + GUIUtils.GetText(id, gender);
	}

	public static string GetHealthColorString(float current, float max)
	{
		return Instance.GetHealthColor(current, max).NguiColorTag;
	}

	private void WorldMapUI(int windowID)
	{
		s_scrollPos = GUILayout.BeginScrollView(s_scrollPos);
		List<string> list = new List<string>();
		foreach (MapData loadedMap in WorldMap.Instance.LoadedMaps)
		{
			if (loadedMap != null && loadedMap.IsAvailable)
			{
				list.Add(loadedMap.SceneName);
			}
		}
		list.Sort();
		foreach (string item in list)
		{
			if (GUILayout.Button(item))
			{
				GameInput.HandleAllClicks();
				if ((bool)GameState.s_playerCharacter)
				{
					GameState.s_playerCharacter.StartPointLink = StartPoint.PointLocation.ReferenceByName;
					GameState.s_playerCharacter.StartPointName = "PartySpawner";
				}
				GameState.LoadedGame = false;
				GameState.ChangeLevel(item);
				UIWorldMapManager.Instance.HideWindow();
			}
		}
		if (GUILayout.Button("CANCEL"))
		{
			UIWorldMapManager.Instance.HideWindow();
		}
		GUILayout.EndScrollView();
	}
}
