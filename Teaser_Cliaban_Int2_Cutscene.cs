using System.Collections;
using UnityEngine;

public class Teaser_Cliaban_Int2_Cutscene : BasePuppetScript
{
	public Transform StartA;

	public Transform EndA;

	public Transform StartB;

	public Transform EndB;

	public Transform CameraStart;

	public Transform CameraEnd;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override IEnumerator RunScript()
	{
		CameraControl c = CameraControl.Instance;
		StartCoroutine(GetActorComponent<PuppetModeController>("Skuldr_Small_A").PathToPoint(EndA.position, 0f, walk: false));
		StartCoroutine(GetActorComponent<PuppetModeController>("Skuldr_Small_B").PathToPoint(EndB.position, 0f, walk: false));
		yield return new WaitForSeconds(0.5f);
		c.FocusOnPoint(CameraEnd.position, 5f);
		yield return StartCoroutine(WaitForCamera());
		yield return new WaitForSeconds(0.5f);
		EndScene();
	}
}
