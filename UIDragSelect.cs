using UnityEngine;

public class UIDragSelect : MonoBehaviour
{
	private UIWidget m_Widget;

	public static UIDragSelect Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		m_Widget = GetComponent<UIWidget>();
		Hide();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void Set(Rect r)
	{
		Vector2 min = r.min;
		float num = r.width;
		float num2 = r.height;
		if (num < 0f)
		{
			num *= -1f;
			min.x -= num;
		}
		if (num2 < 0f)
		{
			num2 *= -1f;
			min.y -= num2;
		}
		Camera nGUICamera = InGameUILayout.NGUICamera;
		num = Mathf.Floor(num);
		num2 = Mathf.Floor(num2);
		Vector2 vector = nGUICamera.ScreenToWorldPoint(min + new Vector2(num, num2));
		m_Widget.transform.localScale = new Vector2(num, num2) * InGameUILayout.toNguiScale;
		m_Widget.transform.position = ((Vector2)nGUICamera.ScreenToWorldPoint(min) + vector) / 2f;
	}

	public void Show(bool visible = true)
	{
		m_Widget.alpha = (visible ? 1 : 0);
	}

	public void Hide()
	{
		Show(visible: false);
	}
}
