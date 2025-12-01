using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace TenJutsu;

internal class NineSliceSprite(TextureRegion region, string name, float depth)
{
    public void Draw(SpriteBatch spriteBatch, Rectangle destination)
    {
        var slice = region.GetSlice(name) as NinePatchSlice;
        if (slice is null)
        {
            throw new ArgumentException(nameof(region));
        }

        var source9 = PatchUtil.Create(slice.Bounds, slice.CenterBounds);
        var destination9 = PatchUtil.Create(destination,
            new Rectangle(
                source9[0].Width,
                source9[0].Height,
                destination.Width - source9[0].Width - source9[2].Width,
                destination.Height - source9[0].Height - source9[6].Height));

        foreach (var (src, dest) in source9.Zip(destination9))
        {
            spriteBatch.Draw(
                region.Texture,
                dest,
                src,
                Color.White,
                0,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth: depth);
        }
    }
}