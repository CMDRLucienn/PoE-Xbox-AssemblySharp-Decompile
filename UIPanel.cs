using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Panel")]
public class UIPanel : MonoBehaviour
{
	public enum DebugInfo
	{
		None,
		Gizmos,
		Geometry
	}

	public delegate void OnChangeDelegate();

	public delegate void AlphaChanged(float alpha);

	public OnChangeDelegate onChange;

	public bool showInPanelTool = true;

	public bool generateNormals;

	public bool depthPass;

	public bool widgetsAreStatic;

	public bool cullWhileDragging = true;

	[HideInInspector]
	public Matrix4x4 worldToLocal = Matrix4x4.identity;

	[HideInInspector]
	[SerializeField]
	private float mAlpha = 1f;

	[HideInInspector]
	[SerializeField]
	private DebugInfo mDebugInfo = DebugInfo.Gizmos;

	[HideInInspector]
	[SerializeField]
	private bool mPixelSnap = true;

	[HideInInspector]
	[SerializeField]
	private UIDrawCall.Clipping mClipping;

	[HideInInspector]
	[SerializeField]
	private Vector4 mClipRange = Vector4.zero;

	[HideInInspector]
	[SerializeField]
	private Vector2 mClipSoftness = new Vector2(40f, 40f);

	private BetterList<UIWidget> mWidgets = new BetterList<UIWidget>();

	private BetterList<Material> mChanged = new BetterList<Material>();

	private BetterList<UIDrawCall> mDrawCalls = new BetterList<UIDrawCall>();

	private Dictionary<Material, BetterList<Vector3>> mMaterialVertBuffers = new Dictionary<Material, BetterList<Vector3>>();

	private Dictionary<Material, BetterList<Vector3>> mMaterialNormBuffers = new Dictionary<Material, BetterList<Vector3>>();

	private Dictionary<Material, BetterList<Vector4>> mMaterialTansBuffers = new Dictionary<Material, BetterList<Vector4>>();

	private Dictionary<Material, BetterList<Vector2>> mMaterialUVBuffers = new Dictionary<Material, BetterList<Vector2>>();

	private Dictionary<Material, BetterList<Color32>> mMaterialColorBuffers = new Dictionary<Material, BetterList<Color32>>();

	private GameObject mGo;

	private Transform mTrans;

	private Camera mCam;

	private int mLayer = -1;

	private bool mDepthChanged;

	private float mCullTime;

	private float mUpdateTime;

	private float mMatrixTime;

	private static float[] mTemp = new float[4];

	private Vector2 mMin = Vector2.zero;

	private Vector2 mMax = Vector2.zero;

	private UIPanel[] mChildPanels;

	public GameObject cachedGameObject
	{
		get
		{
			if (mGo == null)
			{
				mGo = base.gameObject;
			}
			return mGo;
		}
	}

	public Transform cachedTransform
	{
		get
		{
			if (mTrans == null)
			{
				mTrans = base.transform;
			}
			return mTrans;
		}
	}

	public float alpha
	{
		get
		{
			return mAlpha;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (mAlpha != num)
			{
				if (this.OnAlphaChanged != null)
				{
					this.OnAlphaChanged(num);
				}
				mAlpha = num;
				for (int i = 0; i < mDrawCalls.size; i++)
				{
					UIDrawCall uIDrawCall = mDrawCalls[i];
					MarkMaterialAsChanged(uIDrawCall.material, sort: false);
				}
				for (int j = 0; j < mWidgets.size; j++)
				{
					mWidgets[j].MarkAsChangedLite();
				}
			}
		}
	}

	public DebugInfo debugInfo
	{
		get
		{
			return mDebugInfo;
		}
		set
		{
			if (mDebugInfo != value)
			{
				mDebugInfo = value;
				BetterList<UIDrawCall> betterList = drawCalls;
				HideFlags hideFlags = ((mDebugInfo == DebugInfo.Geometry) ? (HideFlags.DontSave | HideFlags.NotEditable) : HideFlags.HideAndDontSave);
				int i = 0;
				for (int size = betterList.size; i < size; i++)
				{
					GameObject obj = betterList[i].gameObject;
					NGUITools.SetActiveSelf(obj, state: false);
					obj.hideFlags = hideFlags;
					NGUITools.SetActiveSelf(obj, state: true);
				}
			}
		}
	}

	public bool pixelSnap
	{
		get
		{
			return mPixelSnap;
		}
		set
		{
			if (mPixelSnap != value)
			{
				mPixelSnap = value;
				mMatrixTime = 0f;
				UpdateDrawcalls();
			}
		}
	}

	public UIDrawCall.Clipping clipping
	{
		get
		{
			return mClipping;
		}
		set
		{
			if (mClipping != value)
			{
				mClipping = value;
				mMatrixTime = 0f;
				UpdateDrawcalls();
			}
		}
	}

	public Vector4 clipRange
	{
		get
		{
			return mClipRange;
		}
		set
		{
			if (mClipRange != value)
			{
				mCullTime = ((mCullTime == 0f) ? 0.001f : (Time.realtimeSinceStartup + 0.15f));
				mClipRange = value;
				mMatrixTime = 0f;
				UpdateDrawcalls();
			}
		}
	}

	public Vector2 clipSoftness
	{
		get
		{
			return mClipSoftness;
		}
		set
		{
			if (mClipSoftness != value)
			{
				mClipSoftness = value;
				UpdateDrawcalls();
			}
		}
	}

	public BetterList<UIWidget> widgets => mWidgets;

	public BetterList<UIDrawCall> drawCalls
	{
		get
		{
			int num = mDrawCalls.size;
			while (num > 0)
			{
				if (mDrawCalls[--num] == null)
				{
					mDrawCalls.RemoveAt(num);
				}
			}
			return mDrawCalls;
		}
	}

	public event AlphaChanged OnAlphaChanged;

	public void SetAlphaRecursive(float val, bool rebuildList)
	{
		if (rebuildList || mChildPanels == null)
		{
			mChildPanels = GetComponentsInChildren<UIPanel>(includeInactive: true);
		}
		int i = 0;
		for (int num = mChildPanels.Length; i < num; i++)
		{
			mChildPanels[i].alpha = val;
		}
	}

	private bool IsVisible(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		UpdateTransformMatrix();
		a = worldToLocal.MultiplyPoint3x4(a);
		b = worldToLocal.MultiplyPoint3x4(b);
		c = worldToLocal.MultiplyPoint3x4(c);
		d = worldToLocal.MultiplyPoint3x4(d);
		mTemp[0] = a.x;
		mTemp[1] = b.x;
		mTemp[2] = c.x;
		mTemp[3] = d.x;
		float num = Mathf.Min(mTemp);
		float num2 = Mathf.Max(mTemp);
		mTemp[0] = a.y;
		mTemp[1] = b.y;
		mTemp[2] = c.y;
		mTemp[3] = d.y;
		float num3 = Mathf.Min(mTemp);
		float num4 = Mathf.Max(mTemp);
		if (num2 < mMin.x)
		{
			return false;
		}
		if (num4 < mMin.y)
		{
			return false;
		}
		if (num > mMax.x)
		{
			return false;
		}
		if (num3 > mMax.y)
		{
			return false;
		}
		return true;
	}

	public bool IsVisible(Vector3 worldPos)
	{
		if (mAlpha < 0.001f)
		{
			return false;
		}
		if (mClipping == UIDrawCall.Clipping.None)
		{
			return true;
		}
		UpdateTransformMatrix();
		Vector3 vector = worldToLocal.MultiplyPoint3x4(worldPos);
		if (vector.x < mMin.x)
		{
			return false;
		}
		if (vector.y < mMin.y)
		{
			return false;
		}
		if (vector.x > mMax.x)
		{
			return false;
		}
		if (vector.y > mMax.y)
		{
			return false;
		}
		return true;
	}

	public bool IsVisible(UIWidget w)
	{
		if (!w._CachedVisibleDirty)
		{
			return w._CachedVisible;
		}
		if (mAlpha < 0.001f)
		{
			return false;
		}
		if (!w.enabled || !NGUITools.GetActive(w.cachedGameObject) || w.alpha < 0.001f)
		{
			return false;
		}
		if ((bool)w.containedBy)
		{
			return IsVisible(w.containedBy.BoundedBy);
		}
		if (mClipping == UIDrawCall.Clipping.None)
		{
			return true;
		}
		Vector2 relativeSize = w.relativeSize;
		Vector2 vector = Vector2.Scale(w.pivotOffset, relativeSize);
		Vector2 vector2 = vector;
		vector.x += relativeSize.x;
		vector.y -= relativeSize.y;
		Transform obj = w.cachedTransform;
		Vector3 a = obj.TransformPoint(vector);
		Vector3 b = obj.TransformPoint(new Vector2(vector.x, vector2.y));
		Vector3 c = obj.TransformPoint(new Vector2(vector2.x, vector.y));
		Vector3 d = obj.TransformPoint(vector2);
		w.CacheVisibility(IsVisible(a, b, c, d));
		return w._CachedVisible;
	}

	public void MarkMaterialAsChanged(Material mat, bool sort)
	{
		if (mat != null)
		{
			if (sort)
			{
				mDepthChanged = true;
			}
			if (!mChanged.Contains(mat))
			{
				mChanged.Add(mat);
			}
		}
	}

	public void AddWidget(UIWidget w)
	{
		if (w != null && !mWidgets.Contains(w))
		{
			mWidgets.Add(w);
			if (!mChanged.Contains(w.material))
			{
				mChanged.Add(w.material);
			}
			mDepthChanged = true;
		}
	}

	public void RemoveWidget(UIWidget w)
	{
		if (w != null && w != null && mWidgets != null && mWidgets.Remove(w) && w.material != null)
		{
			mChanged.Add(w.material);
		}
	}

	private UIDrawCall GetDrawCall(Material mat, bool createIfMissing)
	{
		BetterList<UIDrawCall> betterList = drawCalls;
		int i = 0;
		for (int size = betterList.size; i < size; i++)
		{
			UIDrawCall uIDrawCall = betterList.buffer[i];
			if (uIDrawCall.material == mat)
			{
				return uIDrawCall;
			}
		}
		UIDrawCall uIDrawCall2 = null;
		if (createIfMissing)
		{
			GameObject obj = new GameObject("_UIDrawCall [" + mat.name + "]");
			Object.DontDestroyOnLoad(obj);
			obj.layer = cachedGameObject.layer;
			uIDrawCall2 = obj.AddComponent<UIDrawCall>();
			uIDrawCall2.material = mat;
			mDrawCalls.Add(uIDrawCall2);
		}
		return uIDrawCall2;
	}

	private void Awake()
	{
		mGo = base.gameObject;
		mTrans = base.transform;
	}

	private void Start()
	{
		mLayer = mGo.layer;
		UICamera uICamera = UICamera.FindCameraForLayer(mLayer);
		mCam = ((uICamera != null) ? uICamera.cachedCamera : NGUITools.FindCameraForLayer(mLayer));
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnEnable()
	{
		int num = 0;
		while (num < mWidgets.size)
		{
			UIWidget uIWidget = mWidgets.buffer[num];
			if (uIWidget != null)
			{
				MarkMaterialAsChanged(uIWidget.material, sort: true);
				num++;
			}
			else
			{
				mWidgets.RemoveAt(num);
			}
		}
	}

	private void OnDisable()
	{
		int num = mDrawCalls.size;
		while (num > 0)
		{
			UIDrawCall uIDrawCall = mDrawCalls.buffer[--num];
			if (uIDrawCall != null)
			{
				NGUITools.DestroyImmediate(uIDrawCall.gameObject);
			}
		}
		mDrawCalls.Release();
		mChanged.Release();
	}

	private void UpdateTransformMatrix()
	{
		if (mUpdateTime != 0f && mMatrixTime == mUpdateTime)
		{
			return;
		}
		mMatrixTime = mUpdateTime;
		worldToLocal = cachedTransform.worldToLocalMatrix;
		if (mClipping != 0)
		{
			Vector2 vector = new Vector2(mClipRange.z, mClipRange.w);
			if (vector.x == 0f)
			{
				vector.x = ((mCam == null) ? Screen.width : mCam.pixelWidth);
			}
			if (vector.y == 0f)
			{
				vector.y = ((mCam == null) ? Screen.height : mCam.pixelHeight);
			}
			vector *= 0.5f;
			mMin.x = mClipRange.x - vector.x;
			mMin.y = mClipRange.y - vector.y;
			mMax.x = mClipRange.x + vector.x;
			mMax.y = mClipRange.y + vector.y;
		}
	}

	public void UpdateDrawcalls()
	{
		Vector4 vector = Vector4.zero;
		if (mClipping != 0)
		{
			vector = new Vector4(mClipRange.x, mClipRange.y, mClipRange.z * 0.5f, mClipRange.w * 0.5f);
		}
		if (vector.z == 0f)
		{
			vector.z = (float)Screen.width * 0.5f;
		}
		if (vector.w == 0f)
		{
			vector.w = (float)Screen.height * 0.5f;
		}
		RuntimePlatform platform = Application.platform;
		if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
		{
			vector.x -= 0.5f;
			vector.y += 0.5f;
		}
		Transform transform = cachedTransform;
		int i = 0;
		for (int size = mDrawCalls.size; i < size; i++)
		{
			UIDrawCall obj = mDrawCalls.buffer[i];
			obj.clipping = mClipping;
			obj.clipRange = vector;
			obj.clipSoftness = mClipSoftness;
			obj.depthPass = depthPass && mClipping == UIDrawCall.Clipping.None;
			obj.pixelSnap = mPixelSnap;
			Transform obj2 = obj.transform;
			obj2.position = transform.position;
			obj2.rotation = transform.rotation;
			obj2.localScale = transform.lossyScale;
		}
	}

	private void Fill(Material mat)
	{
		BetterList<Vector3> value = null;
		mMaterialVertBuffers.TryGetValue(mat, out value);
		if (value == null)
		{
			value = new BetterList<Vector3>();
			mMaterialVertBuffers[mat] = value;
		}
		BetterList<Vector2> value2 = null;
		mMaterialUVBuffers.TryGetValue(mat, out value2);
		if (value2 == null)
		{
			value2 = new BetterList<Vector2>();
			mMaterialUVBuffers[mat] = value2;
		}
		BetterList<Color32> value3 = null;
		mMaterialColorBuffers.TryGetValue(mat, out value3);
		if (value3 == null)
		{
			value3 = new BetterList<Color32>();
			mMaterialColorBuffers[mat] = value3;
		}
		BetterList<Vector3> value4 = null;
		BetterList<Vector4> value5 = null;
		if (generateNormals)
		{
			mMaterialNormBuffers.TryGetValue(mat, out value4);
			if (value4 == null)
			{
				value4 = new BetterList<Vector3>();
				mMaterialNormBuffers[mat] = value4;
			}
			mMaterialTansBuffers.TryGetValue(mat, out value5);
			if (value5 == null)
			{
				value5 = new BetterList<Vector4>();
				mMaterialTansBuffers[mat] = value5;
			}
		}
		int num = 0;
		while (num < mWidgets.size)
		{
			UIWidget uIWidget = mWidgets.buffer[num];
			if (uIWidget == null)
			{
				mWidgets.RemoveAt(num);
				continue;
			}
			if (uIWidget.material == mat && uIWidget.isVisible)
			{
				if (!(uIWidget.panel == this))
				{
					mWidgets.RemoveAt(num);
					continue;
				}
				if (generateNormals)
				{
					uIWidget.WriteToBuffers(value, value2, value3, value4, value5);
				}
				else
				{
					uIWidget.WriteToBuffers(value, value2, value3, null, null);
				}
			}
			num++;
		}
		if (value.size > 0)
		{
			UIDrawCall drawCall = GetDrawCall(mat, createIfMissing: true);
			drawCall.depthPass = depthPass && mClipping == UIDrawCall.Clipping.None;
			drawCall.Set(value, generateNormals ? value4 : null, generateNormals ? value5 : null, value2, value3);
		}
		else
		{
			UIDrawCall drawCall2 = GetDrawCall(mat, createIfMissing: false);
			if (drawCall2 != null)
			{
				mDrawCalls.Remove(drawCall2);
				NGUITools.DestroyImmediate(drawCall2.gameObject);
			}
		}
		value.Clear();
		value4?.Clear();
		value5?.Clear();
		value2.Clear();
		value3.Clear();
	}

	private void LateUpdate()
	{
		mUpdateTime = Time.realtimeSinceStartup;
		UpdateTransformMatrix();
		if (mLayer != cachedGameObject.layer)
		{
			mLayer = mGo.layer;
			UICamera uICamera = UICamera.FindCameraForLayer(mLayer);
			mCam = ((uICamera != null) ? uICamera.cachedCamera : NGUITools.FindCameraForLayer(mLayer));
			SetChildLayer(cachedTransform, mLayer);
			int i = 0;
			for (int size = drawCalls.size; i < size; i++)
			{
				mDrawCalls.buffer[i].gameObject.layer = mLayer;
			}
		}
		bool forceVisible = !cullWhileDragging && (clipping == UIDrawCall.Clipping.None || mCullTime > mUpdateTime);
		int j = 0;
		for (int size2 = mWidgets.size; j < size2; j++)
		{
			UIWidget uIWidget = mWidgets[j];
			if (uIWidget.UpdateGeometry(this, forceVisible) && !mChanged.Contains(uIWidget.material))
			{
				mChanged.Add(uIWidget.material);
			}
		}
		int k = 0;
		for (int size3 = mWidgets.size; k < size3; k++)
		{
			mWidgets[k]._CachedVisibleDirty = true;
		}
		if (mChanged.size != 0 && onChange != null)
		{
			onChange();
		}
		if (mDepthChanged)
		{
			mDepthChanged = false;
			mWidgets.Sort(UIWidget.CompareFunc);
		}
		int l = 0;
		for (int size4 = mChanged.size; l < size4; l++)
		{
			Fill(mChanged.buffer[l]);
		}
		UpdateDrawcalls();
		mChanged.Clear();
	}

	public void Refresh()
	{
		UIWidget[] componentsInChildren = GetComponentsInChildren<UIWidget>();
		int i = 0;
		for (int num = componentsInChildren.Length; i < num; i++)
		{
			componentsInChildren[i].Update();
		}
		LateUpdate();
	}

	public Vector2 Extremes(Vector2 min, Vector2 max)
	{
		float num = clipRange.z * 0.5f;
		float num2 = clipRange.w * 0.5f;
		Vector2 vector = new Vector2(clipRange.x - num, clipRange.y - num2);
		Vector2 vector2 = new Vector2(clipRange.x + num, clipRange.y + num2);
		if (clipping == UIDrawCall.Clipping.SoftClip)
		{
			vector.x += clipSoftness.x;
			vector.y += clipSoftness.y;
			vector2.x -= clipSoftness.x;
			vector2.y -= clipSoftness.y;
		}
		Vector2 zero = Vector2.zero;
		if (vector.x - 0.01f < min.x)
		{
			zero.x = -1f;
		}
		if (vector2.x + 0.01f > max.x)
		{
			zero.x = 1f;
		}
		if (vector.y - 0.01f < min.y)
		{
			zero.y = -1f;
		}
		if (vector2.y + 0.01f > max.y)
		{
			zero.y = 1f;
		}
		return zero;
	}

	public Vector3 CalculateConstrainOffset(Vector2 min, Vector2 max)
	{
		float num = clipRange.z * 0.5f;
		float num2 = clipRange.w * 0.5f;
		Vector2 minRect = new Vector2(min.x, min.y);
		Vector2 maxRect = new Vector2(max.x, max.y);
		Vector2 minArea = new Vector2(clipRange.x - num, clipRange.y - num2);
		Vector2 maxArea = new Vector2(clipRange.x + num, clipRange.y + num2);
		if (clipping == UIDrawCall.Clipping.SoftClip)
		{
			minArea.x += clipSoftness.x;
			minArea.y += clipSoftness.y;
			maxArea.x -= clipSoftness.x;
			maxArea.y -= clipSoftness.y;
		}
		return NGUIMath.ConstrainRect(minRect, maxRect, minArea, maxArea);
	}

	public bool ConstrainTargetToBounds(Transform target, ref Bounds targetBounds, bool immediate)
	{
		Vector3 vector = CalculateConstrainOffset(targetBounds.min, targetBounds.max);
		if (vector.magnitude > 0f)
		{
			if (immediate)
			{
				target.localPosition += vector;
				targetBounds.center += vector;
				SpringPosition component = target.GetComponent<SpringPosition>();
				if (component != null)
				{
					component.enabled = false;
				}
			}
			else
			{
				SpringPosition springPosition = SpringPosition.Begin(target.gameObject, target.localPosition + vector, 13f);
				springPosition.ignoreTimeScale = true;
				springPosition.worldSpace = false;
			}
			return true;
		}
		return false;
	}

	public bool ConstrainTargetToBounds(Transform target, bool immediate)
	{
		Bounds targetBounds = NGUIMath.CalculateRelativeWidgetBounds(cachedTransform, target);
		return ConstrainTargetToBounds(target, ref targetBounds, immediate);
	}

	private static void SetChildLayer(Transform t, int layer)
	{
		for (int i = 0; i < t.childCount; i++)
		{
			Transform child = t.GetChild(i);
			if (child.GetComponent<UIPanel>() == null)
			{
				if (child.GetComponent<UIWidget>() != null)
				{
					child.gameObject.layer = layer;
				}
				SetChildLayer(child, layer);
			}
		}
	}

	public static UIPanel Find(Transform trans, bool createIfMissing)
	{
		Transform transform = trans;
		UIPanel uIPanel = null;
		while (uIPanel == null && trans != null)
		{
			uIPanel = trans.GetComponent<UIPanel>();
			if (uIPanel != null || trans.parent == null)
			{
				break;
			}
			trans = trans.parent;
		}
		if (createIfMissing && uIPanel == null && trans != transform)
		{
			uIPanel = trans.gameObject.AddComponent<UIPanel>();
			SetChildLayer(uIPanel.cachedTransform, uIPanel.cachedGameObject.layer);
		}
		return uIPanel;
	}

	public static UIPanel Find(Transform trans)
	{
		return Find(trans, createIfMissing: true);
	}
}
