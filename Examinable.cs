using UnityEngine;

[AddComponentMenu("Toolbox/Examinable")]
public class Examinable : Usable
{
	private UIBarkString m_Display;

	public float UseRadius = 3f;

	public float ArrivalDistance;

	public DatabaseString ExamineText = new DatabaseString(DatabaseString.StringTableType.Interactables);

	public float DisplayTime = 5f;

	protected PE_Collider2D m_collider;

	public override float UsableRadius => UseRadius;

	public override float ArrivalRadius => ArrivalDistance;

	public override bool IsUsable => base.IsVisible;

	protected override void Start()
	{
		base.Start();
		m_collider = GetComponent<PE_Collider2D>();
	}

	public override bool Use(GameObject user)
	{
		if (m_Display == null)
		{
			FireUseAudio();
			BeginExamine();
			return true;
		}
		return false;
	}

	private void BeginExamine()
	{
		if (m_Display == null)
		{
			m_Display = UIBarkstringManager.Instance.AddManualBark("", m_collider.Center, DisplayTime);
			m_Display.OnDestroyed += OnEndExamine;
			m_Display.ManualSetData(ExamineText.GetText(), DisplayTime);
			m_Display.Show();
			m_collider.ManualHideIcon = true;
			Console.AddMessage(UIConversationManager.Instance.ConversationDescriptionColorString + ExamineText.GetText().Trim(), Console.ConsoleState.Dialogue);
			ScriptEvent component = GetComponent<ScriptEvent>();
			if ((bool)component)
			{
				component.ExecuteScript(ScriptEvent.ScriptEvents.OnExamineStart);
			}
		}
	}

	private void OnEndExamine(UIBarkString sender)
	{
		m_Display = null;
		if ((bool)m_collider)
		{
			m_collider.ManualHideIcon = false;
		}
		ScriptEvent component = GetComponent<ScriptEvent>();
		if ((bool)component)
		{
			component.ExecuteScript(ScriptEvent.ScriptEvents.OnExamineEnd);
		}
	}
}
