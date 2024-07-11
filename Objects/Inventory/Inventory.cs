using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

public class Inventory
{
	private const string ITEMS_LOCATION = "res://Objects/Inventory/Items/Items.xml";
	private const string XML_ELEMENT_ITEM = "Item";
	public Dictionary<Item, int> Items { get; set; }

	public Inventory(Dictionary<Item, int> mItems)
	{
		this.Items = mItems;
	}

	public Inventory()
	{
		this.Items = new Dictionary<Item, int>();
	}

	public void AddItem(int itemCode)
	{
		XmlParser parser = GetItemXmlParser();
		while(parser.Read() != Error.FileEof)
		{
			if(parser.GetNodeName() ==  XML_ELEMENT_ITEM && parser.GetAttributeValue(0).ToInt() == itemCode)
			{
				Item item = new Item(parser.GetAttributeValue(0).ToInt(), parser.GetAttributeValue(1));
				if(Items.ContainsKey(item))
					Items[item] = Items[item] + 1;
				else
					Items.Add(item, 0);
			}
		}
	}

	public void RemoveItem(int itemCode)
	{
		Item item = Items.Keys.Single(x => x.Id == itemCode);
		if(Items[item] - 1 > 0)
			Items[item] = Items[item] - 1;
		else
			Items.Remove(item);
	}

	private XmlParser GetItemXmlParser()
	{
		XmlParser parser = new XmlParser();
		parser.Open(ITEMS_LOCATION);
		return parser; 
	}
}
