using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;
using System.Threading.Tasks;

public partial class Main : Node2D
{
	bool buttonInputLock = false;
	char keyPressed;
	bool decisionMade = false;

	Godot.Collections.Dictionary keymap;
	public override void _Ready()
	{
		for (int i = 36; i <= 96; i++)
		{
			Button button = GetNode<Button>("./Buttons/KeyboardButtons/Button" + i);
			button.Pressed += async () => await on_button_press((string)button.Name);
		}
		Button closeButton = GetNode<Button>("Buttons/MenuButtons/Close");
		Button resetButton = GetNode<Button>("Buttons/MenuButtons/Reset");
		Button applyButton = GetNode<Button>("Buttons/MenuButtons/Apply");
		TextureButton resetDefaultButton = GetNode<TextureButton>("Buttons/MenuButtons/ResetDefaults");
		closeButton.Pressed += () => GetTree().Quit();
		resetButton.Pressed += () => ResetKeymap();
		applyButton.Pressed += () => GetNode<Node>("FileChecker").Call("OverwriteKeymap", keymap);
		resetDefaultButton.Pressed += () => ResetDefaultKeymap();

		var fileChecker = GetNode<Node>("FileChecker");
		var background = GetNode<ColorRect>("Background");
		Godot.Collections.Dictionary settings = (Godot.Collections.Dictionary)fileChecker.Call("ReadSettings", "./settings.json");
		int theme = (int)settings["theme"];
		switch (theme) //0 is light mode, 1 is dark mode, 2 is system default
		{
			case 0:
				background.Color = new Color((float)0.851, (float)0.851, (float)0.851);
				break;
			case 1:
				background.Color = new Color((float)0.322, (float)0.322, (float)0.322);
				break;
			case 2:
				if (System.OperatingSystem.IsLinux())
				{
					Process p = new Process();
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.FileName = "gsettings";
					p.StartInfo.Arguments = "get org.gnome.desktop.interface color-scheme";
					p.Start();
					string result = p.StandardOutput.ReadToEnd();
					if (result.Contains("dark"))
					{
						background.Color = new Color((float)0.322, (float)0.322, (float)0.322);
						break;
					}
					else
					{
						background.Color = new Color((float)0.851, (float)0.851, (float)0.851);
						break;
					}
				}
				else if (System.OperatingSystem.IsWindows())
				{
					string keyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";
					int windowTheme = (int)Registry.CurrentUser.OpenSubKey(keyPath).GetValue("AppsUseLightTheme");

					if (windowTheme == 0)
					{
						background.Color = new Color((float)0.322, (float)0.322, (float)0.322);
						break;
					}
					else
					{
						background.Color = new Color((float)0.851, (float)0.851, (float)0.851);
						break;
					}
				}
				break;
		}

		keymap = (Godot.Collections.Dictionary)fileChecker.Call("ReadKeymap", "./keymap.json");
		Char[] specialchars = ['!', '@', '$', '%', '^', '*', '('];
		foreach (var value in keymap)
		{
			var node = GetNode<RichTextLabel>("Buttons/KeyboardButtons/Button" + value.Key + "/AssignedKeyLabel");
			char charValue = char.Parse(value.Value.ToString());
			switch (Char.IsUpper(charValue) || specialchars.Contains(charValue))
			{
				case true:
					node.Text = "[color=white]" + value.Value;
					break;
				case false:
					node.Text = "[color=black]" + value.Value;
					break;
			}
		}

	}
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		Dictionary<string, char> specialchars = new Dictionary<string, char>
		{
			["Quoteleft"] = '¬', // Row One
			["Key1"] = '!',
			["Key2"] = '"',
			["Key3"] = '£',
			["Key4"] = '$',
			["Key5"] = '%',
			["Key6"] = '^',
			["Key7"] = '&',
			["Key8"] = '*',
			["Key9"] = '(',
			["Key0"] = ')',
			["Minus"] = '_',
			["Equal"] = '+',
			["Bracketleft"] = '{', // Row Two
			["Bracketright"] = '}',
			["Semicolon"] = ':', // Row Three
			["Apostrophe"] = '@',
			["Numbersign"] = '~',
			["Backslash"] = '|', // Row Four
			["Comma"] = '<',
			["Period"] = '>',
			["Slash"] = '?'
		};
		Dictionary<string, char> characters = new Dictionary<string, char>
		{
			["Quoteleft"] = '`', // Row One
			["Key1"] = '1',
			["Key2"] = '2',
			["Key3"] = '3',
			["Key4"] = '4',
			["Key5"] = '5',
			["Key6"] = '6',
			["Key7"] = '7',
			["Key8"] = '8',
			["Key9"] = '9',
			["Key0"] = '0',
			["Minus"] = '-',
			["Equal"] = '=',
			["Bracketleft"] = '[', // Row Two
			["Bracketright"] = ']',
			["Semicolon"] = ';', // Row Three
			["Apostrophe"] = '@',
			["Numbersign"] = '#',
			["Backslash"] = '\\', // Row Four
			["Comma"] = ',',
			["Period"] = '.',
			["Slash"] = '/'
		};
		string[] blacklistedkeys = ["Ctrl", "Alt", "Shift"];
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && buttonInputLock)
		{
			if (blacklistedkeys.Contains(keyEvent.Keycode.ToString()))
			{
				GD.Print(String.Format("Blacklisted key detected: {0}", keyEvent.Keycode.ToString()));
				return;
			}
			switch (keyEvent.ShiftPressed)
			{
				case true:
					if (specialchars.ContainsKey(keyEvent.Keycode.ToString()))
					{
						GD.Print(specialchars[keyEvent.Keycode.ToString()]);
						SendVarSignal(specialchars[keyEvent.Keycode.ToString()]);
						break;
					}
					else
					{
						GD.Print(keyEvent.Keycode.ToString().ToLower());
						SendVarSignal((char)keyEvent.Keycode);
						break;
					}
				case false:
					if (characters.ContainsKey(keyEvent.Keycode.ToString()))
					{
						GD.Print(characters[keyEvent.Keycode.ToString()]);
						SendVarSignal(characters[keyEvent.Keycode.ToString()]);
						break;
					}
					else
					{
						GD.Print(keyEvent.Keycode.ToString().ToLower());
						SendVarSignal((char)keyEvent.Keycode);
						break;
					}
			}
		}
    }

	private void SendVarSignal(char key)
	{
		key = char.ToLower(key);
		keyPressed = key;
		decisionMade = true;
		buttonInputLock = false;
	}
	private void ResetDefaultKeymap()
	{
		Godot.Collections.Dictionary<Variant, Variant> defaultKeymap = new Godot.Collections.Dictionary<Variant, Variant>
		{
			[36] = "1",
			[37] = "!",
			[38] = "2",
			[39] = "@",
			[40] = "3",
			[41] = "4",
			[42] = "$",
			[43] = "5",
			[44] = "%",
			[45] = "6",
			[46] = "^",
			[47] = "7",
			[48] = "8",
			[49] = "*",
			[50] = "9",
			[51] = "(",
			[52] = "0",
			[53] = "q",
			[54] = "Q",
			[55] = "w",
			[56] = "W",
			[57] = "e",
			[58] = "E",
			[59] = "r",
			[60] = "t",
			[61] = "T",
			[62] = "y",
			[63] = "Y",
			[64] = "u",
			[65] = "i",
			[66] = "I",
			[67] = "o",
			[68] = "O",
			[69] = "p",
			[70] = "P",
			[71] = "a",
			[72] = "s",
			[73] = "S",
			[74] = "d",
			[75] = "D",
			[76] = "f",
			[77] = "g",
			[78] = "G",
			[79] = "h",
			[80] = "H",
			[81] = "j",
			[82] = "J",
			[83] = "k",
			[84] = "l",
			[85] = "L",
			[86] = "z",
			[87] = "Z",
			[88] = "x",
			[89] = "c",
			[90] = "C",
			[91] = "v",
			[92] = "V",
			[93] = "b",
			[94] = "B",
			[95] = "n",
			[96] = "m"
		};

		Char[] specialchars = ['!', '@', '$', '%', '^', '*', '('];
		foreach (var value in defaultKeymap)
		{
			var node = GetNode<RichTextLabel>("Buttons/KeyboardButtons/Button" + value.Key + "/AssignedKeyLabel");
			char charValue = char.Parse(value.Value.ToString());
			switch (Char.IsUpper(charValue) || specialchars.Contains(charValue))
			{
				case true:
					node.Text = "[color=white]" + value.Value;
					break;
				case false:
					node.Text = "[color=black]" + value.Value;
					break;
			}
		}
		GD.Print(defaultKeymap);
		GetNode<Node>("FileChecker").Call("OverwriteKeymap", defaultKeymap);
	}
	private void ResetKeymap()
	{
		Node fileChecker = GetNode<Node>("FileChecker");
		keymap = (Godot.Collections.Dictionary)fileChecker.Call("ReadKeymap", "./keymap.json");
		Char[] specialchars = ['!', '@', '$', '%', '^', '*', '('];
		foreach (var value in keymap)
		{
			var node = GetNode<RichTextLabel>("Buttons/KeyboardButtons/Button" + value.Key + "/AssignedKeyLabel");
			char charValue = char.Parse(value.Value.ToString());
			switch (Char.IsUpper(charValue) || specialchars.Contains(charValue))
			{
				case true:
					node.Text = "[color=white]" + value.Value;
					break;
				case false:
					node.Text = "[color=black]" + value.Value;
					break;
			}
		}
	}

	private async Task on_button_press(string ButtonName)
	{
		Button button = GetNode<Button>("./Buttons/KeyboardButtons/" + ButtonName);
		RichTextLabel buttonLabel = GetNode<RichTextLabel>("./Buttons/KeyboardButtons/" + ButtonName + "/AssignedKeyLabel");
		string buttonColour;
		switch (buttonLabel.Text.Contains("[color=black]"))
		{
			case true:
				buttonColour = "black";
				buttonLabel.Text = String.Format("[color={0}]...", buttonColour);
				break;
			case false:
				buttonColour = "white";
				buttonLabel.Text = String.Format("[color={0}]...", buttonColour);
				break;
		}
		buttonInputLock = false;
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		buttonInputLock = true;
		while (buttonInputLock)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		if (decisionMade != true)
		{
			Godot.Collections.Dictionary<string, string> keymapDict = (Godot.Collections.Dictionary<string, string>)GetNode<Node>("FileChecker").Call("ReadKeymap", "./keymap.json");
			buttonLabel.Text = String.Format("[color={0}]{1}", buttonColour, keymapDict[ButtonName.Substring(6, 2)]);
		}
		else
		{
			buttonLabel.Text = String.Format("[color={0}]{1}", buttonColour, keyPressed);
			keymap[ButtonName.Substring(6, 2)] = keyPressed.ToString();
		}
	}
}
