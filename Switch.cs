using UnityEngine;

public class Switch : Usable, iCanBeDetected
{
	public float InteractRadius = 2f;

	public float ArrivalDistance;

	public bool UsableInCombat = true;

	[Persistent]
	public bool Enabled = true;

	public override float UsableRadius => InteractRadius;

	public override float ArrivalRadius => ArrivalDistance;

	public override bool IsUsable
	{
		get
		{
			if (Enabled && base.IsVisible)
			{
				if (!UsableInCombat)
				{
					return !GameState.InCombat;
				}
				return true;
			}
			return false;
		}
	}

	protected override void Start()
	{
		base.Start();
		PE_Collider2D component = GetComponent<PE_Collider2D>();
		if ((bool)component)
		{
			component.enabled = Enabled;
		}
	}

	public void OnDetection()
	{
		Enabled = true;
		PE_Collider2D component = GetComponent<PE_Collider2D>();
		if ((bool)component)
		{
			component.enabled = true;
		}
	}

	public void Restored()
	{
		PE_Collider2D component = GetComponent<PE_Collider2D>();
		if ((bool)component)
		{
			component.enabled = Enabled;
		}
	}

	public override bool Use(GameObject user)
	{
		FireUseAudio();
		return IsUsable;
	}
}
