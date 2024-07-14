using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.ComponentModel;

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
		if (Items.Count >= 10)
			return;

		XmlParser parser = GetItemXmlParser();
		while (parser.Read() != Error.FileEof)
		{
			if (parser.GetNodeType() == XmlParser.NodeType.Element
			   && parser.GetNodeName() == XML_ELEMENT_ITEM
			   && parser.GetAttributeValue(0).ToInt() == itemCode)
			{
				KeyValuePair<Item, int> item = Items.SingleOrDefault(x => x.Key.Id == parser.GetAttributeValue(0).ToInt());
				if (item.Equals(default(KeyValuePair<Item,int>)))
				{
					Item inventoryItem = new(parser.GetAttributeValue(0).ToInt(),
											parser.GetAttributeValue(1),
											SceneHelper.LoadTexture2D(parser.GetAttributeValue(2)));
					Items.Add(inventoryItem, 1);
				}
				else
				{
					Items[item.Key] = Items[item.Key] + 1;
				}
			}
		}
		parser.Dispose();
	}

	public void RemoveItem(int itemCode)
	{
		Item item = Items.Keys.Single(x => x.Id == itemCode);
    
		if (Items[item] - 1 > 0)
			Items[item] = Items[item] - 1;
		else
			Items.Remove(item);
	}

	private XmlParser GetItemXmlParser()
	{
		XmlParser parser = new();
		parser.Open(ITEMS_LOCATION);
		return parser;
	}
}
