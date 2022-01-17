using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIConsole : MonoBehaviour
{
	public UIConsoleEntry EntryPrefab;

	public UIConsoleTabs Tabs;

	public UIConsoleMode ModePrefab;

	public int MaxEntries = 100;

	private UIConsoleMode[] m_Modes = new UIConsoleMode[2];

	private bool m_SizeChanged;

	public UIWidget Background;

	public GameObject Collider;

	public UIWidget LeftHandle;

	public UIWidget CornerHandle;

	public UIWidget TopHandle;

	public UIAnchor ContractedAnchor;

	public UIAnchor TransitionAnchorPoint;

	public TweenBetweenTransforms UpdateWith;

	public GameObject LeftMarker;

	public GameObject LastPartyPortraitMarker;

	public GameObject ScreenBR;

	public Vector2 MinSize = new Vector2(150f, 55f);

	public Vector2 MaxMargins = new Vector2(0f, 28f);

	public Vector2 DefaultSize;

	public UIResolutionScaler MatchScale;

	public UIPanel ContentPanel;

	private UIStretch m_PanelStretch;

	private UIPanelOrigin m_PanelOrigin;

	public static UIConsole Instance { get; private set; }

	public float MaxWidth { get; private set; }

	private Vector3 LeftMarkerPosition
	{
		get
		{
			if (!LastPartyPortraitMarker)
			{
				return LeftMarker.transform.localPosition;
			}
			Vector3 result = LeftMarker.transform.parent.InverseTransformPoint(LastPartyPortraitMarker.transform.position);
			if (LeftMarker.transform.localPosition.x > result.x)
			{
				return LeftMarker.transform.localPosition;
			}
			return result;
		}
	}

	private void Awake()
	{
		Instance = this;
		if ((bool)UpdateWith)
		{
			UpdateWith.OnPositionChanged += OnTweenerCallback;
		}
		EntryPrefab.gameObject.SetActive(value: false);
		m_Modes[0] = ModePrefab;
		m_Modes[0].Mode = Console.ConsoleState.Combat;
		for (int i = 1; i < m_Modes.Length; i++)
		{
			m_Modes[i] = UnityEngine.Object.Instantiate(ModePrefab.gameObject).GetComponent<UIConsoleMode>();
			m_Modes[i].transform.parent = ModePrefab.transform.parent;
			m_Modes[i].transform.localPosition = ModePrefab.transform.localPosition;
			m_Modes[i].transform.localScale = ModePrefab.transform.localScale;
			m_Modes[i].transform.localRotation = ModePrefab.transform.localRotation;
			m_Modes[i].Mode = (Console.ConsoleState)i;
		}
		ChangeState(Console.ConsoleState.Combat);
		SetSize(DefaultSize);
	}

	private void Start()
	{
		m_PanelStretch = ContentPanel.GetComponent<UIStretch>();
		m_PanelOrigin = ContentPanel.GetComponent<UIPanelOrigin>();
		MinSize.y = Background.transform.localScale.y;
		UIEventListener uIEventListener = UIEventListener.Get(LeftHandle.gameObject);
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnHandleLeft));
		UIEventListener uIEventListener2 = UIEventListener.Get(TopHandle.gameObject);
		uIEventListener2.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener2.onDrag, new UIEventListener.VectorDelegate(OnHandleTop));
		UIEventListener uIEventListener3 = UIEventListener.Get(CornerHandle.gameObject);
		uIEventListener3.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener3.onDrag, new UIEventListener.VectorDelegate(OnHandleCorner));
		UIEventListener uIEventListener4 = UIEventListener.Get(CornerHandle.gameObject);
		uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, new UIEventListener.VoidDelegate(OnCornerClicked));
		GameState.OnLevelLoaded += OnLevelLoaded;
		GameResources.OnLoadedSave += OnLoadedSave;
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		if ((bool)ContentPanel)
		{
			UIDraggablePanel component = ContentPanel.GetComponent<UIDraggablePanel>();
			if ((bool)component)
			{
				component.ResetPosition();
			}
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		if ((bool)UpdateWith)
		{
			UpdateWith.OnPositionChanged -= OnTweenerCallback;
		}
		GameResources.OnLoadedSave -= OnLoadedSave;
		GameState.OnLevelLoaded -= OnLevelLoaded;
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void SetSize(Vector2 size)
	{
		Vector2 delta = size - (Vector2)Background.transform.localScale;
		delta.x *= -1f;
		AdjustBackground(delta);
	}

	private void OnLoadedSave()
	{
		ClearAll();
	}

	public void ClearAll()
	{
		UIDraggablePanel component = ContentPanel.GetComponent<UIDraggablePanel>();
		component.ResetPosition();
		UIConsoleMode[] modes = m_Modes;
		for (int i = 0; i < modes.Length; i++)
		{
			modes[i].Clear();
		}
		component.ResetPosition();
	}

	private void OnTweenerCallback(GameObject go, Vector2 vec)
	{
		if (!GameState.IsLoading)
		{
			AdjustBackground(Vector2.zero);
		}
	}

	private void OnHandleLeft(GameObject go, Vector2 delta)
	{
		delta.y = 0f;
		OnHandleCorner(go, delta);
	}

	private void OnHandleTop(GameObject go, Vector2 delta)
	{
		delta.x = 0f;
		OnHandleCorner(go, delta);
	}

	private void OnHandleCorner(GameObject go, Vector2 delta)
	{
		AdjustBackground(delta);
		InGameHUD.Instance.CombatLogOutSize = Background.transform.localScale;
	}

	private void AdjustBackground(Vector2 delta)
	{
		m_SizeChanged = true;
		float num = MatchScale.GetScaleX() / InGameUILayout.Root.pixelSizeAdjustment;
		float num2 = MatchScale.GetScaleY() / InGameUILayout.Root.pixelSizeAdjustment;
		if (num == 0f)
		{
			delta.x = 0f;
		}
		else
		{
			delta.x /= num;
		}
		if (num2 == 0f)
		{
			delta.y = 0f;
		}
		else
		{
			delta.y /= num2;
		}
		CornerHandle.gameObject.transform.localPosition += (Vector3)delta;
		Vector2 vector = new Vector2(ScreenBR.transform.localPosition.x - (LeftMarkerPosition.x + 28f) - MaxMargins.x, (float)Screen.height / num2 - MaxMargins.y * 2f);
		float value = Background.transform.localScale.x - delta.x;
		float value2 = Background.transform.localScale.y + delta.y;
		value = Mathf.Clamp(value, MinSize.x, vector.x);
		value2 = Mathf.Clamp(value2, MinSize.y, vector.y);
		Background.transform.localScale = new Vector3(value, value2, Background.transform.localScale.z);
		if (Background.transform.localScale.y > (vector.y - MinSize.y) / 2f)
		{
			TransitionAnchorPoint.pixelOffset = new Vector2(-42f, -42f);
		}
		else
		{
			TransitionAnchorPoint.pixelOffset = new Vector2(42f, 42f);
		}
		if ((bool)m_PanelStretch)
		{
			m_PanelStretch.Update();
		}
		if ((bool)m_PanelOrigin)
		{
			m_PanelOrigin.DoUpdate();
		}
		InGameHUD.Instance.m_CombatLogCurrentSize = Background.transform.localScale;
	}

	private void OnCornerClicked(GameObject go)
	{
		if ((Vector2)Background.transform.localScale != DefaultSize)
		{
			SetSize(DefaultSize);
		}
		else
		{
			SetSize(InGameHUD.Instance.CombatLogOutSize);
		}
	}

	public void LogStateChanged(bool state)
	{
		if (!state)
		{
			ChangeState(Console.ConsoleState.Dialogue);
		}
		else
		{
			ChangeState(Console.ConsoleState.Combat);
		}
	}

	public void ChangeState(Console.ConsoleState state)
	{
		UIDraggablePanel component = ContentPanel.GetComponent<UIDraggablePanel>();
		component.ResetPosition();
		for (int i = 0; i < m_Modes.Length; i++)
		{
			m_Modes[i].gameObject.SetActive(i == (int)state);
		}
		component.ResetPosition();
	}

	private void Update()
	{
		MaxWidth = ContentPanel.clipRange.z;
		ContractedAnchor.pixelOffset.x = Background.transform.localScale.x * m_PanelStretch.relativeSize.x - 12f;
		List<Console.ConsoleMessage> list = Console.Instance.FetchConsoleMessages();
		if (list == null)
		{
			return;
		}
		if (list.Any())
		{
			Console.ConsoleMessage consoleMessage = list.Last();
			if (consoleMessage.m_mode == Console.ConsoleState.DialogueBig)
			{
				Tabs.ForceShowDialogue();
			}
			else if (consoleMessage.m_mode == Console.ConsoleState.Combat)
			{
				Tabs.ForceShowCombat();
			}
		}
		foreach (Console.ConsoleMessage item in list)
		{
			AddEntry(item);
		}
	}

	private void LateUpdate()
	{
		if (m_SizeChanged)
		{
			RepositionCurrentTable();
		}
		m_SizeChanged = false;
	}

	private void AddEntry(Console.ConsoleMessage message)
	{
		for (int i = 0; i < m_Modes.Length; i++)
		{
			m_Modes[i].AddEntry(message);
		}
	}

	public void RepositionCurrentTable()
	{
		for (int i = 0; i < m_Modes.Length; i++)
		{
			if (m_Modes[i].gameObject.activeSelf)
			{
				m_Modes[i].Reposition();
			}
		}
	}
}
