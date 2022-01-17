using UnityEngine;

[RequireComponent(typeof(UIAtlas))]
public class UIAtlasMirror : MonoBehaviour
{
	public UIAtlas[] TargetAtlases;

	public static void Mirror(UIAtlas atlas)
	{
		if (!atlas)
		{
			return;
		}
		UIAtlasMirror component = atlas.GetComponent<UIAtlasMirror>();
		if ((bool)component)
		{
			UIAtlas[] targetAtlases = component.TargetAtlases;
			foreach (UIAtlas obj in targetAtlases)
			{
				obj.spriteList = atlas.spriteList;
				obj.coordinates = atlas.coordinates;
				obj.pixelSize = atlas.pixelSize;
				obj.MarkAsDirty();
			}
		}
	}
}
