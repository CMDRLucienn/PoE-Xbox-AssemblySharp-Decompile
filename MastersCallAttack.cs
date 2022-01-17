using UnityEngine;

public class MastersCallAttack : AttackMelee
{
	private bool m_ignoreValidTargets = true;

	private GameObject Master => GameUtilities.FindMaster(base.Owner);

	private void Reset()
	{
		m_ignoreValidTargets = true;
	}

	public override bool ForceTarget(out GameObject hitObject)
	{
		hitObject = Master;
		return hitObject != null;
	}

	public override bool IgnoreValidTargetCheck()
	{
		return m_ignoreValidTargets;
	}

	public override GameObject Launch(GameObject enemy, int variationOverride)
	{
		int num = m_ability.UsesLeft();
		enemy = Master;
		m_ignoreValidTargets = false;
		HandleBeam(base.Owner, enemy, Vector3.zero);
		m_ignoreValidTargets = true;
		TeleportToMaster();
		if (num == m_ability.UsesLeft())
		{
			m_ability.ActivateCooldown();
		}
		m_ability.AttackComplete = true;
		return enemy;
	}

	private void TeleportToMaster()
	{
		GameUtilities.LaunchEffect(OnStartGroundVisualEffect, 1f, base.Owner.transform.position, m_ability);
		Mover component = base.Owner.GetComponent<Mover>();
		Vector3 position = Master.transform.position;
		Vector3 vector = component.transform.position - position;
		vector.y = 0f;
		vector.Normalize();
		position += vector * component.Radius;
		base.Owner.transform.position = GameUtilities.NearestUnoccupiedLocation(position, component.Radius, 10f, component);
		component.MoveToGround();
		base.Owner.transform.rotation = Master.transform.rotation;
		LaunchOnStartVisualEffect();
		GameUtilities.LaunchEffect(OnStartGroundVisualEffect, 1f, base.Owner.transform.position, m_ability);
	}

	public override void OnImpact(GameObject self, GameObject enemy, bool isMainTarget)
	{
		if (enemy != Master)
		{
			base.OnImpact(self, enemy, isMainTarget);
		}
	}
}
