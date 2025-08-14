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
	public Godot.Collections.Dictionary<string, string> ReadSettings(string settingslocation)
	{
		string jsonString = File.ReadAllText(settingslocation);
		JsonNode settingsNode = JsonNode.Parse(jsonString);
		var csdictionary = new System.Collections.Generic.Dictionary<string, JsonNode>();
		var dictionary = new Godot.Collections.Dictionary<string, string>();

		foreach (var value in settingsNode.AsObject())
		{
			csdictionary.Add(value.Key, value.Value);
		}

		foreach (var value in csdictionary)
		{
			dictionary.Add(value.Key, value.Value.ToString());
		}
		return dictionary;
	}
	public Godot.Collections.Dictionary<string, string> ReadKeymap(string keymaplocation)
	{
		string jsonString = File.ReadAllText(keymaplocation);
		JsonNode settingsNode = JsonNode.Parse(jsonString);
		var csdictionary = new System.Collections.Generic.Dictionary<string, string>();
		var dictionary = new Godot.Collections.Dictionary<string, string>();
		foreach (var value in settingsNode["keymap"].AsObject())
		{
			csdictionary.Add(value.Key, (string)value.Value);
		}
		foreach (var value in csdictionary)
		{
			dictionary.Add(value.Key, value.Value);
		}
		return dictionary;
	}
}
