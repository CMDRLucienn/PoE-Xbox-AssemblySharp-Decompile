using System.Collections;
using UnityEngine;

public class DilemmaJudge_Cutscene : BasePuppetScript
{
	public Transform PlayerWaypoint;

	public override IEnumerator RunScript()
	{
		AnimationController playerAnim = GameState.s_playerCharacter.GetComponent<AnimationController>();
		playerAnim.transform.position = PlayerWaypoint.transform.position;
		playerAnim.transform.rotation = PlayerWaypoint.transform.rotation;
		playerAnim.DesiredAction.m_actionType = AnimationController.ActionType.Ambient;
		playerAnim.DesiredAction.m_variation = 14;
		playerAnim.Loop = true;
		yield return new WaitForSeconds(CutsceneComponent.FailsafeTimer);
		playerAnim.DesiredAction.m_actionType = AnimationController.ActionType.None;
		EndScene();
	}
}
