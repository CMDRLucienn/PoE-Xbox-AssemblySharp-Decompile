// IEMod.QuickControls.Controls.QuickPage
using Patchwork.Attributes;
using UnityEngine;

[PatchedByType("IEMod.QuickControls.Controls.QuickPage")]
[NewType(null, null)]
public class QuickPage : QuickControl
{
	[PatchedByMember("System.Void IEMod.QuickControls.Controls.QuickPage::.ctor(UnityEngine.Transform,System.String,UnityEngine.GameObject)")]
	public QuickPage(Transform parent = null, string name = "QuickPage", GameObject altPrototype = null)
	{
		base.GameObject = new GameObject();
		GameObject gameObject = altPrototype ?? Prefabs.QuickPage;
		base.Name = name;
		base.Parent = parent;
		base.LocalScale = gameObject.transform.localScale;
		base.LocalPosition = gameObject.transform.localPosition;
	}
}
