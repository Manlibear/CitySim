using Godot;

namespace CitySim.Data.Characters;

// Describes the frame layout shared by all character spritesheets.
// Create one .tres of this in the editor, fill in your animation rows/columns,
// then assign it to PersonPresenter. Every layer variant just needs a matching PNG.
[GlobalClass]
public partial class CharacterSpriteLayout : Resource
{
    [Export] public Vector2I FrameSize { get; set; } = new(64, 64);
    [Export] public CharacterAnimDef[] Animations { get; set; } = [];
}
