using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
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
    private readonly GraphicsDeviceManager graphics;

    private LDtkWorld world = null!;
    private SpriteBatch spriteBatch = null!;
    private LDtkLevel currentLevel = null!;
    private Camera camera = null!;
    private float pixelScale = 1f;
    private LdtkRenderer renderer = null!;
    private Player player = null!;
    private readonly List<Entity> entities = [];

    public TenJutsuGame()
    {
        graphics = new GraphicsDeviceManager(this);
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
        player = new Player(playerStart.Position);

        base.Initialize();

        renderer = new LdtkRenderer(spriteBatch, Content, worldFile);

        _ = renderer.PrerenderLevel(currentLevel);

        camera = new Camera(GraphicsDevice);
    }

    protected override void LoadContent()
    {
        // Create a new SpriteBatch, which can be used to draw textures.
        spriteBatch = new SpriteBatch(GraphicsDevice);

        var file = Content.Load<AsepriteFile>("entities");
        var spriteSheet = file.CreateSpriteSheet(GraphicsDevice, onlyVisibleLayers: true);

        player.Load(spriteSheet, spriteBatch);
    }

    protected override void Update(GameTime gameTime)
    {
        var collisions = currentLevel.GetIntGrid("Collisions");
        player.Update(gameTime, collisions);

        var cameraMinX = currentLevel.WorldX + (GraphicsDevice.Viewport.Width / 2f / pixelScale);
        var cameraMaxX = currentLevel.WorldX + currentLevel.PxWid - (GraphicsDevice.Viewport.Width / 2f / pixelScale);
        var cameraPosition = new Vector2(
            cameraMinX > player.CurrentInitialPosition.X
                ? cameraMinX
                : cameraMaxX < player.CurrentInitialPosition.X
                    ? cameraMaxX
                    : player.CurrentInitialPosition.X,
            currentLevel.WorldY > player.CurrentInitialPosition.Y ? currentLevel.WorldY : player.CurrentInitialPosition.Y);
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

        player.Draw(gameTime);

        spriteBatch.End();

        base.Draw(gameTime);
    }
}

internal abstract class Entity
{
    private SpriteBatch _spriteBatch = null!;
    private AsepriteSlice slice = null!;

    public virtual void Load(AsepriteSlice slice, SpriteBatch spriteBatch)
    {
        this.slice = slice;
        // _spriteBatch = spriteBatch;
        // idle = spriteSheet.CreateAnimatedSprite("kIdle");
        // idle.Play();
    }

    public virtual void Update(GameTime gameTime, LDtkIntGrid collisions)
    {
    }

    public virtual void Draw(GameTime gameTime)
    {
    }
}

// internal class Door(Vector2 initialPosition) : Entity
// {
//     public override void Load(AsepriteSlice slice, SpriteBatch spriteBatch)
//     {
//     }
// }

internal class Player(Vector2 initialPosition) : Entity
{
    private SpriteBatch _spriteBatch = null!;
    private AnimatedSprite idle = null!;
    public Vector2 CurrentInitialPosition { get; private set; } = initialPosition;
    private Rectangle HitBox => new(CurrentInitialPosition.ToPoint() + new Point(-4, 5), new Point(7, 3));

    public void Load(SpriteSheet spriteSheet, SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
        idle = spriteSheet.CreateAnimatedSprite("kIdle");
        idle.Play();
    }


    public override void Update(GameTime gameTime, LDtkIntGrid collisions)
    {
        var oldPosition = CurrentInitialPosition;

        idle.Update(gameTime);

        var keyboardState = Keyboard.GetState();

        var speed = 100f;
        if (keyboardState.IsKeyDown(Keys.Left))
        {
            CurrentInitialPosition = CurrentInitialPosition with { X = CurrentInitialPosition.X - (speed * (float)gameTime.ElapsedGameTime.TotalSeconds) };
        }
        else if (keyboardState.IsKeyDown(Keys.Right))
        {
            CurrentInitialPosition = CurrentInitialPosition with { X = CurrentInitialPosition.X + (speed * (float)gameTime.ElapsedGameTime.TotalSeconds) };
        }

        if (keyboardState.IsKeyDown(Keys.Up))
        {
            CurrentInitialPosition = CurrentInitialPosition with { Y = CurrentInitialPosition.Y - (speed * (float)gameTime.ElapsedGameTime.TotalSeconds) };
        }
        else if (keyboardState.IsKeyDown(Keys.Down))
        {
            CurrentInitialPosition = CurrentInitialPosition with { Y = CurrentInitialPosition.Y + (speed * (float)gameTime.ElapsedGameTime.TotalSeconds) };
        }

        if (collisions.GetValueAt(collisions.FromWorldToGridSpace(HitBox.TopLeft())) != 0
            || collisions.GetValueAt(collisions.FromWorldToGridSpace(HitBox.TopRight())) != 0
            || collisions.GetValueAt(collisions.FromWorldToGridSpace(HitBox.BottomLeft())) != 0
            || collisions.GetValueAt(collisions.FromWorldToGridSpace(HitBox.BottomRight())) != 0)
        {
            CurrentInitialPosition = oldPosition;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var spritePosition = CurrentInitialPosition - new Vector2(16, 16);
        _spriteBatch.Draw(idle, spritePosition);
    }
}

internal static class RectangleExtensions
{
    public static Point TopLeft(this Rectangle rect) => new Point(rect.X, rect.Y);
    public static Point TopRight(this Rectangle rect) => new Point(rect.Right, rect.Y);
    public static Point BottomLeft(this Rectangle rect) => new Point(rect.Right, rect.Bottom);
    public static Point BottomRight(this Rectangle rect) => new Point(rect.Right, rect.Bottom);
}