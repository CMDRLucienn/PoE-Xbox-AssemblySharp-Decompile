using System.Collections;
using UnityEngine;

public class Teaser_Cliaban_Ext_Cutscene : BasePuppetScript
{
	public PartyWaypoint EndWaypoint;

	public Transform CameraEnd;

	private SyncCameraOrthoSettings cameraSettings;

	public GameObject Lightning;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override IEnumerator RunScript()
	{
		CameraControl c = CameraControl.Instance;
		cameraSettings = SyncCameraOrthoSettings.Instance;
		StartCoroutine(ZoomIn(2f));
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
		yield return new WaitForSeconds(1.5f);
		c.FocusOnPoint(CameraEnd.position, 6f);
		yield return StartCoroutine(WaitForCamera());
		yield return new WaitForSeconds(0.8f);
		Lightning.SetActive(value: true);
		yield return new WaitForSeconds(2.5f);
		EndScene();
	}

	private IEnumerator ZoomIn(float time)
	{
		float t = 0f;
		float rate = 1f / time;
		while (t < 1f)
		{
			t += Time.deltaTime * rate;
			cameraSettings.SetZoomLevel(Mathf.SmoothStep(2f, 1f, t), force: false);
			yield return null;
		}
	}
}
