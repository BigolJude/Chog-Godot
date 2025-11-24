using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
public class DialogTree {
	private struct Folder {
		internal const string SCENE = "res://Scenes/";
		internal const string DIALOG = SCENE + "Dialog/";
	}

    private struct Element {
		internal const string DIALOG = "Dialog";
        internal const string ANSWER = "Answer";
		internal const string CHALLENGE = "Challenge";
		internal const int EVENT_ATTRIBUTE_COUNT = 3;
        internal struct Challenge
		{
			internal const string INCORRECT = "Incorrect";
			internal const string CORRECT = "Correct";
		}
		internal struct DialogIndex
		{
			internal const int TEXT = 0;
			internal const int RESPONSE = 1;
			internal const int TYPE = 2;
			internal const int DATA = 3;
		}
	}

    public int [] Depth { get; private set; } = {};
	public Dialog CurrentDialog { get; private set; }
	private readonly List<Dialog> DialogOptions;
	public List<Dialog> CurrentDialogOptions { get; private set; }
	public string ExpectedDialogText { get; private set; } = "";
	public string CurrentDialogText { get; private set; } = "";

    public DialogTree(string DialogLocation) {
        this.DialogOptions = ParseDialogXml(DialogLocation);
        this.GetNextDialogOptions();
    }

    private List<Dialog> ParseDialogXml(string interactionDescription)
	{

		XmlParser parser = new();
		parser.Open(Folder.DIALOG + interactionDescription + GlobalStrings.XmlSuffix);
		
		Stack dialogLocations = new();
		List<Dialog> dialogOptions = new();
		int currentDialogIndex = 0;
		
		while(parser.Read() != Error.FileEof)
		{
			switch (parser.GetNodeType())
			{
				case XmlParser.NodeType.Element:
				{
					if (!parser.GetNodeName().Equals(Element.DIALOG))
						break;

					currentDialogIndex = 0;

					// IsEmpty() checks if the XML value is self-terminating.
					if (parser.IsEmpty() && dialogLocations.Count > 0)
					{
						currentDialogIndex = (int)dialogLocations.Peek();
						currentDialogIndex++;
						dialogLocations.Pop();	
					}

					dialogLocations.Push(currentDialogIndex);
					int[] newDialogLocations = dialogLocations.ToArray()
															  .Select(x => (int)x)
															  .ToArray();
					Array.Reverse(newDialogLocations);


					AddDialog(parser, dialogOptions, newDialogLocations);
					break;
				}
				case XmlParser.NodeType.ElementEnd:
				{
					if (dialogLocations.Count > 0)
					{	
						dialogLocations.Pop();

						// Checking again to make sure the stack isn't empty after pop.
						if (dialogLocations.Count > 0)
						{
							currentDialogIndex = (int)dialogLocations.Peek();
							currentDialogIndex++;
						}
					}
					break;
				}
			}
		}
		return dialogOptions;
	}

	private void AddDialog(XmlParser parser, List<Dialog> dialogOptions, int[] newDialogLocations)
	{
		string text = parser.GetAttributeValue(Element.DialogIndex.TEXT);
		string response = parser.GetAttributeValue(Element.DialogIndex.RESPONSE);

		GD.Print(text +" - " + PrintArray(newDialogLocations));

		if (parser.GetAttributeCount() >= Element.EVENT_ATTRIBUTE_COUNT)
		{
			if (!Enum.TryParse<DialogType>(parser.GetAttributeValue(Element.DialogIndex.TYPE), ignoreCase: true, out DialogType type))
				GD.Print("Something went wrong while parsing DialogType");

			GD.Print(type);

			if (type == DialogType.Event || type == DialogType.Transition || type == DialogType.Challenge)
				dialogOptions.Add(new DialogEvent(newDialogLocations, text, response, type, parser.GetAttributeValue(Element.DialogIndex.DATA)));
			else
				dialogOptions.Add(new Dialog(newDialogLocations, text, response, type));
		}
		else
			dialogOptions.Add(new Dialog(newDialogLocations, text, response));
	}

    private bool IsDialogNext(Dialog dialog)
		=> (PrintArray(dialog.Depth.Take(Depth.Length).ToArray()) == PrintArray(Depth)
			   && dialog.Depth.Length == Depth.Length + 1)
			   || (Depth.Length == 0
			   && dialog.Depth.Length == 1);

    private string PrintArray(int[] array)
	{
		string[] stringArray = array.Select(x => x.ToString()).ToArray();
		return "[" + String.Join(", ", stringArray) + "]";
	}

	public void SetCurrentDialog(string selectedText)
		=> CurrentDialog = CurrentDialogOptions.Find(x => x.Text == selectedText);

    public void GetNextDialogOptions()
		=> CurrentDialogOptions = DialogOptions.Where(x => IsDialogNext(x)).ToList();

	public void Reset() {
		ResetCurrentText();
		Depth = Array.Empty<int>();
	}

    public void ResetCurrentText() {
        CurrentDialogText = string.Empty;
		ExpectedDialogText = string.Empty;
    }

    public void Forwards(Dialog Dialog) {
        CurrentDialogText = string.Empty;
		Depth = Dialog.Depth;
		GD.Print(PrintArray(Depth));
        GetNextDialogOptions();
		ExpectedDialogText = Dialog.Response;
    }

    public void Backwards() {
        Depth = Depth.SkipLast(1).ToArray();
        GetNextDialogOptions();
    }

    public bool DialogDisplayed()
		=> CurrentDialogText.Length != ExpectedDialogText.Length;

    public void ScrollDialogText()
		=> CurrentDialogText += ExpectedDialogText[CurrentDialogText.Length];
	
}