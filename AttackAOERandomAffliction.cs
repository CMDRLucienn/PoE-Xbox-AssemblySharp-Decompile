using UnityEngine;

[AddComponentMenu("Attacks/AOERandomAffliction")]
public class AttackAOERandomAffliction : AttackAOE
{
	protected override void Init()
	{
		base.Init();
		ApplyOneRandomAffliction = true;
	}
}
