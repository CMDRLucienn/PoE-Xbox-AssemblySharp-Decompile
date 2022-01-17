using UnityEngine;

public class ScaleByPause : MonoBehaviour
{
	public UITweener Tween;

	private float m_TweenDur;

	public float Duration;

	public Vector3 PausedScale = Vector3.one;

	public Vector3 UnpausedScale = Vector3.one;

	public bool OnX = true;

	public bool OnY = true;

	public bool OnZ = true;

	private bool m_MyState;

	private float m_Dir;

	private float m_Time;

	public bool TweenIn;

	public bool TweenOut = true;

	private void Start()
	{
		m_Dir = -1f;
		m_TweenDur = Tween.duration;
	}

	private void Update()
	{
		bool flag = TimeController.Instance == null || TimeController.Instance.Paused;
		if (flag && !m_MyState)
		{
			if (!TweenIn)
			{
				Tween.duration = 0f;
			}
			else
			{
				Tween.duration = m_TweenDur;
			}
			if ((bool)Tween)
			{
				Tween.Play(forward: true);
			}
			else
			{
				m_Dir = 1f;
			}
		}
		else if (!flag && m_MyState)
		{
			if (!TweenOut)
			{
				Tween.duration = 0f;
			}
			else
			{
				Tween.duration = m_TweenDur;
			}
			if ((bool)Tween)
			{
				Tween.Play(forward: false);
			}
			else
			{
				m_Dir = -1f;
			}
		}
		m_MyState = flag;
		if (m_Dir != 0f)
		{
			m_Time += m_Dir * TimeController.sUnscaledDelta;
			if (m_Time < 0f)
			{
				m_Time = 0f;
				m_Dir = 0f;
			}
			else if (m_Time >= Duration)
			{
				m_Time = Duration;
				m_Dir = 0f;
			}
			float num = m_Time / Duration;
			base.transform.localScale = new Vector3(OnX ? (num * (PausedScale.x - UnpausedScale.x) + UnpausedScale.x) : base.transform.localScale.x, OnY ? (num * (PausedScale.y - UnpausedScale.y) + UnpausedScale.y) : base.transform.localScale.y, OnZ ? (num * (PausedScale.z - UnpausedScale.z) + UnpausedScale.z) : base.transform.localScale.z);
		}
	}
}
