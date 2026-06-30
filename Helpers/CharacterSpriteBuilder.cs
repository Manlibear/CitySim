using Godot;
using CitySim.Data.Characters;

namespace CitySim.Helpers;

public static class CharacterSpriteBuilder
{
    public static SpriteFrames Build(Texture2D texture, CharacterSpriteLayout layout)
    {
        var frames = new SpriteFrames();
        frames.RemoveAnimation("default");

        foreach (var def in layout.Animations)
        {
            frames.AddAnimation(def.AnimationName);
            frames.SetAnimationLoopMode(def.AnimationName, def.Loop ? SpriteFrames.LoopMode.Linear : SpriteFrames.LoopMode.None);
            frames.SetAnimationSpeed(def.AnimationName, def.Fps);

            for (var i = 0; i < def.FrameCount; i++)
            {
                frames.AddFrame(def.AnimationName, new AtlasTexture
                {
                    Atlas = texture,
                    Region = new Rect2(
                        (def.StartColumn + i) * layout.FrameSize.X,
                        def.Row * layout.FrameSize.Y,
                        layout.FrameSize.X,
                        layout.FrameSize.Y
                    )
                });
            }
        }

        return frames;
    }
}
