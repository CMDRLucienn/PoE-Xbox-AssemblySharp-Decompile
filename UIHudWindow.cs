using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIHudWindow : MonoBehaviour
{
	public enum WindowType
	{
		Area_Map,
		Chant_Editor,
		Character_Creation,
		Character_Sheet,
		Conversation,
		Crafting,
		Death,
		End_Game_Slides,
		Formations,
		Grimoire,
		Interstitial,
		Inventory,
		Items_Popup,
		Journal,
		Loot,
		Map,
		Memorial_Manager,
		Options,
		Party_Management,
		Rest_Box,
		Save_Load,
		Store,
		Stronghold,
		Stronghold_Companion_Picker,
		Stash_Popup,
		Character_Customizer,
		GENERAL
	}

	public enum WindowActionType
	{
		Open,
		Close
	}

	public delegate void HandleInputDelegate();

	public delegate void WindowHiddenDelegate(UIHudWindow window);

	public delegate void WindowShownDelegate();

	public bool PausesGame = true;

	public bool HidesHud;

	public bool DimsBackground = true;

	[HideInInspector]
	public bool DimsBackgroundTemp;

	public bool InReplaceGroup = true;

	public bool Replaceable;

	public bool EatsKeyInput = true;

	public bool EatsMouseInput = true;

	public int Priority = 5;

	public bool ClickOffCloses = true;

	public bool EscapeCloses = true;

	public bool MessageBox;

	public bool ResolutionInsensitive;

	[HideInInspector]
	public bool CanDeactivate = true;

	public MappedControl ToggleKey;

	public MappedControl AltKey;

	public WindowType Window_Type = WindowType.GENERAL;

	[HideInInspector]
	public bool FirstFrame = true;

	public Collider DragHandle;

	public GameObject Window;

	public UIPanel OverPanel;

	private List<UIHudWindow> m_IAmSuspending = new List<UIHudWindow>();

	private bool m_Suspended;

	public HandleInputDelegate OnHandleInput;

	public WindowHiddenDelegate OnWindowHidden;

	public WindowShownDelegate OnWindowShown;

	private bool m_Visible;

	private bool m_IsShowing;

	private bool m_ShowFailed;

	private bool m_IsHiding;

	public bool IsVisible => m_Visible;

	protected bool AlternateMode { get; private set; }

	public List<UIHudWindow> IAmSuspending => m_IAmSuspending;

	public UIWidget SwitcherAnchor { get; private set; }

	public virtual int CyclePosition => -1;

	protected virtual void OnDestroy()
	{
		if (WindowActive() && (bool)UIWindowManager.Instance)
		{
			UIWindowManager.Instance.WindowHidden(this, unsuspend: true);
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public bool WindowActive()
	{
		if (m_Visible && !m_Suspended)
		{
			return !m_IsHiding;
		}
		return false;
	}

	public void Suspend()
	{
		m_Suspended = true;
		if (m_Visible)
		{
			Suspended();
			DoSuspend();
			if ((bool)OverPanel)
			{
				OverPanel.Refresh();
			}
			UIWindowManager.Instance.WindowHidden(this, unsuspend: false);
		}
	}

	public void Unsuspend()
	{
		if (!m_Suspended)
		{
			return;
		}
		m_Suspended = false;
		if (m_Visible)
		{
			Unsuspended();
			DoUnsuspend();
			if ((bool)OverPanel)
			{
				OverPanel.Refresh();
			}
			UIWindowManager.Instance.WindowShown(this);
		}
	}

	protected virtual void DoSuspend()
	{
		if ((bool)Window)
		{
			Window.SetActive(value: false);
		}
	}

	protected virtual void DoUnsuspend()
	{
		if ((bool)Window)
		{
			Window.SetActive(value: true);
		}
	}

	public void Init()
	{
		UIWindowSwitcherAnchor componentInChildren = GetComponentInChildren<UIWindowSwitcherAnchor>();
		if ((bool)componentInChildren)
		{
			SwitcherAnchor = componentInChildren.GetComponent<UIWidget>();
		}
		if ((bool)DragHandle)
		{
			UIEventListener uIEventListener = UIEventListener.Get(DragHandle.gameObject);
			uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragged));
		}
		if (OverPanel == null)
		{
			OverPanel = GetComponent<UIPanel>();
		}
		if (ResolutionInsensitive && !GetComponent<UIResolutionScaler>())
		{
			UIResolutionScaler uIResolutionScaler = Window.AddComponent<UIResolutionScaler>();
			uIResolutionScaler.DesignedHeight = 900;
			uIResolutionScaler.MaxUpscaleX = 1920;
			uIResolutionScaler.MaxUpscaleY = 1080;
			uIResolutionScaler.ScaleZValue = true;
		}
	}

	protected void OnDragged(GameObject go, Vector2 delta)
	{
		if ((bool)Window)
		{
			SetPosition(Window.transform.localPosition + (Vector3)delta);
		}
	}

	public void SetPosition(Vector2 pend)
	{
		if (!Window)
		{
			return;
		}
		Transform transform = Window.transform;
		UIAnchor component = Window.GetComponent<UIAnchor>();
		if (!component || !component.enabled)
		{
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(transform);
			float num = (InGameUILayout.Root ? ((float)InGameUILayout.Root.activeHeight / 2f) : 540f);
			float num2 = num * (float)Screen.width / (float)Screen.height;
			float num3 = 0f - num2 - (pend.x - bounds.extents.x);
			if (num3 > 0f)
			{
				pend.x += num3;
			}
			num3 = 0f - num - (pend.y - bounds.extents.y);
			if (num3 > 0f)
			{
				pend.y += num3;
			}
			num3 = num2 - (pend.x + bounds.extents.x);
			if (num3 < 0f)
			{
				pend.x += num3;
			}
			num3 = num - (pend.y + bounds.extents.y);
			if (num3 < 0f)
			{
				pend.y += num3;
			}
			transform.localPosition = pend;
		}
	}

	public virtual void HandleInput()
	{
		if (OnHandleInput != null)
		{
			OnHandleInput();
		}
	}

	public bool HideWindow(bool force = false)
	{
		if (m_IsHiding)
		{
			return true;
		}
		if ((bool)UIAbilityTooltipManager.Instance)
		{
			UIAbilityTooltipManager.Instance.HideAll();
		}
		if (m_IsShowing)
		{
			m_ShowFailed = true;
		}
		else if (m_Visible)
		{
			bool flag = true;
			try
			{
				m_IsHiding = true;
				flag = Hide(force);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
			}
			finally
			{
				m_IsHiding = false;
			}
			if (flag || force)
			{
				GlobalAudioPlayer.SPlay(Window_Type, WindowActionType.Close);
				m_ShowFailed = false;
				m_Visible = false;
				DimsBackgroundTemp = false;
				AlternateMode = false;
				if ((bool)Window)
				{
					Window.SetActive(value: false);
					if ((bool)OverPanel)
					{
						OverPanel.Refresh();
					}
				}
				if (OnWindowHidden != null)
				{
					OnWindowHidden(this);
				}
				UIWindowManager.Instance.WindowHidden(this, unsuspend: true);
				UIWindowManager.Instance.BackgroundToDefaultDepth();
				TutorialManager.TutorialTrigger trigger = new TutorialManager.TutorialTrigger(TutorialManager.TriggerType.UIWINDOW_CLOSED);
				trigger.WindowType = Window_Type;
				TutorialManager.STriggerTutorialsOfType(trigger);
			}
			return flag;
		}
		return true;
	}

	public bool ShowWindow()
	{
		if (!m_Visible)
		{
			if (!UIWindowManager.Instance.WindowCanShow(this))
			{
				return false;
			}
			if (InReplaceGroup && !UIWindowManager.Instance.HideReplaceableWindows())
			{
				return false;
			}
			if (!InReplaceGroup && !MessageBox)
			{
				UIWindowManager.Instance.HideFor(this);
			}
			m_IsShowing = true;
			FirstFrame = true;
			m_Visible = true;
			m_Suspended = false;
			if ((bool)Window)
			{
				Window.SetActive(value: true);
				if ((bool)OverPanel)
				{
					OverPanel.Refresh();
				}
			}
			GlobalAudioPlayer.SPlay(Window_Type, WindowActionType.Open);
			if (MessageBox)
			{
				base.gameObject.transform.localPosition = new Vector3(base.gameObject.transform.localPosition.x, base.gameObject.transform.localPosition.y, UIWindowManager.Instance.NextMessageBoxDepth(this));
			}
			if ((DimsBackground || DimsBackgroundTemp) && MessageBox)
			{
				UIWindowManager.Instance.BackgroundToDepth(base.transform.localPosition.z);
			}
			else if (ClickOffCloses && MessageBox)
			{
				UIWindowManager.Instance.NonDimBackgroundToDepth(base.transform.localPosition.z);
			}
			try
			{
				Show();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
			}
			m_IsShowing = false;
			if (m_ShowFailed)
			{
				HideWindow();
				return false;
			}
			if ((bool)Window)
			{
				SetPosition(Window.transform.localPosition);
			}
			UIWindowManager.Instance.WindowShown(this);
			TutorialManager.TutorialTrigger trigger = new TutorialManager.TutorialTrigger(TutorialManager.TriggerType.UIWINDOW_OPENED);
			trigger.WindowType = Window_Type;
			TutorialManager.STriggerTutorialsOfType(trigger);
			if (OnWindowShown != null)
			{
				OnWindowShown();
			}
			return true;
		}
		return false;
	}

	protected virtual bool Hide(bool forced)
	{
		return true;
	}

	protected virtual void Show()
	{
	}

	protected virtual void Suspended()
	{
	}

	protected virtual void Unsuspended()
	{
	}

	public virtual bool CanShow()
	{
		return true;
	}

	public bool Toggle()
	{
		AlternateMode = false;
		return ToggleHelper();
	}

	public bool ToggleAlt()
	{
		AlternateMode = true;
		return ToggleHelper();
	}

	private bool ToggleHelper()
	{
		if (m_Visible)
		{
			if (WindowActive())
			{
				return HideWindow();
			}
			return false;
		}
		return ShowWindow();
	}

	public void HandleVisibilityInput1()
	{
		if (WindowActive() && EscapeCloses && GameInput.GetControlDown(MappedControl.CLOSE_WINDOW, handle: true) && HideWindow())
		{
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
		}
	}

	public void HandleVisibilityInput2()
	{
		if (ToggleKey != 0 && GameInput.GetControlDown(ToggleKey, handle: true))
		{
			if ((!WindowActive() || UIWindowManager.Instance.IsFocused(this)) && Toggle())
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
			}
		}
		else if (AltKey != 0 && GameInput.GetControlDown(AltKey, handle: true) && (!WindowActive() || UIWindowManager.Instance.IsFocused(this)))
		{
			ToggleAlt();
		}
	}
}
