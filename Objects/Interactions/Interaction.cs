using Godot;
using System;
using System.Threading;

public partial class Interaction : Area2D
{
	private const string INTERACTION = "interact";
	private const string GUI_LOCATION = "/root/Node2D/GUI";
	private bool InRange = false;
	
	[Export]
	public string Name { get; set; } = String.Empty;

	[Export]
	public InteractionType Type { get; set; } = InteractionType.None;
	
	[Export]
	public string Description { get; set; } = String.Empty;
		
	[Signal]
	public delegate void OnEnterEventHandler(string interactionName);

	[Signal]
	public delegate void OnLeaveEventHandler();
	
	[Signal]
	public delegate void OnInteractionEventHandler(InteractionType type, string interactionDescription);
		
	// Called when the node enters the scene tree for the first time.
 	public override void _Ready()
	{
		GUI gui = GetNode<GUI>(GUI_LOCATION);
		Callable onEnter = Callable.From(() => gui.OnInteractionEnter(Name));
		Connect(SignalName.OnEnter, onEnter);
		
		Callable onLeave = Callable.From(() => gui.OnInteractionLeave());
		Connect(SignalName.OnLeave, onLeave)	;
		
		Callable onInteraction = Callable.From(() => gui.OnInteraction(Type, Description));
		Connect(SignalName.OnInteraction, onInteraction);

		Callable onBodyEntered = new(this, MethodName.OnBodyEntered);
		Connect(SignalName.BodyEntered, onBodyEntered);

		Callable onBodyExited = new(this, MethodName.OnBodyExited);
		Connect(SignalName.BodyExited, onBodyExited);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed(INTERACTION) && InRange)
		{
			EmitSignal(SignalName.OnInteraction);
			if(Type == InteractionType.Item)
			{
				Hide();
			}
		}
	}
	
	private void OnBodyEntered(Node2D body)
	{
		EmitSignal(SignalName.OnEnter);	
		InRange = true;
	}
	
	private void OnBodyExited(Node2D body)
	{
		EmitSignal(SignalName.OnLeave);
		InRange = false;
	}
}
