public class MoverData
{
	public float Radius;

	public float Acceleration;

	public float AnimationRunSpeed;

	public float AnimationWalkSpeed;

	public float RunSpeed;

	public float WalkSpeed;

	public MoverData(Mover mover)
	{
		Radius = mover.Radius;
		Acceleration = mover.Acceleration;
		AnimationRunSpeed = mover.AnimationRunSpeed;
		AnimationWalkSpeed = mover.AnimationWalkSpeed;
		RunSpeed = mover.RunSpeed;
		WalkSpeed = mover.WalkSpeed;
	}

	public void ApplyTo(Mover mover)
	{
		mover.Radius = Radius;
		mover.Acceleration = Acceleration;
		mover.AnimationRunSpeed = AnimationRunSpeed;
		mover.AnimationWalkSpeed = AnimationWalkSpeed;
		mover.RunSpeed = RunSpeed;
		mover.WalkSpeed = WalkSpeed;
	}
}
