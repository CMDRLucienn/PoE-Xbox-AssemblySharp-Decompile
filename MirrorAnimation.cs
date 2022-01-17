using UnityEngine;

public class MirrorAnimation : MonoBehaviour
{
	public AnimationController m_parentController;

	public Mover m_parentMover;

	private AnimationController m_animationController;

	private void Start()
	{
		m_animationController = GetComponent<AnimationController>();
	}

	private void Update()
	{
		if (!(m_parentController == null) && !(m_animationController == null))
		{
			m_animationController.DesiredAction = m_parentController.DesiredAction;
			m_animationController.SetReaction(m_parentController.CurrentReaction);
			if (!(m_parentMover == null))
			{
				m_animationController.OverrideSpeed = m_parentMover.Speed;
			}
		}
	}
}
