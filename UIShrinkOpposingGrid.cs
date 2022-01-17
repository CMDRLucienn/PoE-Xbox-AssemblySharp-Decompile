using UnityEngine;

[ExecuteInEditMode]
public class UIShrinkOpposingGrid : MonoBehaviour
{
	public bool ResizeX;

	public bool ResizeY;

	public int BaseSize;

	public UIGrid Grid;

	private void Update()
	{
		base.transform.localScale = new Vector3(ResizeX ? ((float)BaseSize - (float)Grid.ChildCount * Grid.cellWidth) : base.transform.localScale.x, ResizeY ? ((float)BaseSize - (float)Grid.ChildCount * Grid.cellHeight) : base.transform.localScale.y, base.transform.localScale.z);
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}
}
