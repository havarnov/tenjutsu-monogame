namespace LDtkTypes;

// This file was automatically generated, any modifications will be lost!
#pragma warning disable

using LDtk;
using Microsoft.Xna.Framework;

public partial class Item : ILDtkEntity
{
    public static Item Default() => new()
    {
        Identifier = "Item",
        Uid = 69,
        Size = new Vector2(16f, 16f),
        Pivot = new Vector2(0.5f, 1f),
        SmartColor = new Color(148, 217, 179, 255),

    };

    public string Identifier { get; set; }
    public System.Guid Iid { get; set; }
    public int Uid { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Pivot { get; set; }
    public Rectangle Tile { get; set; }

    public Color SmartColor { get; set; }

    public ItemType type { get; set; }
}
#pragma warning restore
