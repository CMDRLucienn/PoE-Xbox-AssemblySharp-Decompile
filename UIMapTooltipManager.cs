using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMapTooltipManager : MonoBehaviour
{
	public static bool Locked;

	public GameObject TooltipPrefab;

	public UIMapTooltip BigTooltip;

	public int TooltipSpacing = 5;

	private Dictionary<GameObject, UIMapTooltip> m_ActiveTips = new Dictionary<GameObject, UIMapTooltip>();

	private List<UIMapTooltip> m_TipPool = new List<UIMapTooltip>();

	private List<GameObject> m_DeleteTips = new List<GameObject>();

	public Color FoeColor;

	public Color FriendColor;

	public Color NeutralColor;

	public Color BackerColor;

	private bool m_LevelUnloading = true;

	private List<UIScreenRectangleItem> m_ScreenRectBuffer = new List<UIScreenRectangleItem>();

	private LinkedList<Tuple<UIScreenRectangleItem, float>> m_LinkedBuffer = new LinkedList<Tuple<UIScreenRectangleItem, float>>();

	public static UIMapTooltipManager Instance { get; private set; }

	private float GetTooltipDelay
	{
		get
		{
			if (!InGameHUD.Instance.HighlightActive)
			{
				return UICamera.tooltipDelay;
			}
			return 0f;
		}
	}

	private void Awake()
	{
		Instance = this;
		GameState.OnLevelUnload += OnLevelUnload;
		GameState.OnLevelLoaded += OnLevelLoaded;
		BigTooltip.gameObject.SetActive(value: false);
		TooltipPrefab.SetActive(value: false);
	}

	private void Start()
	{
		if (TooltipPrefab.GetComponent<UIMapTooltip>() == null)
		{
			Debug.LogError("UIMapTooltipManager prefab isn't a UIMapTooltip.");
		}
		NewToPool();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameState.OnLevelUnload -= OnLevelUnload;
		GameState.OnLevelLoaded -= OnLevelLoaded;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		Dictionary<GameObject, UIMapTooltip>.Enumerator enumerator = m_ActiveTips.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (!(enumerator.Current.Value == null))
				{
					enumerator.Current.Value.RevealedByMouse = false;
					enumerator.Current.Value.RevealedByAttackCursor = false;
				}
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		if (InGameHUD.Instance != null && InGameHUD.Instance.ShowHUD)
		{
			if (Locked || InGameHUD.Instance.HighlightActive)
			{
				for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
				{
					Faction faction = Faction.ActiveFactionComponents[i];
					if (faction == null || (GameState.InCombat && faction.RelationshipToPlayer != 0) || !faction.CanShowTooltip)
					{
						continue;
					}
					PartyMemberAI component = faction.GetComponent<PartyMemberAI>();
					if (!component || !component.IsActiveInParty)
					{
						AIPackageController component2 = faction.GetComponent<AIPackageController>();
						if (component2 == null || component2.SummonType != AIController.AISummonType.Pet)
						{
							Show(faction.gameObject, byMouse: true, Locked);
						}
					}
				}
			}
			if (UIWindowManager.MouseInputAvailable && (bool)GameCursor.CharacterUnderCursor)
			{
				Faction component3 = GameCursor.CharacterUnderCursor.GetComponent<Faction>();
				Health component4 = GameCursor.CharacterUnderCursor.GetComponent<Health>();
				if ((bool)component3 && component3.CanShowTooltip && (bool)component4 && !component4.ShowDead)
				{
					if (GameCursor.ActiveCursorIsTargeting || (component3.RelationshipToPlayer != 0 && GameState.InCombat))
					{
						BigTooltip.gameObject.SetActive(value: true);
						BigTooltip.NotifyShown();
						BigTooltip.Set(component3.gameObject);
						BigTooltip.RevealedByAttackCursor = true;
						BigTooltip.RevealedByMouse = true;
					}
					else if (component3.MousedOverTime >= GetTooltipDelay)
					{
						BigTooltip.Hide();
						if ((bool)GameCursor.CharacterUnderCursor.GetComponent<PartyMemberAI>())
						{
							Show(component3.gameObject, byMouse: true, byAttack: false);
						}
						else if (GameState.InCombat && component3.RelationshipToPlayer == Faction.Relationship.Neutral)
						{
							Show(component3.gameObject, byMouse: true, byAttack: false);
						}
						else if (!GameState.InCombat)
						{
							Show(component3.gameObject, byMouse: true, byAttack: false);
						}
					}
				}
				else
				{
					BigTooltip.Hide();
				}
			}
			else if (!GameCursor.CharacterUnderCursor)
			{
				BigTooltip.Hide();
			}
		}
		if (BigTooltip.gameObject.activeSelf && BigTooltip.TargetIsDead)
		{
			BigTooltip.Hide();
		}
		m_DeleteTips.Clear();
		enumerator = m_ActiveTips.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<GameObject, UIMapTooltip> current = enumerator.Current;
				if (!(current.Value == null) && (!current.Value.RevealedByMouse || current.Value.TargetIsDead))
				{
					m_DeleteTips.Add(current.Key);
				}
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		for (int j = 0; j < m_DeleteTips.Count; j++)
		{
			Hide(m_DeleteTips[j]);
		}
		enumerator = m_ActiveTips.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				UIMapTooltip value = enumerator.Current.Value;
				if (!value || !value.gameObject.activeSelf || !value.RevealedByAttackCursor)
				{
					continue;
				}
				for (int k = 0; k < PartyMemberAI.SelectedPartyMembers.Length; k++)
				{
					if ((bool)PartyMemberAI.SelectedPartyMembers[k])
					{
						PartyMemberAI component5 = PartyMemberAI.SelectedPartyMembers[k].GetComponent<PartyMemberAI>();
						if ((bool)component5)
						{
							value.ProcessOpposer(component5);
						}
					}
				}
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		SpaceTooltips();
	}

	private void OnLevelUnload(object sender, EventArgs e)
	{
		m_LevelUnloading = true;
		ResetAll();
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		m_LevelUnloading = false;
		ResetAll();
	}

	public void ResetAll()
	{
		List<UIMapTooltip> list = new List<UIMapTooltip>();
		list.AddRange(m_ActiveTips.Values);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Hide();
		}
		BigTooltip.gameObject.SetActive(value: false);
	}

	private void NewToPool()
	{
		GameObject obj = UnityEngine.Object.Instantiate(TooltipPrefab);
		obj.transform.parent = TooltipPrefab.transform.parent;
		obj.transform.localScale = new Vector3(1f, 1f, 1f);
		obj.SetActive(value: false);
		UIMapTooltip component = obj.GetComponent<UIMapTooltip>();
		m_TipPool.Add(component);
	}

	public UIMapTooltip Show(GameObject target, bool byMouse, bool byAttack)
	{
		if (target == null || m_LevelUnloading)
		{
			return null;
		}
		if (UIBarkstringManager.Instance.IsBarking(target))
		{
			return null;
		}
		if (!m_ActiveTips.ContainsKey(target))
		{
			if (m_TipPool.Count <= 0)
			{
				NewToPool();
			}
			UIMapTooltip uIMapTooltip = m_TipPool[m_TipPool.Count - 1];
			m_TipPool.RemoveAt(m_TipPool.Count - 1);
			uIMapTooltip.gameObject.SetActive(value: true);
			uIMapTooltip.Set(target);
			uIMapTooltip.RevealedByMouse = byMouse;
			uIMapTooltip.RevealedByAttackCursor = byAttack;
			m_ActiveTips.Add(target, uIMapTooltip);
			uIMapTooltip.NotifyShown();
			return uIMapTooltip;
		}
		UIMapTooltip uIMapTooltip2 = m_ActiveTips[target];
		uIMapTooltip2.RevealedByMouse = byMouse || m_ActiveTips[target].RevealedByMouse;
		uIMapTooltip2.RevealedByAttackCursor = byAttack || m_ActiveTips[target].RevealedByAttackCursor;
		uIMapTooltip2.NotifyShown();
		return m_ActiveTips[target];
	}

	public void Hide(GameObject target)
	{
		if ((bool)target && m_ActiveTips.ContainsKey(target))
		{
			m_ActiveTips[target].Hide();
		}
	}

	public void ReturnToPool(GameObject target)
	{
		if ((bool)target && m_ActiveTips.ContainsKey(target))
		{
			UIMapTooltip uIMapTooltip = m_ActiveTips[target];
			if (m_ActiveTips.Remove(target))
			{
				m_TipPool.Add(uIMapTooltip);
			}
			uIMapTooltip.Reset();
			uIMapTooltip.gameObject.SetActive(value: false);
		}
	}

	public void ReturnToPool(UIMapTooltip affected)
	{
		if ((bool)affected && m_ActiveTips.ContainsValue(affected))
		{
			if (m_ActiveTips.Remove(affected.Target))
			{
				m_TipPool.Add(affected);
			}
			affected.Reset();
			affected.gameObject.SetActive(value: false);
		}
	}

	public void HideNonmouse(GameObject target)
	{
		if ((bool)target && m_ActiveTips.ContainsKey(target) && !m_ActiveTips[target].RevealedByMouse)
		{
			Hide(target);
		}
	}

	public void SpaceTooltips()
	{
		m_ScreenRectBuffer.Clear();
		Dictionary<GameObject, UIMapTooltip>.Enumerator enumerator = m_ActiveTips.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				UIMapTooltip value = enumerator.Current.Value;
				if ((bool)value && value.gameObject.activeInHierarchy)
				{
					m_ScreenRectBuffer.Add(value);
				}
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		IList<UIBarkString> activeBarks = UIBarkstringManager.Instance.GetActiveBarks();
		for (int i = 0; i < activeBarks.Count; i++)
		{
			if ((bool)activeBarks[i] && activeBarks[i].gameObject.activeInHierarchy)
			{
				m_ScreenRectBuffer.Add(activeBarks[i]);
			}
		}
		UIScreenRectangleItem[][] array = Clustering.DoCluster(m_ScreenRectBuffer, 350f);
		foreach (UIScreenRectangleItem[] array2 in array)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = float.MaxValue;
			foreach (UIScreenRectangleItem uIScreenRectangleItem in array2)
			{
				num += uIScreenRectangleItem.GetPosition().x;
				num2 += uIScreenRectangleItem.GetPosition().y;
				num3 = Mathf.Min(uIScreenRectangleItem.GetPosition().y, num3);
			}
			Vector2 vector = new Vector2(num / (float)array2.Length, num2 / (float)array2.Length - (num2 / (float)array2.Length - num3) / 2f);
			m_LinkedBuffer.Clear();
			foreach (UIScreenRectangleItem obj in array2)
			{
				float sqrMagnitude = (obj.GetPosition() - vector).sqrMagnitude;
				LinkedListNode<Tuple<UIScreenRectangleItem, float>> linkedListNode = new LinkedListNode<Tuple<UIScreenRectangleItem, float>>(new Tuple<UIScreenRectangleItem, float>(obj, sqrMagnitude));
				LinkedListNode<Tuple<UIScreenRectangleItem, float>> linkedListNode2 = m_LinkedBuffer.First;
				bool flag = false;
				while (linkedListNode2 != null && !flag)
				{
					if (sqrMagnitude <= linkedListNode2.Value.Second)
					{
						m_LinkedBuffer.AddBefore(linkedListNode2, linkedListNode);
						flag = true;
					}
					linkedListNode2 = linkedListNode2.Next;
				}
				if (!flag)
				{
					m_LinkedBuffer.AddLast(linkedListNode);
				}
			}
			for (LinkedListNode<Tuple<UIScreenRectangleItem, float>> linkedListNode3 = m_LinkedBuffer.First; linkedListNode3 != null; linkedListNode3 = linkedListNode3.Next)
			{
				linkedListNode3.Value.First.CorrectingOffset = Vector2.zero;
				for (LinkedListNode<Tuple<UIScreenRectangleItem, float>> linkedListNode4 = m_LinkedBuffer.First; linkedListNode4 != linkedListNode3; linkedListNode4 = linkedListNode4.Next)
				{
					Rect screenBounds = linkedListNode4.Value.First.GetScreenBounds();
					if (linkedListNode3.Value.First.GetScreenBounds().Intersects(screenBounds.Pad(TooltipSpacing), out var i2))
					{
						if (i2.width + linkedListNode3.Value.First.CorrectingOffset.x > 2.5f * (i2.height + linkedListNode3.Value.First.CorrectingOffset.y))
						{
							linkedListNode3.Value.First.CorrectingOffset.y += i2.height;
						}
						else if (linkedListNode3.Value.First.BasePosition.x < vector.x)
						{
							linkedListNode3.Value.First.CorrectingOffset.x -= i2.width;
						}
						else
						{
							linkedListNode3.Value.First.CorrectingOffset.x += i2.width;
						}
					}
				}
			}
		}
	}
}
