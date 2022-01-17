using AI.Achievement;
using UnityEngine;

[AddComponentMenu("AI/Dummy")]
public class AIControllerDummy : AIController
{
	public override void InitAI()
	{
		if (m_ai == null)
		{
			m_ai = AIStateManager.StateManagerPool.Allocate();
			m_ai.Owner = base.gameObject;
			m_ai.AIController = this;
			m_ai.SetDefaultState(AIStateManager.StatePool.Allocate<Idle>());
		}
		else
		{
			m_ai.PopAllStates();
		}
		InitMover();
	}

	protected override void OnDestroy()
	{
		if (m_ai != null)
		{
			m_ai.AbortStateStack();
			AIStateManager.StateManagerPool.Free(m_ai);
			m_ai = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override void Update()
	{
		if (!GameState.IsLoading)
		{
			base.Update();
		}
	}
}
