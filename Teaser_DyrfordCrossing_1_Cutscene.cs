using System.Collections;
using UnityEngine;

public class Teaser_DyrfordCrossing_1_Cutscene : BasePuppetScript
{
	public PartyWaypoint EndWaypoint;

	public Transform CameraEnd;

	public override IEnumerator RunScript()
	{
		CameraControl c = CameraControl.Instance;
		for (int i = 0; i < 6; i++)
		{
			if ((bool)ActorParty[i])
			{
				PuppetModeController component = ActorParty[i].GetComponent<PuppetModeController>();
				if (EndWaypoint.Waypoints.Length > i && EndWaypoint.Waypoints[i] != null && component != null)
				{
					StartCoroutine(component.PathToPoint(EndWaypoint.Waypoints[i].transform.position, 0f, walk: false));
				}
			}
		}
		yield return new WaitForSeconds(0.5f);
		c.FocusOnPoint(CameraEnd.position, 4f);
		yield return StartCoroutine(WaitForCamera());
		yield return new WaitForSeconds(1.5f);
		EndScene();
	}
}
