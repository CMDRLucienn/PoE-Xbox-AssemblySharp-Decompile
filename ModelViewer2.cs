using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ModelViewer2 : MonoBehaviour
{
	private GameObject character;

	private NPCAppearance appearance;

	private Equipment equipment;

	private ComboBox RaceComboBox;

	private ComboBox RacialBodyTypeComboBox;

	private ComboBox GenderComboBox;

	private ComboBox HeadComboBox;

	private ComboBox HeadMatComboBox;

	private ComboBox HairComboBox;

	private ComboBox HairMatComboBox;

	private ComboBox FacialHairComboBox;

	private ComboBox FacialHairMatComboBox;

	private ComboBox ArmorTypeComboBox;

	private ComboBox ArmorComboBox;

	private ComboBox ArmorMatComboBox;

	private ComboBox HelmComboBox;

	private ComboBox HelmMatComboBox;

	private ComboBox GlovesComboBox;

	private ComboBox GlovesMatComboBox;

	private ComboBox BootsComboBox;

	private ComboBox BootsMatComboBox;

	private GUIStyle listStyle = new GUIStyle();

	private ColorPicker SkinColorPicker;

	private ColorPicker HairColorPicker;

	private ColorPicker PrimaryColorPicker;

	private ColorPicker SecondaryColorPicker;

	private bool isDirty;

	private bool equipDefaultItems = true;

	private string characterDir = string.Empty;

	private CameraControl m_cameraControl;

	private Camera m_staticCamera;

	private bool m_useGameCamera;

	private string m_saveFilename;

	private void Start()
	{
	}

	private void EquipItem(Equippable.EquipmentSlot slot, AppearancePiece.BodyPiece bodyType, AppearancePiece.ArmorType armorType, int modelVariation, int materialVariation)
	{
		UnequipSlot(slot);
		Equippable equippable = new GameObject(slot.ToString()).AddComponent<Equippable>();
		equippable.Appearance = new AppearancePiece();
		equippable.Appearance.armorType = armorType;
		equippable.Appearance.bodyPiece = bodyType;
		equippable.Appearance.modelVariation = modelVariation;
		equippable.Appearance.materialVariation = materialVariation;
		Equippable equippable2 = equipment.Equip(equippable, slot, enforceRecoveryPenalty: false);
		if ((bool)equippable2)
		{
			GameUtilities.Destroy(equippable2);
		}
	}

	private void UnequipSlot(Equippable.EquipmentSlot slot)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(slot);
		if (itemInSlot != null)
		{
			itemInSlot = equipment.UnEquip(itemInSlot, slot);
			GameUtilities.Destroy(itemInSlot.gameObject);
		}
	}

	private void CopyToDefaultEquipment()
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		Equippable itemInSlot2 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Head);
		Equippable itemInSlot3 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Hands);
		Equippable itemInSlot4 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Feet);
		equipment.DefaultEquippedItems.Slots[2].Val = itemInSlot;
		equipment.DefaultEquippedItems.Slots[0].Val = itemInSlot2;
		equipment.DefaultEquippedItems.Slots[5].Val = itemInSlot3;
		equipment.DefaultEquippedItems.Slots[7].Val = itemInSlot4;
	}

	private void Update()
	{
	}

	private void SetRace(NPCAppearance.Race race)
	{
		if (race != appearance.race)
		{
			appearance.race = race;
			appearance.racialBodyType = ((appearance.race != NPCAppearance.Race.GOD) ? appearance.race : NPCAppearance.Race.HUM);
			appearance.avatar = null;
			appearance.gameObject.GetComponent<Animator>().avatar = null;
			isDirty = true;
		}
	}

	private void SetRacialBodyType(NPCAppearance.Race race)
	{
		if (race != appearance.racialBodyType)
		{
			appearance.racialBodyType = race;
			appearance.avatar = null;
			appearance.gameObject.GetComponent<Animator>().avatar = null;
			isDirty = true;
		}
	}

	private void SetGender(NPCAppearance.Sex gender)
	{
		if (gender != appearance.gender)
		{
			appearance.gender = gender;
			appearance.avatar = null;
			appearance.gameObject.GetComponent<Animator>().avatar = null;
			isDirty = true;
		}
	}

	private void SetHead(string filename)
	{
		int num = ExtractIndex(filename);
		if (num != appearance.headAppearance.modelVariation)
		{
			appearance.headAppearance.modelVariation = num;
			appearance.headAppearance.materialVariation = 1;
		}
		isDirty = true;
	}

	private void SetHeadMaterial(int variant)
	{
		appearance.headAppearance.materialVariation = variant;
		isDirty = true;
	}

	private void SetHair(string filename)
	{
		int num = ExtractIndex(filename);
		if (num != appearance.hairAppearance.modelVariation)
		{
			appearance.hairAppearance.modelVariation = num;
			appearance.hairAppearance.materialVariation = 1;
		}
		appearance.hasHair = true;
		isDirty = true;
	}

	private void SetHairMaterial(int variant)
	{
		appearance.hairAppearance.materialVariation = variant;
		isDirty = true;
	}

	private void SetFacialHair(string filename)
	{
		int num = ExtractIndex(filename);
		if (num != appearance.facialHairAppearance.modelVariation)
		{
			appearance.facialHairAppearance.modelVariation = num;
			appearance.facialHairAppearance.materialVariation = 1;
		}
		appearance.hasFacialHair = true;
		isDirty = true;
	}

	private void SetFacialHairMaterial(int variant)
	{
		appearance.facialHairAppearance.materialVariation = variant;
		isDirty = true;
	}

	private void SetArmorType(AppearancePiece.ArmorType armorType)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if (itemInSlot != null && armorType != itemInSlot.Appearance.armorType)
		{
			itemInSlot.Appearance.armorType = armorType;
			itemInSlot.Appearance.modelVariation = 1;
			itemInSlot.Appearance.materialVariation = 1;
			isDirty = true;
		}
	}

	private void SetArmor(string filename)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if ((bool)itemInSlot)
		{
			int num = ExtractIndex(filename);
			if (itemInSlot.Appearance.modelVariation != num)
			{
				itemInSlot.Appearance.modelVariation = num;
				itemInSlot.Appearance.materialVariation = 1;
			}
		}
		isDirty = true;
	}

	private void SetArmorMaterial(int variant)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if ((bool)itemInSlot)
		{
			itemInSlot.Appearance.materialVariation = variant;
		}
		isDirty = true;
	}

	private void SetHelm(string filename)
	{
		int modelVariation = ExtractIndex(filename);
		EquipItem(Equippable.EquipmentSlot.Head, AppearancePiece.BodyPiece.Helm, AppearancePiece.ArmorType.None, modelVariation, 1);
		isDirty = true;
	}

	private void SetHelmMaterial(int variant)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Head);
		if ((bool)itemInSlot)
		{
			itemInSlot.Appearance.materialVariation = variant;
		}
		isDirty = true;
	}

	private void SetGloves(string filename)
	{
		int modelVariation = ExtractIndex(filename);
		EquipItem(Equippable.EquipmentSlot.Hands, AppearancePiece.BodyPiece.Gloves, AppearancePiece.ArmorType.None, modelVariation, 1);
		isDirty = true;
	}

	private void SetGlovesMaterial(int variant)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Hands);
		if ((bool)itemInSlot)
		{
			itemInSlot.Appearance.materialVariation = variant;
		}
		isDirty = true;
	}

	private void SetBoots(string filename)
	{
		int modelVariation = ExtractIndex(filename);
		EquipItem(Equippable.EquipmentSlot.Feet, AppearancePiece.BodyPiece.Boots, AppearancePiece.ArmorType.None, modelVariation, 1);
		isDirty = true;
	}

	private void SetBootsMaterial(int variant)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Feet);
		if ((bool)itemInSlot)
		{
			itemInSlot.Appearance.materialVariation = variant;
		}
		isDirty = true;
	}

	private int GetFileIndex(string[] filenames, int index)
	{
		string value = index.ToString("D2") + ".fbx";
		for (int i = 0; i < filenames.Length; i++)
		{
			if (filenames[i].EndsWith(value))
			{
				return i;
			}
		}
		return -1;
	}

	private int GetFileIndex(int[] indices, int index)
	{
		for (int i = 0; i < indices.Length; i++)
		{
			if (indices[i] == index)
			{
				return i;
			}
		}
		return -1;
	}

	private int ExtractIndex(string filename)
	{
		string s = filename.Substring(filename.Length - 6, 2);
		int result = 0;
		if (int.TryParse(s, out result))
		{
			return result;
		}
		return 0;
	}

	private string GetGenderPrefix(NPCAppearance.Sex gender)
	{
		if (appearance.gender == NPCAppearance.Sex.Male)
		{
			return "M_";
		}
		if (appearance.gender == NPCAppearance.Sex.Female)
		{
			return "F_";
		}
		return string.Empty;
	}

	private int[] GetMaterials(string prefix, int index)
	{
		List<int> sharedMaterials = GetSharedMaterials(prefix, index);
		List<int> genderMaterials = GetGenderMaterials(prefix, index);
		List<int> localMaterials = GetLocalMaterials(prefix, index);
		foreach (int item in genderMaterials)
		{
			if (!sharedMaterials.Contains(item))
			{
				sharedMaterials.Add(item);
			}
		}
		foreach (int item2 in localMaterials)
		{
			if (!sharedMaterials.Contains(item2))
			{
				sharedMaterials.Add(item2);
			}
		}
		sharedMaterials.Sort();
		return sharedMaterials.ToArray();
	}

	private int[] GetEquipmentMaterials(string prefix, Equippable equip, Equippable.EquipmentSlot slot, int index)
	{
		List<int> sharedMaterials = GetSharedMaterials(prefix, index);
		List<int> sharedEquipmentMaterials = GetSharedEquipmentMaterials(equip, slot, index);
		List<int> genderEquipmentMaterials = GetGenderEquipmentMaterials(equip, slot, index);
		List<int> localEquipmentMaterials = GetLocalEquipmentMaterials(equip, slot, index);
		foreach (int item in sharedEquipmentMaterials)
		{
			if (!sharedMaterials.Contains(item))
			{
				sharedMaterials.Add(item);
			}
		}
		foreach (int item2 in genderEquipmentMaterials)
		{
			if (!sharedMaterials.Contains(item2))
			{
				sharedMaterials.Add(item2);
			}
		}
		foreach (int item3 in localEquipmentMaterials)
		{
			if (!sharedMaterials.Contains(item3))
			{
				sharedMaterials.Add(item3);
			}
		}
		sharedMaterials.Sort();
		return sharedMaterials.ToArray();
	}

	private List<int> GetSharedMaterials(string prefix, int index)
	{
		List<int> list = new List<int>();
		string text = $"{prefix}{index:D2}";
		string path = Path.Combine(characterDir, Path.Combine(Path.Combine("Textures", prefix), text));
		if (Directory.Exists(path))
		{
			string[] files = Directory.GetFiles(path, "m_" + text + "*.mat");
			foreach (string filename in files)
			{
				int num = ExtractIndex(filename);
				if (num > 0)
				{
					list.Add(num);
				}
			}
		}
		return list;
	}

	private List<int> GetSharedEquipmentMaterials(Equippable equip, Equippable.EquipmentSlot slot, int index)
	{
		List<int> list = new List<int>();
		string armorTypeShortString = equip.Appearance.GetArmorTypeShortString();
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if ((bool)itemInSlot)
		{
			armorTypeShortString = itemInSlot.Appearance.GetArmorTypeShortString();
			index = itemInSlot.Appearance.modelVariation;
		}
		string text = $"{armorTypeShortString}{index:D2}";
		string text2 = string.Empty;
		switch (slot)
		{
		case Equippable.EquipmentSlot.Armor:
			text2 = "Body";
			break;
		case Equippable.EquipmentSlot.Feet:
			text2 = "Boots";
			break;
		case Equippable.EquipmentSlot.Hands:
			text2 = "Gloves";
			break;
		}
		string path = Path.Combine(characterDir, Path.Combine("Textures", text));
		if (Directory.Exists(path))
		{
			string[] files = Directory.GetFiles(path, "m_" + text + text2 + "_*.mat");
			foreach (string filename in files)
			{
				int num = ExtractIndex(filename);
				if (num > 0)
				{
					list.Add(num);
				}
			}
		}
		return list;
	}

	private List<int> GetGenderMaterials(string prefix, int index)
	{
		List<int> list = new List<int>();
		string text = $"{prefix}{index:D2}";
		string path = Path.Combine(characterDir, Path.Combine(appearance.gender.ToString(), Path.Combine(Path.Combine("Textures", prefix), text)));
		if (Directory.Exists(path))
		{
			string[] files = Directory.GetFiles(path, "m_" + text + "*.mat");
			foreach (string filename in files)
			{
				int num = ExtractIndex(filename);
				if (num > 0)
				{
					list.Add(num);
				}
			}
		}
		return list;
	}

	private List<int> GetGenderEquipmentMaterials(Equippable equip, Equippable.EquipmentSlot slot, int index)
	{
		List<int> list = new List<int>();
		string armorTypeShortString = equip.Appearance.GetArmorTypeShortString();
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if ((bool)itemInSlot)
		{
			armorTypeShortString = itemInSlot.Appearance.GetArmorTypeShortString();
			index = itemInSlot.Appearance.modelVariation;
		}
		string text = $"{armorTypeShortString}{index:D2}";
		string text2 = string.Empty;
		switch (slot)
		{
		case Equippable.EquipmentSlot.Armor:
			text2 = "Body";
			break;
		case Equippable.EquipmentSlot.Feet:
			text2 = "Boots";
			break;
		case Equippable.EquipmentSlot.Hands:
			text2 = "Gloves";
			break;
		}
		string path = Path.Combine(characterDir, Path.Combine(appearance.gender.ToString(), Path.Combine(Path.Combine("Textures//Body", armorTypeShortString), text)));
		if (Directory.Exists(path))
		{
			string[] files = Directory.GetFiles(path, "m_" + GetGenderPrefix(appearance.gender) + text + "_" + text2 + "*.mat");
			foreach (string filename in files)
			{
				int num = ExtractIndex(filename);
				if (num > 0)
				{
					list.Add(num);
				}
			}
		}
		return list;
	}

	private List<int> GetLocalMaterials(string prefix, int index)
	{
		List<int> list = new List<int>();
		string text = $"{prefix}{index:D2}";
		string path = Path.Combine(characterDir, Path.Combine(appearance.gender.ToString(), Path.Combine(Path.Combine(Path.Combine(appearance.race.ToString(), "Textures"), prefix), text)));
		if (Directory.Exists(path))
		{
			string[] files = Directory.GetFiles(path, "m_" + GetGenderPrefix(appearance.gender) + appearance.race.ToString() + "_" + text + "*.mat");
			foreach (string filename in files)
			{
				int num = ExtractIndex(filename);
				if (num > 0)
				{
					list.Add(num);
				}
			}
		}
		return list;
	}

	private List<int> GetLocalEquipmentMaterials(Equippable equip, Equippable.EquipmentSlot slot, int index)
	{
		List<int> list = new List<int>();
		string armorTypeShortString = equip.Appearance.GetArmorTypeShortString();
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if ((bool)itemInSlot)
		{
			armorTypeShortString = itemInSlot.Appearance.GetArmorTypeShortString();
			index = itemInSlot.Appearance.modelVariation;
		}
		string text = $"{armorTypeShortString}{index:D2}";
		string text2 = string.Empty;
		switch (slot)
		{
		case Equippable.EquipmentSlot.Feet:
			text2 = "Body";
			break;
		case Equippable.EquipmentSlot.Armor:
			text2 = "Boots";
			break;
		case Equippable.EquipmentSlot.Hands:
			text2 = "Gloves";
			break;
		}
		string path = Path.Combine(characterDir, Path.Combine(Path.Combine(appearance.gender.ToString(), appearance.race.ToString()), Path.Combine(Path.Combine("Body", armorTypeShortString), "Materials")));
		if (Directory.Exists(path))
		{
			string[] files = Directory.GetFiles(path, "m_" + GetGenderPrefix(appearance.gender) + text + "_" + text2 + "*.mat");
			foreach (string filename in files)
			{
				int num = ExtractIndex(filename);
				if (num > 0)
				{
					list.Add(num);
				}
			}
		}
		return list;
	}

	private string GetHeadDir()
	{
		return Path.Combine(characterDir, Path.Combine(appearance.gender.ToString(), Path.Combine(appearance.race.ToString(), "Head")));
	}

	private string GetHeadPrefix()
	{
		return string.Concat(GetGenderPrefix(appearance.gender), appearance.race, "_Head");
	}

	private string GetHairDir()
	{
		return Path.Combine(characterDir, Path.Combine(appearance.gender.ToString(), Path.Combine(appearance.race.ToString(), "Hair")));
	}

	private string GetHairPrefix()
	{
		return string.Concat(GetGenderPrefix(appearance.gender), appearance.race, "_Hair");
	}

	private string GetFacialHairDir()
	{
		return Path.Combine(characterDir, Path.Combine(appearance.gender.ToString(), Path.Combine(appearance.race.ToString(), "FacialHair")));
	}

	private string GetFacialHairPrefix()
	{
		return string.Concat(GetGenderPrefix(appearance.gender), appearance.race, "_FacialHair");
	}

	private string GetArmorDir()
	{
		string path = Path.Combine(characterDir, Path.Combine(appearance.gender.ToString(), Path.Combine(appearance.race.ToString(), "Body")));
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if ((bool)itemInSlot)
		{
			return Path.Combine(path, itemInSlot.Appearance.GetArmorTypeShortString());
		}
		return Path.Combine(path, "None");
	}

	private string GetArmorPrefix()
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if ((bool)itemInSlot)
		{
			return string.Concat(GetGenderPrefix(appearance.gender), appearance.race, "_", itemInSlot.Appearance.GetArmorTypeShortString());
		}
		return "None";
	}

	private string GetHelmDir()
	{
		return Path.Combine(characterDir, Path.Combine(appearance.gender.ToString(), Path.Combine(appearance.race.ToString(), "Helm")));
	}

	private string GetHelmPrefix()
	{
		return string.Concat(GetGenderPrefix(appearance.gender), appearance.race, "_Helm");
	}

	private string GetGlovesDir()
	{
		return Path.Combine(characterDir, Path.Combine(appearance.gender.ToString(), Path.Combine(appearance.race.ToString(), "Gloves")));
	}

	private string GetGlovesPrefix()
	{
		return string.Concat(GetGenderPrefix(appearance.gender), appearance.race, "_Gloves");
	}

	private string GetBootsDir()
	{
		return Path.Combine(characterDir, Path.Combine(appearance.gender.ToString(), Path.Combine(appearance.race.ToString(), "Boots")));
	}

	private string GetBootsPrefix()
	{
		return string.Concat(GetGenderPrefix(appearance.gender), appearance.race, "_Boots");
	}

	private int[] GetHeadMaterials()
	{
		return GetMaterials("Head", appearance.headAppearance.modelVariation);
	}

	private int[] GetHairMaterials()
	{
		return GetMaterials("Hair", appearance.hairAppearance.modelVariation);
	}

	private int[] GetFacialHairMaterials()
	{
		return GetMaterials("FacialHair", appearance.facialHairAppearance.modelVariation);
	}

	private int[] GetArmorMaterials()
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if ((bool)itemInSlot)
		{
			return GetEquipmentMaterials("Body", itemInSlot, Equippable.EquipmentSlot.Armor, itemInSlot.Appearance.modelVariation);
		}
		return new int[1] { 1 };
	}

	private int[] GetHelmMaterials()
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Head);
		if ((bool)itemInSlot)
		{
			return GetEquipmentMaterials("Helm", itemInSlot, Equippable.EquipmentSlot.Head, itemInSlot.Appearance.modelVariation);
		}
		return new int[1] { 1 };
	}

	private int[] GetGlovesMaterials()
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Hands);
		if ((bool)itemInSlot)
		{
			return GetEquipmentMaterials("Gloves", itemInSlot, Equippable.EquipmentSlot.Hands, itemInSlot.Appearance.modelVariation);
		}
		return new int[1] { 1 };
	}

	private int[] GetBootsMaterials()
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Feet);
		if ((bool)itemInSlot)
		{
			return GetEquipmentMaterials("Boots", itemInSlot, Equippable.EquipmentSlot.Feet, itemInSlot.Appearance.modelVariation);
		}
		return new int[1] { 1 };
	}

	private void OnGUI()
	{
	}

	private void Save(string filename)
	{
	}

	private void SaveAs()
	{
	}

	private void Open()
	{
	}

	public void SaveToFile()
	{
		TextWriter textWriter = new StreamWriter("D:\\modelviewer.txt");
		textWriter.WriteLine((int)appearance.race);
		textWriter.WriteLine((int)appearance.racialBodyType);
		textWriter.WriteLine((int)appearance.gender);
		textWriter.WriteLine(appearance.headAppearance.modelVariation);
		textWriter.WriteLine(appearance.headAppearance.materialVariation);
		textWriter.WriteLine(appearance.hairAppearance.modelVariation);
		textWriter.WriteLine(appearance.hairAppearance.materialVariation);
		textWriter.WriteLine(appearance.facialHairAppearance.modelVariation);
		textWriter.WriteLine(appearance.facialHairAppearance.materialVariation);
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if ((bool)itemInSlot)
		{
			textWriter.WriteLine("1");
			textWriter.WriteLine((int)itemInSlot.Appearance.armorType);
			textWriter.WriteLine(itemInSlot.Appearance.modelVariation);
			textWriter.WriteLine(itemInSlot.Appearance.materialVariation);
		}
		else
		{
			textWriter.WriteLine("0");
		}
		Equippable itemInSlot2 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Head);
		if ((bool)itemInSlot2)
		{
			textWriter.WriteLine("1");
			textWriter.WriteLine(itemInSlot2.Appearance.modelVariation);
			textWriter.WriteLine(itemInSlot2.Appearance.materialVariation);
		}
		else
		{
			textWriter.WriteLine("0");
		}
		Equippable itemInSlot3 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Hands);
		if ((bool)itemInSlot3)
		{
			textWriter.WriteLine("1");
			textWriter.WriteLine(itemInSlot3.Appearance.modelVariation);
			textWriter.WriteLine(itemInSlot3.Appearance.materialVariation);
		}
		else
		{
			textWriter.WriteLine("0");
		}
		Equippable itemInSlot4 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Feet);
		if ((bool)itemInSlot4)
		{
			textWriter.WriteLine("1");
			textWriter.WriteLine(itemInSlot4.Appearance.modelVariation);
			textWriter.WriteLine(itemInSlot4.Appearance.materialVariation);
		}
		else
		{
			textWriter.WriteLine("0");
		}
		textWriter.Close();
	}

	public void LoadFromFile()
	{
		if (!File.Exists("D:\\modelviewer.txt"))
		{
			return;
		}
		TextReader textReader = new StreamReader("D:\\modelviewer.txt");
		string s = textReader.ReadLine();
		appearance.race = (NPCAppearance.Race)int.Parse(s);
		string s2 = textReader.ReadLine();
		appearance.racialBodyType = (NPCAppearance.Race)int.Parse(s2);
		string s3 = textReader.ReadLine();
		appearance.gender = (NPCAppearance.Sex)int.Parse(s3);
		string s4 = textReader.ReadLine();
		string s5 = textReader.ReadLine();
		appearance.headAppearance.modelVariation = int.Parse(s4);
		appearance.headAppearance.materialVariation = int.Parse(s5);
		string s6 = textReader.ReadLine();
		string s7 = textReader.ReadLine();
		appearance.hairAppearance.modelVariation = int.Parse(s6);
		appearance.hairAppearance.materialVariation = int.Parse(s7);
		string s8 = textReader.ReadLine();
		string s9 = textReader.ReadLine();
		appearance.facialHairAppearance.modelVariation = int.Parse(s8);
		appearance.facialHairAppearance.materialVariation = int.Parse(s9);
		if (textReader.ReadLine() == "1")
		{
			string s10 = textReader.ReadLine();
			string s11 = textReader.ReadLine();
			string s12 = textReader.ReadLine();
			Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
			if (itemInSlot == null)
			{
				EquipItem(Equippable.EquipmentSlot.Armor, AppearancePiece.BodyPiece.Body, (AppearancePiece.ArmorType)int.Parse(s10), int.Parse(s11), int.Parse(s12));
			}
			else
			{
				itemInSlot.Appearance.armorType = (AppearancePiece.ArmorType)int.Parse(s10);
				itemInSlot.Appearance.modelVariation = int.Parse(s11);
				itemInSlot.Appearance.materialVariation = int.Parse(s12);
			}
		}
		else
		{
			Equippable itemInSlot2 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
			if (itemInSlot2 != null)
			{
				itemInSlot2 = equipment.UnEquip(itemInSlot2, Equippable.EquipmentSlot.Armor);
				Object.Destroy(itemInSlot2.gameObject);
			}
		}
		if (textReader.ReadLine() == "1")
		{
			string s13 = textReader.ReadLine();
			string s14 = textReader.ReadLine();
			Equippable itemInSlot3 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Head);
			if (itemInSlot3 == null)
			{
				EquipItem(Equippable.EquipmentSlot.Head, AppearancePiece.BodyPiece.Helm, AppearancePiece.ArmorType.None, int.Parse(s13), int.Parse(s14));
			}
			else
			{
				itemInSlot3.Appearance.modelVariation = int.Parse(s13);
				itemInSlot3.Appearance.materialVariation = int.Parse(s14);
			}
		}
		else
		{
			Equippable itemInSlot4 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Head);
			if (itemInSlot4 != null)
			{
				itemInSlot4 = equipment.UnEquip(itemInSlot4, Equippable.EquipmentSlot.Head);
				Object.Destroy(itemInSlot4.gameObject);
			}
		}
		if (textReader.ReadLine() == "1")
		{
			string s15 = textReader.ReadLine();
			string s16 = textReader.ReadLine();
			Equippable itemInSlot5 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Hands);
			if (itemInSlot5 == null)
			{
				EquipItem(Equippable.EquipmentSlot.Hands, AppearancePiece.BodyPiece.Gloves, AppearancePiece.ArmorType.None, int.Parse(s15), int.Parse(s16));
			}
			else
			{
				itemInSlot5.Appearance.modelVariation = int.Parse(s15);
				itemInSlot5.Appearance.materialVariation = int.Parse(s16);
			}
		}
		else
		{
			Equippable itemInSlot6 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Hands);
			if (itemInSlot6 != null)
			{
				itemInSlot6 = equipment.UnEquip(itemInSlot6, Equippable.EquipmentSlot.Hands);
				Object.Destroy(itemInSlot6.gameObject);
			}
		}
		if (textReader.ReadLine() == "1")
		{
			string s17 = textReader.ReadLine();
			string s18 = textReader.ReadLine();
			Equippable itemInSlot7 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Feet);
			if (itemInSlot7 == null)
			{
				EquipItem(Equippable.EquipmentSlot.Feet, AppearancePiece.BodyPiece.Boots, AppearancePiece.ArmorType.None, int.Parse(s17), int.Parse(s18));
			}
			else
			{
				itemInSlot7.Appearance.modelVariation = int.Parse(s17);
				itemInSlot7.Appearance.materialVariation = int.Parse(s18);
			}
		}
		else
		{
			Equippable itemInSlot8 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Feet);
			if (itemInSlot8 != null)
			{
				itemInSlot8 = equipment.UnEquip(itemInSlot8, Equippable.EquipmentSlot.Feet);
				Object.Destroy(itemInSlot8.gameObject);
			}
		}
		textReader.Close();
		appearance.avatar = null;
		appearance.gameObject.GetComponent<Animator>().avatar = null;
		isDirty = true;
	}

	private void OnSetSkinColor(Color color)
	{
		appearance.skinColor = color;
		isDirty = true;
	}

	private void OnSetTempSkinColor(Color color)
	{
		appearance.skinColor = color;
		isDirty = true;
	}

	private void OnGetSkinColor(ColorPicker picker)
	{
		if (appearance != null)
		{
			picker.NotifyColor(appearance.skinColor);
		}
	}

	private void OnSetHairColor(Color color)
	{
		appearance.hairColor = color;
		isDirty = true;
	}

	private void OnSetTempHairColor(Color color)
	{
		appearance.hairColor = color;
		isDirty = true;
	}

	private void OnGetHairColor(ColorPicker picker)
	{
		if (appearance != null)
		{
			picker.NotifyColor(appearance.hairColor);
		}
	}

	private void OnSetPrimaryColor(Color color)
	{
		appearance.primaryColor = color;
		isDirty = true;
	}

	private void OnSetTempPrimaryColor(Color color)
	{
		appearance.primaryColor = color;
		isDirty = true;
	}

	private void OnGetPrimaryColor(ColorPicker picker)
	{
		picker.NotifyColor(appearance.primaryColor);
	}

	private void OnSetSecondaryColor(Color color)
	{
		appearance.secondaryColor = color;
		isDirty = true;
	}

	private void OnSetTempSecondaryColor(Color color)
	{
		appearance.secondaryColor = color;
		isDirty = true;
	}

	private void OnGetSecondaryColor(ColorPicker picker)
	{
		picker.NotifyColor(appearance.secondaryColor);
	}
}
