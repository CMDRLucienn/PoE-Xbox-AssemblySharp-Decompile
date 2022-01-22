using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Toolbox/Rest Zone")]
public class RestZone : MonoBehaviour
{
	[Flags]
	public enum CannotCampReason
	{
		InCombat = 1,
		NoSupplies = 2,
		NoCampMap = 4,
		PartyNotGathered = 8,
		PartyCanSeeEnemy = 0x10,
		Error = 0x20
	}

	public enum Mode
	{
		Inn,
		Camp,
		Scripted
	}

	private int m_triggerCount;

	private static Mode m_restMode;

	private static RestMovieMode m_overrideMovie = RestMovieMode.None;

	protected CannotCampReason m_reason_cannot_camp;

	public bool CanRest;

	private static List<RestZone> m_InZones = new List<RestZone>();

	public const int DEFAULT_REST_HOURS = 8;

	public static bool PartyInRestZone => m_InZones.Count > 0;

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public static void Reset()
	{
		m_InZones.Clear();
	}

	public static void ShowRestUI(Mode restMode, RestMovieMode overrideMovie = RestMovieMode.None)
	{
		UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.YESNO, GUIUtils.GetText(356), GUIUtils.Format(359, 8));
		m_restMode = restMode;
		m_overrideMovie = overrideMovie;
		uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, new UIMessageBox.OnEndDialog(OnConfirmDialog));
	}

	private static void OnConfirmDialog(UIMessageBox.Result result, UIMessageBox owner)
	{
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			Rest(m_restMode);
		}
		m_overrideMovie = RestMovieMode.None;
	}

	public static void Rest(Mode formode)
	{
		RestMovieMode forMode = RestMovieMode.None;
		switch (formode)
		{
		case Mode.Camp:
			forMode = RestMovieMode.Camp;
			break;
		case Mode.Inn:
			forMode = RestMovieMode.Inn;
			break;
		}
		if ((bool)GameState.s_playerCharacter && GameGlobalVariables.IsPlayerWatcher())
		{
			Health component = GameState.s_playerCharacter.GetComponent<Health>();
			CharacterStats component2 = GameState.s_playerCharacter.GetComponent<CharacterStats>();
			if (component.HealthPercentage < 0.25f || component2.GetFatigueLevel() >= CharacterStats.FatigueLevel.Minor)
			{
				forMode = RestMovieMode.Watcher;
			}
		}
		if (m_overrideMovie != RestMovieMode.None)
		{
			UIRestMovieManager.Instance.ForMode = m_overrideMovie;
			m_overrideMovie = RestMovieMode.None;
		}
		else
		{
			UIRestMovieManager.Instance.ForMode = forMode;
		}
		UIRestMovieManager.Instance.ShowWindow();
		if (GlobalAudioPlayer.Instance != null)
		{
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.Rest);
		}
		WorldTime.Instance.AdvanceTimeByHours(8, isResting: true);
		TriggerOnResting();
		GameUtilities.ClearFadingEffects();
		if (formode == Mode.Camp)
		{
			foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
			{
				CharacterStats component3 = onlyPrimaryPartyMember.GetComponent<CharacterStats>();
				if ((bool)component3 && component3.LastSelectedSurvivalBonus >= 0)
				{
					int survival = component3.CalculateSkill(CharacterStats.SkillType.Survival);
					if (AfflictionData.Instance.SurvivalCampEffects.IsBonusValid(component3.LastSelectedSurvivalBonus, component3.LastSelectedSurvivalSubBonus, survival))
					{
						component3.ApplyAffliction(AfflictionData.Instance.SurvivalCampEffects.GetBestBonusByIndex(component3.LastSelectedSurvivalBonus, component3.LastSelectedSurvivalSubBonus, survival));
					}
				}
			}
		}
		if ((bool)AchievementTracker.Instance)
		{
			AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumRestsUsed);
		}
	}

	public static void TriggerOnResting()
	{
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI != null)
			{
				CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
				if (component != null)
				{
					component.HandleGameOnResting();
				}
			}
		}
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			foreach (StoredCharacterInfo companion in stronghold.GetCompanions())
			{
				companion.HasRested = true;
			}
		}
		GameState.TriggerRestMode();
		UIHudAlerts.Cancel(UIActionBarOnClick.ActionType.Camp);
	}

	public static void ShowCampUI()
	{
		CannotCampReason cannotCampReason = WhyCannotCamp();
		if ((cannotCampReason & ~(CannotCampReason.NoSupplies | CannotCampReason.NoCampMap)) == 0 && UIDisableIfFreeRestAvailable.FreeRestAvailable())
		{
			UIStoreManager.Instance.Vendor = Stronghold.Instance.BrighthollowPrefab;
			UIStoreManager.Instance.ShowWindow();
			return;
		}
		if (cannotCampReason == (CannotCampReason)0)
		{
			UIRestConfirmBox.OnDialogEnd = (UIRestConfirmBox.OnEndDialog)Delegate.Combine(UIRestConfirmBox.OnDialogEnd, new UIRestConfirmBox.OnEndDialog(OnConfirmCamp));
			UIRestConfirmBox.Instance.SetData(UIRestConfirmBox.RestType.CAMP, GUIUtils.GetText(2302));
			return;
		}
		string text = "*No Camp Error*";
		if ((cannotCampReason & CannotCampReason.InCombat) > (CannotCampReason)0)
		{
			text = GUIUtils.GetText(1349);
		}
		else if ((cannotCampReason & CannotCampReason.NoCampMap) > (CannotCampReason)0)
		{
			text = GUIUtils.GetText(1351);
		}
		else if ((cannotCampReason & CannotCampReason.PartyNotGathered) > (CannotCampReason)0)
		{
			text = GUIUtils.GetText(1352);
		}
		else if ((cannotCampReason & CannotCampReason.PartyCanSeeEnemy) > (CannotCampReason)0)
		{
			text = GUIUtils.GetText(1353);
		}
		else if ((cannotCampReason & CannotCampReason.NoSupplies) > (CannotCampReason)0)
		{
			text = GUIUtils.GetText(1350);
		}
		UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", text);
	}

	private static void OnConfirmCamp(UIRestConfirmBox.Result result)
	{
		UIRestConfirmBox.OnDialogEnd = (UIRestConfirmBox.OnEndDialog)Delegate.Remove(UIRestConfirmBox.OnDialogEnd, new UIRestConfirmBox.OnEndDialog(OnConfirmCamp));
		switch (result)
		{
		case UIRestConfirmBox.Result.REST:
			Camp();
			break;
		case UIRestConfirmBox.Result.STASH:
		{
			UIWindowManager.Instance.CloseAllWindows();
			UIInventoryManager.Instance.ImmediateStashAccess();
			UIInventoryManager instance = UIInventoryManager.Instance;
			instance.OnWindowHidden = (UIHudWindow.WindowHiddenDelegate)Delegate.Combine(instance.OnWindowHidden, new UIHudWindow.WindowHiddenDelegate(CampFromStash));
			break;
		}
		}
	}

	public static bool CanCamp()
	{
		return WhyCannotCamp() == (CannotCampReason)0;
	}

	private static void CampFromStash(UIHudWindow window)
	{
		UIInventoryManager instance = UIInventoryManager.Instance;
		instance.OnWindowHidden = (UIHudWindow.WindowHiddenDelegate)Delegate.Remove(instance.OnWindowHidden, new UIHudWindow.WindowHiddenDelegate(CampFromStash));
		Camp();
	}

	public static void Camp()
	{
		PlayerInventory inventory = GameState.s_playerCharacter.Inventory;

		if (IEModOptions.MaxCampingSupplies != IEModOptions.MaxCampingSuppliesOptions.Disabled)
		{
			if (inventory)
			{
				inventory.CampingSuppliesTotal--;
			}
			Rest(Mode.Camp);
		}
	}

	public static CannotCampReason WhyCannotCamp()
	{
		CannotCampReason cannotCampReason = (CannotCampReason)0;
		if (GameState.s_playerCharacter == null)
		{
			return CannotCampReason.Error;
		}
		if (GameState.InCombat)
		{
			cannotCampReason |= CannotCampReason.InCombat;
		}
		if (IEModOptions.MaxCampingSupplies != IEModOptions.MaxCampingSuppliesOptions.Disabled)
		{
			PlayerInventory inventory = GameState.s_playerCharacter.Inventory;
			if (inventory == null || inventory.CampingSuppliesTotal == 0)
			{
				cannotCampReason |= CannotCampReason.NoSupplies;
			}
		}
		if (GameState.Instance.CurrentMap != null && !GameState.Instance.CurrentMap.GetCanCamp())
		{
			cannotCampReason |= CannotCampReason.NoCampMap;
		}
		if (PartyHelper.PartyCanSeeEnemy())
		{
			cannotCampReason |= CannotCampReason.PartyCanSeeEnemy;
		}
		return cannotCampReason;
	}

	public void OnTriggerEnter(Collider other)
	{
		if (!other.gameObject.GetComponent<PartyMemberAI>())
		{
			return;
		}
		m_triggerCount++;
		if (m_triggerCount == PartyMemberAI.PartyMembers.Length)
		{
			if (!CanRest)
			{
				m_InZones.Add(this);
			}
			CanRest = true;
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if ((bool)other.gameObject.GetComponent<PartyMemberAI>())
		{
			m_triggerCount--;
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(PartyMemberAI));
			if (m_triggerCount < array.Length)
			{
				m_InZones.Remove(this);
				CanRest = false;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (!(GetComponent<Collider>() == null))
		{
			DrawUtility.DrawCollider(base.transform, GetComponent<Collider>(), new Color(1f, 0.5f, 0f, 1f));
		}
	}
}
