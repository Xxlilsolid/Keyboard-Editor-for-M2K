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
	string keyPressed = null;
	public override void _Ready()
	{
		for (int i = 36; i <= 96; i++)
		{
			Button button = GetNode<Button>("./KeyboardButtons/Button" + i);
			button.Pressed += async () => await on_button_press((string)button.Name);
		}


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

		Godot.Collections.Dictionary keymap = (Godot.Collections.Dictionary)fileChecker.Call("ReadKeymap", "./keymap.json");
		Char[] specialchars = ['!', '@', '$', '%', '^', '*', '('];
		foreach (var value in keymap)
		{
			var node = GetNode<RichTextLabel>("KeyboardButtons/Button" + value.Key + "/AssignedKeyLabel");
			char charValue = char.Parse(value.Value.ToString());
			GD.Print(charValue.GetType());
			GD.Print(charValue);
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
		Dictionary<char, char> specialchars = new Dictionary<char, char>
		{
			['1'] = '!', // Row one
			['2'] = '"',
			['3'] = '£',
			['4'] = '$',
			['5'] = '%',
			['6'] = '^',
			['7'] = '&',
			['8'] = '*',
			['9'] = '(',
			['0'] = ')',
			['-'] = '_',
			['='] = '+',
			['['] = '{', // Row two
			[']'] = '}',
			['\\'] = '|', // Row three
			[';'] = ':', 
			['@'] = '\'',
			['#'] = '~',
			[','] = '<',
			['.'] = '>',
			['/'] = '?'
		};
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && buttonInputLock)
		{
			switch (keyEvent.ShiftPressed)
			{
				case true:
					break;
				case false:
					break;
			}
		}
    }

	private async Task on_button_press(string ButtonName)
	{
		Button button = GetNode<Button>("./KeyboardButtons/" + ButtonName);
		RichTextLabel buttonLabel = GetNode<RichTextLabel>("./KeyboardButtons/" + ButtonName + "/AssignedKeyLabel");
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
		bool decisionMade = false;
		while (buttonInputLock)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		if (decisionMade != true)
		{
			Godot.Collections.Dictionary<string, string> keymapDict = (Godot.Collections.Dictionary<string, string>)GetNode<Node>("FileChecker").Call("ReadKeymap", "./keymap.json");
			buttonLabel.Text = String.Format("[color={0}]{1}", buttonColour, keymapDict[ButtonName.Substring(6, 2)]);
		}
	}
}
