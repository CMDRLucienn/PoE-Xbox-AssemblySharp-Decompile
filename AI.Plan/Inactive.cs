namespace AI.Plan;

public class Inactive : GameAIState
{
	public override void OnEnter()
	{
		base.OnEnter();
		StopMover();
	}
}
