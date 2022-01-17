using UnityEngine;

public class TweenBetweenTransforms : UITweener
{
	public Transform from;

	public Transform to;

	private Transform mTrans;

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

	public Vector3 position
	{
		get
		{
			return cachedTransform.localPosition;
		}
		set
		{
			cachedTransform.localPosition = value;
		}
	}

	public event UIEventListener.VectorDelegate OnPositionChanged;

	protected override void OnUpdate(float factor, bool isFinished)
	{
		Vector3 vector = from.transform.localPosition * (1f - factor) + to.transform.localPosition * factor;
		if (this.OnPositionChanged != null && vector != cachedTransform.localPosition)
		{
			this.OnPositionChanged(base.gameObject, vector);
		}
		cachedTransform.localPosition = vector;
	}

	public static TweenPosition Begin(GameObject go, float duration, Vector3 pos)
	{
		TweenPosition tweenPosition = UITweener.Begin<TweenPosition>(go, duration);
		tweenPosition.from = tweenPosition.position;
		tweenPosition.to = pos;
		if (duration <= 0f)
		{
			tweenPosition.Sample(1f, isFinished: true);
			tweenPosition.enabled = false;
		}
		return tweenPosition;
	}
}
