using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using nkast.Aether.Physics2D.Dynamics;

namespace TenJutsu;

internal class Destructible : Entity
{
    private SpriteBatch _spriteBatch = null!;

    private float sizeWidth = 0.4f;
    private float sizeHeight = 0.25f;
    private readonly LDtkTypes.Destructible destructible;
    private readonly TextureRegion region;
    private readonly Body body;

    public Destructible(
        LDtkTypes.Destructible destructible,
        TextureRegion region,
        World world)
    {
        this.destructible = destructible;
        this.region = region;
        var position =
            destructible.Position
            - (destructible.Pivot * destructible.Size)
            + new Vector2(
                (int)(destructible.tile.W * (sizeWidth / 2)),
                (int)(destructible.tile.H * (1f - sizeHeight)));
        body = world.CreateBody(new nkast.Aether.Physics2D.Common.Vector2(position.X, position.Y), bodyType: BodyType.Dynamic);
        body.CreateRectangle(destructible.Size.X, destructible.Size.Y, 10f, nkast.Aether.Physics2D.Common.Vector2.Zero);
        body.LinearDamping = 15;
        body.Tag = this;
    }

    public override Rectangle? HitBox => new Rectangle(
        new Point((int)body.Position.X, (int)body.Position.Y)
        - (destructible.Pivot * destructible.Size).ToPoint()
        + new Point(
            (int)(destructible.tile.W * (sizeWidth/2)),
            (int)(destructible.tile.H * (1f - sizeHeight))),
        new Point(
            (int)(destructible.tile.W * (1f - sizeWidth)),
            (int)(destructible.tile.H * sizeHeight)));

    public void Load(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
    }

    public override void Update(GameTime gameTime)
    {
    }

    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.Draw(
            region.Texture,
            new Rectangle(
                new Point((int)body.Position.X, (int)body.Position.Y) - (destructible.Pivot * (destructible.Size/2)).ToPoint(),
                new Point(destructible.tile.W, destructible.tile.H)),
            destructible.tile,
            Color.White,
            rotation: 0,
            origin: Vector2.One,
            effects: SpriteEffects.None,
            layerDepth: Depth);
    }
}