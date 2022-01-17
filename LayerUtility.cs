using UnityEngine;

public static class LayerUtility
{
	public const string m_Background_Layer_Name = "Background";

	public const string m_Walkable_Name = "Walkable";

	public const string m_Wall_Name = "Wall";

	public const string m_Dynamics_Name = "Dynamics";

	public const string m_Dynamics_NoShadow_Name = "Dynamics No Shadow";

	public const string m_Dynamics_NoOcclusion_Name = "Dynamics No Occlusion";

	public const string m_Dynamics_NoOcclusion_NoShadow_Name = "Dynamics No Shadow No Occlusion";

	public const string m_Door_Layer_Name = "Doors";

	public const string m_Character_Name = "Character";

	public const string m_InGame_UI = "InGameUI";

	public const string m_Paperdoll = "Paperdoll";

	public static int InGameUILayer = FindLayerValue("InGameUI");

	public static int FindLayerValue(string layer_name)
	{
		int num = LayerMask.NameToLayer(layer_name);
		if (num == -1)
		{
			Debug.LogError("Cannot find layer: \"" + layer_name + "\". Please add missing layer and reimport!");
			num = 0;
		}
		return num;
	}

	public static int FindLayerMask(string layer_name)
	{
		return 1 << FindLayerValue(layer_name);
	}

	public static void SetAllLayers(GameObject obj, int layer)
	{
		obj.layer = layer;
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			SetAllLayers(obj.transform.GetChild(i).gameObject, layer);
		}
	}
}
