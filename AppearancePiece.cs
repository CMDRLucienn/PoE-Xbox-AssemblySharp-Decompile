using System;
using UnityEngine;

[Serializable]
public class AppearancePiece
{
	public enum BodyPiece
	{
		None,
		Body,
		Boots,
		Cape,
		Gloves,
		Helm,
		Hair,
		Head,
		Facialhair
	}

	public enum ArmorType
	{
		None,
		Cloth,
		Padded,
		Hide,
		Leather,
		Scale,
		Mail,
		Brigandine,
		Plate,
		BreastPlate
	}

	public static string CHARACTER_PATH = "Art/Character/";

	public static char PARTIAL_MESH = 'P';

	public static char SHOW_MESH = 'S';

	public static char HIDE_MESH = 'H';

	public static char EXTRA_MESH = 'E';

	public BodyPiece bodyPiece;

	public ArmorType armorType;

	public int modelVariation = 1;

	public int materialVariation = 1;

	public string specialOverride;

	[NonSerialized]
	public bool hideBoots;

	[NonSerialized]
	public bool hideGloves;

	[NonSerialized]
	public bool hideHead;

	[NonSerialized]
	public bool hideHair;

	[NonSerialized]
	public bool partialHair;

	[NonSerialized]
	public bool hideFacialHair;

	[NonSerialized]
	public bool partialFacialHair;

	public string GetArmorTypeShortString()
	{
		return (new string[10] { "ND", "CL", "PA", "HD", "LE", "SC", "MA", "BR", "PL", "BP" })[(int)armorType];
	}

	public void ApplySuffixToAppearance(string suffix)
	{
		int length = suffix.Length;
		if (length < 2 || length > 3)
		{
			return;
		}
		char c = char.ToUpper(suffix[1]);
		char c2 = ((length == 3) ? char.ToUpper(suffix[2]) : '\0');
		if (bodyPiece == BodyPiece.Body)
		{
			if (c == HIDE_MESH)
			{
				hideGloves = true;
			}
			if (c2 == HIDE_MESH)
			{
				hideBoots = true;
			}
		}
		else if (bodyPiece == BodyPiece.Helm)
		{
			if (c == HIDE_MESH)
			{
				hideHair = true;
			}
			else if (c == PARTIAL_MESH)
			{
				partialHair = true;
			}
			if (c2 == HIDE_MESH)
			{
				hideFacialHair = true;
			}
			else if (c2 == PARTIAL_MESH)
			{
				partialFacialHair = true;
			}
		}
	}

	public string GetModelMeshName(NPCAppearance appearance, bool bodyOverride, bool partialOverride)
	{
		if (!string.IsNullOrEmpty(specialOverride))
		{
			return $"{specialOverride}_{bodyPiece}";
		}
		if (bodyPiece == BodyPiece.Body || bodyOverride)
		{
			return $"{appearance.GetGenderShortString()}_{appearance.GetBodyString()}_{GetArmorTypeShortString()}{modelVariation:D2}_{bodyPiece}";
		}
		if (partialOverride)
		{
			return $"{appearance.GetGenderShortString()}_{appearance.GetBodyString()}_{bodyPiece}{modelVariation:D2}_P";
		}
		if ((bodyPiece == BodyPiece.Head || bodyPiece == BodyPiece.Hair) && appearance.race == NPCAppearance.Race.GOD)
		{
			return string.Format("{0}_{1}_{4}_{2}{3:D2}", appearance.GetGenderShortString(), appearance.GetBodyString(), bodyPiece, modelVariation, appearance.GetSubRaceString());
		}
		if (bodyPiece == BodyPiece.Head && appearance.subrace == NPCAppearance.Subrace.Wild_Orlan)
		{
			return string.Format("{0}_{1}_{2}{3:D2}", appearance.GetGenderShortString(), appearance.GetHeadSubRaceFolder(), "Head", modelVariation);
		}
		return $"{appearance.GetGenderShortString()}_{appearance.GetBodyString()}_{bodyPiece}{modelVariation:D2}";
	}

	public string GetModelFileName(NPCAppearance appearance, bool bodyOverride)
	{
		if (!string.IsNullOrEmpty(specialOverride))
		{
			return $"{specialOverride}_{bodyPiece}";
		}
		if (bodyPiece == BodyPiece.Body || bodyOverride)
		{
			return $"{appearance.GetGenderShortString()}_{appearance.GetBodyString()}_{GetArmorTypeShortString()}{modelVariation:D2}";
		}
		if ((bodyPiece == BodyPiece.Head || bodyPiece == BodyPiece.Hair) && appearance.race == NPCAppearance.Race.GOD)
		{
			return string.Format("{0}_{1}_{4}_{2}{3:D2}", appearance.GetGenderShortString(), appearance.GetBodyString(), "Head", modelVariation, appearance.GetSubRaceString());
		}
		if (bodyPiece == BodyPiece.Head && appearance.subrace == NPCAppearance.Subrace.Wild_Orlan)
		{
			return string.Format("{0}_{1}_{2}{3:D2}", appearance.GetGenderShortString(), appearance.GetHeadSubRaceFolder(), "Head", modelVariation);
		}
		return $"{appearance.GetGenderShortString()}_{appearance.GetBodyString()}_{bodyPiece}{modelVariation:D2}";
	}

	public string GetModelPath(NPCAppearance appearance, bool bodyOverride)
	{
		if (!string.IsNullOrEmpty(specialOverride))
		{
			return $"{CHARACTER_PATH}SpecialNPC/{specialOverride}/";
		}
		if (bodyPiece == BodyPiece.Body || bodyOverride)
		{
			return $"{CHARACTER_PATH}{appearance.GetGenderFullString()}/{appearance.GetBodyString()}/Body/{GetArmorTypeShortString()}/";
		}
		if (bodyPiece == BodyPiece.Hair && appearance.race == NPCAppearance.Race.GOD)
		{
			return string.Format("{0}{1}/{2}/{3}/{4}/", CHARACTER_PATH, appearance.GetGenderFullString(), appearance.GetBodyString(), "Head", appearance.GetSubRaceFolder());
		}
		if (bodyPiece == BodyPiece.Head)
		{
			return string.Format("{0}{1}/{2}/{3}/{4}/", CHARACTER_PATH, appearance.GetGenderFullString(), appearance.GetBodyString(), "Head", appearance.GetHeadSubRaceFolder());
		}
		return $"{CHARACTER_PATH}{appearance.GetGenderFullString()}/{appearance.GetBodyString()}/{bodyPiece}/";
	}

	public string GetModelFullPath(NPCAppearance appearance, bool bodyOverride)
	{
		return $"{GetModelPath(appearance, bodyOverride)}{GetModelFileName(appearance, bodyOverride)}";
	}

	public string GetMaterialFileName(NPCAppearance appearance, bool bodyOverride)
	{
		if (!string.IsNullOrEmpty(specialOverride))
		{
			return $"m_{specialOverride}_{bodyPiece}";
		}
		if (bodyPiece == BodyPiece.Body || bodyOverride)
		{
			return $"m_{appearance.GetGenderShortString()}_{GetArmorTypeShortString()}{modelVariation:D2}_{bodyPiece}_V{materialVariation:D2}";
		}
		if (bodyPiece == BodyPiece.Head)
		{
			return $"m_{appearance.GetGenderShortString()}_{appearance.GetRaceString()}_{bodyPiece}{modelVariation:D2}_V{materialVariation:D2}";
		}
		return $"m_{bodyPiece}{modelVariation:D2}_V{materialVariation:D2}";
	}

	public string GetSubMeshMaterialFileName(NPCAppearance appearance, bool bodyOverride, Material m)
	{
		string text = (m ? m.name.Replace("(Instance)", "") : "");
		if (text.Length > 2)
		{
			return $"{GetMaterialPath(appearance, bodyOverride)}{text.Substring(0, text.Length - 3)}{materialVariation:D2}";
		}
		return "";
	}

	public string GetMaterialPath(NPCAppearance appearance, bool bodyOverride)
	{
		if (!string.IsNullOrEmpty(specialOverride))
		{
			return $"{CHARACTER_PATH}SpecialNPC/{specialOverride}/";
		}
		if (bodyPiece == BodyPiece.Body || bodyOverride)
		{
			return string.Format("{0}{1}/Textures/Body/{2}/{2}{3:D2}/", CHARACTER_PATH, appearance.GetGenderFullString(), GetArmorTypeShortString(), modelVariation);
		}
		if (bodyPiece == BodyPiece.Head)
		{
			return string.Format("{0}{1}/{2}/Textures/{3}/{3}{4:D2}/", CHARACTER_PATH, appearance.GetGenderFullString(), appearance.GetRaceString(), bodyPiece, modelVariation);
		}
		return string.Format("{0}Textures/{1}/{1}{2:D2}/", CHARACTER_PATH, bodyPiece, modelVariation);
	}

	public string GetMaterialFullPath(NPCAppearance appearance, bool bodyOverride)
	{
		return $"{GetMaterialPath(appearance, bodyOverride)}{GetMaterialFileName(appearance, bodyOverride)}";
	}
}
