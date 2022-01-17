using System;
using System.Collections.Generic;
using System.Text;
using AI.Achievement;
using AI.Plan;
using AI.Player;
using UnityEngine;

public class AIStateManager
{
	public enum FormationType
	{
		BLOCK,
		VEE,
		SINGLE_FILE
	}

	private static Type[] s_pooledStateTypes = new Type[27]
	{
		typeof(AI.Achievement.Attack),
		typeof(Dead),
		typeof(HitReact),
		typeof(Idle),
		typeof(KnockedDown),
		typeof(PathToPosition),
		typeof(Patrol),
		typeof(PerformAction),
		typeof(PerformReaction),
		typeof(AI.Achievement.ReloadWeapon),
		typeof(Stunned),
		typeof(Unconscious),
		typeof(AI.Achievement.Wait),
		typeof(ApproachTarget),
		typeof(CasterScanForTarget),
		typeof(ScanForTarget),
		typeof(AI.Plan.WaitForClearPath),
		typeof(Follow),
		typeof(AI.Player.Ability),
		typeof(AI.Player.Attack),
		typeof(Move),
		typeof(AI.Player.ReloadWeapon),
		typeof(TargetedAttack),
		typeof(UseObject),
		typeof(AI.Player.Wait),
		typeof(AI.Player.WaitForClearPath),
		typeof(WaitForSceneTransition)
	};

	private GameObject m_owner;

	private AIController m_aiController;

	private AIState m_lastUpdatedState;

	private List<AIState> m_stateStack = new List<AIState>(8);

	private List<AIState> m_statesToFree = new List<AIState>(8);

	public static ObjectPool<AIStateManager> StateManagerPool = new ObjectPool<AIStateManager>(128);

	public static MultiTypeObjectPool StatePool = new MultiTypeObjectPool(s_pooledStateTypes, 256);

	public GameObject Owner
	{
		get
		{
			return m_owner;
		}
		set
		{
			m_owner = value;
		}
	}

	public AIController AIController
	{
		get
		{
			return m_aiController;
		}
		set
		{
			m_aiController = value;
		}
	}

	public AIState CurrentState
	{
		get
		{
			if (m_stateStack.Count <= 0)
			{
				return null;
			}
			return m_stateStack[m_stateStack.Count - 1];
		}
	}

	public AIState QueuedState
	{
		get
		{
			if (m_stateStack.Count <= 1)
			{
				return null;
			}
			return m_stateStack[m_stateStack.Count - 2];
		}
	}

	public AIState DefaultState
	{
		get
		{
			if (m_stateStack.Count <= 0)
			{
				return null;
			}
			return m_stateStack[0];
		}
	}

	public bool InCombat => CurrentState?.InCombat ?? false;

	public GameObject CurrentTarget
	{
		get
		{
			if (m_stateStack.Count <= 0)
			{
				return null;
			}
			for (int num = m_stateStack.Count - 1; num >= 0; num--)
			{
				GameObject currentTarget = m_stateStack[num].CurrentTarget;
				if (currentTarget != null)
				{
					return currentTarget;
				}
				if (!m_stateStack[num].UseQueuedTarget)
				{
					break;
				}
			}
			return null;
		}
	}

	public AttackBase CurrentAttack
	{
		get
		{
			if (m_stateStack.Count <= 0)
			{
				return null;
			}
			for (int num = m_stateStack.Count - 1; num >= 0; num--)
			{
				AttackBase currentAttack = m_stateStack[num].CurrentAttack;
				if (currentAttack != null)
				{
					return currentAttack;
				}
				if (!m_stateStack[num].UseQueuedTarget)
				{
					break;
				}
			}
			return null;
		}
	}

	public AIState LastUpdatedState => m_lastUpdatedState;

	public AIStateManager()
	{
	}

	public AIStateManager(GameObject owner)
	{
		m_owner = owner;
	}

	public AIState FindState(Type type)
	{
		for (int num = m_stateStack.Count - 1; num >= 0; num--)
		{
			if (m_stateStack[num].GetType() == type)
			{
				return m_stateStack[num];
			}
		}
		return null;
	}

	public AIState GetQueuedState(int index)
	{
		if (m_stateStack.Count <= index + 1)
		{
			return null;
		}
		return m_stateStack[m_stateStack.Count - index - 1];
	}

	public void SetDefaultState(AIState defaultState)
	{
		if (m_stateStack.Count != 0)
		{
			Debug.LogError("The default AI state cannot be set because the state stack is not empty.");
		}
		defaultState.Init(this, m_owner);
		m_stateStack.Add(defaultState);
	}

	public void DestroyStateStack()
	{
		if (m_stateStack.Count > 0)
		{
			FreeAllStates();
		}
	}

	public void PushState(AIState state)
	{
		PushState(state, clearStack: false);
	}

	public void PushState(AIState state, bool clearStack)
	{
		if (m_stateStack.Count <= 0)
		{
			Debug.LogError("Cannot push a state on to an empty state stack.");
			return;
		}
		AIState currentState = CurrentState;
		if (state == null || currentState == state)
		{
			return;
		}
		bool flag = state.Priority != currentState.Priority || currentState.CanCancel;
		int num = m_stateStack.Count - 1;
		bool flag2 = false;
		if (state.Priority < currentState.Priority)
		{
			if (!currentState.AllowsQueueing || !state.CanBeQueuedIfLowerPriority)
			{
				if (m_lastUpdatedState != state)
				{
					m_statesToFree.Add(state);
				}
				return;
			}
			flag2 = true;
		}
		state.Init(this, m_owner);
		if (!flag || flag2)
		{
			num--;
		}
		if (clearStack)
		{
			for (int num2 = num; num2 > 0; num2--)
			{
				AIState aIState = m_stateStack[num2];
				if (aIState != m_lastUpdatedState)
				{
					m_statesToFree.Add(aIState);
				}
				m_stateStack.RemoveAt(num2);
				aIState.OnCancel();
			}
		}
		if (flag2)
		{
			m_stateStack.Insert(m_stateStack.Count - 1, state);
		}
		else if (!flag)
		{
			m_stateStack.Insert(m_stateStack.Count - 1, state);
			currentState.Interrupt();
		}
		else
		{
			m_stateStack.Add(state);
		}
	}

	public void PopCurrentState()
	{
		if (m_stateStack.Count > 1)
		{
			AIState currentState = CurrentState;
			m_stateStack.RemoveAt(m_stateStack.Count - 1);
			if (currentState != m_lastUpdatedState && !m_statesToFree.Contains(currentState))
			{
				m_statesToFree.Add(currentState);
			}
		}
	}

	public void PopState(AIState state)
	{
		int num = m_stateStack.IndexOf(state);
		if (num > 0)
		{
			AIState aIState = m_stateStack[num];
			m_stateStack.RemoveAt(num);
			if (aIState != m_lastUpdatedState && !m_statesToFree.Contains(aIState))
			{
				m_statesToFree.Add(aIState);
			}
		}
	}

	public void PopState(Type stateType)
	{
		for (int num = m_stateStack.Count - 1; num >= 1; num--)
		{
			if (stateType.IsAssignableFrom(m_stateStack[num].GetType()))
			{
				AIState aIState = m_stateStack[num];
				m_stateStack.RemoveAt(num);
				if (aIState != m_lastUpdatedState && !m_statesToFree.Contains(aIState))
				{
					m_statesToFree.Add(aIState);
				}
				break;
			}
		}
	}

	public void PopStates(Type stateType)
	{
		int num = m_stateStack.Count - 1;
		while (num >= 1 && stateType.IsAssignableFrom(m_stateStack[num].GetType()))
		{
			AIState aIState = m_stateStack[num];
			m_stateStack.RemoveAt(num);
			if (aIState != m_lastUpdatedState && !m_statesToFree.Contains(aIState))
			{
				m_statesToFree.Add(aIState);
			}
			num--;
		}
	}

	public void QueueState(AIState state)
	{
		if (m_stateStack.Count > 0 && !m_stateStack[m_stateStack.Count - 1].AllowsQueueing)
		{
			m_statesToFree.Add(state);
			return;
		}
		if (m_stateStack.Count <= 1)
		{
			PushState(state, clearStack: false);
			return;
		}
		state.Init(this, m_owner);
		m_stateStack.Insert(1, state);
	}

	public void QueueStateAtTop(AIState state)
	{
		if (m_stateStack.Count > 0 && !m_stateStack[m_stateStack.Count - 1].AllowsQueueing)
		{
			m_statesToFree.Add(state);
			return;
		}
		if (m_stateStack.Count <= 1)
		{
			PushState(state, clearStack: false);
			return;
		}
		state.Init(this, m_owner);
		m_stateStack.Insert(m_stateStack.Count - 1, state);
	}

	public void TransferCurrentState(AIStateManager newManager)
	{
		if (m_stateStack.Count > 1)
		{
			AIState currentState = CurrentState;
			m_stateStack.RemoveAt(m_stateStack.Count - 1);
			m_lastUpdatedState = null;
			newManager.PushState(currentState);
			newManager.m_lastUpdatedState = currentState;
		}
	}

	public void AbortStateStack()
	{
		if (CurrentState != null)
		{
			CurrentState.BaseAbort();
		}
		DestroyStateStack();
	}

	public void PopAllStates()
	{
		if (CurrentState != null)
		{
			CurrentState.BaseAbort();
		}
		while (m_stateStack.Count > 1)
		{
			AIState currentState = CurrentState;
			PopCurrentState();
			currentState.OnCancel();
		}
	}

	public void ClearQueuedStates(int depth)
	{
		if (depth < 2)
		{
			return;
		}
		while (m_stateStack.Count > depth)
		{
			AIState item = m_stateStack[1];
			if (!m_statesToFree.Contains(item))
			{
				m_statesToFree.Add(item);
			}
			m_stateStack.RemoveAt(1);
		}
	}

	public void ClearQueuedStates()
	{
		ClearQueuedStates(2);
	}

	public void Update()
	{
		AIState currentState = CurrentState;
		if (GameState.Paused)
		{
			currentState?.UpdateWhenPaused();
			return;
		}
		if (m_lastUpdatedState != currentState)
		{
			ProcessTransition();
		}
		else
		{
			currentState?.BaseUpdate();
		}
		PostUpdate();
	}

	public void PostUpdate()
	{
		if (GameState.Paused || m_statesToFree.Count <= 0)
		{
			return;
		}
		foreach (AIState item in m_statesToFree)
		{
			item.Free();
		}
		m_statesToFree.Clear();
	}

	protected void ProcessTransition()
	{
		if (m_lastUpdatedState != null)
		{
			m_lastUpdatedState.BaseExit();
			if (!IsStateInStack(m_lastUpdatedState) && !m_statesToFree.Contains(m_lastUpdatedState))
			{
				m_statesToFree.Add(m_lastUpdatedState);
			}
			m_lastUpdatedState = null;
		}
		AIState currentState = CurrentState;
		currentState.BaseEnter();
		m_lastUpdatedState = currentState;
		if (m_statesToFree.Contains(m_lastUpdatedState))
		{
			m_statesToFree.Remove(m_lastUpdatedState);
		}
	}

	public bool IsStateInStack(AIState state)
	{
		for (int i = 0; i < m_stateStack.Count; i++)
		{
			if (m_stateStack[i] == state)
			{
				return true;
			}
		}
		return false;
	}

	public void Free()
	{
		if (Owner != null && Owner.GetComponent<PartyMemberAI>() != null)
		{
			Debug.Log(Owner.name + " cleaned up state " + GetType().ToString());
		}
		FreeAllStates();
		Owner = null;
		StateManagerPool.Free(this);
	}

	private void FreeAllStates()
	{
		m_lastUpdatedState = null;
		while (m_stateStack.Count > 0)
		{
			m_stateStack[m_stateStack.Count - 1].Free();
			m_stateStack.RemoveAt(m_stateStack.Count - 1);
		}
		if (m_statesToFree.Count <= 0)
		{
			return;
		}
		foreach (AIState item in m_statesToFree)
		{
			item.Free();
		}
		m_statesToFree.Clear();
	}

	public AttackBase GetCurrentAttack()
	{
		for (int num = m_stateStack.Count - 1; num >= 0; num--)
		{
			AttackBase currentAttack = m_stateStack[num].CurrentAttack;
			if (currentAttack != null)
			{
				return currentAttack;
			}
		}
		return null;
	}

	public bool IsPathingObstacle()
	{
		if (m_aiController != null && m_aiController.IgnoreAsCutsceneObstacle)
		{
			return false;
		}
		return CurrentState?.IsPathingObstacle() ?? false;
	}

	public bool CanBeNudgedBy(Mover pather)
	{
		return CurrentState?.CanBeNudgedBy(pather) ?? false;
	}

	public bool IsMoving()
	{
		return CurrentState?.IsMoving() ?? false;
	}

	public bool IsPathBlocked()
	{
		return CurrentState?.IsPathBlocked() ?? false;
	}

	public bool PerformsSoftSteering()
	{
		return CurrentState?.PerformsSoftSteering() ?? false;
	}

	public bool IsIdling()
	{
		return CurrentState?.IsIdling() ?? false;
	}

	public bool IsPerformingSecondPartOfFullAttack()
	{
		return CurrentState?.IsPerformingSecondPartOfFullAttack ?? false;
	}

	public bool AllowEngagementUpdate()
	{
		return CurrentState?.AllowEngagementUpdate() ?? false;
	}

	public bool IsExecutingDefaultState()
	{
		return m_stateStack.Count == 1;
	}

	public void BuildDebugText(StringBuilder text)
	{
		for (int num = m_stateStack.Count - 1; num >= 0; num--)
		{
			text.AppendLine(m_stateStack[num].GetType().ToString() + m_stateStack[num].GetDebugText());
		}
	}

	public void BaseEnter()
	{
		if (CurrentState != null)
		{
			CurrentState.BaseEnter();
		}
	}

	public void BaseExit()
	{
		if (CurrentState != null)
		{
			CurrentState.BaseExit();
		}
	}

	public void BaseCancel()
	{
		if (CurrentState != null)
		{
			CurrentState.BaseCancel();
		}
	}

	public void BaseAbort()
	{
		if (CurrentState != null)
		{
			CurrentState.BaseAbort();
		}
	}

	public Type GetStateType()
	{
		if (CurrentState == null)
		{
			return null;
		}
		return CurrentState.GetType();
	}

	public virtual void OnEvent(GameEventArgs args)
	{
		if (CurrentState != null)
		{
			CurrentState.OnEvent(args);
		}
	}
}
