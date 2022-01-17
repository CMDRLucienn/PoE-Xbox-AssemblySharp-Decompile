using UnityEngine;

[AddComponentMenu("NGUI/Tween/Tween Color")]
public class TweenColor : UITweener
{
	public delegate void ColorChanged(Color color);

	public Color from = Color.red;

	public Color to = Color.white;

	private UIWidget mWidget;

	private Light mLight;

	public Color color
	{
		get
		{
			if (mWidget != null)
			{
				return mWidget.color;
			}
			if (mLight != null)
			{
				return mLight.color;
			}
			if ((bool)GetComponent<Renderer>())
			{
				Material material = GetComponent<Renderer>().sharedMaterial ?? GetComponent<Renderer>().material;
				if (material != null)
				{
					return material.color;
				}
			}
			return Color.black;
		}
		set
		{
			if (mWidget != null)
			{
				mWidget.color = value;
			}
			if ((bool)GetComponent<Renderer>())
			{
				Material material = GetComponent<Renderer>().sharedMaterial ?? GetComponent<Renderer>().material;
				if (material != null)
				{
					material.color = value;
				}
			}
			if (mLight != null)
			{
				mLight.color = value;
				mLight.enabled = value.r + value.g + value.b > 0.01f;
			}
			if (this.OnColorChanged != null)
			{
				this.OnColorChanged(value);
			}
		}
	}

	public event ColorChanged OnColorChanged;

	private void Awake()
	{
		mWidget = GetComponentInChildren<UIWidget>();
		mLight = GetComponent<Light>();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected override void OnUpdate(float factor, bool isFinished)
	{
		color = Color.Lerp(from, to, factor);
	}

	public static TweenColor Begin(GameObject go, float duration, Color color)
	{
		TweenColor tweenColor = UITweener.Begin<TweenColor>(go, duration);
		tweenColor.from = tweenColor.color;
		tweenColor.to = color;
		if (duration <= 0f)
		{
			tweenColor.Sample(1f, isFinished: true);
			tweenColor.enabled = false;
		}
		return tweenColor;
	}
}
