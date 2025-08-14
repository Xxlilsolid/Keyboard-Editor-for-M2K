using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;


public partial class Main : Node2D
{
	public override void _Ready()
	{
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

		var keyboardCanvasLayer = GetNode<CanvasLayer>("KeyboardButtons");
		Godot.Collections.Dictionary keymap = (Godot.Collections.Dictionary)fileChecker.Call("ReadKeymap", "./keymap.json");
		foreach (var value in keymap)
		{
			var node = GetNode<RichTextLabel>("KeyboardButtons/Button" + value.Key + "/AssignedKeyLabel");
			node.Text = (string)value.Value;
		}

	}
	public override void _Process(double delta)
	{
	}
}
