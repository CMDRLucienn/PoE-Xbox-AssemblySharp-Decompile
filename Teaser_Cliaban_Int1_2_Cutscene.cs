using System.Collections;
using UnityEngine;

public class Teaser_Cliaban_Int1_2_Cutscene : BasePuppetScript
{
	public Transform CameraEnd;

	public GameObject Spectre;

	public GenericAbility SpellMissile;

	public Transform SpectreEnd;

	public override IEnumerator RunScript()
	{
		CameraControl c = CameraControl.Instance;
		yield return new WaitForSeconds(0.5f);
		PuppetModeController component = ActorParty[1].GetComponent<PuppetModeController>();
		PuppetModeController pmc3 = Spectre.GetComponent<PuppetModeController>();
		StartCoroutine(component.LaunchAbility(Spectre, SpellMissile, allowDamage: true));
		yield return new WaitForSeconds(2.6f);
		StartCoroutine(pmc3.MoveDirectlyToPoint(SpectreEnd.position, 0f, walk: false));
		c.FocusOnPoint(CameraEnd.position, 1f);
		yield return StartCoroutine(WaitForCamera());
		yield return new WaitForSeconds(2f);
		EndScene();
	}
}
