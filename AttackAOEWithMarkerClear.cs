using UnityEngine;

[ClassTooltip("Normal AOE, but afterward clears ALL GenericMarkers with tag 'MarkerTag' originating from the attacker (globally).")]
public class AttackAOEWithMarkerClear : AttackAOE
{
	[Tooltip("Tag of GenercMarkers to remove.")]
	public string MarkerTag;

	public override void OnImpact(GameObject self, Vector3 hitPosition)
	{
		base.OnImpact(self, hitPosition);
		RemoveMarkers();
	}

	public override void OnImpact(GameObject self, GameObject enemy, bool isMainTarget)
	{
		base.OnImpact(self, enemy, isMainTarget);
		RemoveMarkers();
	}

	private void RemoveMarkers()
	{
		for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
		{
			CharacterStats component = ComponentUtils.GetComponent<CharacterStats>(Faction.ActiveFactionComponents[i]);
			if (!component)
			{
				continue;
			}
			for (int num = component.ActiveStatusEffects.Count - 1; num >= 0; num--)
			{
				StatusEffect statusEffect = component.ActiveStatusEffects[num];
				if (statusEffect.Owner == base.Owner && statusEffect.Params.AffectsStat == StatusEffect.ModifiedStat.GenericMarker)
				{
					component.ClearEffect(statusEffect);
				}
			}
		}
	}
}
