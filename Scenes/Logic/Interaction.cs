using Godot;
using System;

public partial class Interaction : Area2D
{
	
	private const string INTERACTION = "interact";
	
	[Export]
	public string Name { get; set; } = String.Empty;
	
	[Export]
	public string Description { get; set; } = String.Empty;
		
	[Signal]
	public delegate void OnEnterEventHandler(string InteractionName);

	[Signal]
	public delegate void OnLeaveEventHandler();
	
	[Signal]
	public delegate void OnInteractionEventHandler(string InteractionDescription);
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed(INTERACTION))
		{
			EmitSignal(SignalName.OnInteraction, Description);
		}
	}
	
	private void OnBodyEntered(Node2D body)
	{
		EmitSignal(SignalName.OnEnter, Name);	
	}
	
	private void OnBodyExited(Node2D body)
	{
		EmitSignal(SignalName.OnLeave);
	}
}
