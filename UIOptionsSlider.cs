using System;
using UnityEngine;

public class UIOptionsSlider : MonoBehaviour
{
	public delegate void OnSettingChanged(UIOptionsSlider sender, int newSetting);

	public UISprite Puck;

	public UISprite Track;

	public Collider DownArrow;

	public Collider UpArrow;

	public bool AudioOnDrag;

	public GUIDatabaseString[] SpotTooltipStrings;

	public OnSettingChanged OnChanged;

	private int m_Setting = 1;

	private float m_NotchSize;

	private bool m_TrackHovered;

	public float PuckMin;

	public float PuckMax;

	public int Range;

	public virtual int Setting
	{
		get
		{
			return m_Setting;
		}
		set
		{
			SetSetting(value);
		}
	}

	protected void SetSetting(int value)
	{
		value = Mathf.Clamp(value, 0, Range - 1);
		if (value != m_Setting)
		{
			m_Setting = value;
			if (OnChanged != null)
			{
				OnChanged(this, m_Setting);
			}
		}
		CalculateParameters();
		Puck.transform.localPosition = new Vector3(PuckMin + (float)m_Setting * m_NotchSize, Puck.transform.localPosition.y, Puck.transform.localPosition.z);
	}

	private void CalculateParameters()
	{
		float num = 0f;
		if (PuckMin == 0f && PuckMax == 0f)
		{
			PuckMin = Track.transform.localPosition.x;
			num = Track.transform.localScale.x;
		}
		else
		{
			num = PuckMax - PuckMin;
		}
		m_NotchSize = num / (float)(Range - 1);
	}

	public virtual void Awake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Puck);
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDrag));
		UIEventListener uIEventListener2 = UIEventListener.Get(Puck);
		uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnPuckHovered));
		UIEventListener uIEventListener3 = UIEventListener.Get(Track);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnClickTrack));
		UIEventListener uIEventListener4 = UIEventListener.Get(Track);
		uIEventListener4.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener4.onHover, new UIEventListener.BoolDelegate(OnHoverTrack));
		CalculateParameters();
		if ((bool)DownArrow)
		{
			UIEventListener uIEventListener5 = UIEventListener.Get(DownArrow);
			uIEventListener5.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener5.onClick, new UIEventListener.VoidDelegate(OnDown));
		}
		if ((bool)UpArrow)
		{
			UIEventListener uIEventListener6 = UIEventListener.Get(UpArrow);
			uIEventListener6.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener6.onClick, new UIEventListener.VoidDelegate(OnUp));
		}
	}

	private void Update()
	{
		if (m_TrackHovered && SpotTooltipStrings != null && SpotTooltipStrings.Length != 0)
		{
			int settingByMousePosition = GetSettingByMousePosition();
			if (settingByMousePosition < SpotTooltipStrings.Length)
			{
				UIOptionsTooltip.Show(SpotTooltipStrings[settingByMousePosition].GetText());
			}
		}
	}

	private void OnDrag(GameObject go, Vector2 delta)
	{
		int setting = Setting;
		MovePuckByMouse();
		if (AudioOnDrag)
		{
			PlayAudio(setting);
		}
	}

	private void OnClickTrack(GameObject go)
	{
		int setting = Setting;
		MovePuckByMouse();
		PlayAudio(setting);
	}

	private void OnHoverTrack(GameObject go, bool over)
	{
		m_TrackHovered = over;
	}

	private void OnPuckHovered(GameObject go, bool over)
	{
		if (over)
		{
			int setting = Setting;
			if (setting < SpotTooltipStrings.Length)
			{
				UIOptionsTooltip.Show(SpotTooltipStrings[setting].GetText());
			}
		}
	}

	private void OnDown(GameObject sender)
	{
		int setting = Setting;
		Setting--;
		PlayAudio(setting);
	}

	private void OnUp(GameObject sender)
	{
		int setting = Setting;
		Setting++;
		PlayAudio(setting);
	}

	private void MovePuckByMouse()
	{
		Setting = GetSettingByMousePosition();
	}

	private int GetSettingByMousePosition()
	{
		Camera nGUICamera = InGameUILayout.NGUICamera;
		return Mathf.FloorToInt((base.transform.worldToLocalMatrix.MultiplyPoint3x4(nGUICamera.ScreenToWorldPoint(GameInput.MousePosition)).x - PuckMin - m_NotchSize / 2f) / m_NotchSize) + 1;
	}

	private void PlayAudio(int oldsetting)
	{
		if (oldsetting < Setting)
		{
			if ((bool)GlobalAudioPlayer.Instance)
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.Increment);
			}
		}
		else if (oldsetting > Setting && (bool)GlobalAudioPlayer.Instance)
		{
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.Decrement);
		}
	}
}
