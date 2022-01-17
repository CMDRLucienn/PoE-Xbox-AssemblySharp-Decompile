using System.Collections;
using UnityEngine;

public class PlayerAmbientPuppetScript : BasePuppetScript
{
	public Transform PlayerWaypoint;

	public AmbientAnimation Animation;

	public bool Loop;

	public override IEnumerator RunScript()
	{
		AnimationController playerAnim = GameState.s_playerCharacter.GetComponent<AnimationController>();
		playerAnim.transform.position = PlayerWaypoint.transform.position;
		playerAnim.transform.rotation = PlayerWaypoint.transform.rotation;
		playerAnim.DesiredAction.m_actionType = AnimationController.ActionType.Ambient;
		playerAnim.DesiredAction.m_variation = (int)Animation;
		playerAnim.Loop = Loop;
		yield return new WaitForSeconds(CutsceneComponent.FailsafeTimer);
		playerAnim.DesiredAction.m_actionType = AnimationController.ActionType.None;
		EndScene();
	}
}
