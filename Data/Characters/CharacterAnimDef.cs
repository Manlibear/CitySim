using Godot;

namespace CitySim.Data.Characters;

// One animation entry in a CharacterSpriteLayout.
// Describes where on the spritesheet the frames live.
[GlobalClass]
public partial class CharacterAnimDef : Resource
{
    [Export] public string AnimationName { get; set; } = "";
    [Export] public int Row { get; set; }
    [Export] public int StartColumn { get; set; }
    [Export] public int FrameCount { get; set; } = 1;
    [Export] public float Fps { get; set; } = 8f;
    [Export] public bool Loop { get; set; } = true;
}
