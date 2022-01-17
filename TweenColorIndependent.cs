using UnityEngine;

public class TweenColorIndependent : UITweener
{
	public Color from = Color.white;

	public Color to = Color.white;

	public float sample;

	public Color color { get; set; }

	protected override void OnUpdate(float factor, bool isFinished)
	{
		color = Color.Lerp(from, to, factor);
		sample = factor;
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

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}
}
