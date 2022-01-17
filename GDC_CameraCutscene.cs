using System.Collections;
using UnityEngine;

public class GDC_CameraCutscene : BasePuppetScript
{
	public Spline CameraSpline;

	public override IEnumerator RunScript()
	{
		CameraControl c = CameraControl.Instance;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime / 1000f;
			if ((bool)CameraSpline)
			{
				Vector3 point = CameraSpline.Evaluate(t);
				c.FocusOnPoint(point, Time.deltaTime);
			}
			yield return null;
		}
		EndScene();
	}
}
