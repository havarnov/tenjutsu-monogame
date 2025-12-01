using Microsoft.Xna.Framework;

namespace TenJutsu;

internal abstract class Entity
{
    public abstract Rectangle? HitBox { get; }

    protected float Depth => HitBox?.Bottom / 1000f ?? 1f;

    public abstract void Update(GameTime gameTime);

    public abstract void Draw(GameTime gameTime);
}