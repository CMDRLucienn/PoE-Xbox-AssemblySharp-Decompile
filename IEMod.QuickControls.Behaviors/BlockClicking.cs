// IEMod.QuickControls.Behaviors.BlockClicking
using System.Linq;
using Patchwork.Attributes;
using UnityEngine;

[NewType(null, null)]
[PatchedByType("IEMod.QuickControls.Behaviors.BlockClicking")]
public static class BlockClicking
{
	[PatchedByMember("System.Void IEMod.QuickControls.Behaviors.BlockClicking::Apply(IEMod.QuickControls.QuickDropdown`1<T>)")]
	public static void Apply<T>(QuickDropdown<T> button)
	{
		GameObject gameObject = button.GameObject;
		gameObject.Descendant("Background").AddComponent<UINoClick>().BlockClicking = true;
		gameObject.Descendant("BackgroundDropdown").AddComponent<UINoClick>().BlockClicking = true;
		gameObject.ComponentsInDescendants<UILabel>().ToList().ForEach(delegate (UILabel x)
		{
			if (!x.gameObject.HasComponent<BoxCollider>())
			{
				x.gameObject.AddComponent<BoxCollider>().size = Vector3.one;
			}
			x.gameObject.AddComponent<UINoClick>().BlockClicking = true;
		});
	}
}
