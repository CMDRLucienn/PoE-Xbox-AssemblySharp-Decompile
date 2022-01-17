using UnityEngine;

public class UIGridStretch : MonoBehaviour
{
	public UIGrid Grid;

	public bool X = true;

	public bool Y = true;

	private void Update()
	{
		if ((bool)Grid && Grid.arrangement == UIGrid.Arrangement.Vertical)
		{
			base.transform.localScale = new Vector3(1f, Y ? ((float)Grid.ChildCount * Grid.cellHeight) : base.transform.localScale.y, 0f);
		}
	}
}
