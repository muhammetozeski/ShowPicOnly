using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ShowPicOnly;

/// <summary>
/// Produces the pictures the window shows: an image file, the clipboard content, or text drawn on a bitmap.
/// </summary>
static class PictureLoader
{
    const string DefaultFontName = "Arial";
    const int DefaultFontSizeInPixels = 60;
    const float WidthDivideRatio = 1.5f;

    /// <summary>
    /// Loads an image file. A path that does not exist or a file that is not an image is written to <see cref="ErrorLog"/>.
    /// </summary>
    /// <param name="path">Path of the image file. Double quotes in it are removed. Can be null.</param>
    /// <returns>The image, or null when the path is null, the file does not exist, or the file cannot be read as an image.</returns>
    public static Image? FromFile(string? path)
    {
        path = path?.Replace("\"", "");
        if (path is null) return null;
        if (!File.Exists(path))
        {
            ErrorLog.Write($"\"{path}\" does not exist.");
            return null;
        }

        try { return Image.FromFile(path); }
        catch (Exception exception)
        {
            ErrorLog.Write($"\"{path}\" could not be loaded as an image.", exception);
            return null;
        }
    }

    /// <summary>
    /// Turns the clipboard content into a picture. An image is used as it is, from copied files the first one is loaded
    /// with <see cref="FromFile"/>, and text is drawn with <see cref="FromText"/>. A clipboard that cannot be read is
    /// written to <see cref="ErrorLog"/>.
    /// </summary>
    /// <returns>The picture, or null when the clipboard holds none of these or its content cannot be turned into a picture.</returns>
    public static Image? FromClipboard()
    {
        try
        {
            if (Clipboard.ContainsImage()) return Clipboard.GetImage();
            if (Clipboard.ContainsFileDropList()) return Clipboard.GetFileDropList() is [var firstFile, ..] ? FromFile(firstFile) : null;
            return Clipboard.ContainsText() ? FromText(Clipboard.GetText()) : null;
        }
        catch (Exception exception)
        {
            ErrorLog.Write("The clipboard could not be read.", exception);
            return null;
        }
    }

    /// <summary>
    /// Draws text on a bitmap that is exactly as large as the text, then narrows the bitmap by <see cref="WidthDivideRatio"/>.
    /// A text that cannot be drawn, for example because the bitmap would be too large, is written to <see cref="ErrorLog"/>.
    /// </summary>
    /// <param name="text">The text to draw. Line breaks start new lines.</param>
    /// <param name="fontName">Font family of the letters.</param>
    /// <param name="fontSizeInPixels">Height of the font in pixels.</param>
    /// <returns>The bitmap, or null when the text is null or empty, or too large for a bitmap.</returns>
    public static Bitmap? FromText(string? text, string fontName = DefaultFontName, int fontSizeInPixels = DefaultFontSizeInPixels)
    {
        if (string.IsNullOrEmpty(text)) return null;

        try
        {
            using Font font = new(fontName, fontSizeInPixels, FontStyle.Bold, GraphicsUnit.Pixel);
            Size textSize;
            using (Bitmap measuringBitmap = new(1, 1))
            using (Graphics measuringGraphics = Graphics.FromImage(measuringBitmap))
                textSize = Size.Truncate(measuringGraphics.MeasureString(text, font));

            using Bitmap textBitmap = new(textSize.Width, textSize.Height);
            using (Graphics graphics = Graphics.FromImage(textBitmap))
            using (SolidBrush textBrush = new(Palette.TextForeground))
            {
                graphics.Clear(Palette.TextBackground);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.DrawString(text, font, textBrush, 0, 0);
            }

            //it's being so wide. idk why. so i should reduce the width size like this:
            return new Bitmap(textBitmap, new Size((int)(textBitmap.Width / WidthDivideRatio), textBitmap.Height));
        }
        catch (Exception exception)
        {
            ErrorLog.Write($"A text of {text.Length} characters could not be drawn.", exception);
            return null;
        }
    }
}
