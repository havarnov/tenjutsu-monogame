using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;

namespace TenJutsu;

internal class Hero(Vector2 initialPosition)
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

        private State _current = default;
        public State Current
        {
            get => _current;
            set
            {
                if (value == _current)
                {
                    return;
                }

                _animation?.Reset();
                _animation = _animations[value];
                _animation.FlipHorizontally = _facingLeft;
                _animation.Play();
                _current = value;
            }
        }

        public void Load(SpriteSheet spriteSheet)
        {
            _animations[State.Idle] = spriteSheet.CreateAnimatedSprite("kIdle");
            _animations[State.Running] = spriteSheet.CreateAnimatedSprite("kRun");
            Current = State.Idle;
        }
    }

    private enum State
    {
        Idle = 1,
        Running = 2,
    }

    private SpriteBatch _spriteBatch = null!;
    private TextureRegion _region = null!;

    public Vector2 CurrentPosition = initialPosition;
    public Rectangle HitBox => new(CurrentPosition.ToPoint() + new Point(-4, 5), new Point(7, 3));

    private readonly StateManager state = new();

    public void Load(
        SpriteSheet spriteSheet,
        TextureRegion region,
        SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
        _region = region;
        state.Load(spriteSheet);
    }


    public void Update(GameTime gameTime, LDtkIntGrid collisions)
    {
        state.Animation.Update(gameTime);

        var keyboardState = Keyboard.GetState();

        var speed = 1f;

        var horizontalInput =
            (keyboardState.IsKeyDown(Keys.Left) ? -1 : 0)
            + (keyboardState.IsKeyDown(Keys.Right) ? 1 : 0);

        var verticalInput =
            (keyboardState.IsKeyDown(Keys.Up) ? -1 : 0)
            + (keyboardState.IsKeyDown(Keys.Down) ? 1 : 0);

        var moveVector = new Vector2(horizontalInput, verticalInput);
        if (moveVector == Vector2.Zero)
        {
            state.Current = State.Idle;
            return;
        }

        state.Current = State.Running;

        var totalDesiredMovement = moveVector;
        totalDesiredMovement.Normalize();
        totalDesiredMovement *= speed;

        Vector2 startOfFramePosition = CurrentPosition;
        Vector2 finalEffectiveMovement = totalDesiredMovement;

        // Temp test for X axis
        CurrentPosition.X = startOfFramePosition.X + totalDesiredMovement.X;
        CurrentPosition.Y = startOfFramePosition.Y;

        bool collidedHorizontally = CheckForCollisionAtCurrentPosition(collisions);

        if (collidedHorizontally)
        {
            finalEffectiveMovement.X = 0;
        }

        // Temp test for Y axis
        CurrentPosition.X = startOfFramePosition.X;
        CurrentPosition.Y = startOfFramePosition.Y + totalDesiredMovement.Y;

        bool collidedVertically = CheckForCollisionAtCurrentPosition(collisions);

        if (collidedVertically)
        {
            finalEffectiveMovement.Y = 0;
        }

        if (finalEffectiveMovement != Vector2.Zero
            && (finalEffectiveMovement.X == 0 || finalEffectiveMovement.Y == 0))
        {
            // Only one component is non-zero
            finalEffectiveMovement.Normalize();
            finalEffectiveMovement *= speed;
        }

        // --- Apply the final, adjusted movement ---
        // Reset position to start before applying the final, combined movement
        CurrentPosition = startOfFramePosition;
        CurrentPosition += finalEffectiveMovement;

        if (finalEffectiveMovement.X != 0)
        {
            state.FacingLeft = finalEffectiveMovement.X < 0;
        }
    }

    private bool CheckForCollisionAtCurrentPosition(LDtkIntGrid collisions)
    {
        return collisions.GetValueAt(collisions.FromWorldToGridSpace(HitBox.TopLeft())) != 0
               || collisions.GetValueAt(collisions.FromWorldToGridSpace(HitBox.TopRight())) != 0
               || collisions.GetValueAt(collisions.FromWorldToGridSpace(HitBox.BottomLeft())) != 0
               || collisions.GetValueAt(collisions.FromWorldToGridSpace(HitBox.BottomRight())) != 0;
    }

    public void Draw(GameTime gameTime)
    {
        var groundShadowSlice = _region.GetSlice("groundShadow");
        _spriteBatch.Draw(
            _region.Texture,
            new Rectangle(
                (int)CurrentPosition.X - 8,
                (int)(CurrentPosition.Y + 6.5f),
                groundShadowSlice.Bounds.Width,
                groundShadowSlice.Bounds.Height),
            groundShadowSlice.Bounds,
            Color.White);

        var spritePosition = CurrentPosition - new Vector2(16, 16);
        _spriteBatch.Draw(state.Animation, spritePosition);
    }
}