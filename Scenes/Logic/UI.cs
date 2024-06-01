using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class UI : Control
{
	private const string XML_ELEMENT_DIALOG = "Dialog";
	private const string XML_ELEMENT_OPTION = "Option";
	private const string DIALOG_FOLDER = "res://Scenes/Dialog/";
	private const string INTERACTION_LABEL = "InteractionHContainer/Label";
	private const string CHATBOX_CONTAINER = "ChatBoxVContainer";
	private const string CHATBOX_LABEL = CHATBOX_CONTAINER + "/" + "Label";
	private const string XML_SUFFIX = ".xml";
	private const bool HIDE = false;
	private const bool SHOW = true;
	
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
		ChangeControlNodeVisibility(INTERACTION_LABEL, HIDE);
		ChangeControlNodeVisibility(CHATBOX_CONTAINER, SHOW);
		ParseDialogXml(InteractionDescription);
		ItemList chatList = GetNode<ItemList>("ChatBoxVContainer");
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
						dialogLocations.Push(currentY);
						currentY = (int)dialogLocations.Peek();	
						break;
					}
					case (XML_ELEMENT_OPTION):
					{
						currentY++;
						string text = parser.GetAttributeValue(0);
						string response = parser.GetAttributeValue(1);
						
						int [] array = new int [dialogLocations.Count];
						int i = 0;
						foreach(var dialogLocation in dialogLocations)
						{
							array[i] = (int)dialogLocation;
							i++;
						}

						GD.Print();
						dialogOptions.Add(new Dialog(array, text, response));
						break;
					}
				}
			}
			if(parser.GetNodeType() == XmlParser.NodeType.ElementEnd || parser.GetNodeName() == XML_ELEMENT_DIALOG)
			{
				dialogLocations.Pop();
			}
		}
	}
}

public class Dialog
{
	public int [] Depth { get; }
	public string Text { get; }
	public string Response { get; }
	
	public Dialog (int [] mDepth, string mText, string mResponse)
	{
		this.Depth = mDepth; 
		this.Text = mText;
		this.Response = mResponse;
	}
}
