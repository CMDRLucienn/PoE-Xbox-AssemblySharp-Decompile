using UnityEngine;

public class UIStorePageButton : MonoBehaviour
{
	public UIStorePageType TabType;

	public UIWidget HighlightWidget;

	private UIMultiSpriteImageButton m_ImageButton;

	private void Start()
	{
		m_ImageButton = GetComponent<UIMultiSpriteImageButton>();
		UIStoreManager.Instance.OnPageChanged += OnPageChanged;
		OnPageChanged(UIStoreManager.Instance.Page);
	}

	private void OnClick()
	{
		UIStoreManager.Instance.Page = TabType;
	}

	private void OnPageChanged(UIStorePageType page)
	{
		HighlightWidget.alpha = ((page == TabType) ? 1f : 0f);
		m_ImageButton.ForceDown(page == TabType);
	}
}
