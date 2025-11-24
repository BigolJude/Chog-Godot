using System;
using System.Dynamic;
using Godot;

public class Order
{

	public Order()
	{

	}

}

public class Customer
{

	public string name { get; set; } = "";
	public Order order { get; set; }
	public Customer()
	{

	}
}
