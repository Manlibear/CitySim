using Godot;
using CitySim.Data.Characters;
using CitySim.Helpers;

namespace CitySim.Scripts;

// Attach to each AnimatedSprite2D layer on a character (Body, Hair, Clothing, etc.).
// Set SpriteFolderPath to e.g. "res://Content/Sprites/Characters/Hair" and VariantIndex to the variant number.
// Call Rebuild() with the shared layout to generate SpriteFrames at runtime.
[GlobalClass]
public partial class CharacterLayer : AnimatedSprite2D
{
    [Export] public string SpriteFolderPath { get; set; } = "";
    [Export] public int VariantIndex { get; set; } = 1;

    public void Rebuild(CharacterSpriteLayout layout)
    {
        if (string.IsNullOrEmpty(SpriteFolderPath)) return;

        var path = $"{SpriteFolderPath}/{VariantIndex:D2}.png";
        var texture = GD.Load<Texture2D>(path);
        if (texture == null)
        {
            GD.PushWarning($"CharacterLayer '{Name}': no texture at {path}");
            return;
        }

        SpriteFrames = CharacterSpriteBuilder.Build(texture, layout);
    }
}
