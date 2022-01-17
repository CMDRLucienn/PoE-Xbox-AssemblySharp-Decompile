using UnityEngine;

public abstract class AttackAbility : AttackBase
{
	protected override AnimationController.ActionType ActionType => AnimationController.ActionType.Attack;

	protected override void Start()
	{
		base.Start();
		GenericAbility component = GetComponent<GenericAbility>();
		if ((bool)component)
		{
			m_parent = component.Owner;
			return;
		}
		m_parent = base.gameObject;
		while (m_parent.transform.parent != null)
		{
			m_parent = m_parent.transform.parent.gameObject;
		}
	}

	public override GameObject Launch(GameObject enemy, int variationOverride)
	{
		enemy = base.Launch(enemy, variationOverride);
		GenericAbility component = GetComponent<GenericAbility>();
		if ((bool)component)
		{
			component.Activate(enemy);
		}
		return enemy;
	}

	public override void Launch(Vector3 location, GameObject enemy, int variationOverride)
	{
		base.Launch(location, enemy, variationOverride);
		GenericAbility component = GetComponent<GenericAbility>();
		if ((bool)component)
		{
			component.Activate(location);
		}
	}
}
