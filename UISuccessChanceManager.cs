using System;
using System.Collections.Generic;
using UnityEngine;

public class UISuccessChanceManager : MonoBehaviour
{
	[Serializable]
	public struct ColorPair
	{
		public Color Text;

		public Color Background;
	}

	private Stack<UISuccessChanceWidget> m_WidgetPool = new Stack<UISuccessChanceWidget>();

	private Dictionary<GameObject, UISuccessChanceWidget> m_ActiveWidgets = new Dictionary<GameObject, UISuccessChanceWidget>();

	public UISuccessChanceWidget RootObject;

	private List<GameObject> m_KillList = new List<GameObject>();

	[Tooltip("List of text/background colors. They are spread evenly across the 0-100% range.")]
	public ColorPair[] Colors;

	[Tooltip("List of text/background colors for colorblind mode. They are spread evenly across the 0-100% range.")]
	public ColorPair[] ColorsColorblind;

	public ColorPair UnknownColors;

	public float Alpha;

	public static UISuccessChanceManager Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		RootObject.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public ColorPair GetColorsForRatio(float rat)
	{
		int value = Mathf.FloorToInt(rat * (float)Colors.Length);
		value = Mathf.Clamp(value, 0, Colors.Length - 1);
		return (GameState.Mode.GetOption(GameOption.BoolOption.COLORBLIND_MODE) ? ColorsColorblind : Colors)[value];
	}

	public void HideDead()
	{
		Dictionary<GameObject, UISuccessChanceWidget>.Enumerator enumerator = m_ActiveWidgets.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (!enumerator.Current.Value.KeepAlive)
				{
					m_KillList.Add(enumerator.Current.Key);
				}
				enumerator.Current.Value.KeepAlive = false;
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		for (int i = 0; i < m_KillList.Count; i++)
		{
			Hide(m_KillList[i]);
		}
		m_KillList.Clear();
	}

	public void Show(CharacterStats attacker, GameObject target, AttackBase attack, GenericAbility ability)
	{
		if (GameState.Mode.GetOption(GameOption.BoolOption.DISPLAY_RELATIVE_DEFENSES) && (bool)target && (bool)attacker && (bool)attack && attack.IsHostile(target, attack.DamageData) && attack.DefendedBy != CharacterStats.DefenseType.None)
		{
			if (m_ActiveWidgets.TryGetValue(target, out var value))
			{
				value.DoKeepAlive(attacker, attack, ability);
				return;
			}
			value = ((m_WidgetPool.Count <= 0) ? NGUITools.AddChild(base.gameObject, RootObject.gameObject).GetComponent<UISuccessChanceWidget>() : m_WidgetPool.Pop());
			m_ActiveWidgets[target] = value;
			value.Set(attacker, target.GetComponent<CharacterStats>(), attack, ability);
		}
	}

	public void Hide(GameObject target)
	{
		if (m_ActiveWidgets.ContainsKey(target))
		{
			UISuccessChanceWidget uISuccessChanceWidget = m_ActiveWidgets[target];
			uISuccessChanceWidget.gameObject.SetActive(value: false);
			m_ActiveWidgets.Remove(target);
			m_WidgetPool.Push(uISuccessChanceWidget);
			uISuccessChanceWidget.gameObject.SetActive(value: false);
		}
	}
}
