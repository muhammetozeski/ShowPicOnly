namespace ShowPicOnly;

/// <summary>
/// Every color the program draws with.
/// </summary>
static class Palette
{
    /// <summary>Window pixels of exactly this color are see-through. It is the window's transparency key.</summary>
    public static readonly Color Transparent = Color.FromArgb(255, 250, 255); //nobody would use this color instead of pure white. i hope...

    /// <summary>The box in the bottom right corner of the window that resizes it.</summary>
    public static readonly Color ResizeBox = Color.Gray;

    /// <summary>Background of pictures drawn from text. It must differ from <see cref="Transparent"/>, otherwise the background turns see-through.</summary>
    public static readonly Color TextBackground = Color.FromArgb(255, 255, 254);

    /// <summary>Letters of pictures drawn from text.</summary>
    public static readonly Color TextForeground = Color.FromArgb(102, 102, 102);
}
