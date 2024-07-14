using Godot;
using System;

public partial class ChogData : Node
{
	public Inventory PlayerInventory { get; set; } = new Inventory();

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
}
