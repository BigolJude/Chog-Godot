using Godot;
using System;

public partial class Interaction : Area2D
{
	
	private const string INTERACTION = "interact";
	
	[Export]
	public string Description { get; set; } = String.Empty;
		
	[Signal]
	public delegate void OnEnterEventHandler();

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
			GD.Print("F Pressed");
		}
	}
	
	private void OnBodyEntered(Node2D body)
	{
		EmitSignal(SignalName.OnEnter);	
	}
	
	private void OnBodyExited(Node2D body)
	{
		EmitSignal(SignalName.OnLeave);
	}
}
