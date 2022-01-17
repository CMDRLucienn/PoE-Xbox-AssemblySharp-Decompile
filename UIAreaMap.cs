using System;
using UnityEngine;

public class UIAreaMap : MonoBehaviour
{
	private Camera m_uiCamera;

	public void FocusCameraOnPointer()
	{
		if ((bool)m_uiCamera)
		{
			Vector3 point = m_uiCamera.ScreenToWorldPoint(GameInput.MousePosition);
			Vector3 vector = base.transform.worldToLocalMatrix.MultiplyPoint3x4(point);
			CameraControl instance = CameraControl.Instance;
			LevelInfo instance2 = LevelInfo.Instance;
			Vector3 backgroundQuadOrigin = instance2.m_backgroundQuadOrigin;
			Vector3 vector2 = instance2.m_backgroundQuadAxisX * instance2.m_backgroundQuadWidth;
			Vector3 vector3 = instance2.m_backgroundQuadAxisY * instance2.m_backgroundQuadHeight;
			Vector3 point2 = backgroundQuadOrigin + vector2 * (vector.x / UIAreaMapManager.Instance.AreaMapWidthFillPercentage + 0.5f) + vector3 * (vector.y / UIAreaMapManager.Instance.AreaMapHeightFillPercentage + 0.5f);
			instance.FocusOnPoint(point2);
			instance.DoUpdate();
		}
	}

	private void OnClick()
	{
		FocusCameraOnPointer();
	}

	private void OnDoubleClick()
	{
		UIAreaMapManager.Instance.HideWindow();
	}

	private void OnRightClick()
	{
		FocusCameraOnPointer();
	}

	private void OnDrag(object go)
	{
	}

	private void OnDrag(GameObject go, Vector2 delta)
	{
		OnClick();
	}

	private void Start()
	{
		m_uiCamera = InGameUILayout.NGUICamera;
		UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDrag));
	}
}
