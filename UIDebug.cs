using System.Collections.Generic;
using UnityEngine;

public class UIDebug : MonoBehaviour
{
	private class OnScreenText
	{
		public UILabel m_label;

		public float? m_onScreenTime;

		public OnScreenText(UILabel label, float? onScreenTime)
		{
			m_label = label;
			m_onScreenTime = onScreenTime;
		}
	}

	public enum Department
	{
		None,
		Programming,
		Animation,
		Design,
		Art
	}

	private static UIDebug m_Instance;

	private static bool s_Quitting;

	public UILabel DebugText;

	private Dictionary<string, OnScreenText> TextDictionary = new Dictionary<string, OnScreenText>();

	private Camera m_uiCamera;

	private List<string> m_removeList = new List<string>();

	private const float TEXT_SCALE = 25f;

	private const float MAX_WARNINGS_SHOWN = 5f;

	private const float TEXT_OFFSET = 5f;

	private const float MAX_SCREEN_PERCENTAGE_WIDTH = 0.85f;

	public static UIDebug Instance
	{
		get
		{
			if (m_Instance == null && !s_Quitting && Application.isPlaying)
			{
				m_Instance = new GameObject
				{
					name = "UIDebugObj"
				}.AddComponent<UIDebug>();
			}
			return m_Instance;
		}
	}

	private void Update()
	{
		if (TextDictionary.Count == 0)
		{
			return;
		}
		if (DebugText == null)
		{
			GameUtilities.Destroy(base.gameObject);
			return;
		}
		if (GameInput.GetKeyDown(KeyCode.Delete))
		{
			RemoveAll();
		}
		if (!m_uiCamera)
		{
			m_uiCamera = Object.FindObjectOfType<UICamera>().GetComponent<Camera>();
		}
		int num = 0;
		float num2 = 0.9f;
		foreach (KeyValuePair<string, OnScreenText> item in TextDictionary)
		{
			OnScreenText value = item.Value;
			if (value.m_onScreenTime.HasValue)
			{
				UILabel label = value.m_label;
				if ((float)num < 5f)
				{
					value.m_onScreenTime -= Time.deltaTime;
					if (value.m_onScreenTime.Value <= 0f)
					{
						m_removeList.Add(item.Key);
						continue;
					}
					label.enabled = true;
					label.alpha = 1f;
					SetTextPosition(item.Key, 0.5f, num2, UIWidget.Pivot.Center);
					label.transform.localScale = new Vector3(25f, 25f, 1f);
					label.lineWidth = (int)((float)Screen.width * 0.85f);
					num2 -= (label.font.CalculatePrintedSize(label.processedText, encoding: true, UIFont.SymbolStyle.None).y * 25f + 5f) / (float)Screen.height;
					num++;
				}
				else
				{
					label.enabled = false;
				}
			}
			value.m_label.transform.localPosition = new Vector3(value.m_label.transform.localPosition.x, value.m_label.transform.localPosition.y, -16f);
		}
		foreach (string remove in m_removeList)
		{
			RemoveText(remove);
		}
		m_removeList.Clear();
	}

	private void Start()
	{
		if (m_Instance != null && m_Instance != this)
		{
			TextDictionary = m_Instance.TextDictionary;
			GameUtilities.DestroyImmediate(m_Instance.gameObject);
		}
		m_Instance = this;
	}

	private void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnApplicationQuit()
	{
		s_Quitting = true;
	}

	private Color GetDepartmentColor(Department department)
	{
		return department switch
		{
			Department.Programming => Color.magenta, 
			Department.Animation => Color.green, 
			Department.Design => Color.cyan, 
			Department.Art => Color.yellow, 
			Department.None => Color.red, 
			_ => Color.red, 
		};
	}

	public void LogOnceOnlyWarning(string text, Department concernedDepartment, float onScreenTime)
	{
	}

	public void LogOnScreenWarning(string text, Department concernedDepartment, float onScreenTime)
	{
		Debug.LogError("ERROR: " + text);
	}

	private OnScreenText CreateDebugLabel(string name)
	{
		if (DebugText == null)
		{
			UICamera uICamera = Object.FindObjectOfType<UICamera>();
			if ((bool)uICamera)
			{
				GameObject gameObject = NGUITools.AddChild(uICamera.gameObject);
				gameObject.AddComponent<UILabel>();
				DebugText = gameObject.GetComponent<UILabel>();
				UIFont[] array = Resources.FindObjectsOfTypeAll<UIFont>();
				UIFont font = array[0];
				UIFont[] array2 = array;
				foreach (UIFont uIFont in array2)
				{
					if (uIFont.name == "arial")
					{
						font = uIFont;
					}
				}
				DebugText.font = font;
				gameObject.AddComponent<UIAnchor>();
				UIAnchor component = gameObject.GetComponent<UIAnchor>();
				component.uiCamera = uICamera.GetComponent<Camera>();
				component.side = UIAnchor.Side.BottomLeft;
				component.relativeOffset = new Vector2(0.05f, 0.95f);
				DebugText.color = Color.white;
				DebugText.effectStyle = UILabel.Effect.Shadow;
				DebugText.gameObject.SetActive(value: false);
			}
		}
		if (DebugText == null)
		{
			return null;
		}
		UILabel component2 = Object.Instantiate(DebugText.gameObject).GetComponent<UILabel>();
		component2.transform.parent = DebugText.transform.parent;
		component2.gameObject.name = name;
		OnScreenText onScreenText = new OnScreenText(component2, null);
		TextDictionary.Add(name, onScreenText);
		return onScreenText;
	}

	public void SetText(string text)
	{
		SetText("DebugDefault", text);
	}

	public void SetText(string text, Color color)
	{
		SetText("DebugDefault", text, color);
	}

	public void SetText(string name, string text)
	{
		if (!TextDictionary.TryGetValue(name, out var value))
		{
			value = CreateDebugLabel(name);
		}
		value.m_label.text = text;
		value.m_label.MakePixelPerfect();
		value.m_label.gameObject.SetActive(value: true);
	}

	public void SetText(string name, string text, Color color)
	{
		if (!TextDictionary.TryGetValue(name, out var value))
		{
			value = CreateDebugLabel(name);
		}
		value.m_label.text = text;
		value.m_label.MakePixelPerfect();
		value.m_label.color = color;
		value.m_label.gameObject.SetActive(value: true);
	}

	public void SetText(string name, string text, float xScreenPercentage, float yScreenPercentage)
	{
		SetText(name, text);
		SetTextPosition(name, xScreenPercentage, yScreenPercentage);
	}

	public void SetText(string name, string text, Color color, float xScreenPercentage, float yScreenPercentage)
	{
		SetText(name, text, color);
		SetTextPosition(name, xScreenPercentage, yScreenPercentage);
	}

	public void SetTextPosition(string name, float xScreenPercentage, float yScreenPercentage)
	{
		if (TextDictionary.TryGetValue(name, out var value))
		{
			value.m_label.GetComponent<UIAnchor>().relativeOffset = new Vector2(xScreenPercentage, yScreenPercentage);
		}
	}

	public void SetTextPosition(string name, float xScreenPercentage, float yScreenPercentage, UIWidget.Pivot pivotDir)
	{
		if (TextDictionary.TryGetValue(name, out var value))
		{
			value.m_label.GetComponent<UIAnchor>().relativeOffset = new Vector2(xScreenPercentage, yScreenPercentage);
			value.m_label.pivot = pivotDir;
		}
	}

	public void RemoveText(string name)
	{
		if (TextDictionary.ContainsKey(name))
		{
			GameUtilities.Destroy(TextDictionary[name].m_label.gameObject);
			TextDictionary.Remove(name);
		}
	}

	public void RemoveAll()
	{
		foreach (KeyValuePair<string, OnScreenText> item in TextDictionary)
		{
			GameUtilities.Destroy(item.Value.m_label.gameObject);
		}
		TextDictionary.Clear();
	}
}
