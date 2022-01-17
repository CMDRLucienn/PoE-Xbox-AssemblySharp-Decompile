using System;
using UnityEngine;

[Obsolete("Use ISelectACharacter getters instead.")]
public class UIMapTooltipValue : MonoBehaviour
{
	public UILabel NameLabel;

	public UISprite NameSprite;

	public UILabel ValueLabel;

	public TweenColorIndependent ColorBlinker;

	public Color GoodColor1;

	public Color GoodColor2;

	public Color BadColor1;

	public Color BadColor2;

	public float MaxPulseDur = 2f;

	public float MinPulseDur = 0.1f;

	public int MaxDifference = 50;

	public float MaxDifferenceRatio = 2f;

	private bool m_UsesRatio;

	private bool m_IsAverage;

	public Color UnknownColor = Color.grey;

	private int m_Known;

	private float m_Opposition;

	private int m_OpposerCount;

	private float m_Value;

	private string m_BaseText = "";

	private string m_OpposerText = "";

	private bool m_TweenActive => ColorBlinker.duration > 0f;

	private float m_PulseDur
	{
		get
		{
			if (m_UsesRatio)
			{
				float num = Mathf.Clamp(m_Opposition / m_Value, 1f / MaxDifferenceRatio, MaxDifferenceRatio);
				if (num < 1f)
				{
					num = 1f / num;
				}
				return MaxPulseDur - (MaxPulseDur - MinPulseDur) * num / MaxDifferenceRatio;
			}
			return MaxPulseDur - (MaxPulseDur - MinPulseDur) * Mathf.Clamp(m_Opposition - m_Value, -MaxDifference, MaxDifference) / (float)MaxDifference;
		}
	}

	private void Update()
	{
		m_OpposerText = "";
		if (m_Known == 2)
		{
			_ = m_Opposition;
			_ = 0f;
		}
		if (m_IsAverage)
		{
			m_OpposerText = m_OpposerText + " (" + GUIUtils.GetText(1461) + ")";
		}
		ValueLabel.text = m_BaseText + m_OpposerText;
		RefreshPulse();
		m_Opposition = 0f;
		m_OpposerCount = 0;
		if (m_TweenActive)
		{
			NameLabel.color = Color.white;
			ValueLabel.color = ColorBlinker.color;
		}
		else
		{
			NameLabel.color = Color.white;
			ValueLabel.color = Color.white;
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Clear()
	{
		ValueLabel.text = "";
		NameLabel.color = Color.white;
		ValueLabel.color = Color.white;
		base.gameObject.SetActive(value: false);
	}

	public void SetName(string name)
	{
		NameLabel.text = name + ":";
		NameLabel.gameObject.SetActive(value: true);
	}

	public void SetNameSprite(string sprite)
	{
		NameSprite.spriteName = sprite;
		NameSprite.gameObject.SetActive(value: true);
	}

	public void SetDataAvg(float val, int known, bool usesRat)
	{
		SetData(val, known, usesRat);
		m_IsAverage = true;
	}

	public void SetData(float val, int known, bool usesRat)
	{
		m_IsAverage = false;
		m_UsesRatio = usesRat;
		m_Known = known;
		m_Value = val;
		string text = "";
		if (known <= 1)
		{
			text = "[" + NGUITools.EncodeColor(UnknownColor) + "]";
		}
		text = ((known != 0) ? (text + val.ToString("####0.0")) : (text + GUIUtils.GetText(1980)));
		if (known == 1)
		{
			text += GUIUtils.GetText(1980);
		}
		m_BaseText = text;
		ValueLabel.text = m_BaseText + m_OpposerText;
	}

	private void RefreshPulse()
	{
		if (m_OpposerCount > 0 && m_Known == 2)
		{
			ColorBlinker.duration = m_PulseDur;
		}
		else
		{
			ColorBlinker.duration = 0f;
		}
		if (m_Value > m_Opposition)
		{
			ColorBlinker.to = BadColor1;
			ColorBlinker.from = BadColor2;
		}
		else
		{
			ColorBlinker.to = GoodColor1;
			ColorBlinker.from = GoodColor2;
		}
	}

	public void AddOpposer(float val)
	{
		if (val != 0f)
		{
			m_Opposition = ((float)m_OpposerCount * m_Opposition + val) / (float)(m_OpposerCount + 1);
			m_OpposerCount++;
		}
	}
}
