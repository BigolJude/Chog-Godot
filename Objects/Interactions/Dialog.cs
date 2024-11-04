using System;

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

// Dialog specifically for events eg. Moving things.
public class DialogEvent : Dialog
{
	public string EventData { get; }
	
	public DialogEvent(int [] mDepth, string mText, string mResponse, DialogType mType, string mEventData) : base(mDepth, mText, mResponse, mType)
	{
		EventData = mEventData;
	}
}

// Used for events within dialog eg. Exiting or Triggering Events
public enum DialogType
{
	None,
	Exit,
	Event,
	Transition,
	Challenge
}
