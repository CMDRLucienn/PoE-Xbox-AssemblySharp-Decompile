using UnityEngine;

public class Animate : Usable
{
	public GameObject target;

	private bool m_MouseOver;

	public override float UsableRadius => 2f;

	public override float ArrivalRadius => 0f;

	public override bool IsUsable => base.IsVisible;

	private void StartAnimation()
	{
		target.GetComponent<Animation>().Play();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (m_MouseOver && GameInput.GetControlUp(MappedControl.INTERACT, handle: true))
		{
			GameState.s_playerCharacter.ObjectClicked(this);
		}
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

	public override bool Use(GameObject user)
	{
		FireUseAudio();
		StartAnimation();
		return true;
	}
}
