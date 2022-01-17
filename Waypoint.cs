using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Toolbox/Waypoint")]
public class Waypoint : MonoBehaviour
{
	public static List<Waypoint> s_ActiveWayPoints = new List<Waypoint>();

	public Waypoint NextWaypoint;

	[Range(0f, 120f)]
	public float PauseTime = 1f;

	public int LoopCount;

	public AmbientAnimation AmbientAnimation;

	[Tooltip("If true, NPCs with abduction abilities can teleport their victims and themselves to this waypoint.")]
	public bool IsAbductionWaypoint;

	public bool WalkOnly;

	public int AmbientVariation => (int)AmbientAnimation;

	private void OnEnable()
	{
		if (!s_ActiveWayPoints.Contains(this))
		{
			s_ActiveWayPoints.Add(this);
		}
	}

	private void OnDisable()
	{
		s_ActiveWayPoints.Remove(this);
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnDrawGizmosSelected()
	{
		if (!(NextWaypoint == null))
		{
			Gizmos.color = Color.white;
			Gizmos.DrawLine(base.gameObject.transform.position, NextWaypoint.transform.position);
			Gizmos.color = Color.red;
			Gizmos.DrawCube(NextWaypoint.transform.position, NextWaypoint.transform.localScale);
			Gizmos.color = Color.green;
			Gizmos.DrawCube(base.transform.position, base.transform.localScale);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.forward);
	}
}
