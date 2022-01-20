// IEMod.QuickControls.QuickButton
using System;
using System.Linq;
using Patchwork.Attributes;
using UnityEngine;

[NewType(null, null)]
[PatchedByType("IEMod.QuickControls.QuickButton")]
public class QuickButton : QuickControl
{
	private GameObject _collider;

	public string Caption
	{
		[PatchedByMember("System.String IEMod.QuickControls.QuickButton::get_Caption()")]
		get
		{
			return base.GameObject.ComponentInDescendants<GUIStringLabel>().FormatString;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickButton::set_Caption(System.String)")]
		set
		{
			GUIStringLabel gUIStringLabel = base.GameObject.ComponentInDescendants<GUIStringLabel>();
			gUIStringLabel.FormatString = value;
			gUIStringLabel.RefreshText();
		}
	}

	public GameObject Collider
	{
		[PatchedByMember("UnityEngine.GameObject IEMod.QuickControls.QuickButton::get_Collider()")]
		get
		{
			AssertAlive();
			return _collider ?? (_collider = base.GameObject.Component<UIMultiSpriteImageButton>().Collider);
		}
	}

	public UIMultiSpriteImageButton ButtonComponent
	{
		[PatchedByMember("UIMultiSpriteImageButton IEMod.QuickControls.QuickButton::get_ButtonComponent()")]
		get
		{
			return base.GameObject.ComponentInDescendants<UIMultiSpriteImageButton>();
		}
	}

	[method: PatchedByMember("System.Void IEMod.QuickControls.QuickButton::add_Click(System.Action`1<IEMod.QuickControls.QuickButton>)")]
	public event Action<QuickButton> Click;

	[method: PatchedByMember("System.Void IEMod.QuickControls.QuickButton::add_Press(System.Action`2<IEMod.QuickControls.QuickButton,System.Boolean>)")]
	public event Action<QuickButton, bool> Press;

	[PatchedByMember("System.Void IEMod.QuickControls.QuickButton::.ctor(UnityEngine.Transform,System.String,UnityEngine.GameObject,System.Boolean)")]
	public QuickButton(Transform parent = null, string name = "QuickButton", GameObject altPrototype = null, bool addCollider = true)
	{
		GameObject original = altPrototype ?? Prefabs.QuickButton;
		GameObject gameObject = UnityEngine.Object.Instantiate(original);
		gameObject.name = name;
		gameObject.transform.parent = parent;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		base.GameObject = gameObject;
		UIMultiSpriteImageButton buttonComponent = ButtonComponent;
		buttonComponent.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(buttonComponent.onClick, (UIEventListener.VoidDelegate)delegate
		{
			RaiseClick(this);
		});
		UIMultiSpriteImageButton buttonComponent2 = ButtonComponent;
		buttonComponent2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(buttonComponent2.onPress, (UIEventListener.BoolDelegate)delegate (GameObject go, bool state)
		{
			RaisePress(this, state);
		});
		(from x in gameObject.Children()
		 where x.HasComponent<Collider>()
		 select x).ToList().ForEach(UnityEngine.Object.DestroyImmediate);
		if (addCollider)
		{
			_collider = new GameObject("Collider");
			_collider.transform.parent = gameObject.transform;
			_collider.transform.localScale = new Vector3(269f, 56f, 1f);
			_collider.transform.localPosition = new Vector3(0f, 0f, -2f);
			_collider.layer = 14;
			_collider.AddComponent<BoxCollider>().size = new Vector3(1f, 1f, 1f);
			_collider.AddComponent<UINoClick>().BlockClicking = true;
			_collider.AddComponent<UIEventListener>();
		}
		IEDebug.Log("Created: " + name);
	}

	[PatchedByMember("System.Void IEMod.QuickControls.QuickButton::RaiseClick(IEMod.QuickControls.QuickButton)")]
	protected virtual void RaiseClick(QuickButton obj)
	{
		this.Click?.Invoke(obj);
	}

	[PatchedByMember("System.Void IEMod.QuickControls.QuickButton::RaisePress(IEMod.QuickControls.QuickButton,System.Boolean)")]
	protected virtual void RaisePress(QuickButton arg1, bool arg2)
	{
		this.Press?.Invoke(arg1, arg2);
	}
}
