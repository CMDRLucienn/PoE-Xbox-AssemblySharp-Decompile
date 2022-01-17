using UnityEngine;

public class UIGrimoireLevelArrow : MonoBehaviour
{
	public int Direction = 1;

	private void OnClick()
	{
		if (Direction < 0)
		{
			UIGrimoireManager.Instance.LevelButtons.DecLevel();
		}
		else if (Direction > 0)
		{
			UIGrimoireManager.Instance.LevelButtons.IncLevel();
		}
	}
}
