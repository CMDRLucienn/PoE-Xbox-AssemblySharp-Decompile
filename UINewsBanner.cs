using UnityEngine;

public class UINewsBanner : MonoBehaviour
{
	[SerializeField]
	private int m_maxWidth;

	[SerializeField]
	private UITexture m_image;

	private BoxCollider m_collider;

	public bool HasImage => m_image.mainTexture != null;

	private void Awake()
	{
		m_collider = GetComponent<BoxCollider>();
		UINewsManager.OnBannerTextureLoaded += OnBannerTextureLoaded;
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		m_collider.enabled = m_image.alpha > 0f;
	}

	private void OnDestroy()
	{
		UINewsManager.OnBannerTextureLoaded -= OnBannerTextureLoaded;
	}

	private void OnClick()
	{
		if (!string.IsNullOrEmpty(UINewsManager.Instance.BannerLinkURL))
		{
			Application.OpenURL(UINewsManager.Instance.BannerLinkURL);
		}
	}

	private void OnBannerTextureLoaded(Texture2D texture)
	{
		if (texture != null)
		{
			SetTexture(texture);
			base.gameObject.SetActive(value: true);
		}
	}

	public void SetTexture(Texture2D texture)
	{
		m_image.mainTexture = texture;
		Vector3 localScale = new Vector3(texture.width, texture.height, 1f);
		float num = localScale.x / localScale.y;
		if (localScale.x > (float)m_maxWidth)
		{
			localScale.x = m_maxWidth;
			localScale.y = localScale.x / num;
		}
		m_image.transform.localScale = localScale;
	}
}
