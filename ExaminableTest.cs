using UnityEngine;

public class ExaminableTest : MonoBehaviour
{
	public ConversationObject ConversationFile = new ConversationObject();

	private FlowChartPlayer ActiveConversation;

	private void Start()
	{
		ActiveConversation = null;
	}

	private void Update()
	{
		if (ActiveConversation != null)
		{
			if (ConversationManager.Instance.IsConversationActive(ActiveConversation))
			{
				return;
			}
			ActiveConversation = null;
		}
		if (!GameInput.GetControlUp(MappedControl.INTERACT, handle: true))
		{
			return;
		}
		Ray ray = Camera.main.ScreenPointToRay(GameInput.MousePosition);
		Collider[] componentsInChildren = base.transform.GetComponentsInChildren<Collider>();
		if (componentsInChildren == null)
		{
			return;
		}
		Collider[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].bounds.IntersectRay(ray))
			{
				StartConversation();
				break;
			}
		}
	}

	private void StartConversation()
	{
		ActiveConversation = ConversationManager.Instance.StartConversation(ConversationFile.Filename, base.gameObject, FlowChartPlayer.DisplayMode.Standard);
	}
}
