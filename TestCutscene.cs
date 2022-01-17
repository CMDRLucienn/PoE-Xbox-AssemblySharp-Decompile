using System.Collections;
using UnityEngine;

public class TestCutscene : BasePuppetScript
{
	public Transform Waypoint1;

	public float cameraTimer;

	public GameObject Dwarf;

	public string Conversation;

	public GameObject Medreth;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override IEnumerator RunScript()
	{
		FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Cutscene, 2f);
		yield return StartCoroutine(WaitForFade(FadeManager.FadeState.None));
		StartCoroutine(GetActorComponent<PuppetModeController>("NPC_Medreth_Guard_Dwarf").PathToPoint(Waypoint1.transform.position, 0f, walk: true));
		yield return StartCoroutine(WaitForMover(GetActorComponent<Mover>("NPC_Medreth_Guard_Dwarf")));
		EndScene();
		if ((bool)Medreth)
		{
			ConversationManager.Instance.StartConversation(Conversation, 0, Medreth, FlowChartPlayer.DisplayMode.Cutscene);
		}
	}
}
