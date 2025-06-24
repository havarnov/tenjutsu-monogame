using AsepriteDotNet.Aseprite;
using LDtk;
using LDtkTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using TenJutsu;

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

        var playerStart = currentLevel.GetEntity<PlayerStart>();
        _hero = new Hero(playerStart.Position);

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
            var newDoor = new Door(door, region);
            newDoor.Load(spriteBatch);
            entities.Add(newDoor);
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
        var collisions = currentLevel.GetIntGrid("Collisions");

        var oldPosition = _hero.CurrentPosition;

        _hero.Update(gameTime, collisions, entities);

        foreach (var entity in entities)
        {
            entity.Update(gameTime, collisions);
        }

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

        _hero.Draw(gameTime);

        spriteBatch.End();

        base.Draw(gameTime);
    }
}

internal abstract class Entity
{
    public abstract Rectangle? HitBox { get; }

    public virtual void Load(SpriteBatch spriteBatch)
    {
    }

    public virtual void Update(GameTime gameTime, LDtkIntGrid collisions)
    {
    }

    public virtual void Draw(GameTime gameTime)
    {
    }
}

internal class Door(LDtkTypes.Door door, TextureRegion region) : Entity
{
    private SpriteBatch _spriteBatch = null!;
    private NineSliceSprite _nineSliceSprite = null!;
    private Vector2 _initialPosition = door.Position;
    public override Rectangle? HitBox => new Rectangle(_initialPosition.ToPoint(), door.Size.ToPoint());

    public override void Load(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
        _nineSliceSprite = new NineSliceSprite(region, "door");
    }

    public override void Draw(GameTime gameTime)
    {
        _nineSliceSprite.Draw(
            _spriteBatch,
            new Rectangle(_initialPosition.ToPoint(), door.Size.ToPoint()));
    }
}

internal static class RectangleExtensions
{
    public static Point TopLeft(this Rectangle rect) => new Point(rect.X, rect.Y);
    public static Point TopRight(this Rectangle rect) => new Point(rect.Right, rect.Y);
    public static Point BottomLeft(this Rectangle rect) => new Point(rect.Right, rect.Bottom);
    public static Point BottomRight(this Rectangle rect) => new Point(rect.Right, rect.Bottom);
}

internal class NineSliceSprite(TextureRegion region, string name)
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
                Color.White);
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