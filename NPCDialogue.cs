using System.Collections.Generic;
using System.Linq;
using AI.Achievement;
using OEIFormats.FlowCharts.Conversations;
using UnityEngine;

public class NPCDialogue : Usable
{
	public ConversationObject ConversationFile = new ConversationObject();

	private FlowChartPlayer ActiveConversation;

	public float UseRadius = 2f;

	public float ArrivalDistance;

	[Tooltip("Does this dialogue interrupt and pause any other conversations the character might be participating in?")]
	public bool Interrupts = true;

	[Tooltip("If set, the character faces the user when interacted with.")]
	public bool FaceUser;

	[HideInInspector]
	[Persistent]
	public bool wantsToTalk;

	public override bool IsUsable
	{
		get
		{
			bool flag = false;
			AIController aIController = GameUtilities.FindActiveAIController(base.gameObject);
			if (aIController != null)
			{
				flag = aIController.IsBusy;
			}
			if (!GameState.InCombat && base.IsVisible)
			{
				return !flag;
			}
			return false;
		}
	}

	public override float UsableRadius => UseRadius;

	public override float ArrivalRadius => ArrivalDistance;

	protected override void Start()
	{
		base.Start();
		ConversationFile.Start();
		base.gameObject.layer = LayerUtility.FindLayerValue("Character");
		ActiveConversation = null;
		if (Animation == UseAnimation.High || Animation == UseAnimation.Low)
		{
			Animation = UseAnimation.None;
		}
	}

	protected override void OnDestroy()
	{
		MyDestroy();
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnApplicationQuit()
	{
		MyDestroy();
	}

	private void MyDestroy()
	{
		if (ActiveConversation != null && ConversationManager.Instance != null)
		{
			if (!ActiveConversation.Completed)
			{
				ConversationManager.Instance.EndConversation(ActiveConversation);
			}
			ActiveConversation = null;
		}
	}

	public override bool Use(GameObject user)
	{
		if (!IsUsable || ActiveConversation != null)
		{
			return false;
		}
		if ((from uibs in UIBarkstringManager.Instance.GetActiveBarks()
			where uibs.GetSpeaker() == base.gameObject
			select uibs).Any())
		{
			if (!Interrupts)
			{
				return false;
			}
			IList<UIBarkString> activeBarks = UIBarkstringManager.Instance.GetActiveBarks();
			for (int num = activeBarks.Count - 1; num >= 0; num--)
			{
				activeBarks[num].Kill(instant: true, finishScripts: true);
			}
		}
		FireUseAudio();
		StartConversation(user);
		return true;
	}

	private void Update()
	{
		if (ActiveConversation != null)
		{
			if (ConversationManager.Instance.IsConversationActive(ActiveConversation))
			{
				return;
			}
			ActiveConversation = null;
		}
		PartyMemberAI component = GetComponent<PartyMemberAI>();
		if (!(component == null) && component.IsInSlot)
		{
			return;
		}
		if (IsUsable && GameCursor.ObjectUnderCursor == base.gameObject && !GameCursor.ActiveCursorIsTargeting)
		{
			if (GameInput.GetControlUp(MappedControl.INTERACT))
			{
				HandleMouseUp();
				GameInput.HandleAllClicks();
			}
		}
		else if (!IsUsable && GameCursor.ObjectUnderCursor == base.gameObject && GameInput.GetControlUp(MappedControl.INTERACT) && GetComponent<Faction>().RelationshipToPlayer != Faction.Relationship.Hostile)
		{
			Console.AddMessage(GUIUtils.GetTextWithLinks(1759), Color.white);
		}
	}

	private void StartConversation(GameObject user)
	{
		if (ActiveConversation == null)
		{
			ActiveConversation = ConversationManager.Instance.StartConversation(ConversationFile.Filename, base.gameObject, FlowChartPlayer.DisplayMode.Standard);
			if (FaceUser)
			{
				SpeakerFaceUser(ActiveConversation, base.gameObject, user);
			}
		}
	}

	public static void SpeakerFaceUser(FlowChartPlayer convo, GameObject speaker, GameObject user)
	{
		AIController aIController = GameUtilities.FindActiveAIController(speaker);
		if ((bool)aIController && (bool)user)
		{
			Vector3 forwardFacing = user.transform.position - aIController.transform.position;
			forwardFacing.y = 0f;
			forwardFacing.Normalize();
			bool flag = convo.GetCurrentNode() is DialogueNode dialogueNode && dialogueNode.DisplayType == DisplayType.Bark;
			Wait wait = AIStateManager.StatePool.Allocate<Wait>();
			wait.ForwardFacing = forwardFacing;
			wait.FlagTurnWhilePaused = !flag;
			wait.Duration = (flag ? convo.Timer : 1f);
			aIController.StateManager.PushState(wait);
		}
	}

	private void HandleMouseUp()
	{
		if (IsUsable && ConversationFile.Filename != null)
		{
			GameInput.HandleAllClicks();
			GameState.s_playerCharacter.ObjectClicked(this);
		}
	}
}
