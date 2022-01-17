using System;

public class TransitionEventArgs : EventArgs
{
	public MapData TargetMap;

	public TransitionEventArgs(MapData target)
	{
		TargetMap = target;
	}
}
