using Microsoft.Xna.Framework;

namespace TenJutsu;

internal abstract class Entity
{
    public abstract void Update(GameTime gameTime);

    public abstract void Draw(GameTime gameTime);
}