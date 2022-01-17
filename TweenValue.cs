using UnityEngine;

public class TweenValue : UITweener
{
	public float from;

	public float to = 1f;

	public float Value { get; private set; }

	protected override void OnUpdate(float factor, bool isFinished)
	{
		Value = Mathf.Lerp(from, to, factor);
	}

	public static TweenValue Begin(GameObject go, float duration, float value)
	{
		TweenValue tweenValue = UITweener.Begin<TweenValue>(go, duration);
		tweenValue.from = tweenValue.Value;
		tweenValue.to = value;
		if (duration <= 0f)
		{
			tweenValue.Sample(1f, isFinished: true);
			tweenValue.enabled = false;
		}
		return tweenValue;
	}
}
