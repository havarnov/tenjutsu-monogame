using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using nkast.Aether.Physics2D.Dynamics;

namespace TenJutsu;

internal class Door : Entity
{
    private SpriteBatch _spriteBatch = null!;
    private NineSliceSprite _nineSliceSprite = null!;
    private Vector2 _initialPosition;
    private readonly Body body;

    private readonly LDtkTypes.Door door;
    private readonly TextureRegion region;

    public Door(LDtkTypes.Door door,
        TextureRegion region,
        World world)
    {
        this.door = door;
        this.region = region;
        _initialPosition = door.Position;
        body = world.CreateBody(
            new nkast.Aether.Physics2D.Common.Vector2(door.Position.X, door.Position.Y)
            + new nkast.Aether.Physics2D.Common.Vector2(door.Size.X / 2, door.Size.Y / 2));
        body.CreateRectangle(door.Size.X, door.Size.Y, 1f, nkast.Aether.Physics2D.Common.Vector2.Zero);
        body.Tag = this;
    }

    public override Rectangle? HitBox => new Rectangle(_initialPosition.ToPoint(), door.Size.ToPoint());

    public void Load(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
        _nineSliceSprite = new NineSliceSprite(region, "door", Depth);
    }

    public override void Update(GameTime gameTime)
    {
    }

    public override void Draw(GameTime gameTime)
    {
        _nineSliceSprite.Draw(
            _spriteBatch,
            new Rectangle(_initialPosition.ToPoint(), door.Size.ToPoint()));
    }
}