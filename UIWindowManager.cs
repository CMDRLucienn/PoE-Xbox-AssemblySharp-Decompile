using System;
using System.Collections.Generic;
using System.Linq;
using AnimationOrTween;
using UnityEngine;

public class UIWindowManager : MonoBehaviour
{
	public static bool s_windowsInitialized = false;

	private List<UIHudWindow> m_Windows;

	public WindowList WindowPrefabs;

	public string WindowPrefabsString;

	public GameObject MessageBoxPrefab;

	private static List<UIMessageBox> m_MessageBoxPool = new List<UIMessageBox>();

	public GameObject MessageBoxLitePrefab;

	private static List<UIMessageBox> m_MessageBoxLitePool = new List<UIMessageBox>();

	[Tooltip("The item inspect box.")]
	public GameObject ExamineBoxPrefab;

	[Tooltip("The double-wide item inspect box.")]
	public GameObject ExamineBoxLargePrefab;

	public GameObject StringPromptPrefab;

	public UITweener DimBackgroundTween;

	private bool m_WindowHasBgDimmed;

	private bool m_WindowHasGamePaused;

	private bool m_WindowHasHudHidden;

	private bool m_WindowHasInputDisabled;

	private int m_WindowWantsHideHud;

	private const int HIDE_HUD_FRAME_DELAY = 2;

	public GameObject NonDimBackgroundObject;

	private bool m_IBlockedCursor;

	private int m_BackgroundFwd;

	private int m_NDBackgroundFwd;

	private bool m_KeyInputAvailable = true;

	private bool m_MouseInputAvailable = true;

	private static int s_DisableVisibilityHandling = 0;

	public static UIWindowManager Instance { get; private set; }

	public static bool KeyInputAvailable
	{
		get
		{
			if (Instance != null)
			{
				return Instance.m_KeyInputAvailable;
			}
			return true;
		}
	}

	public static bool MouseInputAvailable
	{
		get
		{
			if (Instance != null)
			{
				return Instance.m_MouseInputAvailable;
			}
			return true;
		}
	}

	public event UIHudWindow.WindowHiddenDelegate OnWindowHidden;

	private void Awake()
	{
		LevelStartWrapperEnter.WriteLineToLog("AWAKE: " + GetType().ToString() + ", GameObject = " + base.gameObject);
		Instance = this;
		UIGlobalSelectAPartyMember.Initialize();
		GameState.OnLevelUnload += OnLevelUnload;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
			s_windowsInitialized = false;
		}
		if (WindowPrefabs == null && !string.IsNullOrEmpty(WindowPrefabsString))
		{
			GameResources.ClearPrefabReference(WindowPrefabsString);
		}
		GameState.OnLevelUnload -= OnLevelUnload;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnLevelUnload(object sender, EventArgs e)
	{
		CloseAllWindows();
	}

	public static ISelectACharacter FindParentISelectACharacter(Transform trs)
	{
		Transform parent = trs.parent;
		while ((bool)parent)
		{
			IEnumerable<MonoBehaviour> source = from m in parent.GetComponents<MonoBehaviour>()
				where m is ISelectACharacter
				select m;
			if (source.Any())
			{
				return (ISelectACharacter)source.First();
			}
			parent = parent.parent;
		}
		return null;
	}

	private static int CompareWindowPriorities(UIHudWindow x, UIHudWindow y)
	{
		return y.Priority.CompareTo(x.Priority);
	}

	private UIHudWindow CreateWindow(GameObject pfb)
	{
		if (pfb == null)
		{
			Debug.LogError("UIWindowManager.CreateWindow: window prefab is null.");
		}
		GameObject gameObject = null;
		foreach (Transform item in base.transform)
		{
			if ((bool)item && (bool)item.gameObject && item.gameObject.name.Equals(pfb.name))
			{
				gameObject = item.gameObject;
				break;
			}
		}
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(pfb);
		}
		Transform obj = gameObject.transform;
		obj.parent = base.transform;
		obj.localScale = new Vector3(1f, 1f, 1f);
		obj.localPosition = new Vector3(0f, 0f, -7f);
		UIHudWindow component = gameObject.GetComponent<UIHudWindow>();
		component.Init();
		if ((bool)component.Window)
		{
			component.Window.SetActive(value: false);
		}
		return component;
	}

	private void Start()
	{
		LevelStartWrapperEnter.WriteLineToLog("START: " + GetType().ToString() + ", GameObject = " + base.gameObject);
		WindowList windowList = GetWindowList();
		if (windowList.Windows != null)
		{
			m_Windows = new List<UIHudWindow>(windowList.Windows.Length);
			GameObject[] windows = windowList.Windows;
			foreach (GameObject gameObject in windows)
			{
				if ((bool)gameObject)
				{
					UIHudWindow item = CreateWindow(gameObject);
					m_Windows.Add(item);
				}
			}
			m_Windows.Sort(CompareWindowPriorities);
			s_windowsInitialized = true;
		}
		if (DimBackgroundTween != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(DimBackgroundTween.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnBgClicked));
		}
		if (NonDimBackgroundObject != null)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(NonDimBackgroundObject.gameObject);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnBgClicked));
		}
		if (DimBackgroundTween != null)
		{
			DimBackgroundTween.Play(forward: false);
		}
		if (NonDimBackgroundObject != null)
		{
			NonDimBackgroundObject.SetActive(value: false);
		}
	}

	public void ToggleWindow(Type type)
	{
		for (int num = m_Windows.Count - 1; num >= 0; num--)
		{
			UIHudWindow uIHudWindow = m_Windows[num];
			if (uIHudWindow.GetType() == type)
			{
				uIHudWindow.Toggle();
				break;
			}
		}
	}

	public void ShowWindow(Type type)
	{
		for (int num = m_Windows.Count - 1; num >= 0; num--)
		{
			UIHudWindow uIHudWindow = m_Windows[num];
			if (uIHudWindow.GetType() == type)
			{
				uIHudWindow.ShowWindow();
				break;
			}
		}
	}

	public void HideWindow(Type type)
	{
		for (int num = m_Windows.Count - 1; num >= 0; num--)
		{
			UIHudWindow uIHudWindow = m_Windows[num];
			if (uIHudWindow.GetType() == type)
			{
				uIHudWindow.HideWindow();
				break;
			}
		}
	}

	private WindowList GetWindowList()
	{
		WindowList result = null;
		if (WindowPrefabs != null)
		{
			result = WindowPrefabs;
		}
		else if (!string.IsNullOrEmpty(WindowPrefabsString))
		{
			result = GameResources.LoadPrefab<WindowList>(WindowPrefabsString, instantiate: false);
		}
		return result;
	}

	public bool IsFocused(UIHudWindow window)
	{
		if (!window.WindowActive())
		{
			return false;
		}
		for (int i = 0; i < m_Windows.Count; i++)
		{
			if (m_Windows[i] == window)
			{
				return true;
			}
			if (m_Windows[i].WindowActive() && m_Windows[i].EatsKeyInput)
			{
				return false;
			}
		}
		return window.WindowActive();
	}

	public void RecreateWindow(Type type)
	{
		int num = 0;
		foreach (UIHudWindow window in m_Windows)
		{
			if (window.GetType() == type)
			{
				WindowList windowList = GetWindowList();
				GameObject pfb = null;
				GameObject[] windows = windowList.Windows;
				foreach (GameObject gameObject in windows)
				{
					if (gameObject.GetComponent<UIHudWindow>().GetType() == type)
					{
						pfb = gameObject;
						break;
					}
				}
				m_Windows[num] = CreateWindow(pfb);
				GameUtilities.Destroy(window.gameObject);
				break;
			}
			num++;
		}
	}

	private void Update()
	{
		m_KeyInputAvailable = true;
		m_MouseInputAvailable = true;
		if (m_Windows == null)
		{
			return;
		}
		if ((bool)InGameHUD.Instance)
		{
			if (AnyWindowHidingHud())
			{
				if (m_WindowWantsHideHud > 0)
				{
					m_WindowWantsHideHud--;
					if (!m_WindowHasHudHidden && m_WindowWantsHideHud <= 0)
					{
						m_WindowHasHudHidden = true;
						InGameHUD.Instance.ShowHUD = false;
					}
				}
			}
			else if (m_WindowHasHudHidden)
			{
				m_WindowHasHudHidden = false;
				InGameHUD.Instance.ShowHUD = true;
			}
		}
		int num = 0;
		if (GameInput.GetControlUp(MappedControl.MENU_CYCLE_LEFT))
		{
			num = -1;
		}
		if (GameInput.GetControlUp(MappedControl.MENU_CYCLE_RIGHT))
		{
			num = 1;
		}
		if (num != 0)
		{
			int num2 = -1;
			int num3 = 0;
			for (int i = 0; i < m_Windows.Count; i++)
			{
				if (m_Windows[i].WindowActive() && m_Windows[i].CyclePosition >= 0)
				{
					num2 = m_Windows[i].CyclePosition;
				}
				num3 = Mathf.Max(m_Windows[i].CyclePosition, num3);
			}
			num3++;
			if (num2 >= 0)
			{
				bool flag = false;
				int num4 = (num2 + num + num3) % num3;
				while (!flag && num4 != num2)
				{
					for (int j = 0; j < m_Windows.Count; j++)
					{
						if (flag)
						{
							break;
						}
						if (m_Windows[j].CyclePosition == num4)
						{
							m_Windows[j].ShowWindow();
							flag = true;
						}
					}
					num4 = (num4 + num + num3) % num3;
				}
			}
		}
		for (int k = 0; k < m_Windows.Count; k++)
		{
			if (m_Windows[k] != null && m_Windows[k].WindowActive() && m_KeyInputAvailable)
			{
				m_Windows[k].HandleVisibilityInput1();
				m_KeyInputAvailable = !m_Windows[k].EatsKeyInput && m_KeyInputAvailable;
			}
		}
		m_KeyInputAvailable = true;
		if ((bool)UIGlobalInventory.Instance)
		{
			UIGlobalInventory.Instance.HandleInput();
		}
		for (int l = 0; l < m_Windows.Count; l++)
		{
			UIHudWindow uIHudWindow = m_Windows[l];
			if (uIHudWindow == null)
			{
				continue;
			}
			if (s_DisableVisibilityHandling == 0)
			{
				uIHudWindow.HandleVisibilityInput2();
			}
			if (!uIHudWindow.WindowActive())
			{
				continue;
			}
			if (m_KeyInputAvailable || m_MouseInputAvailable)
			{
				if (uIHudWindow.FirstFrame)
				{
					uIHudWindow.FirstFrame = false;
				}
				else
				{
					uIHudWindow.HandleInput();
				}
			}
			m_KeyInputAvailable = !uIHudWindow.EatsKeyInput && m_KeyInputAvailable;
			m_MouseInputAvailable = !uIHudWindow.EatsMouseInput && m_MouseInputAvailable;
		}
		if (!m_KeyInputAvailable)
		{
			GameInput.HandleAllKeys();
		}
		if (!m_MouseInputAvailable)
		{
			GameInput.HandleAllClicks();
			m_IBlockedCursor = true;
			GameCursor.UiCursor = GameCursor.CursorType.Normal;
		}
		else if (m_IBlockedCursor)
		{
			m_IBlockedCursor = false;
			GameCursor.UiCursor = GameCursor.CursorType.None;
		}
	}

	private void OnBgClicked(GameObject go)
	{
		for (int i = 0; i < m_Windows.Count; i++)
		{
			UIHudWindow uIHudWindow = m_Windows[i];
			if (uIHudWindow.WindowActive())
			{
				GameInput.HandleAllClicks();
				if (uIHudWindow.ClickOffCloses)
				{
					uIHudWindow.HideWindow();
				}
				break;
			}
		}
	}

	public void WindowCreated(UIHudWindow window)
	{
		m_Windows.Add(window);
		m_Windows.Sort(CompareWindowPriorities);
	}

	public void WindowDestroyed(UIHudWindow window)
	{
		m_Windows.Remove(window);
	}

	public void PoolMessageBox(UIMessageBox box)
	{
		if (box.ButtonLayout == UIMessageBox.ButtonStyle.LITE)
		{
			if (!m_MessageBoxLitePool.Contains(box))
			{
				m_MessageBoxLitePool.Add(box);
			}
		}
		else if (!m_MessageBoxPool.Contains(box))
		{
			m_MessageBoxPool.Add(box);
		}
	}

	public static UIMessageBox ShowMessageBox(UIMessageBox.ButtonStyle buttons, string title, string text)
	{
		return ShowMessageBox(buttons, title, text, -1);
	}

	public static UIMessageBox ShowMessageBox(UIMessageBox.ButtonStyle buttons, string title, string text, int checkboxStringId)
	{
		UIMessageBox uIMessageBox;
		if (buttons == UIMessageBox.ButtonStyle.LITE)
		{
			m_MessageBoxLitePool.RemoveAll((UIMessageBox b) => b == null);
			if (m_MessageBoxLitePool.Count > 0)
			{
				uIMessageBox = m_MessageBoxLitePool[0];
				m_MessageBoxLitePool.RemoveAt(0);
			}
			else
			{
				uIMessageBox = CreateMessageBox(Instance.MessageBoxLitePrefab) as UIMessageBox;
			}
		}
		else
		{
			m_MessageBoxPool.RemoveAll((UIMessageBox b) => b == null);
			if (m_MessageBoxPool.Count > 0)
			{
				uIMessageBox = m_MessageBoxPool[0];
				m_MessageBoxPool.RemoveAt(0);
			}
			else
			{
				uIMessageBox = CreateMessageBox(Instance.MessageBoxPrefab) as UIMessageBox;
			}
		}
		uIMessageBox.Reset(clearCallback: true);
		uIMessageBox.SetButtons(buttons);
		uIMessageBox.SetData(title, text);
		uIMessageBox.SetCheckBoxLabel(checkboxStringId);
		uIMessageBox.ShowWindow();
		return uIMessageBox;
	}

	public static UIStringPromptBox ShowStringPrompt(int titleId, string text)
	{
		UIStringPromptBox obj = CreateMessageBox(Instance.StringPromptPrefab) as UIStringPromptBox;
		obj.SetCurrentText(text);
		obj.TitleString.StringID = titleId;
		obj.ShowWindow();
		return obj;
	}

	private static UIHudWindow CreateMessageBox(GameObject prefab)
	{
		UIHudWindow component = NGUITools.AddChild(Instance.gameObject, prefab).GetComponent<UIHudWindow>();
		Instance.WindowCreated(component.GetComponent<UIHudWindow>());
		component.Init();
		return component;
	}

	public void SetMessageBoxBackgroundDepth()
	{
		float num = -2f;
		foreach (UIHudWindow window in m_Windows)
		{
			if (window.MessageBox && window.WindowActive())
			{
				num = Mathf.Min(num, window.transform.localPosition.z);
			}
		}
		DimBackgroundTween.transform.localPosition = new Vector3(0f, 0f, num + 2f);
		NonDimBackgroundObject.transform.localPosition = new Vector3(0f, 0f, num + 2f);
	}

	public float NextMessageBoxDepth(UIHudWindow window)
	{
		float num = -16f;
		foreach (UIHudWindow window2 in m_Windows)
		{
			if (window2 != window && window2.MessageBox && window2.WindowActive())
			{
				num = Mathf.Min(num, window2.transform.localPosition.z - 9f);
			}
		}
		return num;
	}

	public void BackgroundToDepth(float box)
	{
		m_BackgroundFwd++;
		DimBackgroundTween.transform.localPosition = new Vector3(0f, 0f, box + 2f);
	}

	public void NonDimBackgroundToDepth(float box)
	{
		m_NDBackgroundFwd = 0;
		NonDimBackgroundObject.transform.localPosition = new Vector3(0f, 0f, box + 2f);
	}

	public void BackgroundToDefaultDepth()
	{
		m_BackgroundFwd--;
		if (m_BackgroundFwd < 0)
		{
			m_BackgroundFwd = 0;
		}
		if (m_BackgroundFwd == 0)
		{
			DimBackgroundTween.transform.localPosition = new Vector3(0f, 0f, -2f);
		}
		m_NDBackgroundFwd--;
		if (m_NDBackgroundFwd < 0)
		{
			m_NDBackgroundFwd = 0;
		}
		if (m_NDBackgroundFwd == 0)
		{
			NonDimBackgroundObject.transform.localPosition = new Vector3(0f, 0f, -2f);
		}
	}

	public static void IncreaseSpriteDepthRecursive(GameObject go, int depth)
	{
		UISprite[] componentsInChildren = go.GetComponentsInChildren<UISprite>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].depth += depth;
		}
	}

	public bool IsWindowActive(Type windowType)
	{
		for (int i = 0; i < m_Windows.Count; i++)
		{
			if (m_Windows[i].WindowActive() && windowType.IsAssignableFrom(m_Windows[i].GetType()))
			{
				return true;
			}
		}
		return false;
	}

	public void SuspendFor(UIHudWindow window)
	{
		for (int num = m_Windows.Count - 1; num >= 0; num--)
		{
			UIHudWindow uIHudWindow = m_Windows[num];
			if (uIHudWindow != window && uIHudWindow.WindowActive())
			{
				window.IAmSuspending.Add(uIHudWindow);
				uIHudWindow.Suspend();
			}
		}
	}

	public void HideFor(UIHudWindow window)
	{
		for (int num = m_Windows.Count - 1; num >= 0; num--)
		{
			UIHudWindow uIHudWindow = m_Windows[num];
			if (uIHudWindow != window && uIHudWindow.WindowActive())
			{
				uIHudWindow.HideWindow();
			}
		}
	}

	public bool WindowCanShow(UIHudWindow window)
	{
		if (!window.CanShow())
		{
			return false;
		}
		if (window.MessageBox)
		{
			return true;
		}
		for (int i = 0; i < m_Windows.Count; i++)
		{
			UIHudWindow uIHudWindow = m_Windows[i];
			if (uIHudWindow == window)
			{
				return true;
			}
			if (uIHudWindow.WindowActive() && !uIHudWindow.Replaceable)
			{
				return false;
			}
		}
		Debug.LogError("An unknown UIHudWindow tried to show ('" + window.name + "').");
		return true;
	}

	public void WindowShown(UIHudWindow window)
	{
		if ((bool)window.SwitcherAnchor && (bool)UIWindowSwitcher.Instance && !GameState.PartyDead)
		{
			UIWindowSwitcher.Instance.Show(window.SwitcherAnchor);
		}
		if (window.HidesHud)
		{
			if (m_WindowWantsHideHud == 0)
			{
				m_WindowWantsHideHud = 2;
			}
			if ((bool)InGameHUD.Instance)
			{
				InGameHUD.Instance.HidePause = true;
			}
		}
		if (!m_WindowHasGamePaused && window.PausesGame)
		{
			m_WindowHasGamePaused = true;
			if ((bool)TimeController.Instance)
			{
				TimeController.Instance.UiPaused = true;
			}
		}
		if (!m_WindowHasBgDimmed && (window.DimsBackground || window.DimsBackgroundTemp))
		{
			m_WindowHasBgDimmed = true;
			if ((bool)DimBackgroundTween)
			{
				DimBackgroundTween.gameObject.SetActive(value: true);
				DimBackgroundTween.Play(forward: true);
			}
			else
			{
				Debug.LogError("UIWindowManager WindowShown: DimBackgroundTween == NULL.");
			}
		}
		else if (window.ClickOffCloses)
		{
			if ((bool)NonDimBackgroundObject)
			{
				NonDimBackgroundObject.SetActive(value: true);
			}
			else
			{
				Debug.LogError("UIWindowManager WindowShown: NonDimBackgroundObject == NULL.");
			}
		}
		if (!m_WindowHasInputDisabled && window.EatsKeyInput)
		{
			m_WindowHasInputDisabled = true;
			if ((bool)CameraControl.Instance)
			{
				CameraControl.Instance.EnablePlayerControl(enableControl: false);
			}
		}
	}

	public bool HideReplaceableWindows()
	{
		bool flag = true;
		for (int num = m_Windows.Count - 1; num >= 0; num--)
		{
			UIHudWindow uIHudWindow = m_Windows[num];
			if ((bool)uIHudWindow && uIHudWindow.WindowActive() && uIHudWindow.Replaceable)
			{
				flag = flag && uIHudWindow.HideWindow();
			}
		}
		for (int num2 = m_Windows.Count - 1; num2 >= 0; num2--)
		{
			UIHudWindow uIHudWindow2 = m_Windows[num2];
			if ((bool)uIHudWindow2 && uIHudWindow2.WindowActive() && uIHudWindow2.Replaceable)
			{
				flag = flag && uIHudWindow2.HideWindow();
			}
		}
		return flag;
	}

	public void WindowHidden(UIHudWindow window, bool unsuspend)
	{
		if (unsuspend)
		{
			foreach (UIHudWindow item in window.IAmSuspending)
			{
				item.Unsuspend();
			}
			window.IAmSuspending.Clear();
		}
		if ((bool)UIAbilityTooltipManager.Instance)
		{
			UIAbilityTooltipManager.Instance.HideAll();
		}
		UIActionBarTooltip.GlobalHide();
		if ((bool)UIWindowSwitcher.Instance && UIWindowSwitcher.Instance.Anchor.widgetContainer == window.SwitcherAnchor)
		{
			UIWindowSwitcher.Instance.Hide();
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		foreach (UIHudWindow window2 in m_Windows)
		{
			if (window2.WindowActive() && window2 != window)
			{
				flag6 |= window2.EatsKeyInput;
				flag |= window2.HidesHud;
				flag3 = window2.EatsMouseInput || flag3;
				flag2 = window2.PausesGame || flag2;
				flag4 = window2.DimsBackground || window2.DimsBackgroundTemp || flag4;
				flag5 |= window2.ClickOffCloses && !window2.DimsBackground;
			}
		}
		if (m_WindowHasInputDisabled && !flag6)
		{
			m_WindowHasInputDisabled = false;
			if ((bool)CameraControl.Instance)
			{
				CameraControl.Instance.EnablePlayerControl(enableControl: true);
			}
		}
		if (!flag && (bool)InGameHUD.Instance)
		{
			InGameHUD.Instance.HidePause = false;
		}
		if (m_WindowHasGamePaused && !flag2)
		{
			m_WindowHasGamePaused = false;
			if ((bool)TimeController.Instance)
			{
				TimeController.Instance.UiPaused = false;
			}
		}
		if (m_WindowHasBgDimmed && !flag4)
		{
			m_WindowHasBgDimmed = false;
			DimBackgroundTween.Play(forward: false);
		}
		if (!flag5)
		{
			NonDimBackgroundObject.SetActive(value: false);
		}
		if (this.OnWindowHidden != null)
		{
			this.OnWindowHidden(window);
		}
	}

	public void CloseAllWindows()
	{
		if (m_Windows == null)
		{
			return;
		}
		for (int num = m_Windows.Count - 1; num >= 0; num--)
		{
			UIHudWindow uIHudWindow = m_Windows[num];
			if (uIHudWindow != null && uIHudWindow.WindowActive())
			{
				uIHudWindow.HideWindow(force: true);
			}
		}
	}

	public bool AnyWindowShowing()
	{
		for (int i = 0; i < m_Windows.Count; i++)
		{
			if ((bool)m_Windows[i] && m_Windows[i].WindowActive())
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyWindowHidingHud()
	{
		for (int i = 0; i < m_Windows.Count; i++)
		{
			if (m_Windows[i].WindowActive() && m_Windows[i].HidesHud)
			{
				return true;
			}
		}
		return false;
	}

	public bool AllWindowsReplaceable()
	{
		for (int i = 0; i < m_Windows.Count; i++)
		{
			if (m_Windows[i].WindowActive() && !m_Windows[i].Replaceable)
			{
				return false;
			}
		}
		return true;
	}

	private void OnDimTweenEnded(UITweener tweener)
	{
		if (tweener.direction == Direction.Reverse)
		{
			DimBackgroundTween.gameObject.SetActive(value: false);
		}
	}

	public static void DisableWindowVisibilityHandling()
	{
		s_DisableVisibilityHandling++;
	}

	public static void EnableWindowVisibilityHandling()
	{
		s_DisableVisibilityHandling = Mathf.Max(0, s_DisableVisibilityHandling - 1);
	}

	public static void ForceFullEnableWindowVisibilityHandling()
	{
		s_DisableVisibilityHandling = 0;
	}
}
