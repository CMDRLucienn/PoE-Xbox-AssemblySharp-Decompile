using System.Collections;
using UnityEngine;

public class Teaser_Heritage_Hill_1_Cutscene : BasePuppetScript
{
	public Transform EndA;

	public Transform EndB;

	public Transform CameraMiddle;

	public Transform CameraEnd;

	public GameObject Fighter;

	public GameObject Mage;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override IEnumerator RunScript()
	{
		CameraControl c = CameraControl.Instance;
		PuppetModeController component = Fighter.GetComponent<PuppetModeController>();
		PuppetModeController component2 = Mage.GetComponent<PuppetModeController>();
		StartCoroutine(component.PathToPoint(EndA.position, 0f, walk: false));
		StartCoroutine(component2.PathToPoint(EndB.position, 0f, walk: true));
		yield return new WaitForSeconds(0.5f);
		c.FocusOnPoint(CameraMiddle.position, 3f);
		yield return StartCoroutine(WaitForCamera());
		yield return new WaitForSeconds(2.5f);
		EndScene();
	}
}
