using System;
using UnityEngine;

public abstract class UIWidget : MonoBehaviour
{
	public enum Pivot
	{
		TopLeft,
		Top,
		TopRight,
		Left,
		Center,
		Right,
		BottomLeft,
		Bottom,
		BottomRight
	}

	[HideInInspector]
	[SerializeField]
	protected Material mMat;

	[HideInInspector]
	[SerializeField]
	protected Texture mTex;

	[HideInInspector]
	[SerializeField]
	private Color mColor = Color.black;

	[HideInInspector]
	[SerializeField]
	private Pivot mPivot = Pivot.Center;

	[HideInInspector]
	[SerializeField]
	private int mDepth;

	protected GameObject mGo;

	protected Transform mTrans;

	protected UIPanel mPanel;

	protected bool mChanged = true;

	protected bool mPlayMode = true;

	private bool mStarted;

	private Matrix4x4 mLocalToPanel;

	private bool mVisibleByPanel = true;

	private float mLastAlpha;

	private UIGeometry mGeom = new UIGeometry();

	public UIBoundsParent containedBy;

	private bool mForceVisible;

	private Vector3 mOldV0;

	private Vector3 mOldV1;

	public bool _CachedVisible;

	public bool _CachedVisibleDirty = true;

	public bool isVisible
	{
		get
		{
			if (mVisibleByPanel)
			{
				return finalAlpha > 0.001f;
			}
			return false;
		}
	}

	public Color color
	{
		get
		{
			return mColor;
		}
		set
		{
			if (mColor != value)
			{
				mColor = value;
				mChanged = true;
			}
		}
	}

	public float alpha
	{
		get
		{
			return mColor.a;
		}
		set
		{
			if (mColor.a != value)
			{
				mColor.a = value;
				mChanged = true;
			}
		}
	}

	public float finalAlpha
	{
		get
		{
			if (mPanel == null)
			{
				CreatePanel();
			}
			if (!(mPanel != null))
			{
				return mColor.a;
			}
			return mColor.a * mPanel.alpha;
		}
	}

	public Pivot pivot
	{
		get
		{
			return mPivot;
		}
		set
		{
			if (mPivot != value)
			{
				Vector3 vector = NGUIMath.CalculateWidgetCorners(this)[0];
				mPivot = value;
				mChanged = true;
				Vector3 vector2 = NGUIMath.CalculateWidgetCorners(this)[0];
				Transform obj = cachedTransform;
				Vector3 position = obj.position;
				float z = obj.localPosition.z;
				position.x += vector.x - vector2.x;
				position.y += vector.y - vector2.y;
				cachedTransform.position = position;
				position = cachedTransform.localPosition;
				position.x = Mathf.Round(position.x);
				position.y = Mathf.Round(position.y);
				position.z = z;
				cachedTransform.localPosition = position;
			}
		}
	}

	public int depth
	{
		get
		{
			return mDepth;
		}
		set
		{
			if (mDepth != value)
			{
				mDepth = value;
				if (mPanel != null)
				{
					mPanel.MarkMaterialAsChanged(material, sort: true);
				}
			}
		}
	}

	public Vector2 pivotOffset
	{
		get
		{
			Vector2 zero = Vector2.zero;
			Vector4 vector = relativePadding;
			Pivot pivot = this.pivot;
			switch (pivot)
			{
			case Pivot.Top:
			case Pivot.Center:
			case Pivot.Bottom:
				zero.x = (vector.x - vector.z - 1f) * 0.5f;
				break;
			case Pivot.TopRight:
			case Pivot.Right:
			case Pivot.BottomRight:
				zero.x = -1f - vector.z;
				break;
			default:
				zero.x = vector.x;
				break;
			}
			switch (pivot)
			{
			case Pivot.Left:
			case Pivot.Center:
			case Pivot.Right:
				zero.y = (vector.w - vector.y + 1f) * 0.5f;
				break;
			case Pivot.BottomLeft:
			case Pivot.Bottom:
			case Pivot.BottomRight:
				zero.y = 1f + vector.w;
				break;
			default:
				zero.y = 0f - vector.y;
				break;
			}
			return zero;
		}
	}

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

	public virtual Material material
	{
		get
		{
			return mMat;
		}
		set
		{
			if (mMat != value)
			{
				if (mMat != null && mPanel != null)
				{
					mPanel.RemoveWidget(this);
				}
				mPanel = null;
				mMat = value;
				mTex = null;
				if (mMat != null && base.enabled)
				{
					CreatePanel();
				}
			}
		}
	}

	public virtual Texture mainTexture
	{
		get
		{
			Material material = this.material;
			if (material != null)
			{
				if (material.mainTexture != null)
				{
					mTex = material.mainTexture;
				}
				else if (mTex != null)
				{
					if (mPanel != null)
					{
						mPanel.RemoveWidget(this);
					}
					mPanel = null;
					mMat.mainTexture = mTex;
					if (base.enabled)
					{
						CreatePanel();
					}
				}
			}
			return mTex;
		}
		set
		{
			Material material = this.material;
			if (!(material == null) && !(material.mainTexture != value))
			{
				return;
			}
			if (mPanel != null)
			{
				mPanel.RemoveWidget(this);
			}
			mPanel = null;
			mTex = value;
			material = this.material;
			if (material != null)
			{
				material.mainTexture = value;
				if (base.enabled)
				{
					CreatePanel();
				}
			}
		}
	}

	public UIPanel panel
	{
		get
		{
			if (mPanel == null)
			{
				CreatePanel();
			}
			return mPanel;
		}
		set
		{
			mPanel = value;
		}
	}

	public virtual Vector2 relativeSize => Vector2.one;

	public virtual Vector4 relativePadding => Vector4.zero;

	public virtual Vector4 border => Vector4.zero;

	public virtual bool keepMaterial => false;

	public virtual bool pixelPerfectAfterResize => false;

	public static BetterList<UIWidget> Raycast(GameObject root, Vector2 mousePos)
	{
		BetterList<UIWidget> betterList = new BetterList<UIWidget>();
		UICamera uICamera = UICamera.FindCameraForLayer(root.layer);
		if (uICamera != null)
		{
			Camera cachedCamera = uICamera.cachedCamera;
			UIWidget[] componentsInChildren = root.GetComponentsInChildren<UIWidget>();
			foreach (UIWidget uIWidget in componentsInChildren)
			{
				if (NGUIMath.DistanceToRectangle(NGUIMath.CalculateWidgetCorners(uIWidget), mousePos, cachedCamera) == 0f)
				{
					betterList.Add(uIWidget);
				}
			}
			betterList.Sort((UIWidget w1, UIWidget w2) => w2.mDepth.CompareTo(w1.mDepth));
		}
		return betterList;
	}

	public static bool RaycastAny(GameObject root, Vector2 mousePos)
	{
		UICamera uICamera = UICamera.FindCameraForLayer(root.layer);
		if (uICamera != null)
		{
			Camera cachedCamera = uICamera.cachedCamera;
			UIWidget[] componentsInChildren = root.GetComponentsInChildren<UIWidget>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (NGUIMath.DistanceToRectangle(NGUIMath.CalculateWidgetCorners(componentsInChildren[i]), mousePos, cachedCamera) == 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static int CompareFunc(UIWidget left, UIWidget right)
	{
		if (left.mDepth > right.mDepth)
		{
			return 1;
		}
		if (left.mDepth < right.mDepth)
		{
			return -1;
		}
		return 0;
	}

	public void MarkAsChangedLite()
	{
		mChanged = true;
	}

	public virtual void MarkAsChanged()
	{
		mChanged = true;
		if (mPanel != null && base.enabled && NGUITools.GetActive(base.gameObject) && !Application.isPlaying && material != null)
		{
			mPanel.AddWidget(this);
			CheckLayer();
		}
	}

	public void CreatePanel()
	{
		if (mPanel == null && base.enabled && NGUITools.GetActive(base.gameObject) && material != null)
		{
			mPanel = UIPanel.Find(cachedTransform, mStarted);
			if (mPanel != null)
			{
				CheckLayer();
				mPanel.AddWidget(this);
				mChanged = true;
			}
		}
	}

	public void CheckLayer()
	{
		if (mPanel != null && mPanel.gameObject.layer != base.gameObject.layer)
		{
			Debug.LogWarning("You can't place widgets on a layer different than the UIPanel that manages them.\nIf you want to move widgets to a different layer, parent them to a new panel instead.", this);
			base.gameObject.layer = mPanel.gameObject.layer;
		}
	}

	[Obsolete("Use ParentHasChanged() instead")]
	public void CheckParent()
	{
		ParentHasChanged();
	}

	public void ParentHasChanged()
	{
		if (!(mPanel != null))
		{
			return;
		}
		UIPanel uIPanel = UIPanel.Find(cachedTransform);
		if (mPanel != uIPanel)
		{
			mPanel.RemoveWidget(this);
			if (!keepMaterial || Application.isPlaying)
			{
				material = null;
			}
			mPanel = null;
			CreatePanel();
		}
	}

	protected virtual void Awake()
	{
		mGo = base.gameObject;
		mPlayMode = Application.isPlaying;
	}

	protected virtual void OnEnable()
	{
		mChanged = true;
		if (!keepMaterial)
		{
			mMat = null;
			mTex = null;
		}
		mPanel = null;
	}

	private void Start()
	{
		mStarted = true;
		OnStart();
		CreatePanel();
	}

	public virtual void Update()
	{
		if (mPanel == null)
		{
			CreatePanel();
		}
	}

	protected virtual void OnDisable()
	{
		if (!keepMaterial)
		{
			material = null;
		}
		else if (mPanel != null)
		{
			mPanel.RemoveWidget(this);
		}
		mPanel = null;
	}

	protected virtual void OnDestroy()
	{
		if (mPanel != null)
		{
			mPanel.RemoveWidget(this);
			mPanel = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void CacheVisibility(bool state)
	{
		_CachedVisibleDirty = false;
		_CachedVisible = state;
	}

	public bool UpdateGeometry(UIPanel p, bool forceVisible)
	{
		if (material != null && p != null)
		{
			mPanel = p;
			bool flag = false;
			float num = finalAlpha;
			bool flag2 = num > 0.001f;
			bool flag3 = forceVisible || mVisibleByPanel;
			if (cachedTransform.hasChanged)
			{
				mTrans.hasChanged = false;
				if (!mPanel.widgetsAreStatic)
				{
					Vector2 vector = relativeSize;
					Vector2 vector2 = pivotOffset;
					Vector4 vector3 = relativePadding;
					float num2 = vector2.x * vector.x - vector3.x;
					float num3 = vector2.y * vector.y + vector3.y;
					float x = num2 + vector.x + vector3.x + vector3.z;
					float y = num3 - vector.y - vector3.y - vector3.w;
					mLocalToPanel = p.worldToLocal * cachedTransform.localToWorldMatrix;
					flag = true;
					Vector3 point = new Vector3(num2, num3, 0f);
					Vector3 point2 = new Vector3(x, y, 0f);
					point = mLocalToPanel.MultiplyPoint3x4(point);
					point2 = mLocalToPanel.MultiplyPoint3x4(point2);
					if (Vector3.SqrMagnitude(mOldV0 - point) > 1E-06f || Vector3.SqrMagnitude(mOldV1 - point2) > 1E-06f)
					{
						mChanged = true;
						mOldV0 = point;
						mOldV1 = point2;
					}
				}
				if (flag2 || mForceVisible != forceVisible)
				{
					mForceVisible = forceVisible;
					flag3 = forceVisible || mPanel.IsVisible(this);
				}
			}
			else if (flag2 && mForceVisible != forceVisible)
			{
				mForceVisible = forceVisible;
				flag3 = mPanel.IsVisible(this);
			}
			if (mVisibleByPanel != flag3)
			{
				mVisibleByPanel = flag3;
				mChanged = true;
			}
			if (mVisibleByPanel && mLastAlpha != num)
			{
				mChanged = true;
			}
			mLastAlpha = num;
			if (mChanged)
			{
				mChanged = false;
				if (isVisible)
				{
					mGeom.Clear();
					OnFill(mGeom.verts, mGeom.uvs, mGeom.cols);
					if (mGeom.hasVertices)
					{
						Vector3 vector4 = pivotOffset;
						Vector2 vector5 = relativeSize;
						vector4.x *= vector5.x;
						vector4.y *= vector5.y;
						if (!flag)
						{
							mLocalToPanel = p.worldToLocal * cachedTransform.localToWorldMatrix;
						}
						mGeom.ApplyOffset(vector4);
						mGeom.ApplyTransform(mLocalToPanel);
					}
					return true;
				}
				if (mGeom.hasVertices)
				{
					mGeom.Clear();
					return true;
				}
			}
		}
		return false;
	}

	public void WriteToBuffers(BetterList<Vector3> v, BetterList<Vector2> u, BetterList<Color32> c, BetterList<Vector3> n, BetterList<Vector4> t)
	{
		mGeom.WriteToBuffers(v, u, c, n, t);
	}

	public virtual void MakePixelPerfect()
	{
		Vector3 localScale = cachedTransform.localScale;
		int num = Mathf.RoundToInt(localScale.x);
		int num2 = Mathf.RoundToInt(localScale.y);
		localScale.x = num;
		localScale.y = num2;
		localScale.z = 1f;
		Vector3 localPosition = cachedTransform.localPosition;
		if (num % 2 == 1 && (pivot == Pivot.Top || pivot == Pivot.Center || pivot == Pivot.Bottom))
		{
			localPosition.x = Mathf.Floor(localPosition.x) + 0.5f;
		}
		else
		{
			localPosition.x = Mathf.Round(localPosition.x);
		}
		if (num2 % 2 == 1 && (pivot == Pivot.Left || pivot == Pivot.Center || pivot == Pivot.Right))
		{
			localPosition.y = Mathf.Ceil(localPosition.y) - 0.5f;
		}
		else
		{
			localPosition.y = Mathf.Round(localPosition.y);
		}
		cachedTransform.localPosition = localPosition;
		cachedTransform.localScale = localScale;
	}

	protected virtual void OnStart()
	{
	}

	public virtual void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
	}
}
