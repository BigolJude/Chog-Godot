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
	private const string XML_SUFFIX = ".xml";
	private const string SCENE_SUFFIX = ".tscn";
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
				PackedScene scene = (PackedScene)ResourceLoader.Load(SCENE_FOLDER + interactionDescription + SCENE_SUFFIX);
				GetTree().ChangeSceneToPacked(scene);
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
		if (visible)
			node.Show();
		else
			node.Hide();
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
				GD.Print(dialog.Text + " - " + dialog.Response);
				GD.Print(PrintArray(DialogDepth));
				GD.Print(PrintArray(dialog.Depth));
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
						ResetDialog();
						EmitSignal(SignalName.OnInteractionEvent, dialogEvent.EventData);
						GD.Print("Dialog event triggerred: " + dialogEvent.EventData);
						return;
					}
					case (DialogType.Transition):
					{
						DialogEvent dialogEvent = (DialogEvent)dialog;
						ChangeScene(dialogEvent.EventData);
						ResetDialog();
						return;
					}
				}
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
		parser.Open(DIALOG_FOLDER + interactionDescription + XML_SUFFIX);
		
		Stack dialogLocations = new Stack();
		List<Dialog> dialogOptions = new List<Dialog>();
		int currentY = 0;
		
		while(parser.Read() != Error.FileEof)
		{
			if(parser.GetNodeType() == XmlParser.NodeType.Element)
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
						string text = parser.GetAttributeValue(0);
						string response = parser.GetAttributeValue(1);
						int [] array = new int [dialogLocations.Count];
						int i = 0;
						
						dialogLocations.Pop();
						dialogLocations.Push(currentY);
						
						foreach(var dialogLocation in dialogLocations)
						{
							array[i] = (int)dialogLocation;
							i++;
						}
						Array.Reverse(array);
						if(parser.GetAttributeCount() >= 3)
						{
							DialogType type;
							if (!Enum.TryParse<DialogType>(parser.GetAttributeValue(2), out type))
							{
								GD.Print("Something went wrong while parsing DialogType");
							}
							
							if (type == DialogType.Event || type == DialogType.Transition)
							{
								string eventText = parser.GetAttributeValue(3);
								dialogOptions.Add(new DialogEvent(array, text, response, type, eventText));
							}
							else
							{
								dialogOptions.Add(new Dialog(array, text, response, type));
							}
						}
						else
						{
							dialogOptions.Add(new Dialog(array, text, response));
						}
						GD.Print(PrintArray(array));
						break;
					}
				}
			}
			if(parser.GetNodeType() == XmlParser.NodeType.ElementEnd 
			   && parser.GetNodeName() == XML_ELEMENT_DIALOG
			   && dialogLocations.Count > 0)
			{
				dialogLocations.Pop();
				if(dialogLocations.Count > 0)
				{
					currentY = (int)dialogLocations.Peek();		
				}
			}
		}
		CurrentDialog = dialogOptions;
	}
	
	private void ResetDialog()
	{
		ChangeControlNodeVisibility(CHATBOX_CONTAINER, HIDE);
		GetNode<ItemList>(CHATBOX_ITEMLIST).Clear();
		DialogDepth = new int []{};
		CurrentDialogText = string.Empty;
		ExpectedDialogText = string.Empty;
		GetNode<Label>(CHATBOX_LABEL).Text = string.Empty;
	}
	
	private string PrintArray(int [] array)
	{
		string [] stringArray = new string [array.Length];
		for(int i = 0; i < array.Length; i++)
		{
			stringArray[i] = array[i].ToString();
		}
		return("[" + String.Join(", ", stringArray) + "]");
	}

	private void ChangeScene(string sceneName)
	{
		PackedScene scene = (PackedScene)ResourceLoader.Load(SCENE_FOLDER + sceneName + SCENE_SUFFIX);
		GetTree().ChangeSceneToPacked(scene);
	}
}
