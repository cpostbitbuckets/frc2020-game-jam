using Godot;
using System;

[Tool]
public class CircleLabel : VBoxContainer
{
    [Export]
    public string Label
    {
        get => label;
        set
        {
            label = value;
            GetNode<Label>("Label").Text = value;
        }
    }
    string label = "Day";

    [Export]
    public string Value
    {
        get => value;
        set
        {
            this.value = value;
            GetNode<Label>("HBoxContainer/CenterContainer/Value").Text = value;
        }
    }
    string value = "0";

    [Export]
    public Color Color
    {
        get => color;
        set
        {
            this.color = value;
            Modulate = value;
        }
    }
    Color color = Colors.White;

    public override void _Ready()
    {

    }

}
