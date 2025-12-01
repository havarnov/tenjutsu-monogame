using Microsoft.Xna.Framework;

namespace TenJutsu;

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