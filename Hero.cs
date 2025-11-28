using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using nkast.Aether.Physics2D.Dynamics;

namespace TenJutsu;

internal class Hero : Entity
{
    private class StateManager
    {
        private readonly Dictionary<State, AnimatedSprite> _animations = [];

        private AnimatedSprite? _animation = null;
        public AnimatedSprite Animation => _animation!;

        private bool _facingLeft = false;

        public bool FacingLeft
        {
            get => _facingLeft;
            set
            {
                _facingLeft = value;
                if (_animation is not null)
                {
                    _animation.FlipHorizontally = _facingLeft;
                }
            }
        }

        public State Current
        {
            get;
            set
            {
                if (value == field)
                {
                    return;
                }

                _animation?.Reset();
                _animation = _animations[value];
                _animation.FlipHorizontally = _facingLeft;
                _animation.Play();
                field = value;

                if (field is State.PunchACharge)
                {
                    Animation.OnFrameEnd += _ =>
                    {
                        Current = State.PunchA;
                        Animation.OnAnimationLoop += _ => { Current = State.Idle; };
                    };
                }
            }
        } = default;

        public void Load(SpriteSheet spriteSheet)
        {
            _animations[State.Idle] = spriteSheet.CreateAnimatedSprite("kIdle");
            _animations[State.Running] = spriteSheet.CreateAnimatedSprite("kRun");
            _animations[State.PunchACharge] = spriteSheet.CreateAnimatedSprite("kPunchA_charge");
            _animations[State.PunchA] = spriteSheet.CreateAnimatedSprite("kPunchA_hit");
            Current = State.Idle;
        }
    }

    private enum State
    {
        Idle = 1,
        Running = 2,
        PunchACharge = 3,
        PunchA = 4,
    }

    private SpriteBatch _spriteBatch = null!;
    private TextureRegion _region = null!;
    private static Vector2 _size = new Vector2(15f, 5f);

    private readonly Body body;

    public Vector2 CurrentPosition => new Vector2(body.Position.X, body.Position.Y);
    public override Rectangle? HitBox => new(CurrentPosition.ToPoint(), _size.ToPoint());

    private readonly StateManager state = new();

    public Hero(World world, Vector2 initialPosition)
    {
        body = world.CreateBody(new nkast.Aether.Physics2D.Common.Vector2(initialPosition.X, initialPosition.Y), bodyType: BodyType.Dynamic);
        body.CreateRectangle(_size.X, _size.Y, 1f, nkast.Aether.Physics2D.Common.Vector2.Zero);
        body.Tag = this;
    }

    public void Load(
        SpriteSheet spriteSheet,
        TextureRegion region,
        SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
        _region = region;
        state.Load(spriteSheet);
    }

    // private State backTo = State.Idle;

    public override void Update(GameTime gameTime)
    {
        state.Animation.Update(gameTime);

        if (state.Current is State.PunchACharge or State.PunchA)
        {
            return;
        }

        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Space))
        {
            state.Current = State.PunchACharge;
            return;
        }

        var horizontalInput =
            (keyboardState.IsKeyDown(Keys.Left) ? -1 : 0)
            + (keyboardState.IsKeyDown(Keys.Right) ? 1 : 0);

        var verticalInput =
            (keyboardState.IsKeyDown(Keys.Up) ? -1 : 0)
            + (keyboardState.IsKeyDown(Keys.Down) ? 1 : 0);

        var moveVector = new Vector2(horizontalInput, verticalInput);
        const float speed = 60f;
        const float damping = 7f;

        if (moveVector == Vector2.Zero)
        {
            if (body.LinearVelocity.LengthSquared() < 0.1)
            {
                body.LinearVelocity = nkast.Aether.Physics2D.Common.Vector2.Zero;
            }
            else
            {
                body.LinearDamping = damping;
            }
        }
        else
        {
            var moveSpeed = moveVector * speed;
            body.LinearVelocity = new nkast.Aether.Physics2D.Common.Vector2(moveSpeed.X, moveSpeed.Y);
        }

        state.FacingLeft = body.LinearVelocity.X < 0 || body.LinearVelocity.X == 0f && state.FacingLeft;

        // if (body.LinearVelocity != nkast.Aether.Physics2D.Common.Vector2.Zero)
        if (moveVector != Vector2.Zero)
        {
            state.Current = State.Running;
        }
        else
        {
            state.Current = State.Idle;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var spritePosition = CurrentPosition - _size - new Vector2(1, 17);
        var groundShadowSlice = _region.GetSlice("groundShadow");
        _spriteBatch.Draw(
            _region.Texture,
            new Rectangle(
                spritePosition.ToPoint() + new Point(10, 21),
                groundShadowSlice.Bounds.Size),
            groundShadowSlice.Bounds,
            Color.White,
            rotation: 0,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: Depth);

        _spriteBatch.Draw(
            state.Animation.TextureRegion,
            spritePosition,
            state.Animation.Color *  state.Animation.Transparency,
            state.Animation.Rotation,
            Vector2.Zero,
            state.Animation.Scale,
            state.Animation.SpriteEffects,
            layerDepth: Depth);
    }
}