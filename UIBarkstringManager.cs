using System;
using System.Collections.Generic;
using System.Linq;
using OEIFormats.FlowCharts;
using OEIFormats.FlowCharts.Conversations;
using UnityEngine;

public class UIBarkstringManager : MonoBehaviour
{
	private List<UIBarkString> m_ActiveBarks = new List<UIBarkString>();

	public GameObject BarkStringPrefab;

	public GameObject FloatStringPrefab;

	public static UIBarkstringManager Instance { get; private set; }

	public IList<UIBarkString> GetActiveBarks()
	{
		return m_ActiveBarks;
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		BarkStringPrefab.SetActive(value: false);
		BarkStringPrefab.transform.localPosition = new Vector3(10000f, 10000f, 16f);
		FloatStringPrefab.SetActive(value: false);
		FloatStringPrefab.transform.localPosition = new Vector3(10000f, 10000f, 16f);
		ConversationManager instance = ConversationManager.Instance;
		instance.FlowChartPlayerAdded = (ConversationManager.FlowChartPlayerDelegate)Delegate.Combine(instance.FlowChartPlayerAdded, new ConversationManager.FlowChartPlayerDelegate(FlowChartPlayerAdded));
		ConversationManager instance2 = ConversationManager.Instance;
		instance2.FlowChartPlayerRemoved = (ConversationManager.FlowChartPlayerDelegate)Delegate.Combine(instance2.FlowChartPlayerRemoved, new ConversationManager.FlowChartPlayerDelegate(FlowChartPlayerRemoved));
		GameState.OnCombatStart += OnCombatStart;
	}

	private void OnDestroy()
	{
		if ((bool)ConversationManager.Instance)
		{
			ConversationManager instance = ConversationManager.Instance;
			instance.FlowChartPlayerAdded = (ConversationManager.FlowChartPlayerDelegate)Delegate.Remove(instance.FlowChartPlayerAdded, new ConversationManager.FlowChartPlayerDelegate(FlowChartPlayerAdded));
			ConversationManager instance2 = ConversationManager.Instance;
			instance2.FlowChartPlayerRemoved = (ConversationManager.FlowChartPlayerDelegate)Delegate.Remove(instance2.FlowChartPlayerRemoved, new ConversationManager.FlowChartPlayerDelegate(FlowChartPlayerRemoved));
		}
		m_ActiveBarks.Clear();
		GameState.OnCombatStart -= OnCombatStart;
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public UIBarkString AddManualBark(string text, Vector3 position, float lifetime)
	{
		UIBarkString uIBarkString = InstantiateNew(BarkStringPrefab);
		uIBarkString.ManualSetData(text, position, lifetime);
		m_ActiveBarks.Add(uIBarkString);
		return uIBarkString;
	}

	private UIBarkString InstantiateNew(GameObject prefab)
	{
		GameObject obj = UnityEngine.Object.Instantiate(prefab);
		obj.transform.parent = prefab.transform.parent;
		obj.transform.localScale = prefab.transform.localScale;
		obj.SetActive(value: true);
		return obj.GetComponent<UIBarkString>();
	}

	private void FlowChartPlayerAdded(FlowChartPlayer player)
	{
		if (player.CurrentNodeID != -1)
		{
			FlowChartNode flowChartNode = player.GetCurrentNode();
			if (flowChartNode is BankNode)
			{
				flowChartNode = player.CurrentFlowChart.GetNextNode(player);
				player.CurrentFlowChart.MoveToNode(flowChartNode.NodeID, player);
			}
			if ((!(flowChartNode is DialogueNode) || flowChartNode.NodeID == 0 || (flowChartNode as DialogueNode).DisplayType == DisplayType.Bark) && !player.Completed && !HasBarkForFlowChart(player))
			{
				GameObject obj = UnityEngine.Object.Instantiate(BarkStringPrefab);
				obj.transform.parent = BarkStringPrefab.transform.parent;
				obj.transform.localScale = BarkStringPrefab.transform.localScale;
				obj.SetActive(value: true);
				UIBarkString component = obj.GetComponent<UIBarkString>();
				component.SetData(player);
				m_ActiveBarks.Add(component);
			}
		}
	}

	private void FlowChartPlayerRemoved(FlowChartPlayer player)
	{
		for (int num = m_ActiveBarks.Count - 1; num >= 0; num--)
		{
			if (m_ActiveBarks[num].DataIs(player))
			{
				m_ActiveBarks[num].Kill();
			}
		}
	}

	public bool HasBarkForFlowChart(FlowChartPlayer player)
	{
		for (int num = m_ActiveBarks.Count - 1; num >= 0; num--)
		{
			if (m_ActiveBarks[num].DataIs(player))
			{
				return true;
			}
		}
		return false;
	}

	public void ReportKill(UIBarkString bs)
	{
		m_ActiveBarks.Remove(bs);
	}

	public bool IsBarking(GameObject speaker)
	{
		return m_ActiveBarks.Any((UIBarkString uibs) => uibs.GetSpeaker() == speaker);
	}

	private void OnCombatStart(object sender, EventArgs args)
	{
		IList<UIBarkString> activeBarks = Instance.GetActiveBarks();
		for (int num = activeBarks.Count - 1; num >= 0; num--)
		{
			if (!GameUtilities.GetBarkStringPersistsOnCombatStart(activeBarks[num].GetCurrentNode()))
			{
				activeBarks[num].Kill(instant: true, finishScripts: true);
			}
		}
	}
}
