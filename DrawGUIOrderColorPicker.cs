using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawGUIOrderColorPicker : MonoBehaviour
{
	public bool loadFromScene = true;

	public ColorPicker[] colorPicker;

	private List<ColorPicker> mColorPickerList;

	private void Start()
	{
		if (loadFromScene)
		{
			colorPicker = Object.FindObjectsOfType<ColorPicker>();
		}
		mColorPickerList = new List<ColorPicker>();
		mColorPickerList.AddRange(colorPicker);
		mColorPickerList = mColorPickerList.OrderBy((ColorPicker item) => item.drawOrder).ToList();
		mColorPickerList.Reverse();
		mColorPickerList.CopyTo(colorPicker);
		foreach (ColorPicker mColorPicker in mColorPickerList)
		{
			mColorPicker.useExternalDrawer = true;
		}
	}

	private void OnGUI()
	{
		foreach (ColorPicker mColorPicker in mColorPickerList)
		{
			if (!mColorPicker.IsDeployed())
			{
				mColorPicker._DrawGUI();
			}
		}
	}
}
