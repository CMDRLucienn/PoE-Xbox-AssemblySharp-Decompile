// IEMod.QuickControls.QuickControl
using System;
using Patchwork.Attributes;
using UnityEngine;

[NewType(null, null)]
[PatchedByType("IEMod.QuickControls.QuickControl")]
public abstract class QuickControl : IGameObjectWrapper
{
	private string _name;

	private GameObject _gameObject;

	bool IGameObjectWrapper.IsAlive
	{
		[PatchedByMember("System.Boolean IEMod.QuickControls.QuickControl::IEMod.QuickControls.IGameObjectWrapper.get_IsAlive()")]
		get
		{
			return _gameObject;
		}
	}

	public GameObject GameObject
	{
		[PatchedByMember("UnityEngine.GameObject IEMod.QuickControls.QuickControl::get_GameObject()")]
		get
		{
			AssertAlive();
			return _gameObject;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickControl::set_GameObject(UnityEngine.GameObject)")]
		protected set
		{
			_gameObject = value;
		}
	}

	public int Layer
	{
		[PatchedByMember("System.Int32 IEMod.QuickControls.QuickControl::get_Layer()")]
		get
		{
			return GameObject.layer;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickControl::set_Layer(System.Int32)")]
		set
		{
			GameObject.layer = value;
			NGUITools.SetLayer(GameObject, value);
		}
	}

	public string Name
	{
		[PatchedByMember("System.String IEMod.QuickControls.QuickControl::get_Name()")]
		get
		{
			if (this.IsAlive())
			{
				return _name = GameObject.name;
			}
			return _name;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickControl::set_Name(System.String)")]
		set
		{
			GameObject.name = (_name = value);
		}
	}

	public Transform Transform
	{
		[PatchedByMember("UnityEngine.Transform IEMod.QuickControls.QuickControl::get_Transform()")]
		get
		{
			return GameObject.transform;
		}
	}

	public Transform Parent
	{
		[PatchedByMember("UnityEngine.Transform IEMod.QuickControls.QuickControl::get_Parent()")]
		get
		{
			return Transform.parent;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickControl::set_Parent(UnityEngine.Transform)")]
		set
		{
			Transform.parent = value;
		}
	}

	public Vector3 LocalPosition
	{
		[PatchedByMember("UnityEngine.Vector3 IEMod.QuickControls.QuickControl::get_LocalPosition()")]
		get
		{
			return GameObject.transform.localPosition;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickControl::set_LocalPosition(UnityEngine.Vector3)")]
		set
		{
			GameObject.transform.localPosition = value;
		}
	}

	public Vector3 LocalScale
	{
		[PatchedByMember("UnityEngine.Vector3 IEMod.QuickControls.QuickControl::get_LocalScale()")]
		get
		{
			return GameObject.transform.localScale;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.QuickControl::set_LocalScale(UnityEngine.Vector3)")]
		set
		{
			GameObject.transform.localScale = value;
		}
	}

	public bool ActiveSelf
	{
		[PatchedByMember("System.Boolean IEMod.QuickControls.QuickControl::get_ActiveSelf()")]
		get
		{
			return GameObject.activeSelf;
		}
	}

	[PatchedByMember("System.Void IEMod.QuickControls.QuickControl::AssertAlive()")]
	protected void AssertAlive()
	{
		if (!this.IsAlive())
		{
			throw new ObjectDisposedException(_name, $"Tried to access the GameObject probably named '{_name}' backing a QuickControl, but it has been destroyed. GameObject's InstanceId: {_gameObject.GetInstanceID()}");
		}
	}

	[PatchedByMember("System.Boolean IEMod.QuickControls.QuickControl::op_True(IEMod.QuickControls.QuickControl)")]
	public static bool operator true(QuickControl qc)
	{
		return qc.IsAlive();
	}

	[PatchedByMember("System.Boolean IEMod.QuickControls.QuickControl::op_False(IEMod.QuickControls.QuickControl)")]
	public static bool operator false(QuickControl qc)
	{
		return !qc.IsAlive();
	}

	[PatchedByMember("UnityEngine.GameObject IEMod.QuickControls.QuickControl::op_Implicit(IEMod.QuickControls.QuickControl)")]
	public static implicit operator GameObject(QuickControl qc)
	{
		return qc.GameObject;
	}

	[PatchedByMember("UnityEngine.Transform IEMod.QuickControls.QuickControl::op_Implicit(IEMod.QuickControls.QuickControl)")]
	public static implicit operator Transform(QuickControl qc)
	{
		return qc.Transform;
	}

	[PatchedByMember("System.Void IEMod.QuickControls.QuickControl::SetActive(System.Boolean)")]
	public void SetActive(bool state)
	{
		GameObject.SetActive(state);
	}

	[PatchedByMember("System.Void IEMod.QuickControls.QuickControl::.ctor()")]
	protected QuickControl()
	{
	}
}
