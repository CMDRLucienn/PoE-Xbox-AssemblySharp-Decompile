using AI.Achievement;
using UnityEngine;

namespace AI.Plan;

public class CasterScanForTarget : GameAIState
{
	protected CasterTargetScanner m_targetScanner;

	protected FidgetController m_fidgetController = new FidgetController();

	public CasterTargetScanner TargetScanner
	{
		get
		{
			if (m_targetScanner == null)
			{
				m_targetScanner = new CasterTargetScanner();
			}
			return m_targetScanner;
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (m_targetScanner != null)
		{
			m_targetScanner.Reset();
		}
		if (m_fidgetController != null)
		{
			m_fidgetController.Reset();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (PackageController != null && PackageController.PreferredPatrolPoint != null)
		{
			Waypoint waypoint = GetCurrentWaypoint();
			if (waypoint == null)
			{
				waypoint = PackageController.PreferredPatrolPoint.GetComponent<Waypoint>();
			}
			if (waypoint != null && (GameUtilities.V3SqrDistance2D(waypoint.transform.position, m_owner.transform.position) > 0.25f || waypoint.NextWaypoint != null))
			{
				Patrol patrol = PushState<Patrol>();
				patrol.StartPoint = waypoint;
				patrol.TargetScanner = TargetScanner;
				return;
			}
		}
		StopMover();
		Equipment component = m_owner.GetComponent<Equipment>();
		if (component != null)
		{
			component.SelectWeaponSet(0, enforceRecoveryPenalty: false);
		}
		InitFidgetController(m_fidgetController, TargetScanner);
		if (m_animation != null)
		{
			m_animation.DesiredAction.m_actionType = AnimationController.ActionType.None;
		}
	}

	public override void Update()
	{
		base.Update();
		if (GameState.Paused || (FogOfWar.PointVisibleInFog(m_ai.gameObject.transform.position) && (TargetScanner.ScanForTarget(m_owner, m_ai, -1f, ignoreIfCurrentTarget: false) || base.Manager.CurrentState != this)))
		{
			return;
		}
		Waypoint currentWaypoint = GetCurrentWaypoint();
		if (currentWaypoint != null && currentWaypoint.AmbientAnimation != 0)
		{
			Wait wait = PushState<Wait>();
			wait.Duration = currentWaypoint.PauseTime;
			wait.ForwardFacing = currentWaypoint.transform.forward;
			wait.WaitForFidgetToFinish = true;
			if (currentWaypoint.LoopCount >= 0)
			{
				wait.Setup(AnimationController.ActionType.Ambient, currentWaypoint.AmbientVariation, currentWaypoint.LoopCount);
			}
			else
			{
				wait.Setup(AnimationController.ActionType.Ambient, currentWaypoint.AmbientVariation);
			}
		}
		else if (!m_ai.MoveToRetreatPosition())
		{
			m_fidgetController.Update();
		}
	}

	public bool ConditionalIsValid(SpellCastData data, GameObject target)
	{
		return TargetScanner.ConditionalIsValid(m_owner, data, target);
	}

	private Waypoint GetCurrentWaypoint()
	{
		if (m_ai.CurrentWaypoint != null)
		{
			return m_ai.CurrentWaypoint;
		}
		if (m_ai.PrevWaypoint != null)
		{
			return m_ai.PrevWaypoint;
		}
		if (PackageController != null && PackageController.PreferredPatrolPoint != null)
		{
			return PackageController.PreferredPatrolPoint.GetComponent<Waypoint>();
		}
		return null;
	}
}
