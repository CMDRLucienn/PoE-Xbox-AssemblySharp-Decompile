using UnityEngine;

namespace AI.Pet;

public class Idle : PetBaseAI
{
	private FidgetController m_fidgetController = new FidgetController();

	private float m_idleTime;

	public override void Reset()
	{
		base.Reset();
		m_idleTime = 0f;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		StopMover();
		if (m_animation != null)
		{
			m_animation.ClearReactions();
			m_animation.ClearActions();
			m_animation.DesiredAction.Reset();
		}
		InitFidgetController(m_fidgetController, null);
	}

	public override void Update()
	{
		base.Update();
		m_fidgetController.Update();
		m_idleTime += Time.deltaTime;
		if (m_distToMasterSq >= 36f)
		{
			PushState<Follow>();
		}
		else if (m_idleTime > 7f)
		{
			m_idleTime = 0f;
			PushState<Investigate>();
		}
	}
}
