using UnityEngine;

[ExecuteInEditMode]
public class UIResolutionScaler : MonoBehaviour
{
	public enum Mode
	{
		TRANSFORM,
		STRETCH
	}

	public int DesignedWidth;

	public int DesignedHeight;

	public int MaxUpscaleX;

	public int MaxUpscaleY;

	public bool UseMaximumScale;

	private float xscale;

	private float yscale;

	private UIRoot mUIRoot;

	private UIStretch mStretch;

	public bool MaintainAspect = true;

	public bool ScaleZValue;

	private int m_previousScreenWidth;

	private int m_previousScreenHeight;

	private Vector3 m_previousRootScale;

	public Mode UseMode;

	private UIRoot RootUI
	{
		get
		{
			if (mUIRoot == null)
			{
				mUIRoot = UIRoot.GetFirstUIRoot();
			}
			return mUIRoot;
		}
	}

	public float GetScaleX()
	{
		return xscale;
	}

	public float GetScaleY()
	{
		return yscale;
	}

	private void Start()
	{
		mUIRoot = GetComponentInParent<UIRoot>();
		if (mUIRoot == null)
		{
			mUIRoot = UIRoot.GetFirstUIRoot();
		}
		mStretch = GetComponent<UIStretch>();
		Apply();
	}

	private void OnDestroy()
	{
		mUIRoot = null;
		mStretch = null;
	}

	public void Apply()
	{
		xscale = 1f;
		yscale = 1f;
		float num = (((bool)RootUI && RootUI.maximumHeight > 0) ? Mathf.Min(RootUI.maximumHeight, Screen.height) : Screen.height);
		float num2 = num / (float)Screen.height;
		float num3 = DesignedWidth;
		if (DesignedWidth > 0 && DesignedHeight == 0)
		{
			num3 /= RootUI.pixelSizeAdjustment;
		}
		if (num3 > 0f)
		{
			xscale = (float)Screen.width / num3 * num2;
		}
		if (DesignedHeight > 0)
		{
			yscale = num / (float)DesignedHeight;
		}
		if (MaxUpscaleX > 0 && num3 > 0f)
		{
			float a = (float)MaxUpscaleX / num3;
			xscale = Mathf.Min(a, xscale);
		}
		if (MaxUpscaleY > 0 && DesignedHeight > 0)
		{
			float a2 = (float)MaxUpscaleY / (float)DesignedHeight;
			yscale = Mathf.Min(a2, yscale);
		}
		if (MaintainAspect)
		{
			if (UseMaximumScale)
			{
				xscale = (yscale = Mathf.Max(xscale, yscale));
			}
			else
			{
				xscale = (yscale = Mathf.Min(xscale, yscale));
			}
		}
		switch (UseMode)
		{
		case Mode.TRANSFORM:
			base.transform.localScale = new Vector3(xscale, yscale, ScaleZValue ? xscale : base.transform.localScale.z);
			break;
		case Mode.STRETCH:
			mStretch.relativeSize = new Vector2(xscale, yscale);
			break;
		}
	}

	private void Update()
	{
		if (m_previousScreenWidth != Screen.width || m_previousScreenHeight != Screen.height || ((bool)InGameUILayout.Root && m_previousRootScale != InGameUILayout.Root.transform.localScale))
		{
			Apply();
		}
		m_previousScreenWidth = Screen.width;
		m_previousScreenHeight = Screen.height;
		if ((bool)InGameUILayout.Root)
		{
			m_previousRootScale = InGameUILayout.Root.transform.localScale;
		}
	}
}
