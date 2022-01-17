using System.Collections;
using UnityEngine;

public class Teaser_Skaen_Cutscene : BasePuppetScript
{
	public PartyWaypoint EndWaypoint;

	public Transform CameraEnd;

	public GenericAbility Blessing;

	public GameObject Skaen1;

	public GameObject Skaen2;

	public GameObject Skaen3;

	public GameObject Skaen4;

	public GameObject Skaen5;

	public Transform Move1;

	public Transform Move2;

	public Transform Move3;

	public Transform Move4;

	public Transform Move5;

	public override IEnumerator RunScript()
	{
		CameraControl c = CameraControl.Instance;
		yield return new WaitForSeconds(0.5f);
		c.FocusOnPoint(CameraEnd.position, 6f);
		PuppetModeController pmc = ActorParty[1].GetComponent<PuppetModeController>();
		yield return new WaitForSeconds(1.5f);
		StartCoroutine(pmc.LaunchAbility(ActorParty[1], Blessing, allowDamage: true));
		PuppetModeController pmc2 = Skaen1.GetComponent<PuppetModeController>();
		PuppetModeController pmc3 = Skaen2.GetComponent<PuppetModeController>();
		PuppetModeController pmc4 = Skaen3.GetComponent<PuppetModeController>();
		PuppetModeController component = Skaen4.GetComponent<PuppetModeController>();
		PuppetModeController component2 = Skaen5.GetComponent<PuppetModeController>();
		StartCoroutine(component.MoveDirectlyToPoint(Move4.position, 0f, walk: true));
		StartCoroutine(component2.MoveDirectlyToPoint(Move5.position, 0f, walk: true));
		yield return new WaitForSeconds(0.1f);
		StartCoroutine(pmc3.MoveDirectlyToPoint(Move2.position, 0f, walk: true));
		StartCoroutine(pmc4.MoveDirectlyToPoint(Move3.position, 0f, walk: true));
		StartCoroutine(pmc2.MoveDirectlyToPoint(Move1.position, 0f, walk: true));
		yield return new WaitForSeconds(0.1f);
		yield return StartCoroutine(WaitForCamera());
		GameState.ForceCombatMode = true;
		yield return new WaitForSeconds(3.5f);
		EndScene();
	}
}
