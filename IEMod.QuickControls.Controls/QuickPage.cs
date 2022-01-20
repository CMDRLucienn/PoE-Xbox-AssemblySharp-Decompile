using UnityEngine;

public class QuickPage : QuickControl
{
	public QuickPage(Transform parent = null, string name = "QuickPage", GameObject altPrototype = null)
	{
		GameObject = new GameObject();
		var proto = altPrototype ?? Prefabs.QuickPage;
		Name = name;
		Parent = parent;
		LocalScale = proto.transform.localScale;
		LocalPosition = proto.transform.localPosition;
	}
}
