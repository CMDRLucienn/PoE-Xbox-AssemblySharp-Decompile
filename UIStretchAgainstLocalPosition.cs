using UnityEngine;

[ExecuteInEditMode]
public class UIStretchAgainstLocalPosition : MonoBehaviour
{
	public Transform OtherObject;

	public Vector2 PixelAdjustment;

	public bool InvertX;

	public bool InvertY;

	private UIWidget m_Widget;

	private UILabel m_Label;

	private void Awake()
	{
		m_Widget = GetComponent<UIWidget>();
		m_Label = m_Widget as UILabel;
	}

	private void Update()
	{
		if ((bool)OtherObject)
		{
			if ((bool)m_Label)
			{
				m_Label.lineWidth = (int)(PixelAdjustment.x + (InvertX ? (0f - OtherObject.localPosition.x) : OtherObject.localPosition.x));
			}
			else
			{
				base.transform.localScale = new Vector3(PixelAdjustment.x + (InvertX ? (0f - OtherObject.localPosition.x) : OtherObject.localPosition.x), PixelAdjustment.y + (InvertY ? (0f - OtherObject.localPosition.y) : OtherObject.localPosition.y), 1f);
			}
		}
	}
}
