using Godot;
using Idler.Core;

namespace IdleGodot;

public partial class Main : Node
{
	public override void _Ready()
	{
		GD.Print(CoreInfo.Name);
	}
}