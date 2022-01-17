using System.Collections;
using AI.Achievement;
using AI.Plan;
using UnityEngine;

public class PuppetModeController : MonoBehaviour
{
	protected AIStateManager m_ai;

	protected bool m_isActive;

	protected Mover m_mover;

	protected AlphaControl m_alphaControl;

	protected bool m_isFogVisible;

	public virtual void ActivatePuppetMode()
	{
		m_ai = new AIStateManager(base.gameObject);
		m_ai.AIController = GetComponent<AIController>();
		m_mover = GetComponent<Mover>();
		m_alphaControl = base.gameObject.GetComponent<AlphaControl>();
		Faction component = GetComponent<Faction>();
		if (component != null)
		{
			component.DrawSelectionCircle = false;
		}
		m_ai.Owner = base.gameObject;
		m_ai.SetDefaultState(AIStateManager.StatePool.Allocate<Inactive>());
		m_isActive = true;
	}

	public virtual void DeactivatePuppetMode()
	{
		if (m_isActive)
		{
			if (m_ai != null)
			{
				m_ai.AbortStateStack();
			}
			Faction component = GetComponent<Faction>();
			if (component != null)
			{
				component.DrawSelectionCircle = true;
			}
			Mover component2 = GetComponent<Mover>();
			if (component2 != null)
			{
				component2.Stop();
				component2.enabled = false;
			}
			m_isActive = false;
		}
	}

	protected virtual void OnDestroy()
	{
		DeactivatePuppetMode();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (m_ai != null)
		{
			if (m_ai.AIController != null)
			{
				m_ai.AIController.UpdateFollowSummoner(isCutscene: true, m_ai);
			}
			m_ai.Update();
		}
		if (m_alphaControl == null)
		{
			m_alphaControl = base.gameObject.GetComponent<AlphaControl>();
		}
		if (!(m_alphaControl != null))
		{
			return;
		}
		bool flag = FogOfWar.Instance.PointVisible(base.transform.position);
		if (flag != m_isFogVisible || !m_alphaControl.IsFadeValid())
		{
			m_isFogVisible = flag;
			if (flag)
			{
				m_alphaControl.FadeIn(0.5f);
			}
			else
			{
				m_alphaControl.FadeOut(0.5f);
			}
		}
	}

	public IEnumerator MoveDirectlyToPoint(Vector3 point, float stopDist, bool walk)
	{
		if (m_ai != null)
		{
			m_ai.PopAllStates();
			AnimationController.MovementType movementType;
			if (walk)
			{
				m_mover.UseWalkSpeed();
				movementType = AnimationController.MovementType.Walk;
			}
			else
			{
				m_mover.UseRunSpeed();
				movementType = AnimationController.MovementType.Run;
			}
			PathToPosition pathToPosition = AIStateManager.StatePool.Allocate<PathToPosition>();
			m_ai.PushState(pathToPosition);
			pathToPosition.Parameters.Destination = point;
			pathToPosition.Parameters.Range = stopDist;
			pathToPosition.Parameters.MovementType = movementType;
			if (m_mover != null)
			{
				m_mover.PathDirectly(point, stopDist);
				m_mover.PathToDestination(point, stopDist);
			}
			while (!m_mover.ReachedGoal)
			{
				yield return new WaitForSeconds(0.1f);
			}
			if (m_mover != null)
			{
				m_mover.Stop();
				m_mover.enabled = false;
			}
		}
	}

	public IEnumerator PathToPoint(Vector3 point, float stopDist, bool walk)
	{
		if (m_ai == null)
		{
			yield break;
		}
		m_ai.PopAllStates();
		PathToPosition pathToPosition = AIStateManager.StatePool.Allocate<PathToPosition>();
		m_ai.PushState(pathToPosition);
		pathToPosition.Parameters.Destination = point;
		pathToPosition.Parameters.Range = stopDist;
		pathToPosition.Parameters.MovementType = (walk ? AnimationController.MovementType.Walk : AnimationController.MovementType.Run);
		m_mover.PathToDestination(point, stopDist);
		m_mover.enabled = true;
		do
		{
			if (m_mover != null && m_mover.ReachedGoal)
			{
				m_ai.PopCurrentState();
				m_mover.Stop();
				m_mover.enabled = false;
			}
			yield return new WaitForSeconds(0.1f);
		}
		while (m_ai != null && !(m_ai.CurrentState is Inactive));
	}

	public bool IsInInactiveState()
	{
		if (m_ai == null || m_ai.CurrentState is Inactive)
		{
			return true;
		}
		return false;
	}

	public void StopMovement()
	{
		if (m_mover.enabled)
		{
			m_ai.PopCurrentState();
			m_mover.Stop();
			m_mover.enabled = false;
		}
	}

	public IEnumerator MoveToTarget(GameObject target, float stopDist, bool walk)
	{
		if (m_ai == null)
		{
			yield break;
		}
		m_ai.PopAllStates();
		PathToPosition pathToPosition = AIStateManager.StatePool.Allocate<PathToPosition>();
		m_ai.PushState(pathToPosition);
		pathToPosition.Parameters.Target = target;
		pathToPosition.Parameters.Range = stopDist;
		do
		{
			if ((bool)m_mover && m_mover.ReachedGoal)
			{
				m_ai.PopCurrentState();
				m_mover.Stop();
				m_mover.enabled = false;
			}
			yield return new WaitForSeconds(0.1f);
		}
		while (m_ai != null && !(m_ai.CurrentState is Inactive));
	}

	public IEnumerator LaunchAttack(GameObject target, AttackBase attack, bool allowDamage)
	{
		if ((bool)target && m_ai != null)
		{
			m_ai.PopAllStates();
			if (!allowDamage)
			{
				target.GetComponent<Health>().TakesDamage = false;
			}
			if (Vector3.Distance(base.transform.position, target.transform.position) > attack.TotalAttackDistance)
			{
				yield return StartCoroutine(MoveToTarget(target, attack.TotalIdealAttackDistance, walk: false));
			}
			AttackBase attackBase = attack;
			if (attackBase.transform.parent == null)
			{
				attackBase = Object.Instantiate(attack);
				attackBase.transform.parent = base.transform;
			}
			Attack attack2 = AIStateManager.StatePool.Allocate<Attack>();
			attack2.Parameters.Attack = attackBase;
			attack2.Parameters.TargetObject = target;
			m_ai.PushState(attack2);
			do
			{
				yield return new WaitForSeconds(0.1f);
			}
			while (m_ai != null && !(m_ai.CurrentState is Inactive));
		}
	}

	public IEnumerator LaunchAbility(GameObject target, GenericAbility ability, bool allowDamage)
	{
		GenericAbility genericAbility = Object.Instantiate(ability);
		genericAbility.Owner = base.gameObject;
		genericAbility.transform.parent = base.transform;
		genericAbility.transform.localPosition = Vector3.zero;
		AttackBase component = genericAbility.GetComponent<AttackBase>();
		if (component != null)
		{
			yield return StartCoroutine(LaunchAttack(target, component, allowDamage));
		}
		else
		{
			genericAbility.Activate(target);
		}
	}
}
