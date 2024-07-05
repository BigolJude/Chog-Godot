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
		GUI gui = GetNode<GUI>("/root/Node2D/GUI");
		Callable onEnter = Callable.From(() => gui.OnInteractionEnter(Name));
		Connect(SignalName.OnEnter, onEnter);
		
		Callable onLeave = Callable.From(() => gui.OnInteractionLeave());
		Connect(SignalName.OnLeave, onLeave);
		
		Callable onInteraction = Callable.From(() => gui.OnInteraction(Description));
		Connect(SignalName.OnInteraction, onInteraction);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed(INTERACTION))
		{
			EmitSignal(SignalName.OnInteraction);
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
