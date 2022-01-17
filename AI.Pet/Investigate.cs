using AI.Achievement;
using UnityEngine;

namespace AI.Pet;

public class Investigate : PetBaseAI
{
	private PathToPosition.Params m_params = new PathToPosition.Params();

	private PathingController m_pathingController = new PathingController();

	public override void Reset()
	{
		base.Reset();
		m_params.Reset();
		m_pathingController.Reset();
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_params.Destination = m_master.transform.position + GameUtilities.V2ToV3(Random.insideUnitCircle * Random.Range(3.5f, 4.5f));
		if (!GameUtilities.LineofSight(m_master.transform.position, m_params.Destination, 1f, includeDynamics: false))
		{
			base.Manager.PopCurrentState();
			return;
		}
		if (!GameUtilities.IsPositionOnNavMesh(m_params.Destination))
		{
			base.Manager.PopCurrentState();
			return;
		}
		m_params.MovementType = AnimationController.MovementType.Walk;
		m_params.Range = 0.5f;
		m_ai.Mover.enabled = true;
		m_pathingController.Init(this, m_params, startPathing: true);
	}

	public override void Update()
	{
		base.Update();
		if (m_master == null)
		{
			base.Manager.PopCurrentState();
			return;
		}
		m_pathingController.Update();
		if (m_distToMasterSq > 36f)
		{
			base.Manager.PopCurrentState();
		}
		else if (m_pathingController.ReachedDestination())
		{
			base.Manager.PopCurrentState();
		}
	}
}
