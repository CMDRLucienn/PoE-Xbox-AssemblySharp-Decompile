using UnityEngine;

public class LoadingTipList : ScriptableObject
{
	public LoadingTip[] Tips = new LoadingTip[0];

	public MapData.LoadingScreenType Area;

	public bool RestrictByArea;

	[HideInInspector]
	public int Act = 1;

	[HideInInspector]
	public bool RestrictByAct;

	public string MapName = "";

	public bool RestrictByMapName;

	public LoadingTip this[int index]
	{
		get
		{
			return Tips[index];
		}
		set
		{
			Tips[index] = value;
		}
	}

	public int Length => Tips.Length;
}
