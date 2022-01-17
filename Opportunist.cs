using UnityEngine;

[ClassTooltip("When the owner flanks an enemy, that enemy recieves the Opportunist Afflictions as long as the flank lasts.")]
public class Opportunist : GenericAbility
{
	public Affliction[] OpportunistAfflictions;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			CharacterStats component = Owner.GetComponent<CharacterStats>();
			if (component != null)
			{
				component.OnBeginFlanking += OnBeginFlank;
				component.OnEndFlanking += OnEndFlank;
			}
			m_permanent = true;
		}
	}

	private void OnBeginFlank(GameObject sender, GameObjectEventArgs args)
	{
		CharacterStats characterStats = (args.Object ? args.Object.GetComponent<CharacterStats>() : null);
		if (!characterStats)
		{
			return;
		}
		for (int i = 0; i < OpportunistAfflictions.Length; i++)
		{
			if ((bool)OpportunistAfflictions[i])
			{
				characterStats.ApplyAffliction(OpportunistAfflictions[i]);
			}
		}
	}

	private void OnEndFlank(GameObject sender, GameObjectEventArgs args)
	{
		CharacterStats characterStats = (args.Object ? args.Object.GetComponent<CharacterStats>() : null);
		if (!characterStats)
		{
			return;
		}
		for (int i = 0; i < OpportunistAfflictions.Length; i++)
		{
			if ((bool)OpportunistAfflictions[i])
			{
				characterStats.ClearEffectFromAffliction(OpportunistAfflictions[i]);
			}
		}
	}
}
