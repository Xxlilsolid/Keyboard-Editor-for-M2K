using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;

public partial class FileChecker : Node
{
	JsonSerializerOptions options = new JsonSerializerOptions
	{
		WriteIndented = true
	};
	public Godot.Collections.Dictionary<string, string> ReadSettings(string settingslocation)
	{
		string jsonString = File.ReadAllText(settingslocation);
		JsonNode settingsNode = JsonNode.Parse(jsonString);
		var dictionary = new Godot.Collections.Dictionary<string, string>();

		foreach (var value in settingsNode.AsObject())
		{
			dictionary.Add(value.Key, value.Value.ToString());
		}
		return dictionary;
	}
	public Godot.Collections.Dictionary<string, string> ReadKeymap(string keymaplocation)
	{
		string jsonString = File.ReadAllText(keymaplocation);
		JsonNode settingsNode = JsonNode.Parse(jsonString);
		var dictionary = new Godot.Collections.Dictionary<string, string>();

		foreach (var value in settingsNode["keymap"].AsObject())
		{
			dictionary.Add(value.Key, value.Value.ToString());
		}
		return dictionary;
	}

	public void OverwriteKeymap(Godot.Collections.Dictionary incomingdictionary)
	{
		JsonObject jsonFile = new JsonObject
		{ 
			["keymap"] = new JsonObject{}
		};

		JsonObject innerdict = (JsonObject)jsonFile["keymap"];
		foreach (KeyValuePair<Variant, Variant> value in incomingdictionary)
		{
			innerdict.Add((string)value.Key, (string)value.Value);
		}
		File.WriteAllText("keymap.json", JsonSerializer.Serialize(jsonFile, options));
	}
}
