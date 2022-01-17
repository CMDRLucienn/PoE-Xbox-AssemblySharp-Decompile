using System;

[Serializable]
public class StrongholdVisitorArrival
{
	public string Tag { get; set; }

	public float TimeToArrive { get; set; }

	public StrongholdVisitorArrival()
	{
	}

	public StrongholdVisitorArrival(string tag, float time)
	{
		Tag = tag;
		TimeToArrive = time;
	}
}
