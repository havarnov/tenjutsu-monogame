namespace LDtkTypes;

// This file was automatically generated, any modifications will be lost!
#pragma warning disable

using LDtk;
using Microsoft.Xna.Framework;

public partial class Destructible : ILDtkEntity
{
    public static Destructible Default() => new()
    {
        Identifier = "Destructible",
        Uid = 36,
        Size = new Vector2(16f, 16f),
        Pivot = new Vector2(0.5f, 1.25f),
        Tile = new TilesetRectangle()
        {
            X = 128,
            Y = 560,
            W = 16,
            H = 16
        },
        SmartColor = new Color(139, 103, 74, 255),

        empty = false,
        breakColor1 = new Color(239, 125, 87, 1),
        breakColor2 = new Color(150, 64, 35, 1),
        yOff = -6,
    };

    public string Identifier { get; set; }
    public System.Guid Iid { get; set; }
    public int Uid { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Pivot { get; set; }
    public Rectangle Tile { get; set; }

    public Color SmartColor { get; set; }

    public TilesetRectangle tile { get; set; }
    public bool empty { get; set; }
    public bool playerDestructible { get; set; }
    public Color breakColor1 { get; set; }
    public Color breakColor2 { get; set; }
    public int yOff { get; set; }
}
#pragma warning restore
