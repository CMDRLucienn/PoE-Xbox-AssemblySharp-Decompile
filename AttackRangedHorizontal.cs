using UnityEngine;

[ClassTooltip("The ranged attack is launched perpendicularly to the character's facing direction, across the target point.")]
public class AttackRangedHorizontal : AttackRanged
{
	[Tooltip("The height of the projectile off the ground.")]
	public float ProjectileHeight = 1f;

	public override bool OverridesProjectileLaunchPosition => true;

	public override Vector3 GetProjectileLaunchPosition(Vector3 target)
	{
		Vector3 result = target;
		if (MultiHitRay)
		{
			Vector3 vector = target - base.Owner.transform.position;
			vector.y = 0f;
			vector.Normalize();
			vector = Quaternion.AngleAxis(90f, Vector3.down) * vector;
			result += vector * (base.AdjustedMultiHitDist / 2f);
		}
		return result;
	}

	public override void Launch(Vector3 location, GameObject enemy, int variationOverride)
	{
		AttackRangedBaseLaunch(location, enemy, variationOverride);
		Vector3 projectileLaunchPosition = GetProjectileLaunchPosition(location);
		projectileLaunchPosition.y += ProjectileHeight;
		StartCoroutine(Launch(projectileLaunchPosition, location, enemy));
	}
}
