using UnityEngine;

namespace Assets.Code.SDK.Toolbox;

public class RealtimeCutscene : Cutscene
{
	public bool HideHUD = true;

	public bool DisablePlayerCameraControl = true;

	public override void StartCutscene()
	{
		if (!GameState.CutsceneAllowed)
		{
			return;
		}
		base.Active = true;
		Cutscene.ActiveCutscenes.Add(this);
		ScriptEvent component = GetComponent<ScriptEvent>();
		if ((bool)component)
		{
			component.ExecuteScript(ScriptEvent.ScriptEvents.OnCutsceneStart);
		}
		SpawnWaypointList.Clear();
		MoveWaypointList.Clear();
		AddSceneActors();
		SpawnPrefabActors();
		AddPuppetControllerToActors();
		if (DisablePlayerCameraControl)
		{
			CameraControl.Instance.EnablePlayerControl(enableControl: false);
			CameraControl.Instance.EnablePlayerScroll(enableScroll: false);
		}
		if (HideHUD)
		{
			InGameHUD.Instance.ShowHUD = false;
		}
		bool flag = false;
		if (base.FowRevealers != null)
		{
			FogOfWarRevealer[] fowRevealers = base.FowRevealers;
			foreach (FogOfWarRevealer fogOfWarRevealer in fowRevealers)
			{
				if ((bool)fogOfWarRevealer)
				{
					fogOfWarRevealer.gameObject.SetActive(value: true);
					flag = true;
				}
			}
		}
		if (!flag && DisableFog)
		{
			FogOfWarRender.Instance.gameObject.SetActive(value: false);
		}
		PartyMemberAI.SafeEnableDisable = true;
		if ((bool)UIWindowManager.Instance)
		{
			UIWindowManager.Instance.CloseAllWindows();
		}
		UIWindowManager.DisableWindowVisibilityHandling();
		if (PauseNonActorCharacters)
		{
			PauseObjects();
		}
		m_cameraOcclusionPass = Object.FindObjectOfType<ScreenTextureScript_Occlusion>();
		if ((bool)m_cameraOcclusionPass)
		{
			m_cameraOcclusionPass.gameObject.SetActive(value: false);
		}
		BasePuppetScript basePuppetScript = GetComponent<BasePuppetScript>();
		if (basePuppetScript == null)
		{
			basePuppetScript = base.gameObject.AddComponent<BasePuppetScript>();
		}
		if ((bool)basePuppetScript)
		{
			basePuppetScript.ReferencedObjects = ActorList.ToArray();
			basePuppetScript.RealPlayer = RealPlayer;
			basePuppetScript.ActorPlayer = ActorPlayer;
			basePuppetScript.RealParty = RealParty;
			basePuppetScript.ActorParty = ActorParty;
			basePuppetScript.FailSafeTimer = FailsafeTimer;
			basePuppetScript.Run();
		}
	}

	public override void EndCutscene(bool callEndScripts)
	{
		base.Active = false;
		Cutscene.ActiveCutscenes.Remove(this);
		if (PauseNonActorCharacters)
		{
			UnPauseObjects();
		}
		RemovePuppetControllerFromActors();
		DestroyPrefabActors();
		RemoveSceneActors();
		CameraControl.Instance.EnablePlayerControl(enableControl: true);
		CameraControl.Instance.EnablePlayerScroll(enableScroll: true);
		InGameHUD.Instance.ShowHUD = true;
		DisableRevealers();
		FogOfWarRender.Instance.gameObject.SetActive(value: true);
		PartyMemberAI.SafeEnableDisable = false;
		UIWindowManager.EnableWindowVisibilityHandling();
		GameInput.EndBlockAllKeys();
		if ((bool)m_cameraOcclusionPass)
		{
			m_cameraOcclusionPass.gameObject.SetActive(value: true);
		}
		m_cameraOcclusionPass = null;
		if (callEndScripts)
		{
			ScriptEvent component = GetComponent<ScriptEvent>();
			if ((bool)component)
			{
				component.ExecuteScript(ScriptEvent.ScriptEvents.OnCutsceneEnd);
			}
		}
	}
}
