using System.IO;
using UnityEngine;

public class ModelViewer : MonoBehaviour
{
	private GameObject character;

	private NPCAppearance appearance;

	private Equipment equipment;

	private bool appearanceToggle = true;

	private bool equipmentToggle = true;

	private bool isDirty;

	private bool equipDefaultItems = true;

	private void Start()
	{
		character = new GameObject("character");
		appearance = character.AddComponent<NPCAppearance>();
		equipment = character.AddComponent<Equipment>();
		appearance.race = NPCAppearance.Race.ELF;
		appearance.racialBodyType = appearance.race;
		appearance.gender = NPCAppearance.Sex.Female;
		appearance.headAppearance.bodyPiece = AppearancePiece.BodyPiece.Head;
		appearance.hairAppearance.bodyPiece = AppearancePiece.BodyPiece.Hair;
		appearance.hasHair = true;
		appearance.facialHairAppearance.bodyPiece = AppearancePiece.BodyPiece.Facialhair;
		appearance.hasFacialHair = false;
		isDirty = true;
		PE_LightEnvironment pE_LightEnvironment = Object.FindObjectOfType(typeof(PE_LightEnvironment)) as PE_LightEnvironment;
		if ((bool)pE_LightEnvironment)
		{
			Shader.SetGlobalTexture("Trenton_EnvironmentMap", pE_LightEnvironment.environmentMap);
			Shader.SetGlobalTexture("Trenton_AnisoEnvironmentMap", pE_LightEnvironment.anisotropicEnvironmentMap);
			Shader.SetGlobalColor("Trenton_Ambient_High", pE_LightEnvironment.ambientUpper);
			Shader.SetGlobalColor("Trenton_Ambient_Low", pE_LightEnvironment.ambientLower);
		}
		else
		{
			Shader.SetGlobalColor("Trenton_Ambient_High", Color.black);
			Shader.SetGlobalColor("Trenton_Ambient_Low", Color.black);
		}
	}

	private void EquipItem(Equippable.EquipmentSlot slot, AppearancePiece.BodyPiece bodyType, AppearancePiece.ArmorType armorType, int modelVariation, int materialVariation)
	{
		Equippable equippable = new GameObject(slot.ToString()).AddComponent<Equippable>();
		equippable.Appearance = new AppearancePiece();
		equippable.Appearance.armorType = armorType;
		equippable.Appearance.bodyPiece = bodyType;
		equippable.Appearance.modelVariation = modelVariation;
		equippable.Appearance.materialVariation = materialVariation;
		Equippable equippable2 = equipment.Equip(equippable, slot, enforceRecoveryPenalty: false);
		if ((bool)equippable2)
		{
			Object.Destroy(equippable2);
		}
	}

	private void Update()
	{
		if (equipDefaultItems)
		{
			EquipItem(Equippable.EquipmentSlot.Armor, AppearancePiece.BodyPiece.Body, AppearancePiece.ArmorType.Mail, 1, 1);
			equipDefaultItems = false;
			LoadFromFile();
		}
		if (isDirty)
		{
			appearance.Generate();
			isDirty = false;
		}
	}

	private void ChangeRace(bool next)
	{
		if (next)
		{
			appearance.race++;
		}
		else
		{
			appearance.race--;
		}
		appearance.race = (NPCAppearance.Race)Mathf.Clamp((int)appearance.race, 0, 5);
		appearance.racialBodyType = ((appearance.race != NPCAppearance.Race.GOD) ? appearance.race : NPCAppearance.Race.HUM);
		appearance.avatar = null;
		appearance.gameObject.GetComponent<Animator>().avatar = null;
		isDirty = true;
	}

	private void ChangeSubrace(bool next)
	{
		if (next)
		{
			appearance.subrace++;
		}
		else
		{
			appearance.subrace--;
		}
		appearance.subrace = (NPCAppearance.Subrace)Mathf.Clamp((int)appearance.subrace, 8, 11);
		appearance.avatar = null;
		appearance.gameObject.GetComponent<Animator>().avatar = null;
		isDirty = true;
	}

	private void ChangeRacialBodyType(bool next)
	{
		if (next)
		{
			appearance.racialBodyType++;
		}
		else
		{
			appearance.racialBodyType--;
		}
		appearance.racialBodyType = (NPCAppearance.Race)Mathf.Clamp((int)appearance.racialBodyType, 0, 4);
		appearance.avatar = null;
		appearance.gameObject.GetComponent<Animator>().avatar = null;
		isDirty = true;
	}

	private void ChangeGender(bool next)
	{
		if (next)
		{
			appearance.gender = NPCAppearance.Sex.Male;
		}
		else
		{
			appearance.gender = NPCAppearance.Sex.Female;
		}
		appearance.avatar = null;
		appearance.gameObject.GetComponent<Animator>().avatar = null;
		isDirty = true;
	}

	private void ChangeHeadMesh(bool next)
	{
		if (next)
		{
			appearance.headAppearance.modelVariation++;
		}
		else
		{
			appearance.headAppearance.modelVariation--;
		}
		appearance.headAppearance.modelVariation = Mathf.Clamp(appearance.headAppearance.modelVariation, 1, 100);
		isDirty = true;
	}

	private void ChangeHeadMaterial(bool next)
	{
		if (next)
		{
			appearance.headAppearance.materialVariation++;
		}
		else
		{
			appearance.headAppearance.materialVariation--;
		}
		appearance.headAppearance.materialVariation = Mathf.Clamp(appearance.headAppearance.materialVariation, 1, 100);
		isDirty = true;
	}

	private void ChangeHairMesh(bool next)
	{
		if (next)
		{
			appearance.hairAppearance.modelVariation++;
		}
		else
		{
			appearance.hairAppearance.modelVariation--;
		}
		appearance.hairAppearance.modelVariation = Mathf.Clamp(appearance.hairAppearance.modelVariation, 1, 100);
		isDirty = true;
	}

	private void ChangeHairMaterial(bool next)
	{
		if (next)
		{
			appearance.hairAppearance.materialVariation++;
		}
		else
		{
			appearance.hairAppearance.materialVariation--;
		}
		appearance.hairAppearance.materialVariation = Mathf.Clamp(appearance.hairAppearance.materialVariation, 1, 100);
		isDirty = true;
	}

	private void ChangeFacialHairMesh(bool next)
	{
		if (next)
		{
			appearance.facialHairAppearance.modelVariation++;
		}
		else
		{
			appearance.facialHairAppearance.modelVariation--;
		}
		appearance.facialHairAppearance.modelVariation = Mathf.Clamp(appearance.facialHairAppearance.modelVariation, 1, 100);
		isDirty = true;
	}

	private void ChangeFacialHairMaterial(bool next)
	{
		if (next)
		{
			appearance.facialHairAppearance.materialVariation++;
		}
		else
		{
			appearance.facialHairAppearance.materialVariation--;
		}
		appearance.facialHairAppearance.materialVariation = Mathf.Clamp(appearance.facialHairAppearance.materialVariation, 1, 100);
		isDirty = true;
	}

	private void ChangeArmorType(bool next)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if ((bool)itemInSlot)
		{
			if (next)
			{
				itemInSlot.Appearance.armorType = itemInSlot.Appearance.armorType + 1;
			}
			else
			{
				itemInSlot.Appearance.armorType = itemInSlot.Appearance.armorType - 1;
			}
			itemInSlot.Appearance.armorType = (AppearancePiece.ArmorType)Mathf.Clamp((int)itemInSlot.Appearance.armorType, 1, 9);
		}
		isDirty = true;
	}

	private void ChangeArmorMesh(bool next)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if ((bool)itemInSlot)
		{
			if (next)
			{
				itemInSlot.Appearance.modelVariation = itemInSlot.Appearance.modelVariation + 1;
			}
			else
			{
				itemInSlot.Appearance.modelVariation = itemInSlot.Appearance.modelVariation - 1;
			}
			itemInSlot.Appearance.modelVariation = Mathf.Clamp(itemInSlot.Appearance.modelVariation, 1, 100);
		}
		isDirty = true;
	}

	private void ChangeArmorMaterial(bool next)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if ((bool)itemInSlot)
		{
			if (next)
			{
				itemInSlot.Appearance.materialVariation = itemInSlot.Appearance.materialVariation + 1;
			}
			else
			{
				itemInSlot.Appearance.materialVariation = itemInSlot.Appearance.materialVariation - 1;
			}
			itemInSlot.Appearance.materialVariation = Mathf.Clamp(itemInSlot.Appearance.materialVariation, 1, 100);
		}
		isDirty = true;
	}

	private void ChangeHelmMesh(bool next)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Head);
		if ((bool)itemInSlot)
		{
			if (next)
			{
				itemInSlot.Appearance.modelVariation = itemInSlot.Appearance.modelVariation + 1;
			}
			else
			{
				itemInSlot.Appearance.modelVariation = itemInSlot.Appearance.modelVariation - 1;
			}
			itemInSlot.Appearance.modelVariation = Mathf.Clamp(itemInSlot.Appearance.modelVariation, 1, 100);
		}
		isDirty = true;
	}

	private void ChangeHelmMaterial(bool next)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Head);
		if ((bool)itemInSlot)
		{
			if (next)
			{
				itemInSlot.Appearance.materialVariation = itemInSlot.Appearance.materialVariation + 1;
			}
			else
			{
				itemInSlot.Appearance.materialVariation = itemInSlot.Appearance.materialVariation - 1;
			}
			itemInSlot.Appearance.materialVariation = Mathf.Clamp(itemInSlot.Appearance.materialVariation, 1, 100);
		}
		isDirty = true;
	}

	private void ChangeHandsMesh(bool next)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Hands);
		if ((bool)itemInSlot)
		{
			if (next)
			{
				itemInSlot.Appearance.modelVariation = itemInSlot.Appearance.modelVariation + 1;
			}
			else
			{
				itemInSlot.Appearance.modelVariation = itemInSlot.Appearance.modelVariation - 1;
			}
			itemInSlot.Appearance.modelVariation = Mathf.Clamp(itemInSlot.Appearance.modelVariation, 1, 100);
		}
		isDirty = true;
	}

	private void ChangeHandsMaterial(bool next)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Hands);
		if ((bool)itemInSlot)
		{
			if (next)
			{
				itemInSlot.Appearance.materialVariation = itemInSlot.Appearance.materialVariation + 1;
			}
			else
			{
				itemInSlot.Appearance.materialVariation = itemInSlot.Appearance.materialVariation - 1;
			}
			itemInSlot.Appearance.materialVariation = Mathf.Clamp(itemInSlot.Appearance.materialVariation, 1, 100);
		}
		isDirty = true;
	}

	private void ChangeFeetMesh(bool next)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Feet);
		if ((bool)itemInSlot)
		{
			if (next)
			{
				itemInSlot.Appearance.modelVariation = itemInSlot.Appearance.modelVariation + 1;
			}
			else
			{
				itemInSlot.Appearance.modelVariation = itemInSlot.Appearance.modelVariation - 1;
			}
			itemInSlot.Appearance.modelVariation = Mathf.Clamp(itemInSlot.Appearance.modelVariation, 1, 100);
		}
		isDirty = true;
	}

	private void ChangeFeetMaterial(bool next)
	{
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Feet);
		if ((bool)itemInSlot)
		{
			if (next)
			{
				itemInSlot.Appearance.materialVariation = itemInSlot.Appearance.materialVariation + 1;
			}
			else
			{
				itemInSlot.Appearance.materialVariation = itemInSlot.Appearance.materialVariation - 1;
			}
			itemInSlot.Appearance.materialVariation = Mathf.Clamp(itemInSlot.Appearance.materialVariation, 1, 100);
		}
		isDirty = true;
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 100, 100f, 50f), "Save"))
		{
			SaveToFile();
		}
		if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 50, 100f, 50f), "Load"))
		{
			LoadFromFile();
		}
		int num = Screen.width - 300;
		int num2 = 50;
		int num3 = 200;
		int num4 = 18;
		GUI.Label(new Rect(num, num2, num3, num4), "Race: " + appearance.GetRaceString());
		num2 += num4 + 5;
		GUI.Label(new Rect(num, num2, num3, num4), "Body Type: " + appearance.GetBodyString());
		num2 += num4 + 5;
		GUI.Label(new Rect(num, num2, num3, num4), "Gender: " + appearance.GetGenderFullString());
		num2 += num4 + 5;
		GUI.Label(new Rect(num, num2, num3, num4), "Head: " + appearance.headAppearance.modelVariation + ", " + appearance.headAppearance.materialVariation);
		num2 += num4 + 5;
		if (appearance.hasHair)
		{
			GUI.Label(new Rect(num, num2, num3, num4), "Hair: " + appearance.hairAppearance.modelVariation + ", " + appearance.hairAppearance.materialVariation);
			num2 += num4 + 5;
		}
		if (appearance.hasFacialHair)
		{
			GUI.Label(new Rect(num, num2, num3, num4), "Facial Hair: " + appearance.facialHairAppearance.modelVariation + ", " + appearance.facialHairAppearance.materialVariation);
			num2 += num4 + 5;
		}
		Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Armor);
		if ((bool)itemInSlot)
		{
			GUI.Label(new Rect(num, num2, num3, num4), "Armor: " + itemInSlot.Appearance.GetArmorTypeShortString() + ", " + itemInSlot.Appearance.modelVariation + ", " + itemInSlot.Appearance.materialVariation);
			num2 += num4 + 5;
		}
		Equippable itemInSlot2 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Head);
		if ((bool)itemInSlot2)
		{
			GUI.Label(new Rect(num, num2, num3, num4), "Helm: " + itemInSlot2.Appearance.modelVariation + ", " + itemInSlot2.Appearance.materialVariation);
			num2 += num4 + 5;
		}
		Equippable itemInSlot3 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Hands);
		if ((bool)itemInSlot3)
		{
			GUI.Label(new Rect(num, num2, num3, num4), "Hands: " + itemInSlot3.Appearance.modelVariation + ", " + itemInSlot3.Appearance.materialVariation);
			num2 += num4 + 5;
		}
		Equippable itemInSlot4 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Feet);
		if ((bool)itemInSlot4)
		{
			GUI.Label(new Rect(num, num2, num3, num4), "Feet: " + itemInSlot4.Appearance.modelVariation + ", " + itemInSlot4.Appearance.materialVariation);
			num2 += num4 + 5;
		}
		int num5 = 15;
		int num6 = 100;
		int num7 = 18;
		int num8 = 10;
		int num9 = 50;
		int num10 = 50;
		if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
		{
			ChangeRace(next: false);
		}
		GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Race");
		if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
		{
			ChangeRace(next: true);
		}
		num10 += num7 + 5;
		if (appearance.race == NPCAppearance.Race.GOD)
		{
			if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
			{
				ChangeRacialBodyType(next: false);
			}
			GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Body Type");
			if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
			{
				ChangeRacialBodyType(next: true);
			}
			num10 += num7 + 5;
			if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
			{
				ChangeSubrace(next: false);
			}
			GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Subrace");
			if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
			{
				ChangeSubrace(next: true);
			}
		}
		else
		{
			num10 += num7 + 5;
		}
		num10 += num7 + 5;
		if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
		{
			ChangeGender(next: false);
		}
		GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Gender");
		if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
		{
			ChangeGender(next: true);
		}
		num10 += num7 + 5;
		appearanceToggle = GUI.Toggle(new Rect(num9, num10, num6, num7), appearanceToggle, "Appearance");
		num10 += num7 + 5;
		if (appearanceToggle)
		{
			num9 += num8;
			GUI.Label(new Rect(num9, num10, num6, num7), "Head");
			num10 += num7 + 5;
			num9 += num8;
			if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
			{
				ChangeHeadMesh(next: false);
			}
			GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Model");
			if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
			{
				ChangeHeadMesh(next: true);
			}
			num10 += num7 + 5;
			if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
			{
				ChangeHeadMaterial(next: false);
			}
			GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Material");
			if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
			{
				ChangeHeadMaterial(next: true);
			}
			num10 += num7 + 5;
			num9 -= num8;
			bool hasHair = appearance.hasHair;
			appearance.hasHair = GUI.Toggle(new Rect(num9, num10, num6, num7), appearance.hasHair, "Hair");
			if (hasHair != appearance.hasHair)
			{
				isDirty = true;
			}
			num10 += num7 + 5;
			if (appearance.hasHair)
			{
				num9 += num8;
				if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
				{
					ChangeHairMesh(next: false);
				}
				GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Model");
				if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
				{
					ChangeHairMesh(next: true);
				}
				num10 += num7 + 5;
				if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
				{
					ChangeHairMaterial(next: false);
				}
				GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Material");
				if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
				{
					ChangeHairMaterial(next: true);
				}
				num10 += num7 + 5;
				num9 -= num8;
			}
			bool hasFacialHair = appearance.hasFacialHair;
			appearance.hasFacialHair = GUI.Toggle(new Rect(num9, num10, num6, num7), appearance.hasFacialHair, "Facial Hair");
			if (hasFacialHair != appearance.hasFacialHair)
			{
				isDirty = true;
			}
			num10 += num7 + 5;
			if (appearance.hasFacialHair)
			{
				num9 += num8;
				if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
				{
					ChangeFacialHairMesh(next: false);
				}
				GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Model");
				if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
				{
					ChangeFacialHairMesh(next: true);
				}
				num10 += num7 + 5;
				if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
				{
					ChangeFacialHairMaterial(next: false);
				}
				GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Material");
				if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
				{
					ChangeFacialHairMaterial(next: true);
				}
				num10 += num7 + 5;
				num9 -= num8;
			}
			num9 -= num8;
		}
		equipmentToggle = GUI.Toggle(new Rect(num9, num10, num6, num7), equipmentToggle, "Equipment");
		num10 += num7 + 5;
		if (!equipmentToggle)
		{
			return;
		}
		num9 += num8;
		GUI.Label(new Rect(num9, num10, num6, num7), "Body");
		num10 += num7 + 5;
		num9 += num8;
		if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
		{
			ChangeArmorType(next: false);
		}
		GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Type");
		if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
		{
			ChangeArmorType(next: true);
		}
		num10 += num7 + 5;
		if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
		{
			ChangeArmorMesh(next: false);
		}
		GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Model");
		if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
		{
			ChangeArmorMesh(next: true);
		}
		num10 += num7 + 5;
		if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
		{
			ChangeArmorMaterial(next: false);
		}
		GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Material");
		if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
		{
			ChangeArmorMaterial(next: true);
		}
		num10 += num7 + 5;
		num9 -= num8;
		Equippable itemInSlot5 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Head);
		bool value = ((itemInSlot5 != null) ? true : false);
		value = GUI.Toggle(new Rect(num9, num10, num6, num7), value, "Helm");
		if (((itemInSlot5 != null) ? 1 : 0) != (value ? 1 : 0))
		{
			if (itemInSlot5 != null)
			{
				itemInSlot5 = equipment.UnEquip(itemInSlot5, Equippable.EquipmentSlot.Head);
				Object.Destroy(itemInSlot5.gameObject);
			}
			else
			{
				EquipItem(Equippable.EquipmentSlot.Head, AppearancePiece.BodyPiece.Helm, AppearancePiece.ArmorType.None, 1, 1);
			}
			isDirty = true;
		}
		num10 += num7 + 5;
		if (value)
		{
			num9 += num8;
			if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
			{
				ChangeHelmMesh(next: false);
			}
			GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Model");
			if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
			{
				ChangeHelmMesh(next: true);
			}
			num10 += num7 + 5;
			if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
			{
				ChangeHelmMaterial(next: false);
			}
			GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Material");
			if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
			{
				ChangeHelmMaterial(next: true);
			}
			num10 += num7 + 5;
			num9 -= num8;
		}
		Equippable itemInSlot6 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Hands);
		bool value2 = ((itemInSlot6 != null) ? true : false);
		value2 = GUI.Toggle(new Rect(num9, num10, num6, num7), value2, "Hands");
		if (((itemInSlot6 != null) ? 1 : 0) != (value2 ? 1 : 0))
		{
			if (itemInSlot6 != null)
			{
				itemInSlot6 = equipment.UnEquip(itemInSlot6, Equippable.EquipmentSlot.Hands);
				Object.Destroy(itemInSlot6.gameObject);
			}
			else
			{
				EquipItem(Equippable.EquipmentSlot.Hands, AppearancePiece.BodyPiece.Gloves, AppearancePiece.ArmorType.None, 1, 1);
			}
			isDirty = true;
		}
		num10 += num7 + 5;
		if (value2)
		{
			num9 += num8;
			if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
			{
				ChangeHandsMesh(next: false);
			}
			GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Model");
			if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
			{
				ChangeHandsMesh(next: true);
			}
			num10 += num7 + 5;
			if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
			{
				ChangeHandsMaterial(next: false);
			}
			GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Material");
			if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
			{
				ChangeHandsMaterial(next: true);
			}
			num10 += num7 + 5;
			num9 -= num8;
		}
		Equippable itemInSlot7 = equipment.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Feet);
		bool value3 = ((itemInSlot7 != null) ? true : false);
		value3 = GUI.Toggle(new Rect(num9, num10, num6, num7), value3, "Feet");
		if (((itemInSlot7 != null) ? 1 : 0) != (value3 ? 1 : 0))
		{
			if (itemInSlot7 != null)
			{
				itemInSlot7 = equipment.UnEquip(itemInSlot7, Equippable.EquipmentSlot.Feet);
				Object.Destroy(itemInSlot7.gameObject);
			}
			else
			{
				EquipItem(Equippable.EquipmentSlot.Feet, AppearancePiece.BodyPiece.Boots, AppearancePiece.ArmorType.None, 1, 1);
			}
			isDirty = true;
		}
		num10 += num7 + 5;
		if (value3)
		{
			num9 += num8;
			if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
			{
				ChangeFeetMesh(next: false);
			}
			GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Model");
			if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
			{
				ChangeFeetMesh(next: true);
			}
			num10 += num7 + 5;
			if (GUI.Button(new Rect(num9, num10, num5, num7), "<"))
			{
				ChangeFeetMaterial(next: false);
			}
			GUI.Label(new Rect(num9 + num5 + 5, num10, num6, num7), "Material");
			if (GUI.Button(new Rect(num9 + num5 + num6 + 10, num10, num5, num7), ">"))
			{
				ChangeFeetMaterial(next: true);
			}
			num10 += num7 + 5;
			num9 -= num8;
		}
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
}
