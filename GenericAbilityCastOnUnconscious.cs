using UnityEngine;

[ClassTooltip("A normal ability, but it will also automatically cast itself on the owner if he becomes unconscious.")]
public class GenericAbilityCastOnUnconscious : GenericAbility
{
	public override bool ListenForDamageEvents => true;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			Health component = Owner.GetComponent<Health>();
			if (component != null)
			{
				component.OnUnconscious += HandleHealthOnUnconscious;
			}
		}
	}

	private void HandleHealthOnUnconscious(GameObject myObject, GameEventArgs args)
	{
		if (!m_activated && ReadyIgnoreRecovery && CanApply())
		{
			m_attackBase.SkipAnimation = true;
			ActivateIgnoreRecovery(Owner);
		}
	}
}
