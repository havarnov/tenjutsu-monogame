using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using nkast.Aether.Physics2D.Dynamics;

namespace TenJutsu;

internal class Destructible : Entity
{
    private SpriteBatch _spriteBatch = null!;

    private readonly LDtkTypes.Destructible destructible;
    private readonly TextureRegion region;
    private readonly Body body;
    private int health = 10;

    public Destructible(
        LDtkTypes.Destructible destructible,
        TextureRegion region,
        World world)
    {
        this.destructible = destructible;
        this.region = region;
        var position =
            destructible.Position
            + new Vector2(0, -5)
            + new Vector2(0, destructible.yOff);
        body = world.CreateBody(new nkast.Aether.Physics2D.Common.Vector2(position.X, position.Y), bodyType: BodyType.Dynamic);
        body.CreateRectangle(destructible.Size.X, destructible.Size.Y, 15f, offset: nkast.Aether.Physics2D.Common.Vector2.Zero);
        body.LinearDamping = 15;
        body.Tag = this;
    }

    private float LayerDepth => (body.Position.Y + (destructible.Size.Y / 2f)) / 1000f;

    public void Load(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
    }

    public override void Hit(Entity hitBy, nkast.Aether.Physics2D.Common.Vector2 point)
    {
        Console.WriteLine($"Destructible hit by {hitBy.GetType()} at {point}.");

        if (!destructible.playerDestructible)
        {
            return;
        }

        base.Hit(hitBy, point);
        health -= 2;
    }

    public override void Update(GameTime gameTime)
    {
        if (health <= 0)
        {
            body.Enabled = false;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        if (health <= 0)
        {
            return;
        }

        var x = (int)Math.Round(body.Position.X);
        var y = (int)Math.Round(body.Position.Y);
        var sizeX = (int)Math.Round(destructible.Size.X / 2f);
        var sizeY = (int)Math.Round(destructible.Size.Y / 2f);
        _spriteBatch.Draw(
            region.Texture,
            new Rectangle(
                new Point(x - sizeX, y - sizeY) + new Point(1, 0),
                new Point(destructible.tile.W, destructible.tile.H)),
            destructible.tile,
            Color.White,
            rotation: 0,
            origin: Vector2.One,
            effects: SpriteEffects.None,
            layerDepth: LayerDepth);
    }
}