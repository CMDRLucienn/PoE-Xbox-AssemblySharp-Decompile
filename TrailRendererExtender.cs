using System.Collections;
using UnityEngine;

public static class TrailRendererExtender
{
	public static void Reset(this TrailRenderer trail, MonoBehaviour instance)
	{
		instance.StartCoroutine(ResetTrail(trail));
	}

	private static IEnumerator ResetTrail(TrailRenderer trail)
	{
		float trailTime = trail.time;
		trail.time = 0f;
		yield return 0;
		trail.time = trailTime;
	}
}
