namespace AI.Player;

public class WaitForSceneTransition : Wait
{
	private SceneTransition m_sceneTransition;

	private bool m_cancelled;

	public SceneTransition TransitionObject
	{
		get
		{
			return m_sceneTransition;
		}
		set
		{
			m_sceneTransition = value;
		}
	}

	public override void Reset()
	{
		base.Reset();
		m_sceneTransition = null;
		m_cancelled = false;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_ai.CancelAllEngagements();
	}

	public override void Update()
	{
		base.Update();
		if (m_cancelled)
		{
			base.Manager.PopCurrentState();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if ((bool)m_sceneTransition)
		{
			m_sceneTransition.CancelTransition();
		}
	}

	public override void OnAbort()
	{
		base.OnAbort();
		if ((bool)m_sceneTransition)
		{
			m_sceneTransition.CancelTransition();
		}
	}

	public void Cancel()
	{
		m_cancelled = true;
	}

	public override bool TurnWhilePaused()
	{
		return false;
	}
}
