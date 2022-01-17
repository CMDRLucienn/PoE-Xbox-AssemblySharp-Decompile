using UnityEngine;

public class RestInteraction : Usable
{
	public RestZone LinkedRestZone;

	public float UseRadius = 2f;

	public float ArrivalDistance;

	private bool m_MouseOver;

	public override float UsableRadius => UseRadius;

	public override float ArrivalRadius => ArrivalDistance;

	public override bool IsUsable
	{
		get
		{
			if (!GameState.InCombat)
			{
				return base.IsVisible;
			}
			return false;
		}
	}

	public override bool Use(GameObject user)
	{
		if ((bool)LinkedRestZone)
		{
			if (LinkedRestZone.CanRest && !GameState.InCombat)
			{
				RestZone.Rest(RestZone.Mode.Camp);
				FireUseAudio();
			}
			else if (!LinkedRestZone.CanRest)
			{
				Console.AddMessage(GUIUtils.GetTextWithLinks(393));
			}
			else if (GameState.InCombat)
			{
				Console.AddMessage(GUIUtils.GetTextWithLinks(394));
			}
		}
		return true;
	}

	private void Update()
	{
		if (m_MouseOver && GameInput.GetControlUp(MappedControl.INTERACT, handle: true))
		{
			GameState.s_playerCharacter.ObjectClicked(this);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnMouseOver()
	{
		if (FogOfWar.PointVisibleInFog(base.transform.position))
		{
			m_MouseOver = true;
			GameCursor.GenericUnderCursor = base.gameObject;
		}
	}

	private void OnMouseExit()
	{
		m_MouseOver = false;
		if (GameCursor.GenericUnderCursor == base.gameObject)
		{
			GameCursor.GenericUnderCursor = null;
		}
	}
}
