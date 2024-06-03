using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class UI : Control
{
	private const string XML_ELEMENT_DIALOG = "Dialog";
	private const string XML_ELEMENT_OPTION = "Option";
	private const string DIALOG_FOLDER = "res://Scenes/Dialog/";
	private const string INTERACTION_LABEL = "InteractionHContainer/Label";
	private const string CHATBOX_CONTAINER = "ChatBoxVContainer";
	private const string CHATBOX_ITEMLIST = CHATBOX_CONTAINER + "/" + "ItemList";
	private const string CHATBOX_LABEL = CHATBOX_CONTAINER + "/" + "Label";
	private const string XML_SUFFIX = ".xml";
	private const string BACK_OPTION = "Back";
	private const bool HIDE = false;
	private const bool SHOW = true;
	
	//TODO - Don't like the name of this change it.
	private InteractionType IType = InteractionType.None;
	
	// Dialog things
	private int [] DialogDepth = {};
	private List<Dialog> CurrentDialog;
	
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
	
	private void OnInteractionEnter()
	{
		ChangeControlNodeVisibility(INTERACTION_LABEL, SHOW);
	}
	
	private void OnInteractionLeave()
	{
		ChangeControlNodeVisibility(INTERACTION_LABEL, HIDE);
		ChangeControlNodeVisibility(CHATBOX_CONTAINER, HIDE);
	}
		
	private void OnInteraction(string InteractionDescription)
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
		ItemList chatList = GetNode<ItemList>("ChatBoxVContainer/ItemList");
		foreach(Dialog dialog in CurrentDialog)
		{	
			// Weird fix but for some painful reason int[] that have the same contents won't evaluate as equal.
			string DialogDepthText = PrintArray(DialogDepth);
			if((PrintArray(dialog.Depth.Take(DialogDepth.Length).ToArray()) == DialogDepthText
				&& dialog.Depth.Length == DialogDepth.Length + 1)
				|| (DialogDepth.Length == 0
				&& dialog.Depth.Length == 1))
			{
				GD.Print(dialog.Text);
				GD.Print(PrintArray(DialogDepth));
				GD.Print(PrintArray(dialog.Depth.Take(DialogDepth.Length).ToArray()));
				chatList.AddItem(dialog.Text);
			}
		}
		if (DialogDepth.Length > 0)
		{
			chatList.AddItem(BACK_OPTION);
		}
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
		ItemList chatList = GetNode<ItemList>("ChatBoxVContainer/ItemList");
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
				GD.Print(dialog.Type);
				if(dialog.Type == DialogType.Exit)
				{
					ChangeControlNodeVisibility(CHATBOX_CONTAINER, HIDE);
					DialogDepth = new int []{};
					return;
				}
				DialogDepth = dialog.Depth;
				Label chatBoxLabel = GetNode<Label>(CHATBOX_LABEL);
				chatBoxLabel.Text = dialog.Response;
				break;
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
						if(parser.GetAttributeCount() == 3)
						{
							DialogType type;
							if (!Enum.TryParse<DialogType>(parser.GetAttributeValue(2), out type))
							{
								GD.Print("Something went wrong while parsing DialogType");
							}

							dialogOptions.Add(new Dialog(array, text, response, type));
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


