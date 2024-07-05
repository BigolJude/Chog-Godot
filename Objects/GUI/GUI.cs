using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class GUI : Control
{
	private const string XML_ELEMENT_DIALOG = "Dialog";
	private const string XML_ELEMENT_OPTION = "Option";
	private const string DIALOG_FOLDER = "res://Scenes/Dialog/";
	private const string INTERACTION_LABEL = "InteractionHContainer/Label";
	private const string CHATBOX_CONTAINER = "ChatBoxVContainer";
	private const string CHATBOX_ITEMLIST = CHATBOX_CONTAINER + "/" + "ItemList";
	private const string DIALOG_SCROLL_DELAY_TIMER = CHATBOX_CONTAINER + "/" + "DialogScrollDelay";
	private const string CHATBOX_LABEL = CHATBOX_CONTAINER + "/" + "Label";
	private const string XML_SUFFIX = ".xml";
	private const bool HIDE = false;
	private const bool SHOW = true;
	
	// These options need to be in some strongly typed folder for language options.
	private const string INTERACTION_LABEL_TEXT = "Press F to {0}.";
	private const string BACK_OPTION = "Back";
	
	//TODO - Don't like the name of this change it.
	private InteractionType IType = InteractionType.None;
	
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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
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
	
	public void OnInteractionEnter(string InteractionName)
	{
		Label label = GetNode<Label>(INTERACTION_LABEL);
		label.Text = string.Format(INTERACTION_LABEL_TEXT, InteractionName);
		label.Show();
	}
	
	public void OnInteractionLeave()
	{
		ChangeControlNodeVisibility(INTERACTION_LABEL, HIDE);
		ChangeControlNodeVisibility(CHATBOX_CONTAINER, HIDE);
		ResetDialog();
	}
		
	public void OnInteraction(string InteractionDescription)
	{
		IType = InteractionType.Dialog;
		ChangeControlNodeVisibility(INTERACTION_LABEL, HIDE);
		ChangeControlNodeVisibility(CHATBOX_CONTAINER, SHOW);
		ParseDialogXml(InteractionDescription);
		
		switch (IType)
		{
			case (InteractionType.Dialog):
			{
				DisplayDialogOptions();
				break;
			}
			case (InteractionType.Move):
			{
				break;
			}
		}
	}
	
	private void DisplayDialogOptions()
	{
		ItemList chatList = GetNode<ItemList>(CHATBOX_ITEMLIST);
		foreach(Dialog dialog in CurrentDialog)
		{	
			// Weird fix but for some painful reason int[] that have the same contents won't evaluate as equal.
			if(IsDialogNext(dialog))
			{
				chatList.AddItem(dialog.Text);
			}
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
		if (visible) {
			node.Show();
		}
		else
		{
			node.Hide();
		}
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
		
		foreach(Dialog dialog in CurrentDialog)
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
						ResetDialog();
						EmitSignal(SignalName.OnInteractionEvent, dialogEvent.Event);
						GD.Print("Dialog event triggerred: " + dialogEvent.Event);
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
					GD.Print(ExpectedDialogText);
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
							
							if (type == DialogType.Event)
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
	
	// Okay no longer for debug purposes but I need to look into the 
	private string PrintArray(int [] array)
	{
		string [] stringArray = new string [array.Length];
		for(int i = 0; i < array.Length; i++)
		{
			stringArray[i] = array[i].ToString();
		}
		return("[" + String.Join(", ", stringArray) + "]");
	}
}

public class Dialog
{
	public int [] Depth { get; }
	public string Text { get; }
	public string Response { get; }
	public DialogType Type{ get;}
	
	public Dialog (int [] mDepth, string mText, string mResponse)
	{
		this.Depth = mDepth; 
		this.Text = mText;
		this.Response = mResponse;
		this.Type = DialogType.None;
	}
	
	public Dialog (int [] mDepth, string mText, string mResponse, DialogType mType)
	{
		this.Depth = mDepth; 
		this.Text = mText;
		this.Response = mResponse;
		this.Type = mType;
	}
}

public class DialogEvent : Dialog
{
	public string Event { get; }
	
	public DialogEvent(int [] mDepth, string mText, string mResponse, DialogType mType, string mEvent) : base(mDepth, mText, mResponse, mType)
	{
		Event = mEvent;
	}
}

// Used for events within dialog eg. Exiting or Triggering
public enum DialogType
{
	None = 0,
	Exit = 1,
	Event = 2
}

public enum InteractionType
{
	None = 0,
	Dialog = 1,
	Move = 2
}
