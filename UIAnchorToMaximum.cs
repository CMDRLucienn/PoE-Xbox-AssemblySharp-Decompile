using UnityEngine;

[ExecuteInEditMode]
public class UIAnchorToMaximum : MonoBehaviour
{
	public bool RefreshAnchors;

	public UIAnchor.Side Side;

	private UIAnchor[] m_Anchors;

	private Transform mTrans;

	private void Start()
	{
		RefreshAnchors = true;
		Update();
	}

	public void Update()
	{
		if (!mTrans)
		{
			mTrans = base.transform;
		}
		if (RefreshAnchors)
		{
			m_Anchors = GetComponents<UIAnchor>();
			for (int i = 0; i < m_Anchors.Length; i++)
			{
				if ((bool)m_Anchors[i])
				{
					m_Anchors[i].enabled = false;
				}
			}
			RefreshAnchors = false;
		}
		if (m_Anchors == null || m_Anchors.Length == 0)
		{
			return;
		}
		Vector3 position = m_Anchors[0].GetPosition();
		bool flag = false;
		bool flag2 = false;
		for (int j = 0; j < m_Anchors.Length; j++)
		{
			if (!m_Anchors[j] || ((bool)m_Anchors[j].widgetContainer && (!m_Anchors[j].widgetContainer.isActiveAndEnabled || !m_Anchors[j].widgetContainer.isVisible)))
			{
				continue;
			}
			Vector3 position2 = m_Anchors[j].GetPosition();
			if (UIWidgetUtils.AnchorIsLeft(Side))
			{
				if (flag)
				{
					position.x = Mathf.Min(position2.x, position.x);
				}
				else
				{
					position.x = position2.x;
					flag = true;
				}
			}
			else if (UIWidgetUtils.AnchorIsRight(Side))
			{
				if (flag)
				{
					position.x = Mathf.Max(position2.x, position.x);
				}
				else
				{
					position.x = position2.x;
					flag = true;
				}
			}
			if (UIWidgetUtils.AnchorIsBottom(Side))
			{
				if (flag2)
				{
					position.y = Mathf.Min(position2.y, position.y);
					continue;
				}
				position.y = position2.y;
				flag2 = true;
			}
			else if (UIWidgetUtils.AnchorIsTop(Side))
			{
				if (flag2)
				{
					position.y = Mathf.Max(position2.y, position.y);
					continue;
				}
				position.y = position2.y;
				flag2 = true;
			}
		}
		if (!flag)
		{
			position.x = mTrans.position.x;
		}
		if (!flag2)
		{
			position.y = mTrans.position.y;
		}
		float z = mTrans.localPosition.z;
		if (mTrans.position != position)
		{
			mTrans.position = position;
		}
		Vector3 localPosition = mTrans.localPosition;
		localPosition.z = z;
		if (localPosition != mTrans.localPosition)
		{
			mTrans.localPosition = localPosition;
		}
	}
}
