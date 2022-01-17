using System.Collections;
using UnityEngine;

[ClassTooltip("After the Attack Hit Time, the attacker will break all engagement and teleport to the targeted location.")]
public class DragonLeap : AttackAOE
{
	public override void Launch(Vector3 location, GameObject enemy, int variationOverride)
	{
		StartCoroutine(TeleportDelay(AttackHitTime, location));
		base.Launch(location, enemy, variationOverride);
	}

	public IEnumerator TeleportDelay(float time, Vector3 position)
	{
		yield return new WaitForSeconds(time);
		TeleportToEnemy(position);
	}

	private void TeleportToEnemy(Vector3 hitPosition)
	{
		AIController.BreakAllEngagements(base.Owner);
		Mover component = base.Owner.GetComponent<Mover>();
		base.Owner.transform.position = GameUtilities.NearestUnoccupiedLocation(hitPosition, component.Radius, 10f, component);
		component.MoveToGround();
	}
}
