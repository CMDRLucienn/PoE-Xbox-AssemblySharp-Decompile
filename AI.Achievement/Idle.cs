using UnityEngine;

namespace AI.Achievement;

public class Idle : GameAIState
{
	protected FidgetController m_fidgetController = new FidgetController();

	public override void Reset()
	{
		base.Reset();
		m_fidgetController.Reset();
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_owner == null)
		{
			Debug.LogError("No owner set for idle state!");
			base.Manager.PopCurrentState();
		}
		else if (!m_ai.MoveToRetreatPosition())
		{
			InitFidgetController(m_fidgetController, null);
			if (m_animation != null)
			{
				m_animation.DesiredAction.m_actionType = AnimationController.ActionType.None;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		m_fidgetController.Update();
	}

	public override bool IsIdling()
	{
		return true;
	}
}
