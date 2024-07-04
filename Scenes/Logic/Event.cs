using Godot;
using System;

public partial class Event : Node2D
{
	[Export]
	public string TriggerValue { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void OnEvent(string Event)
	{
		if(Event == TriggerValue)
		{
			Transform = new Transform2D(0, new Vector2(0,0));
		}
	}
}
