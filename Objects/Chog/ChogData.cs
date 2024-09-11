using Godot;
using System;
using System.Collections.Generic;

public partial class ChogData : Node
{
	public Inventory PlayerInventory { get; set; } = new Inventory();
	public List<String> PlayerActions {get; set;} = new List<string>();

	public override void _Ready()
	{
	}


	public override void _Process(double delta)
	{
	}
}
