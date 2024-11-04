using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class GUI : Control
{
	private const string XML_ELEMENT_DIALOG = "Dialog";
	private const string XML_ELEMENT_OPTION = "Option";
	private const string SCENE_FOLDER = "res://Scenes/";
	private const string DIALOG_FOLDER = SCENE_FOLDER + "Dialog/";
	private const string INTERACTION_LABEL = "InteractionHContainer/Label";
	private const string CHATBOX_CONTAINER = "ChatBoxVContainer";
	private const string CHATBOX_ITEMLIST = CHATBOX_CONTAINER + "/" + "ItemList";
	private const string DIALOG_SCROLL_DELAY_TIMER = CHATBOX_CONTAINER + "/" + "DialogScrollDelay";
	private const string CHATBOX_LABEL = CHATBOX_CONTAINER + "/" + "Label";
	private const bool HIDE = false;
	private const bool SHOW = true;
	
	// These options need to be in some strongly typed folder for language options.
	private const string INTERACTION_LABEL_TEXT = "Press F to {0}.";
	private const string BACK_OPTION = "Back";
	
	// Dialog things
	private int [] DialogDepth = {};
	private List<Dialog> CurrentDialog;
	private string ExpectedDialogText = "";
	private string CurrentDialogText = "";
	
	[Signal]
	public delegate void OnInteractionEventEventHandler(string Event);
	
	public override void _Ready()
	{
		Show();
		ChangeControlNodeVisibility(CHATBOX_CONTAINER, HIDE);
		ChangeControlNodeVisibility(INTERACTION_LABEL, HIDE);
	}

	public override void _Process(double delta)
	{
	}
	
	private void OnDialogScrollDelayTimeout()
	{
		if (CurrentDialogText.Length != ExpectedDialogText.Length)
		{
			CurrentDialogText += ExpectedDialogText[CurrentDialogText.Length];
			Label chatBoxLabel = GetNode<Label>(CHATBOX_LABEL);
			chatBoxLabel.Text = CurrentDialogText;
		}
		else
		{
			CurrentDialogText = string.Empty;
			ExpectedDialogText = string.Empty;
			GetNode<Timer>(DIALOG_SCROLL_DELAY_TIMER).Stop();
		}
	}
	
	public void OnInteractionEnter(string interactionName)
	{
		Label label = GetNode<Label>(INTERACTION_LABEL);
		label.Text = string.Format(INTERACTION_LABEL_TEXT, interactionName);
		label.Show();
	}
	
	public void OnInteractionLeave()
	{
		ChangeControlNodeVisibility(INTERACTION_LABEL, HIDE);
		ChangeControlNodeVisibility(CHATBOX_CONTAINER, HIDE);
		ResetDialog();
	}
		
	public void OnInteraction(InteractionType type, string interactionDescription)
	{
		ChangeControlNodeVisibility(INTERACTION_LABEL, HIDE);
		switch (type)
		{
			case (InteractionType.Dialog):
			{
				ChangeControlNodeVisibility(CHATBOX_CONTAINER, SHOW);
				ParseDialogXml(interactionDescription);
				DisplayDialogOptions();
				break;
			}
			case (InteractionType.Entrance):
			{
				SceneHelper.TransitionScene(this, interactionDescription);
				break;
			}
			case (InteractionType.Item):
			{
				int itemCode = interactionDescription.ToInt();
				GetNode<ChogData>(GlobalStrings.ChogDataLocation).PlayerInventory.AddItem(itemCode);
				break;
			}
		}
	}
	
	private void DisplayDialogOptions()
	{
		ItemList chatList = GetNode<ItemList>(CHATBOX_ITEMLIST);
		foreach(Dialog dialog in CurrentDialog.Where(x => IsDialogNext(x)))
		{	
			// Weird fix but for some painful reason int[] that have the same contents won't evaluate as equal.
			chatList.AddItem(dialog.Text);
		}
		if (DialogDepth.Length > 0)
		{
			chatList.AddItem(BACK_OPTION);
		}
	}
	
	private bool IsDialogNext(Dialog dialog)
	{
		return (PrintArray(dialog.Depth.Take(DialogDepth.Length).ToArray()) == PrintArray(DialogDepth)
			   && dialog.Depth.Length == DialogDepth.Length + 1)
			   || (DialogDepth.Length == 0
			   && dialog.Depth.Length == 1);
	}
	
	private void ChangeControlNodeVisibility(string nodeName, bool visible)
	{
		Control node = GetNode<Control>(nodeName);
		node.Visible = visible;
	}
	
	private void OnItemListSelected(long index)
	{
		ItemList chatList = GetNode<ItemList>(CHATBOX_ITEMLIST);
		if(chatList.GetItemText((int)index) == BACK_OPTION)
		{
			DialogDepth = DialogDepth.ToList().Take(DialogDepth.Length - 1).ToArray();
			chatList.Clear();
			DisplayDialogOptions();	
			return;
		}
		
		foreach(Dialog dialog in CurrentDialog.Where(x => IsDialogNext(x)))
		{
			if(dialog.Text == chatList.GetItemText((int)index))
			{
				switch(dialog.Type) 
				{
					case (DialogType.Exit):
					{
						ResetDialog();
						return;
					}
					case (DialogType.Event):
					{
						DialogEvent dialogEvent = (DialogEvent)dialog;
						EmitSignal(SignalName.OnInteractionEvent, dialogEvent.EventData);
						ResetDialog();
						return;
					}
					case (DialogType.Transition):
					{
						DialogEvent dialogEvent = (DialogEvent)dialog;
						SceneHelper.TransitionScene(this, dialogEvent.EventData);
						ResetDialog();
						return;
					}
					case (DialogType.Challenge):
					{
						
						return;
					}
				}

				// We need to break out of the loop here.
				if(dialog.Type == DialogType.None)
				{
					GetNode<Label>(CHATBOX_LABEL).Text = string.Empty;
					CurrentDialogText = string.Empty;
					DialogDepth = dialog.Depth;
					ExpectedDialogText = dialog.Response;
					GetNode<Timer>(DIALOG_SCROLL_DELAY_TIMER).Start();
					break;
				}
			}	
		}
		chatList.Clear();
		DisplayDialogOptions();
	}
	
	private void ParseDialogXml(string interactionDescription)
	{
		XmlParser parser = new XmlParser();
		parser.Open(DIALOG_FOLDER + interactionDescription + GlobalStrings.XmlSuffix);
		
		Stack dialogLocations = new Stack();
		List<Dialog> dialogOptions = new List<Dialog>();
		int currentY = 0;
		
		while(parser.Read() != Error.FileEof)
		{
			switch(parser.GetNodeType())
			{
				case (XmlParser.NodeType.Element):
				{
					switch (parser.GetNodeName())
					{
						case (XML_ELEMENT_DIALOG):
						{
							currentY = 0;
							dialogLocations.Push(currentY);
							break;
						}
						case (XML_ELEMENT_OPTION):
						{
							currentY++;
							dialogLocations.Pop();
							dialogLocations.Push(currentY);

							int [] newDialogLocations = dialogLocations.ToArray().Select(x => (int)x).ToArray();
							Array.Reverse(newDialogLocations);

							string text = parser.GetAttributeValue(0);
							string response = parser.GetAttributeValue(1);

							if(parser.GetAttributeCount() >= 3)
							{
								if (!Enum.TryParse<DialogType>(parser.GetAttributeValue(2), out DialogType type))
								{
									GD.Print("Something went wrong while parsing DialogType");
								}

								if (type == DialogType.Event || type == DialogType.Transition)
								{
									string eventText = parser.GetAttributeValue(3);
									dialogOptions.Add(new DialogEvent(newDialogLocations, text, response, type, eventText));
								}
								else
								{
									dialogOptions.Add(new Dialog(newDialogLocations, text, response, type));
								}
							}
							else
							{
								dialogOptions.Add(new Dialog(newDialogLocations, text, response));
							}
							break;
						}
					}
					break;
				}
				case(XmlParser.NodeType.ElementEnd):
				{
					if(parser.GetNodeName() == XML_ELEMENT_DIALOG
				   	   && dialogLocations.Count > 0)
					{
						dialogLocations.Pop();

						// Checking again to make sure the stack isn't empty after pop.
						if(dialogLocations.Count > 0)
							currentY = (int)dialogLocations.Peek();		
					}
					break;
				}
			}
		}
		CurrentDialog = dialogOptions;
	}

	private void ResetDialog()
	{
		ChangeControlNodeVisibility(CHATBOX_CONTAINER, HIDE);
		GetNode<ItemList>(CHATBOX_ITEMLIST).Clear();
		DialogDepth = Array.Empty<int>();
		CurrentDialogText = string.Empty;
		ExpectedDialogText = string.Empty;
		GetNode<Label>(CHATBOX_LABEL).Text = string.Empty;
	}

	private string PrintArray(int [] array)
	{
		string [] stringArray = array.Select(x => x.ToString()).ToArray();
		return("[" + String.Join(", ", stringArray) + "]");
	}
}
