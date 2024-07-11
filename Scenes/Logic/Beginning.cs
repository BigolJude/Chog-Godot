using Godot;
using System;

public partial class Beginning : Node2D
{
	private const string GUI_LOCATION = "GUI"; 
	private const string BEGINNING_DIALOG = "Beginning";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<GUI>(GUI_LOCATION).OnInteraction(InteractionType.Dialog, BEGINNING_DIALOG);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
