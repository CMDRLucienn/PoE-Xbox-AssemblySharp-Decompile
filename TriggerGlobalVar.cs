using UnityEngine;

public class TriggerGlobalVar : TriggerLink
{
	[GlobalVariableString]
	public string GlobalVariable;

	public int Value;

	public override void OnTriggerEnter(Collider dude)
	{
		if (CanTrigger(dude.gameObject))
		{
			GlobalVariables.Instance.SetVariable(GlobalVariable, Value);
			m_triggeredCount++;
		}
	}
}
