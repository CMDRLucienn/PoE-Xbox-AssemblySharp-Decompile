using UnityEngine;

[RequireComponent(typeof(UIAnchor))]
[ExecuteInEditMode]
public class UIAnchorToFill : MonoBehaviour
{
	public UISprite SpriteTarget;

	private UIAnchor m_Anchor;

	private void Start()
	{
		m_Anchor = GetComponent<UIAnchor>();
	}

	private void Update()
	{
		if (!SpriteTarget || SpriteTarget.type != UISprite.Type.Filled || SpriteTarget.fillDirection == UISprite.FillDirection.Horizontal || SpriteTarget.fillDirection != UISprite.FillDirection.Vertical)
		{
			return;
		}
		if (SpriteTarget.invert)
		{
			if (m_Anchor.side == UIAnchor.Side.Bottom || m_Anchor.side == UIAnchor.Side.BottomLeft || m_Anchor.side == UIAnchor.Side.BottomRight)
			{
				m_Anchor.pixelOffset.y = (1f - SpriteTarget.fillAmount) * SpriteTarget.transform.localScale.y;
			}
		}
		else if (m_Anchor.side == UIAnchor.Side.Top || m_Anchor.side == UIAnchor.Side.TopLeft || m_Anchor.side == UIAnchor.Side.TopRight)
		{
			m_Anchor.pixelOffset.y = (0f - (1f - SpriteTarget.fillAmount)) * SpriteTarget.transform.localScale.y;
		}
	}
}
