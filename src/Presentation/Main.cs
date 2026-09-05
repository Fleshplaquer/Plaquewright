using Godot;

namespace IdleGodot.Presentation;

public partial class Main : Control
{
	public override void _Ready()
	{
		GD.Print("Idle Godot started.");
	}
}