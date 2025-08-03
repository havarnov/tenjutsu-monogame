using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using Vector2 = nkast.Aether.Physics2D.Common.Vector2;

namespace TenJutsu;

internal class Body
{
    private readonly nkast.Aether.Physics2D.Dynamics.Body _inner;

    public Body(nkast.Aether.Physics2D.Dynamics.Body inner)
    {
        _inner = inner;
        _inner.OnCollision += (sender, other, contact) =>
        {
            // Console.WriteLine(contact.Restitution);
            // contact.Restitution = 1f;
            Console.WriteLine("wall: " + other.Body.Position + ", hero: " + sender.Body.Position);
            Console.WriteLine(contact.IsTouching);
            return true;
        };
    }

    public void ApplyLinearImpulse(Microsoft.Xna.Framework.Vector2 impulse)
    {
        _inner.ApplyLinearImpulse(new Vector2(impulse.X, impulse.Y));
    }

    public Microsoft.Xna.Framework.Vector2 Position => new Microsoft.Xna.Framework.Vector2(_inner.Position.X, _inner.Position.Y);

    public Microsoft.Xna.Framework.Vector2 Velocity
    {
        get => new Microsoft.Xna.Framework.Vector2(_inner.LinearVelocity.X, _inner.LinearVelocity.Y);
        set => _inner.LinearVelocity = new Vector2(value.X, value.Y);
    }
}

internal interface IBodyFactory
{
    Body Create(Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 size);
}


internal class BodyFactory(World world) : IBodyFactory
{
    public Body Create(Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 size)
    {
        var body = world.CreateBody(
            new Vector2(position.X, position.Y),
            0f,
            BodyType.Dynamic);

        body.CreateRectangle(size.X, size.Y, 1f, Vector2.Zero);
        return new Body(body);
    }
}