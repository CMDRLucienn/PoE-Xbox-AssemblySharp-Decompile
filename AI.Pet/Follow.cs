using System;
using AI.Achievement;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Pet;

public class Follow : PetBaseAI
{
	private PathToPosition.Params m_params = new PathToPosition.Params();

	private PathingController m_pathingController = new PathingController();

	public override void OnEnter()
	{
		base.OnEnter();
		m_params.Destination = m_master.transform.position + GameUtilities.V2ToV3(UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(2.5f, 4.5f));
		m_params.Target = m_master;
		m_params.Range = 1.5f;
		if (m_distToMasterSq >= 36f)
		{
			m_params.MovementType = AnimationController.MovementType.Run;
		}
		else
		{
			m_params.MovementType = AnimationController.MovementType.Walk;
		}
		m_ai.Mover.enabled = true;
		m_pathingController.Init(this, m_params, startPathing: true);
		m_pathingController.Mover.OnMovementBlocked += m_OnMovementBlocked;
	}

	private void m_OnMovementBlocked(object sender, EventArgs e)
	{
		NavMeshHit hit;
		if (m_owner == null)
		{
			if (sender is GameObject)
			{
				m_ai.Mover.OnMovementBlocked -= m_OnMovementBlocked;
			}
		}
		else if (m_ai.Mover.GoalUnreachable && NavMesh.Raycast(m_master.transform.position, m_owner.transform.position, out hit, -1))
		{
			m_owner.transform.position = hit.position;
		}
	}

	public override void Update()
	{
		base.Update();
		m_pathingController.Update();
		if (m_pathingController.ReachedDestination())
		{
			base.Manager.PopCurrentState();
		}
	}
}
