using System;
using System.Collections.Generic;
using AI.Achievement;
using AI.Player;
using UnityEngine;

public class UIActionBarOnClick : MonoBehaviour
{
	public enum ActionType
	{
		Defend,
		Attack,
		Cancel,
		Stealth,
		Pack,
		Formation,
		SelectAll,
		Inventory,
		Journal,
		Map,
		Pause,
		Options,
		Character,
		ResetCamera,
		Stronghold,
		SlowMo,
		Camp,
		FastMo
	}

	private delegate void ActionFunction();

	public ActionType action;

	public MappedControl Hotkey;

	public bool TriggerByHotkey = true;

	public GUIDatabaseString TooltipName;

	public GameObject AlertTarget;

	private ActionFunction function;

	private TweenColor m_Alerter;

	private UIImageButtonRevised m_Button;

	private GameObject m_ModalActiveVfx;

	private Vector3 m_lastModalActiveWorldPosition;

	private void Start()
	{
		ActionFunction[] array = new ActionFunction[18]
		{
			HandleDefend, HandleAttack, HandleCancel, HandleStealth, HandlePack, HandleFormation, HandleSelectAll, HandleInventory, HandleJournal, HandleMap,
			HandlePause, HandleOptions, HandleCharacter, HandleResetCamera, HandleStronghold, HandleSlowMo, HandleCamp, HandleFastMo
		};
		function = array[(int)action];
		if ((bool)AlertTarget)
		{
			UIHudAlerts.OnAlertStart = (UIHudAlerts.HudAlertDelegate)Delegate.Combine(UIHudAlerts.OnAlertStart, new UIHudAlerts.HudAlertDelegate(OnAlert));
			UIHudAlerts.OnAlertEnd = (UIHudAlerts.HudAlertDelegate)Delegate.Combine(UIHudAlerts.OnAlertEnd, new UIHudAlerts.HudAlertDelegate(OnAlertCancel));
			m_Alerter = AlertTarget.AddComponent<TweenColor>();
			m_Alerter.from = Color.white;
			m_Alerter.to = Color.black;
			m_Alerter.style = UITweener.Style.PingPong;
			m_Alerter.duration = 0.4f;
			m_Alerter.Reset();
			m_Alerter.enabled = false;
		}
		m_Button = GetComponent<UIImageButtonRevised>();
		if (action == ActionType.Stealth)
		{
			PartyMemberAI.OnAnySelectionChanged += OnStealthModeChanged;
			Stealth.GlobalOnAnyStealthStateChanged += OnStealthModeChanged;
		}
		GameState.OnLevelLoaded += OnLevelLoaded;
		Stronghold instance = Stronghold.Instance;
		instance.OnUpgradeStatusChanged = (Stronghold.UpgradeStatusChanged)Delegate.Combine(instance.OnUpgradeStatusChanged, new Stronghold.UpgradeStatusChanged(OnUpgradeStatusChanged));
		GameResources.OnLoadedSave += OnGameLoaded;
	}

	private void OnDestroy()
	{
		UIHudAlerts.OnAlertStart = (UIHudAlerts.HudAlertDelegate)Delegate.Remove(UIHudAlerts.OnAlertStart, new UIHudAlerts.HudAlertDelegate(OnAlert));
		UIHudAlerts.OnAlertEnd = (UIHudAlerts.HudAlertDelegate)Delegate.Remove(UIHudAlerts.OnAlertEnd, new UIHudAlerts.HudAlertDelegate(OnAlertCancel));
		if (action == ActionType.Stealth)
		{
			PartyMemberAI.OnAnySelectionChanged -= OnStealthModeChanged;
			Stealth.GlobalOnAnyStealthStateChanged -= OnStealthModeChanged;
		}
		GameState.OnLevelLoaded -= OnLevelLoaded;
		if ((bool)Stronghold.Instance)
		{
			Stronghold instance = Stronghold.Instance;
			instance.OnUpgradeStatusChanged = (Stronghold.UpgradeStatusChanged)Delegate.Remove(instance.OnUpgradeStatusChanged, new Stronghold.UpgradeStatusChanged(OnUpgradeStatusChanged));
		}
		GameResources.OnLoadedSave -= OnGameLoaded;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnAlert(ActionType type)
	{
		if (type == action && (bool)m_Alerter)
		{
			m_Alerter.Play(forward: true);
		}
	}

	private void OnAlertCancel(ActionType type)
	{
		if (type == action)
		{
			ClearAlert();
		}
	}

	private void ClearAlert()
	{
		if ((bool)m_Alerter)
		{
			m_Alerter.Reset();
			m_Alerter.enabled = false;
			m_Alerter.GetComponent<UIWidget>().color = m_Alerter.to;
		}
	}

	private void Update()
	{
		if (!GameState.IsLoading && Hotkey != 0 && TriggerByHotkey && GameInput.GetControlDown(Hotkey, handle: true) && UIWindowManager.KeyInputAvailable && function != null)
		{
			if (function != new ActionFunction(HandlePause))
			{
				CancelModes();
			}
			function();
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
		}
		if ((bool)TimeController.Instance)
		{
			if (action == ActionType.FastMo)
			{
				if ((bool)m_Button)
				{
					m_Button.ChangeNormalSprite(TimeController.Instance.Fast ? "FF_active" : "FF");
				}
			}
			else if (action == ActionType.SlowMo && (bool)m_Button)
			{
				m_Button.ChangeNormalSprite(TimeController.Instance.Slow ? "SLOW_active" : "SLOW");
			}
		}
		if ((bool)m_ModalActiveVfx)
		{
			if (m_lastModalActiveWorldPosition != m_ModalActiveVfx.transform.position)
			{
				m_ModalActiveVfx.SetActive(value: false);
			}
			else if (!m_ModalActiveVfx.activeSelf)
			{
				OnStealthModeChanged(null, null);
			}
			m_lastModalActiveWorldPosition = m_ModalActiveVfx.transform.position;
		}
	}

	private void OnClick()
	{
		GameInput.HandleAllClicks();
		if (function != null)
		{
			if (function != new ActionFunction(HandlePause))
			{
				CancelModes();
			}
			function();
		}
	}

	private void CancelModes()
	{
		if ((bool)GameState.s_playerCharacter)
		{
			GameState.s_playerCharacter.CancelModes(cancelAbility: true);
		}
	}

	private void OnTooltip(bool bShow)
	{
		if (bShow)
		{
			string text = "";
			if (Hotkey != 0 && !GameState.Controls.ControlEmpty(Hotkey))
			{
				text = " [" + GameState.Controls.GetControlString(Hotkey) + "]";
			}
			UIActionBarTooltip.GlobalShow(GetComponentInChildren<UIWidget>(), TooltipName.GetText() + text);
		}
		else
		{
			UIActionBarTooltip.GlobalHide();
		}
	}

	private void OnStealthModeChanged(object sender, EventArgs e)
	{
		List<GameObject> list = new List<GameObject>(PartyMemberAI.GetSelectedPartyMembers());
		if (list.Count > 0 && Stealth.IsInStealthMode(list[0]))
		{
			if ((bool)m_ModalActiveVfx)
			{
				GameUtilities.Destroy(m_ModalActiveVfx);
				m_ModalActiveVfx = null;
			}
			if (!m_ModalActiveVfx)
			{
				m_ModalActiveVfx = UIAbilityBar.Instance.InstantiateModalVfx(base.transform.parent, new Vector3(base.transform.localPosition.x - 2f, base.transform.parent.localPosition.y + 2f, -1f));
				m_ModalActiveVfx.transform.localScale = new Vector3(240f, 240f, 240f);
				m_lastModalActiveWorldPosition = m_ModalActiveVfx.transform.position;
			}
			m_ModalActiveVfx.SetActive(value: true);
		}
		else if (m_ModalActiveVfx != null)
		{
			m_ModalActiveVfx.SetActive(value: false);
		}
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		if (action == ActionType.Stealth)
		{
			OnStealthModeChanged(null, null);
		}
		UpdateIcon();
	}

	private void OnUpgradeStatusChanged(StrongholdUpgrade.Type type)
	{
		UpdateIcon();
	}

	private void UpdateIcon()
	{
		if (action == ActionType.Camp)
		{
			if (UIDisableIfFreeRestAvailable.FreeRestAvailable())
			{
				m_Button.ChangeNormalSprite("ICO_rest_stronghold");
			}
			else
			{
				m_Button.ChangeNormalSprite("ICO_rest");
			}
		}
	}

	private void OnGameLoaded()
	{
		ClearAlert();
	}

	private void HandleDefend()
	{
	}

	private void HandleAttack()
	{
		GameState.s_playerCharacter.IsInForceAttackMode = true;
	}

	private void HandleCancel()
	{
		GameState.s_playerCharacter.CancelModes(cancelAbility: true);
		SceneTransition.CancelAllSceneTransitions();
		GameObject[] selectedPartyMembers = PartyMemberAI.SelectedPartyMembers;
		foreach (GameObject gameObject in selectedPartyMembers)
		{
			if (!(gameObject != null) || !(gameObject.GetComponent<PuppetModeController>() == null))
			{
				continue;
			}
			PartyMemberAI component = gameObject.GetComponent<PartyMemberAI>();
			Move move = component.StateManager.CurrentState as Move;
			bool flag = false;
			Vector3 vector = gameObject.transform.position;
			component.StateManager.ClearQueuedStates();
			if (!GameState.InCombat)
			{
				vector = GameUtilities.NearestUnoccupiedLocation(gameObject.transform.position, component.Mover.Radius, 6f, component.Mover);
				if (GameUtilities.V3SqrDistance2D(vector, gameObject.transform.position) > 0.01f)
				{
					flag = true;
				}
			}
			AIState currentState = component.StateManager.CurrentState;
			AI.Achievement.Attack attack = currentState as AI.Achievement.Attack;
			if (currentState != null && (currentState.Priority < 2 || (attack != null && attack.CanUserInterrupt())))
			{
				attack?.OnCancel();
				if (!flag || move == null)
				{
					component.SafePopAllStates();
				}
			}
			if (flag)
			{
				if (move == null)
				{
					move = AIStateManager.StatePool.Allocate<Move>();
					component.StateManager.QueueState(move);
				}
				move.Destination = vector;
				move.Range = 0.05f;
				move.ShowDestinationCircle = false;
				if (component.Mover.enabled)
				{
					move.OnEnter();
				}
			}
		}
	}

	private void HandleStealth()
	{
		List<GameObject> list = new List<GameObject>(PartyMemberAI.GetSelectedPartyMembers());
		bool flag = list.Count > 0 && Stealth.IsInStealthMode(list[0]);
		foreach (GameObject item in list)
		{
			Stealth.SetInStealthMode(item, !flag);
		}
	}

	private void HandlePack()
	{
	}

	private void HandleFormation()
	{
		UIFormationSet.Instance.gameObject.SetActive(!UIFormationSet.Instance.gameObject.activeSelf);
	}

	private void HandleSelectAll()
	{
		PartyMemberAI.SelectAll();
	}

	private void HandleInventory()
	{
		UIInventoryManager.Instance.Toggle();
	}

	private void HandleJournal()
	{
		UIJournalManager.Instance.Toggle();
	}

	private void HandleMap()
	{
		UIAreaMapManager.Instance.Toggle();
	}

	private void HandlePause()
	{
		TimeController.Instance.SafePaused = !TimeController.Instance.Paused;
	}

	private void HandleOptions()
	{
		UIOptionsManager.Instance.Toggle();
	}

	private void HandleCharacter()
	{
		UICharacterSheetManager.Instance.Toggle();
	}

	private void HandleResetCamera()
	{
		SyncCameraOrthoSettings instance = SyncCameraOrthoSettings.Instance;
		if ((bool)instance)
		{
			instance.SetZoomLevel(1f, force: false);
		}
	}

	private void HandleStronghold()
	{
		UIStrongholdManager.Instance.Toggle();
	}

	private void HandleSlowMo()
	{
		TimeController.Instance.ToggleSlow();
	}

	private void HandleCamp()
	{
		RestZone.ShowCampUI();
	}

	private void HandleFastMo()
	{
		TimeController.Instance.ToggleFast();
	}
}
