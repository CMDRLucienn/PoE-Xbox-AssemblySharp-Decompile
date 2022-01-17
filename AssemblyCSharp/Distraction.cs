using UnityEngine;

namespace AssemblyCSharp;

public class Distraction : AttackAbility
{
	protected bool m_inStealth;

	protected override void Start()
	{
		RequiresHitObject = false;
		base.Start();
	}

	public override void OnImpact(GameObject self, Vector3 hitPosition)
	{
		GameEventArgs gameEventArgs = new GameEventArgs();
		gameEventArgs.Type = GameEventType.Ability;
		gameEventArgs.GameObjectData = new GameObject[1];
		gameEventArgs.GameObjectData[0] = base.gameObject;
		gameEventArgs.VectorData = new Vector3[1];
		gameEventArgs.VectorData[0] = hitPosition;
		object[] array = (gameEventArgs.GenericData = new string[1]);
		gameEventArgs.GenericData[0] = "Distraction";
		GameUtilities.BroadcastEvent(typeof(AIController), gameEventArgs);
	}

	public override void BeginTargeting()
	{
		m_inStealth = Stealth.IsInStealthMode(m_ownerStats.gameObject);
		Stealth.SetInStealthMode(m_ownerStats.gameObject, inStealth: true);
	}

	public override void TargetingStopped()
	{
		Stealth.SetInStealthMode(m_ownerStats.gameObject, m_inStealth);
	}
}
