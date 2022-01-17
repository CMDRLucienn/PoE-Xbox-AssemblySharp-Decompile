using System;
using AI.Achievement;
using UnityEngine;

public class OCL : Usable, iCanBeDetected
{
	public enum State
	{
		Closed,
		Open,
		Locked,
		Sealed,
		SealedOpen
	}

	public State StartingState;

	public float UseRadius = 2f;

	public float ArrivalDistance;

	public Item Key;

	public bool RemoveUsedKey = true;

	public Item LockPickItem;

	public int LockDifficulty = 1;

	public bool MustHaveKey;

	protected ScriptEvent m_ScriptEvent;

	[Persistent]
	protected State m_currentState;

	protected AudioBank m_audioBank;

	protected Trap m_trap;

	protected PE_Collider2D m_collider2d;

	protected bool m_Hovered;

	protected bool m_Tooltipped;

	private GameObject m_lastLockUser;

	private float m_TooltipTimer = UICamera.tooltipDelay;

	public override float UsableRadius => UseRadius;

	public override float ArrivalRadius => ArrivalDistance;

	public override bool IsUsable
	{
		get
		{
			if (m_currentState != State.Sealed && m_currentState != State.SealedOpen)
			{
				return base.IsVisible;
			}
			return false;
		}
	}

	public State CurrentState => m_currentState;

	protected override void Start()
	{
		base.Start();
		m_currentState = StartingState;
		if (StartingState == State.Open || StartingState == State.SealedOpen)
		{
			bool num = StartingState == State.SealedOpen;
			m_currentState = State.Closed;
			Open(null, ignoreLock: false);
			if (num)
			{
				m_currentState = State.SealedOpen;
			}
			else
			{
				m_currentState = State.Open;
			}
		}
		m_audioBank = GetComponent<AudioBank>();
		m_ScriptEvent = GetComponent<ScriptEvent>();
		m_trap = GetComponent<Trap>();
		m_collider2d = GetComponent<PE_Collider2D>();
		if ((bool)m_collider2d)
		{
			m_collider2d.OnHover += OnColliderHover;
		}
	}

	protected virtual void Update()
	{
		if (m_Hovered && !m_Tooltipped)
		{
			m_TooltipTimer -= TimeController.sUnscaledDelta;
			if (m_TooltipTimer <= 0f)
			{
				OnColliderTooltip(base.gameObject, over: true);
				m_TooltipTimer = UICamera.tooltipDelay;
			}
		}
		if (GameCursor.ObjectUnderCursor == base.gameObject && !GameState.s_playerCharacter.IsDragSelecting)
		{
			if (!m_Hovered)
			{
				OnColliderHover(base.gameObject, over: true);
			}
		}
		else if (m_Hovered)
		{
			OnColliderHover(base.gameObject, over: false);
		}
	}

	private void OnColliderTooltip(GameObject go, bool over)
	{
		if (CurrentState == State.Locked)
		{
			if (over)
			{
				UILockedDoorTooltip.GlobalShow(this);
			}
			else
			{
				UILockedDoorTooltip.GlobalHide();
			}
		}
		else
		{
			UILockedDoorTooltip.GlobalHide();
		}
		m_Tooltipped = over;
	}

	private void OnColliderHover(GameObject go, bool over)
	{
		m_Hovered = over;
		if (!over && m_Tooltipped)
		{
			OnColliderTooltip(go, over: false);
		}
	}

	public void OnDetection()
	{
		if (m_trap == null)
		{
			m_currentState = State.Closed;
			if (m_collider2d != null)
			{
				m_collider2d.RenderLines = true;
			}
		}
	}

	public void DefaultAction()
	{
		Open();
	}

	public virtual bool Open()
	{
		return Open(null, ignoreLock: false);
	}

	public virtual bool Open(GameObject user, bool ignoreLock)
	{
		if (m_currentState == State.Open || m_currentState == State.SealedOpen)
		{
			return false;
		}
		if ((m_currentState == State.Locked || m_currentState == State.Sealed) && !ignoreLock)
		{
			return false;
		}
		m_currentState = State.Open;
		if (!GameState.IsLoading)
		{
			if (m_ScriptEvent != null)
			{
				SpecialCharacterInstanceID.Add(user, SpecialCharacterInstanceID.SpecialCharacterInstance.User);
				m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnOpen);
			}
			if (m_audioBank != null)
			{
				m_audioBank.PlayFrom("Open");
			}
		}
		if (m_trap == null)
		{
			PE_Collider2D component = GetComponent<PE_Collider2D>();
			if (component != null)
			{
				component.RenderLines = false;
			}
		}
		return true;
	}

	public virtual void SealOpen()
	{
		if (m_currentState != State.Open)
		{
			Open(null, ignoreLock: true);
		}
		m_currentState = State.SealedOpen;
	}

	public virtual void Unseal()
	{
		if (m_currentState == State.SealedOpen)
		{
			m_currentState = State.Open;
		}
		else if (m_currentState == State.Sealed)
		{
			m_currentState = State.Closed;
		}
	}

	public virtual bool Close(GameObject user)
	{
		if (m_currentState == State.Closed || m_currentState == State.Locked || m_currentState == State.Sealed || m_currentState == State.SealedOpen)
		{
			return false;
		}
		m_currentState = State.Closed;
		if (m_ScriptEvent != null)
		{
			SpecialCharacterInstanceID.Add(user, SpecialCharacterInstanceID.SpecialCharacterInstance.User);
			m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnClose);
		}
		if (m_audioBank != null)
		{
			m_audioBank.PlayFrom("Close");
		}
		return true;
	}

	public bool Toggle()
	{
		return Toggle(null, ignoreLock: false);
	}

	public bool Toggle(GameObject user, bool ignoreLock)
	{
		if (m_currentState == State.Sealed || m_currentState == State.SealedOpen)
		{
			return false;
		}
		if (m_currentState == State.Closed)
		{
			return Open(user, ignoreLock);
		}
		return Close(user);
	}

	public bool Lock(GameObject user)
	{
		if (m_currentState == State.Open || m_currentState == State.Sealed || m_currentState == State.SealedOpen)
		{
			return false;
		}
		m_currentState = State.Locked;
		if (m_ScriptEvent != null)
		{
			SpecialCharacterInstanceID.Add(user, SpecialCharacterInstanceID.SpecialCharacterInstance.User);
			m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnLocked);
		}
		return true;
	}

	public bool Unlock(GameObject user)
	{
		m_currentState = State.Closed;
		UILockedDoorTooltip.GlobalHide();
		if (m_audioBank != null)
		{
			m_audioBank.PlayFrom("Unlock");
		}
		UIHealthstringManager.Instance.ShowNotice(GUIUtils.GetText(1891), base.gameObject, 3f);
		if (m_ScriptEvent != null)
		{
			SpecialCharacterInstanceID.Add(user, SpecialCharacterInstanceID.SpecialCharacterInstance.User);
			m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnUnlocked);
		}
		return true;
	}

	public void ToggleLock(GameObject user)
	{
		if (m_currentState == State.Locked)
		{
			Unlock(user);
		}
		else
		{
			Lock(user);
		}
	}

	private void LockedUsed(GameObject user)
	{
		if (m_ScriptEvent != null)
		{
			SpecialCharacterInstanceID.Add(user, SpecialCharacterInstanceID.SpecialCharacterInstance.User);
			m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnLockUsed);
		}
		if (m_audioBank != null)
		{
			m_audioBank.PlayFrom("LockUsed");
		}
	}

	private bool TryLockpick(GameObject user)
	{
		int num = RequiredLockpicks(user);
		bool flag = CanPickLock(user);
		if (!MustHaveKey && flag)
		{
			if (num <= 0)
			{
				StartPickingLock(user);
				return true;
			}
			if (PartyHelper.PartyItemCount(LockPickItem) >= num)
			{
				ShowLockpickUI(user);
				return true;
			}
		}
		CharacterStats characterStats = (user ? user.GetComponent<CharacterStats>() : null);
		int num2 = (characterStats ? characterStats.CalculateSkill(CharacterStats.SkillType.Mechanics) : 0);
		if (flag)
		{
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(2164), LockDifficulty, CharacterStats.NameColored(user), num2, num));
		}
		else
		{
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(2165), LockDifficulty, CharacterStats.NameColored(user), num2));
		}
		GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.LockpickFailed);
		UIHealthstringManager.Instance.ShowNotice(GUIUtils.GetText(30), base.gameObject, 3f);
		return false;
	}

	public void ShowLockpickUI(GameObject user)
	{
		int num = PartyHelper.PartyItemCount(LockPickItem);
		m_lastLockUser = user;
		UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.YESNO, GUIUtils.GetText(292), GUIUtils.Format(293, RequiredLockpicks(user), GUIUtils.Format(1860, num)));
		uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, new UIMessageBox.OnEndDialog(OnConfirmDialog));
	}

	private void OnConfirmDialog(UIMessageBox.Result result, UIMessageBox owner)
	{
		if (m_currentState == State.Locked && result == UIMessageBox.Result.AFFIRMATIVE)
		{
			StartPickingLock(m_lastLockUser);
		}
	}

	private void StartPickingLock(GameObject user)
	{
		m_lastLockUser = user;
		AIController component = user.GetComponent<AIController>();
		if (component == null)
		{
			TriggerLockPick(user);
			return;
		}
		LockPick lockPick = AIStateManager.StatePool.Allocate<LockPick>();
		lockPick.OCL = this;
		component.StateManager.PushState(lockPick);
	}

	public void TriggerLockPick(GameObject user)
	{
		PartyHelper.PartyDestroyItem(LockPickItem, RequiredLockpicks(user));
		PartyMemberAI component = user.GetComponent<PartyMemberAI>();
		if (component != null && component.SoundSet != null)
		{
			component.SoundSet.PlaySound(user, SoundSet.SoundAction.TaskComplete);
		}
		GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.LockpickSuccess);
		AwardLockpickXp();
		Unlock(user);
	}

	public virtual bool TryToUnlockWithKey(GameObject user)
	{
		if (Key == null)
		{
			return false;
		}
		if (PartyHelper.PartyHasItem(Key))
		{
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(283), Key.Name));
			Unlock(user);
			m_currentState = State.Closed;
			if (RemoveUsedKey)
			{
				PartyHelper.PartyDestroyItem(Key, 1);
			}
			return true;
		}
		return false;
	}

	public bool PartyHasKey()
	{
		if (Key == null)
		{
			return false;
		}
		return PartyHelper.PartyHasItem(Key);
	}

	private void AwardLockpickXp()
	{
		int num = BonusXpManager.Instance.PickLockXpModifier * LockDifficulty;
		Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(1650), LockDifficulty, num * PartyHelper.NumPartyMembers), Color.yellow);
		PartyHelper.AssignXPToParty(num, printMessage: false);
	}

	public override bool Use(GameObject user)
	{
		FireUseAudio();
		switch (m_currentState)
		{
		case State.Closed:
			if (m_trap != null)
			{
				m_trap.TrapUseFromOCL(user);
			}
			else
			{
				Open(user, ignoreLock: false);
			}
			return true;
		case State.Open:
			Close(user);
			return true;
		case State.Locked:
		{
			if (m_trap != null && !m_trap.Disarmed && m_trap.Visible)
			{
				m_trap.TrapUseFromOCL(user);
				return true;
			}
			if (TryToUnlockWithKey(user))
			{
				return true;
			}
			bool result = false;
			if (MustHaveKey)
			{
				Console.AddMessage(GUIUtils.GetText(414));
			}
			else
			{
				result = TryLockpick(user);
			}
			LockedUsed(user);
			return result;
		}
		default:
			return false;
		}
	}

	public bool CanPickLock(GameObject user)
	{
		if ((bool)user)
		{
			CharacterStats component = user.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				return CanPickLock(component.CalculateSkill(CharacterStats.SkillType.Mechanics));
			}
		}
		return false;
	}

	public bool CanPickLock(int skill)
	{
		return skill >= LockDifficulty - 1;
	}

	public int RequiredLockpicks(GameObject user)
	{
		if ((bool)user)
		{
			CharacterStats component = user.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				int skill = component.CalculateSkill(CharacterStats.SkillType.Mechanics);
				return RequiredLockpicks(skill);
			}
		}
		return 0;
	}

	public int RequiredLockpicks(int skill)
	{
		int num = skill - LockDifficulty;
		if (num < 0)
		{
			return 3;
		}
		if (num == 0)
		{
			return 1;
		}
		return 0;
	}
}
