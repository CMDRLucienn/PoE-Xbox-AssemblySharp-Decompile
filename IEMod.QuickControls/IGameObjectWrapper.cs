// IEMod.QuickControls.IGameObjectWrapper
using Patchwork.Attributes;
using UnityEngine;

[PatchedByType("IEMod.QuickControls.IGameObjectWrapper")]
[NewType(null, null)]
public interface IGameObjectWrapper
{
	GameObject GameObject
	{
		[PatchedByMember("UnityEngine.GameObject IEMod.QuickControls.IGameObjectWrapper::get_GameObject()")]
		get;
	}

	string Name
	{
		[PatchedByMember("System.String IEMod.QuickControls.IGameObjectWrapper::get_Name()")]
		get;
		[PatchedByMember("System.Void IEMod.QuickControls.IGameObjectWrapper::set_Name(System.String)")]
		set;
	}

	Transform Transform
	{
		[PatchedByMember("UnityEngine.Transform IEMod.QuickControls.IGameObjectWrapper::get_Transform()")]
		get;
	}

	bool IsAlive
	{
		[PatchedByMember("System.Boolean IEMod.QuickControls.IGameObjectWrapper::get_IsAlive()")]
		get;
	}
}
