using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class GUI : Control
{
	private struct XMLElement {
		internal const string DIALOG = "Dialog";
		internal const string OPTION = "Option";
		internal const string CHALLENGE = "Challenge";
	}
	private struct ChatBox {
		private const string CHATBOX_PREFIX = "ChatBox_";
		internal const string BACK_OPTION = "Back";
		internal const string CONTAINER = CHATBOX_PREFIX + "Container";
		internal struct Container {
			internal const string ITEMLIST = CONTAINER + "/" + CHATBOX_PREFIX + "ItemList";
			internal const string LABEL = CONTAINER + "/" + CHATBOX_PREFIX + "Label";
			internal const string DIALOG_SCROLL_DELAY_TIMER = CONTAINER + "/" + "DialogScrollDelay";
		}
	}
	private struct Interaction {
		internal const string LABEL = "Interaction_Container/InteractionNotif_Label";
		internal const string LABEL_TEXT = "Press F to {0}.";
	}
	private struct Challenge {
		private const string CHALLENGE_PREFIX = "Challenge_";
		internal const string CONTAINER = CHALLENGE_PREFIX + "Container";
		private struct Container {
			internal const string CANCEL = CONTAINER + "/" + CHALLENGE_PREFIX + "Cancel";
			internal const string SUBMIT = CONTAINER + "/" + CHALLENGE_PREFIX + "Submit";
		}
	}
	private const bool HIDE = false;
	private const bool SHOW = true;
		
	private DialogTree DialogTree = null;
	
	[Signal]
	public delegate void OnInteractionEventEventHandler(string Event);
	
	public override void _Ready()
	{
		Show();
		ChangeControlNodeVisibility(ChatBox.CONTAINER, HIDE);
		ChangeControlNodeVisibility(Interaction.LABEL, HIDE);
		ChangeControlNodeVisibility(Challenge.CONTAINER, HIDE);
	}

	public override void _Process(double delta)
	{
	}
	
	private void OnDialogScrollDelayTimeout()
	{
		if (DialogTree.DialogDisplayed())
		{
			Label chatBoxLabel = GetNode<Label>(ChatBox.Container.LABEL);
			DialogTree.ScrollDialogText();
			chatBoxLabel.Text = DialogTree.CurrentDialogText;
		}
		else
		{
			DialogTree.ResetCurrentText();
			GetNode<Timer>(ChatBox.Container.DIALOG_SCROLL_DELAY_TIMER).Stop();
		}
	}
	
	public void OnInteractionEnter(string interactionName)
	{
		Label label = GetNode<Label>(Interaction.LABEL);
		label.Text = string.Format(Interaction.LABEL_TEXT, interactionName);
		label.Show();
	}
	
	public void OnInteractionLeave()
	{
		ChangeControlNodeVisibility(Interaction.LABEL, HIDE);
		ChangeControlNodeVisibility(ChatBox.CONTAINER, HIDE);
		ResetDialog();
	}
		
	public void OnInteraction(InteractionType type, string interactionDescription)
	{
		ChangeControlNodeVisibility(Interaction.LABEL, HIDE);
		switch (type)
		{
			case InteractionType.Dialog:
			{
				ChangeControlNodeVisibility(ChatBox.CONTAINER, SHOW);
				DialogTree = new DialogTree(interactionDescription);
				DisplayDialogOptions();
				break;
			}
			case InteractionType.Entrance:
			{
				SceneHelper.TransitionScene(this, interactionDescription);
				break;
			}
			case InteractionType.Item:
			{
				int itemCode = interactionDescription.ToInt();
				GetNode<ChogData>(GlobalStrings.ChogDataLocation).PlayerInventory.AddItem(itemCode);
				break;
			}
		}
	}
	
	private void DisplayDialogOptions() {
		ItemList chatList = GetNode<ItemList>(ChatBox.Container.ITEMLIST);
		foreach(Dialog dialog in DialogTree.CurrentDialogOptions)
		{	
			// Weird fix but for some painful reason int[] that have the same contents won't evaluate as equal.
			chatList.AddItem(dialog.Text);
		}
		if (DialogTree.Depth.Length > 0)
		{
			chatList.AddItem(ChatBox.BACK_OPTION);
		}
	}
	
	private void ChangeControlNodeVisibility(string nodeName, bool visible)
	{
		Control node = GetNode<Control>(nodeName);
		node.Visible = visible;
	}
	
	private void OnItemListSelected(long index)
	{
		ItemList chatList = GetNode<ItemList>(ChatBox.Container.ITEMLIST);
		string ItemText = chatList.GetItemText((int)index);
		if(ItemText == ChatBox.BACK_OPTION)
		{
			DialogTree.Backwards();
			chatList.Clear();
			DisplayDialogOptions();	
			return;
		}
		
		Dialog Dialog = DialogTree.CurrentDialogOptions.Find(x => x.Text == ItemText);
		switch(Dialog.Type) 
		{
			case DialogType.Exit:
			{
				GD.Print("Exited");
				ResetDialog();
				return;
			}
			case DialogType.Event:
			{
				GD.Print("Event");
				DialogEvent dialogEvent = (DialogEvent)Dialog;
				EmitSignal(SignalName.OnInteractionEvent, dialogEvent.EventData);
				ResetDialog();
				return;
			}
			case DialogType.Transition:
			{
				GD.Print("Transitions");
				DialogEvent dialogEvent = (DialogEvent)Dialog;
				SceneHelper.TransitionScene(this, dialogEvent.EventData);
				ResetDialog();
				return;
			}
			case DialogType.Challenge:
			{
				GD.Print("Challenge");
				DialogEvent dialogEvent = (DialogEvent)Dialog;
				ChangeControlNodeVisibility(Challenge.CONTAINER, SHOW);
				GD.Print("Showing container");
				EmitSignal(SignalName.OnInteractionEvent, dialogEvent.EventData);	
				break;
			}
			case DialogType.None:
			{
				GetNode<Label>(ChatBox.Container.LABEL).Text = string.Empty;
				GetNode<Timer>(ChatBox.Container.DIALOG_SCROLL_DELAY_TIMER).Start();
				DialogTree.Forwards(Dialog);
				break;
			}
		}
	
		chatList.Clear();
		DisplayDialogOptions();
	}

	private void ResetDialog()
	{
		ChangeControlNodeVisibility(ChatBox.CONTAINER, HIDE);
		GetNode<ItemList>(ChatBox.Container.ITEMLIST).Clear();
		GetNode<Label>(ChatBox.Container.LABEL).Text = string.Empty;
		DialogTree.Reset();
	}

}
