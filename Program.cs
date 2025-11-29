using System;
using System.Collections.Generic;
using System.Linq;
using AsepriteDotNet.Aseprite;
using LDtk;
using LDtkTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;
using TenJutsu;
using Destructible = TenJutsu.Destructible;
using Vector2 = Microsoft.Xna.Framework.Vector2;

using var game = new TenJutsuGame();
game.Run();

public class TenJutsuGame : Game
{
    private LDtkWorld world = null!;
    private SpriteBatch spriteBatch = null!;
    private LDtkLevel currentLevel = null!;
    private Camera camera = null!;
    private float pixelScale = 1f;
    private LdtkRenderer renderer = null!;
    private Hero _hero = null!;
    private readonly List<Entity> entities = [];
    private World physicsWorld = null!;
    private bool debug = false;

    public TenJutsuGame()
    {
        _ = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        pixelScale = Math.Max(GraphicsDevice.Viewport.Height / 160, 1);

        var worldFile = LDtkFile.FromFile("tenjutsu.ldtk");
        world = worldFile.LoadWorld(Worlds.World.Iid)
                ?? throw new InvalidOperationException();

        currentLevel = world.LoadLevel("Entrance");

        physicsWorld = new World
        {
            Gravity = new nkast.Aether.Physics2D.Common.Vector2(0, 0),
        };

        var playerStart = currentLevel.GetEntity<PlayerStart>();
        _hero = new Hero(physicsWorld, playerStart.Position, entities);
        entities.Add(_hero);

        var collisions = currentLevel.GetIntGrid("Collisions");
        for (int i = 0; i < collisions.GridSize.X; i++)
        {
            for (int j = 0; j < collisions.GridSize.Y; j++)
            {
                if (collisions.GetValueAt(i, j) != 0)
                {
                    physicsWorld.CreateRectangle(
                        collisions.TileSize,
                        collisions.TileSize,
                        1f,
                        new nkast.Aether.Physics2D.Common.Vector2(collisions.WorldPosition.X, collisions.WorldPosition.Y)
                        + new nkast.Aether.Physics2D.Common.Vector2((i * collisions.TileSize) + (collisions.TileSize / 2f), (j * collisions.TileSize) + (collisions.TileSize / 2f)),
                        0f,
                        BodyType.Static);
                }
            }
        }

        base.Initialize();

        renderer = new LdtkRenderer(spriteBatch, Content, worldFile);

        _ = renderer.PrerenderLevel(currentLevel);

        camera = new Camera(GraphicsDevice);
    }

    protected override void LoadContent()
    {
        // Create a new SpriteBatch, which can be used to draw textures.
        spriteBatch = new SpriteBatch(GraphicsDevice);

        var tilesFile = Content.Load<AsepriteFile>("tiles");
        var atlas = tilesFile.CreateTextureAtlas(
            GraphicsDevice,
            layers: tilesFile.Layers.ToArray().Select(l => l.Name).ToList());
        var region = atlas.GetRegion("tiles 0");

        var doors = currentLevel.GetEntities<LDtkTypes.Door>();
        foreach (var door in doors)
        {
            var newDoor = new Door(door, region, physicsWorld);
            newDoor.Load(spriteBatch);
            entities.Add(newDoor);
        }

        var worldFile = Content.Load<AsepriteFile>("world");
        var worldAtlas = worldFile.CreateTextureAtlas(
            GraphicsDevice,
            layers: worldFile.Layers.ToArray().Select(l => l.Name).ToList());
        var worldRegion = worldAtlas.GetRegion("world 0");

        var destructibles = currentLevel.GetEntities<LDtkTypes.Destructible>();
        foreach (var destructible in destructibles)
        {
            var newDestructible = new Destructible(destructible, worldRegion, physicsWorld);
            newDestructible.Load(spriteBatch);
            entities.Add(newDestructible);
        }

        var file = Content.Load<AsepriteFile>("entities");
        var spriteSheet = file.CreateSpriteSheet(GraphicsDevice, onlyVisibleLayers: true);

        _hero.Load(
            spriteSheet,
            region,
            spriteBatch);
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.D))
        {
            debug = !debug;
        }

        foreach (var entity in entities)
        {
            entity.Update(gameTime);
        }

        physicsWorld.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

        var cameraMinX = currentLevel.WorldX + (GraphicsDevice.Viewport.Width / 2f / pixelScale);
        var cameraMaxX = currentLevel.WorldX + currentLevel.PxWid - (GraphicsDevice.Viewport.Width / 2f / pixelScale);
        var cameraPosition = new Vector2(
            cameraMinX > _hero.CurrentPosition.X
                ? cameraMinX
                : cameraMaxX < _hero.CurrentPosition.X
                    ? cameraMaxX
                    : _hero.CurrentPosition.X,
            currentLevel.WorldY > _hero.CurrentPosition.Y ? currentLevel.WorldY : _hero.CurrentPosition.Y);
        camera.Position = cameraPosition;
        camera.Zoom = pixelScale;
        camera.Update();

        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(currentLevel._BgColor);

        renderer.RenderPrerenderedLevelX(currentLevel, camera);

        spriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, transformMatrix: camera.Transform);

        foreach (var entity in entities)
        {
            entity.Draw(gameTime);
        }

        if (debug)
        {
            foreach (var body in physicsWorld.BodyList)
            {
                if (body.FixtureList[0].Shape is PolygonShape s)
                {
                    var _texture = new Texture2D(GraphicsDevice, 1, 1);
                    _texture.SetData([Color.Blue]);
                    var aabb = s.Vertices.GetAABB();
                    spriteBatch.Draw(
                        _texture,
                        new Rectangle(
                            new Point((int)(body.Position.X - aabb.Extents.X), (int)(body.Position.Y - aabb.Extents.Y)),
                            new Point((int)s.Vertices.GetAABB().Extents.X * 2,
                                (int)s.Vertices.GetAABB().Extents.Y * 2)),
                        sourceRectangle: null,
                        Color.White,
                        rotation: 0,
                        origin: Vector2.Zero,
                        effects: SpriteEffects.None,
                        layerDepth: 0.1f);
                }
            }
        }

        spriteBatch.End();

        base.Draw(gameTime);
    }
}

internal abstract class Entity
{
    public abstract Rectangle? HitBox { get; }
    protected float Depth => HitBox?.Bottom / 1000f ?? 1f;

    public abstract void Update(GameTime gameTime);

    public abstract void Draw(GameTime gameTime);
}

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

internal static class PatchUtil
{
    public static Rectangle[] Create(Rectangle Bounds, Rectangle CenterBoundsRel)
    {
        var CenterBounds = new Rectangle(
            Bounds.Location.X + CenterBoundsRel.X,
            Bounds.Location.Y + CenterBoundsRel.Y,
            CenterBoundsRel.Width,
            CenterBoundsRel.Height);

        var widthLeft = CenterBounds.X - Bounds.Location.X;
        var widthCenter = CenterBounds.Width;
        var widthRight = Bounds.Width - widthLeft - widthCenter;

        var heightTop = CenterBounds.Y - Bounds.Location.Y;
        var heightMiddle = CenterBounds.Height;
        var heightBottom = Bounds.Height - heightTop - heightMiddle;

        var topLeft = new Rectangle(
            Bounds.Location.X,
            Bounds.Location.Y,
            widthLeft,
            heightTop);

        var topCenter = new Rectangle(
            CenterBounds.X,
            Bounds.Location.Y,
            widthCenter,
            heightTop);

        var topRight = new Rectangle(
            CenterBounds.X + CenterBounds.Width,
            Bounds.Location.Y,
            widthRight,
            heightTop);

        var middleLeft = new Rectangle(
            Bounds.Location.X,
            CenterBounds.Y,
            widthLeft,
            heightMiddle);

        var middleCenter = CenterBounds;

        var middleRight = new Rectangle(
            CenterBounds.X + CenterBounds.Width,
            CenterBounds.Y,
            widthRight,
            heightMiddle);

        var bottomLeft = new Rectangle(
            Bounds.Location.X,
            CenterBounds.Y + CenterBounds.Height,
            widthLeft,
            heightBottom);

        var bottomCenter = new Rectangle(
            CenterBounds.X,
            CenterBounds.Y + CenterBounds.Height,
            widthCenter,
            heightBottom);

        var bottomRight = new Rectangle(
            CenterBounds.X + CenterBounds.Width,
            CenterBounds.Y + CenterBounds.Height,
            widthRight,
            heightBottom);

        return
        [
            topLeft,
            topCenter,
            topRight,
            middleLeft,
            middleCenter,
            middleRight,
            bottomLeft,
            bottomCenter,
            bottomRight,
        ];
    }
}