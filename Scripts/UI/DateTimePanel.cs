using CitySim.Helpers;
using Godot;
using System;

namespace CitySim.Scripts.UI;

public partial class DateTimePanel : Panel
{
	public required Label? _label {get;set;}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_label = GetNode<Label>("Label");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_label!.Text = SimWorld.Instance.DateTime.ToOrdinal();
	}
}
