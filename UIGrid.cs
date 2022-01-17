using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Grid")]
public class UIGrid : MonoBehaviour
{
	public enum Arrangement
	{
		Horizontal,
		Vertical
	}

	public enum Alignment
	{
		Positive,
		Center,
		Negative
	}

	public Arrangement arrangement;

	public int maxPerLine;

	public float cellWidth = 200f;

	public float cellHeight = 200f;

	public bool repositionNow;

	public bool sorted;

	public bool hideInactive = true;

	public Alignment horizontalAlignment = Alignment.Negative;

	public Alignment verticalAlignment = Alignment.Negative;

	private int childCount;

	private bool mStarted;

	public int ChildCount => childCount;

	private void Start()
	{
		mStarted = true;
		Reposition();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (repositionNow)
		{
			repositionNow = false;
			Reposition();
		}
	}

	public static int SortByName(Transform a, Transform b)
	{
		return string.Compare(a.name, b.name);
	}

	public void Reposition()
	{
		if (!mStarted)
		{
			repositionNow = true;
			return;
		}
		Transform transform = base.transform;
		int num = 0;
		int num2 = 0;
		childCount = 0;
		if (hideInactive)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				if (base.transform.GetChild(i).gameObject.activeSelf)
				{
					childCount++;
				}
			}
		}
		else
		{
			childCount = base.transform.childCount;
		}
		if (sorted)
		{
			List<Transform> list = new List<Transform>();
			for (int j = 0; j < transform.childCount; j++)
			{
				Transform child = transform.GetChild(j);
				if ((bool)child && (!hideInactive || NGUITools.GetActive(child.gameObject)))
				{
					list.Add(child);
				}
			}
			list.Sort(SortByName);
			float num3 = 0f;
			int num4 = ((maxPerLine != 0) ? Mathf.Min(maxPerLine, childCount) : childCount);
			if (horizontalAlignment == Alignment.Center)
			{
				num3 = cellWidth * (float)(num4 - 1) / 2f;
			}
			else if (horizontalAlignment == Alignment.Positive)
			{
				num3 = (float)(num4 - 1) * cellWidth;
			}
			float num5 = 0f;
			int num6 = ((maxPerLine != 0) ? Mathf.CeilToInt((float)childCount / (1f * (float)maxPerLine)) : childCount);
			if (verticalAlignment == Alignment.Center)
			{
				num5 = (float)(-(num6 - 1)) * cellHeight / 2f;
			}
			else if (verticalAlignment == Alignment.Positive)
			{
				num5 = (float)(-(num6 - 1)) * cellHeight;
			}
			int k = 0;
			for (int count = list.Count; k < count; k++)
			{
				Transform transform2 = list[k];
				if (NGUITools.GetActive(transform2.gameObject) || !hideInactive)
				{
					float z = transform2.localPosition.z;
					transform2.localPosition = ((arrangement == Arrangement.Horizontal) ? new Vector3(cellWidth * (float)num - num3, (0f - cellHeight) * (float)num2, z) : new Vector3(cellWidth * (float)num2, (0f - cellHeight) * (float)num - num5, z));
					if (++num >= maxPerLine && maxPerLine > 0)
					{
						num = 0;
						num2++;
					}
				}
			}
		}
		else
		{
			float num7 = 0f;
			int num8 = ((maxPerLine != 0) ? Mathf.Min(maxPerLine, childCount) : childCount);
			if (horizontalAlignment == Alignment.Center)
			{
				num7 = (float)(num8 - 1) * cellWidth / 2f;
			}
			else if (horizontalAlignment == Alignment.Positive)
			{
				num7 = (float)(num8 - 1) * cellWidth;
			}
			float num9 = 0f;
			int num10 = ((maxPerLine != 0) ? Mathf.CeilToInt((float)childCount / (1f * (float)maxPerLine)) : childCount);
			if (verticalAlignment == Alignment.Center)
			{
				num9 = (float)(num10 - 1) * cellHeight / 2f;
			}
			else if (verticalAlignment == Alignment.Positive)
			{
				num9 = (float)(num10 - 1) * cellHeight;
			}
			for (int l = 0; l < transform.childCount; l++)
			{
				Transform child2 = transform.GetChild(l);
				if (child2.gameObject.activeSelf || !hideInactive)
				{
					float z2 = child2.localPosition.z;
					child2.localPosition = ((arrangement == Arrangement.Horizontal) ? new Vector3(cellWidth * (float)num - num7, (0f - cellHeight) * (float)num2 + num9, z2) : new Vector3(cellWidth * (float)num2, (0f - cellHeight) * (float)num, z2));
					if (++num >= maxPerLine && maxPerLine > 0)
					{
						num = 0;
						num2++;
					}
				}
			}
		}
		UIDraggablePanel uIDraggablePanel = NGUITools.FindInParents<UIDraggablePanel>(base.gameObject);
		if (uIDraggablePanel != null)
		{
			uIDraggablePanel.UpdateScrollbars(recalculateBounds: true);
		}
	}
}
