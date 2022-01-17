using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Stretch")]
public class UIStretch : MonoBehaviour
{
	public enum Style
	{
		None,
		Horizontal,
		Vertical,
		Both,
		BasedOnHeight,
		FillKeepingRatio,
		FitInternalKeepingRatio,
		Indentation,
		HorizontalWithVertical
	}

	public Camera uiCamera;

	public UIWidget widgetContainer;

	public UIPanel panelContainer;

	public Style style;

	public bool runOnlyOnce;

	public Vector2 relativeSize = Vector2.one;

	public Vector2 initialSize = Vector2.one;

	public Vector2 pixelAdjustment = Vector2.zero;

	public Vector2 minimumSize = Vector2.zero;

	private Transform mTrans;

	private UIRoot mRoot;

	private Animation mAnim;

	private Rect mRect;

	private UIPanel m_Panel;

	private UILabel m_Label;

	private UITextList m_TextList;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		if (mTrans == null)
		{
			mAnim = GetComponent<Animation>();
			mRect = default(Rect);
			mTrans = base.transform;
			m_Panel = GetComponent<UIPanel>();
			m_Label = GetComponent<UILabel>();
			m_TextList = GetComponent<UITextList>();
		}
	}

	private void Start()
	{
		if (uiCamera == null)
		{
			uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
		}
		mRoot = NGUITools.FindInParents<UIRoot>(base.gameObject);
		Update();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Update()
	{
		Init();
		if ((mAnim != null && mAnim.isPlaying) || style == Style.None)
		{
			return;
		}
		float num = 1f;
		if (panelContainer != null)
		{
			if (panelContainer.clipping == UIDrawCall.Clipping.None)
			{
				mRect.xMin = (float)(-Screen.width) * 0.5f;
				mRect.yMin = (float)(-Screen.height) * 0.5f;
				mRect.xMax = 0f - mRect.xMin;
				mRect.yMax = 0f - mRect.yMin;
			}
			else
			{
				Vector4 clipRange = panelContainer.clipRange;
				mRect.x = clipRange.x - clipRange.z * 0.5f;
				mRect.y = clipRange.y - clipRange.w * 0.5f;
				mRect.width = clipRange.z;
				mRect.height = clipRange.w;
			}
		}
		else if (widgetContainer != null)
		{
			Transform cachedTransform = widgetContainer.cachedTransform;
			Vector3 localScale = cachedTransform.localScale;
			Vector3 localPosition = cachedTransform.localPosition;
			Vector3 vector = widgetContainer.relativeSize;
			Vector3 vector2 = widgetContainer.pivotOffset;
			vector2.y -= 1f;
			vector2.x *= widgetContainer.relativeSize.x * localScale.x;
			vector2.y *= widgetContainer.relativeSize.y * localScale.y;
			mRect.x = localPosition.x + vector2.x;
			mRect.y = localPosition.y + vector2.y;
			mRect.width = vector.x * localScale.x;
			mRect.height = vector.y * localScale.y;
		}
		else
		{
			if (!(uiCamera != null))
			{
				return;
			}
			mRect = uiCamera.pixelRect;
			if (mRoot != null)
			{
				num = mRoot.pixelSizeAdjustment;
			}
		}
		float num2 = mRect.width + pixelAdjustment.x;
		float num3 = mRect.height + pixelAdjustment.y;
		if (num != 1f && num3 > 1f)
		{
			float num4 = (float)mRoot.activeHeight / num3;
			num2 *= num4;
			num3 *= num4;
		}
		Vector3 localScale2 = mTrans.localScale;
		if (style == Style.BasedOnHeight)
		{
			localScale2.x = relativeSize.x * num3;
			localScale2.y = relativeSize.y * num3;
		}
		else if (style == Style.FillKeepingRatio)
		{
			float num5 = num2 / num3;
			if (initialSize.x / initialSize.y < num5)
			{
				float num6 = num2 / initialSize.x;
				localScale2.x = num2;
				localScale2.y = initialSize.y * num6;
			}
			else
			{
				float num7 = num3 / initialSize.y;
				localScale2.x = initialSize.x * num7;
				localScale2.y = num3;
			}
		}
		else if (style == Style.FitInternalKeepingRatio)
		{
			float num8 = num2 / num3;
			if (initialSize.x / initialSize.y > num8)
			{
				float num9 = num2 / initialSize.x;
				localScale2.x = num2;
				localScale2.y = initialSize.y * num9;
			}
			else
			{
				float num10 = num3 / initialSize.y;
				localScale2.x = initialSize.x * num10;
				localScale2.y = num3;
			}
		}
		else if (style == Style.Indentation)
		{
			localScale2.x = num2;
			localScale2.y = 0f;
		}
		else
		{
			if (style == Style.Both || style == Style.Horizontal)
			{
				localScale2.x = relativeSize.x * num2;
			}
			else if (style == Style.HorizontalWithVertical)
			{
				localScale2.x = relativeSize.x * num3;
			}
			else if (pixelAdjustment.x != 0f)
			{
				localScale2.x = relativeSize.x * pixelAdjustment.x;
			}
			if (style == Style.Both || style == Style.Vertical)
			{
				localScale2.y = relativeSize.y * num3;
			}
			else if (pixelAdjustment.y != 0f)
			{
				localScale2.y = relativeSize.y * pixelAdjustment.y;
			}
		}
		if (minimumSize.x != 0f)
		{
			localScale2.x = Mathf.Max(localScale2.x, minimumSize.x);
		}
		if (minimumSize.y != 0f)
		{
			localScale2.y = Mathf.Max(localScale2.y, minimumSize.y);
		}
		if ((bool)m_Panel)
		{
			if (m_Panel.clipRange.z != localScale2.x || m_Panel.clipRange.w != localScale2.y)
			{
				m_Panel.clipRange = new Vector4(m_Panel.clipRange.x, m_Panel.clipRange.y, localScale2.x, localScale2.y);
			}
		}
		else if ((bool)m_Label)
		{
			if (style == Style.Indentation)
			{
				if ((int)localScale2.x != m_Label.indentAmount)
				{
					m_Label.indentAmount = (int)localScale2.x;
				}
			}
			else
			{
				if (style != Style.Horizontal)
				{
					float num11 = localScale2.y / (m_Label.transform.localScale.y + (float)m_Label.font.verticalSpacing);
					if (num11 != (float)m_Label.maxLineCount)
					{
						m_Label.maxLineCount = (int)num11;
					}
				}
				if ((int)localScale2.x != m_Label.lineWidth)
				{
					m_Label.lineWidth = (int)localScale2.x;
				}
			}
		}
		else if ((bool)m_TextList)
		{
			if (style != Style.Vertical)
			{
				m_TextList.maxWidth = (int)localScale2.x;
			}
		}
		else if (mTrans.localScale != localScale2)
		{
			mTrans.localScale = localScale2;
		}
		if (runOnlyOnce && Application.isPlaying)
		{
			Object.Destroy(this);
		}
	}
}
