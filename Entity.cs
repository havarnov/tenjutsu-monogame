using Microsoft.Xna.Framework;
using Vector2 = nkast.Aether.Physics2D.Common.Vector2;

namespace TenJutsu;

internal abstract class Entity
{
    public abstract void Update(GameTime gameTime);

    public abstract void Draw(GameTime gameTime);

    public virtual void Hit(Entity hitBy, Vector2 point) {}
}