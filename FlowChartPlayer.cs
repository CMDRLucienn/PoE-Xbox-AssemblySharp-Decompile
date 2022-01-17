using OEIFormats.FlowCharts;
using UnityEngine;

public class FlowChartPlayer
{
	public enum DisplayMode
	{
		Standard,
		Cutscene,
		Interaction
	}

	public int StartNodeID { get; set; }

	public int CurrentNodeID { get; set; }

	public float Timer { get; set; }

	public FlowChart CurrentFlowChart { get; set; }

	public GameObject OwnerObject { get; set; }

	public DisplayMode FlowChartDisplayMode { get; set; }

	public bool FadeFromBlackOnExit { get; set; }

	public bool Completed { get; private set; }

	public bool DisableVO { get; set; }

	public FlowChartPlayer(FlowChart flowChart, int startNodeID, GameObject ownerObject, DisplayMode displayMode)
	{
		CurrentFlowChart = flowChart;
		StartNodeID = startNodeID;
		OwnerObject = ownerObject;
		CurrentNodeID = -1;
		FlowChartDisplayMode = displayMode;
		FadeFromBlackOnExit = true;
		Completed = false;
	}

	public void SetComplete()
	{
		Completed = true;
	}

	public bool IsTimerOrVoFinished()
	{
		Conversation conversation = CurrentFlowChart as Conversation;
		if (conversation.GetVODuration(CurrentNodeID) > 0f)
		{
			return !conversation.IsVOPlaying(this);
		}
		return Timer <= 0f;
	}

	public FlowChartNode GetCurrentNode()
	{
		return CurrentFlowChart.GetNode(CurrentNodeID);
	}
}
